﻿using Kyoo.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Kyoo.Controllers
{
	public class ThumbnailsManager : IThumbnailsManager
	{
		private readonly IConfiguration _config;
		private readonly IFileManager _files;
		private readonly string _peoplePath;
		private readonly string _providerPath;

		public ThumbnailsManager(IConfiguration configuration, IFileManager files)
		{
			_config = configuration;
			_files = files;
			_peoplePath = Path.GetFullPath(configuration.GetValue<string>("peoplePath"));
			_providerPath = Path.GetFullPath(configuration.GetValue<string>("providerPath"));
		}

		private static async Task DownloadImage(string url, string localPath, string what)
		{
			try
			{
				using WebClient client = new();
				await client.DownloadFileTaskAsync(new Uri(url), localPath);
			}
			catch (WebException exception)
			{
				await Console.Error.WriteLineAsync($"{what} could not be downloaded. Error: {exception.Message}.");
			}
		}

		public async Task<Show> Validate(Show show, bool alwaysDownload)
		{
			if (show.Poster != null)
			{
				string posterPath = await GetShowPoster(show);
				if (alwaysDownload || !File.Exists(posterPath))
					await DownloadImage(show.Poster, posterPath, $"The poster of {show.Title}");
			}
			if (show.Logo != null)
			{
				string logoPath = await GetShowLogo(show);
				if (alwaysDownload || !File.Exists(logoPath))
					await DownloadImage(show.Logo, logoPath, $"The logo of {show.Title}");
			}
			if (show.Backdrop != null)
			{
				string backdropPath = await GetShowBackdrop(show);
				if (alwaysDownload || !File.Exists(backdropPath))
					await DownloadImage(show.Backdrop, backdropPath, $"The backdrop of {show.Title}");
			}
			
			foreach (PeopleRole role in show.People)
				await Validate(role.People, alwaysDownload);

			return show;
		}

		public async Task<People> Validate([NotNull] People people, bool alwaysDownload)
		{
			if (people == null)
				throw new ArgumentNullException(nameof(people));
			string root = _config.GetValue<string>("peoplePath");
			string localPath = Path.Combine(root, people.Slug + ".jpg");
			
			Directory.CreateDirectory(root);
			if (alwaysDownload || !File.Exists(localPath))
				await DownloadImage(people.Poster, localPath, $"The profile picture of {people.Name}");
			
			return people;
		}

		public async Task<Season> Validate(Season season, bool alwaysDownload)
		{
			if (season?.Show?.Path == null)
				return default;

			if (season.Poster != null)
			{
				string localPath = await GetSeasonPoster(season);
				if (alwaysDownload || !File.Exists(localPath))
					await DownloadImage(season.Poster, localPath, $"The poster of {season.Show.Title}'s season {season.SeasonNumber}");
			}
			return season;
		}
		
		public async Task<Episode> Validate(Episode episode, bool alwaysDownload)
		{
			if (episode?.Path == null)
				return default;

			if (episode.Thumb != null)
			{
				string localPath = await GetEpisodeThumb(episode);
				if (alwaysDownload || !File.Exists(localPath))
					await DownloadImage(episode.Thumb, localPath, $"The thumbnail of {episode.Slug}");
			}
			return episode;
		}

		public async Task<ProviderID> Validate(ProviderID provider, bool alwaysDownload)
		{
			if (provider.Logo == null)
				return provider;

			string root = _config.GetValue<string>("providerPath");
			string localPath = Path.Combine(root, provider.Slug + ".jpg");
			
			Directory.CreateDirectory(root);
			if (alwaysDownload || !File.Exists(localPath))
				await DownloadImage(provider.Logo, localPath, $"The logo of {provider.Slug}");
			return provider;
		}

		public Task<string> GetShowBackdrop(Show show)
		{
			if (show?.Path == null)
				throw new ArgumentNullException(nameof(show));
			return Task.FromResult(Path.Combine(_files.GetExtraDirectory(show), "backdrop.jpg"));
		}
		
		public Task<string> GetShowLogo(Show show)
		{
			if (show?.Path == null)
				throw new ArgumentNullException(nameof(show));
			return Task.FromResult(Path.Combine(_files.GetExtraDirectory(show), "logo.png"));
		}
		
		public Task<string> GetShowPoster(Show show)
		{
			if (show?.Path == null)
				throw new ArgumentNullException(nameof(show));
			return Task.FromResult(Path.Combine(_files.GetExtraDirectory(show), "poster.jpg"));
		}

		public Task<string> GetSeasonPoster(Season season)
		{
			if (season == null)
				throw new ArgumentNullException(nameof(season));
			return Task.FromResult(Path.Combine(_files.GetExtraDirectory(season), $"season-{season.SeasonNumber}.jpg"));
		}

		public Task<string> GetEpisodeThumb(Episode episode)
		{
			return Task.FromResult(Path.Combine(
				_files.GetExtraDirectory(episode),
				"Thumbnails",
				$"{Path.GetFileNameWithoutExtension(episode.Path)}.jpg"));
		}

		public Task<string> GetPeoplePoster(People people)
		{
			if (people == null)
				throw new ArgumentNullException(nameof(people));
			string thumbPath = Path.GetFullPath(Path.Combine(_peoplePath, $"{people.Slug}.jpg"));
			if (!thumbPath.StartsWith(_peoplePath) || File.Exists(thumbPath))
				return Task.FromResult<string>(null);
			return Task.FromResult(thumbPath);
		}

		public Task<string> GetProviderLogo(ProviderID provider)
		{
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));
			string thumbPath = Path.GetFullPath(Path.Combine(_providerPath, $"{provider.Slug}.jpg"));
			return Task.FromResult(thumbPath.StartsWith(_providerPath) ? thumbPath : null);
		}
	}
}
