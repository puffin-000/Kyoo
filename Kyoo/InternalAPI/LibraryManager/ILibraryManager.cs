﻿using Kyoo.Models;
using Kyoo.Models.Watch;
using System.Collections.Generic;

namespace Kyoo.InternalAPI
{
    public interface ILibraryManager
    {
        //Read values
        string GetShowExternalIDs(long showID);
        IEnumerable<Show> GetShows();
        Studio GetStudio(long showID);
        List<People> GetDirectors(long showID);
        List<People> GetPeople(long showID);
        List<Genre> GetGenreForShow(long showID);
        List<Season> GetSeasons(long showID);
        int GetSeasonCount(string showSlug, long seasonNumber);
        IEnumerable<Show> GetShowsInCollection(long collectionID);
        List<Show> GetShowsInLibrary(long libraryID);
        IEnumerable<Show> GetShowsByPeople(long peopleID);
        IEnumerable<string> GetLibrariesPath();

        //Internal HTML read
        (List<Track> audios, List<Track> subtitles) GetStreams(long episodeID, string showSlug);
        Track GetSubtitle(string showSlug, long seasonNumber, long episodeNumber, string languageTag, bool forced);

        //Public read
        Library GetLibrary(string librarySlug);
        IEnumerable<Library> GetLibraries();
        Show GetShowBySlug(string slug);
        Season GetSeason(string showSlug, long seasonNumber);
        List<Episode> GetEpisodes(string showSlug);
        List<Episode> GetEpisodes(string showSlug, long seasonNumber);
        Episode GetEpisode(string showSlug, long seasonNumber, long episodeNumber);
        WatchItem GetWatchItem(string showSlug, long seasonNumber, long episodeNumber, bool complete = true);
        People GetPeopleBySlug(string slug);
        Genre GetGenreBySlug(string slug);
        Studio GetStudioBySlug(string slug);
        Collection GetCollection(string slug);

        //Check if value exists
        bool IsCollectionRegistered(string collectionSlug);
        bool IsCollectionRegistered(string collectionSlug, out long collectionID);
        bool IsShowRegistered(string showPath);
        bool IsShowRegistered(string showPath, out long showID);
        bool IsSeasonRegistered(long showID, long seasonNumber);
        bool IsSeasonRegistered(long showID, long seasonNumber, out long seasonID);
        bool IsEpisodeRegistered(string episodePath);

        //Register values
        long RegisterCollection(Collection collection);
        long RegisterShow(Show show);
        long RegisterSeason(Season season);
        long RegisterEpisode(Episode episode);
        void RegisterTrack(Track track);

        long GetOrCreateGenre(Genre genre);
        long GetOrCreateStudio(Studio studio);

        void RegisterShowPeople(long showID, List<People> actors);
        void AddShowToCollection(long showID, long collectionID);
        void RegisterInLibrary(long showID, string libraryPath);

        void ClearSubtitles(long episodeID);
    }
}
