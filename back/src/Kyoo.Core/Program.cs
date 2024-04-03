// Kyoo - A portable and vast media library solution.
// Copyright (c) Kyoo.
//
// See AUTHORS.md and LICENSE file in the project root for full license information.
//
// Kyoo is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// Kyoo is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Kyoo. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using Kyoo.Authentication;
using Kyoo.Core;
using Kyoo.Core.Extensions;
using Kyoo.Meiliseach;
using Kyoo.Postgresql;
using Kyoo.RabbitMq;
using Kyoo.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;

#if DEBUG
const string EnvironmentName = "Development";
#else
const string EnvironmentName = "Production";
#endif

WebApplicationBuilder builder = WebApplication.CreateBuilder(
	new WebApplicationOptions()
	{
		Args = args,
		EnvironmentName = EnvironmentName,
		ApplicationName = "Kyoo",
		ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
	}
);
builder.WebHost.UseKestrel(opt =>
{
	opt.AddServerHeader = false;
});

const string template =
	"[{@t:HH:mm:ss} {@l:u3} {Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1), 25} "
	+ "({@i:D10})] {@m}{#if not EndsWith(@m, '\n')}\n{#end}{@x}";
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Warning()
	.MinimumLevel.Override("Kyoo", LogEventLevel.Verbose)
	.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Verbose)
	.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Fatal)
	.WriteTo.Console(new ExpressionTemplate(template, theme: TemplateTheme.Code))
	.Enrich.WithThreadId()
	.Enrich.FromLogContext()
	.CreateLogger();
AppDomain.CurrentDomain.ProcessExit += (_, _) => Log.CloseAndFlush();
AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
	Log.Fatal(ex.ExceptionObject as Exception, "Unhandled exception");
builder.Host.UseSerilog();

builder
	.Services.AddMvcCore()
	.AddApplicationPart(typeof(CoreModule).Assembly)
	.AddApplicationPart(typeof(AuthenticationModule).Assembly);

builder.Services.ConfigureMvc();
builder.Services.ConfigureOpenApi();
builder.ConfigureKyoo();
builder.ConfigureAuthentication();
builder.ConfigurePostgres();
builder.ConfigureMeilisearch();
builder.ConfigureRabbitMq();

WebApplication app = builder.Build();
CoreModule.Services = app.Services;

app.UseHsts();
app.UseKyooOpenApi();
app.UseResponseCompression();
app.UseRouting();
app.UseAuthentication();
app.MapControllers();

// TODO: wait 4.5.0 and delete this
static void MoveAll(DirectoryInfo source, DirectoryInfo target)
{
	if (source.FullName == target.FullName)
		return;

	Directory.CreateDirectory(target.FullName);

	foreach (FileInfo fi in source.GetFiles())
		fi.MoveTo(Path.Combine(target.ToString(), fi.Name), true);

	foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
	{
		DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
		MoveAll(diSourceSubDir, nextTargetSubDir);
	}
	Directory.Delete(source.FullName);
}

try
{
	string oldDir = "/kyoo/metadata";
	if (Path.Exists(oldDir))
	{
		MoveAll(new DirectoryInfo(oldDir), new DirectoryInfo("/metadata"));
		Log.Information("Old metadata directory migrated.");
	}
}
catch (Exception ex)
{
	Log.Fatal(
		ex,
		"Unhandled error while trying to migrate old metadata images to new directory. Giving up and continuing normal startup."
	);
}

// Activate services that always run in the background
app.Services.GetRequiredService<MeiliSync>();
app.Services.GetRequiredService<RabbitProducer>();

await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
	await MeilisearchModule.Initialize(scope.ServiceProvider);
}

app.Run(Environment.GetEnvironmentVariable("KYOO_BIND_URL") ?? "http://*:5000");