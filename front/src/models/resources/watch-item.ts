/*
 * Kyoo - A portable and vast media library solution.
 * Copyright (c) Kyoo.
 *
 * See AUTHORS.md and LICENSE file in the project root for full license information.
 *
 * Kyoo is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * any later version.
 *
 * Kyoo is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Kyoo. If not, see <https://www.gnu.org/licenses/>.
 */

import { z } from "zod";
import { zdate } from "~/utils/zod";
import { ResourceP, ImagesP } from "../traits";
import { EpisodeP } from "./episode";

/**
 * A video, audio or subtitle track for an episode.
 */
export const TrackP = ResourceP.extend({
	/**
	 * The title of the stream.
	 */
	title: z.string().nullable(),
	/**
	 * The language of this stream (as a ISO-639-2 language code)
	 */
	language: z.string().nullable(),
	/**
	 * The codec of this stream.
	 */
	codec: z.string(),
	/**
	 * Is this stream the default one of it's type?
	 */
	isDefault: z.boolean(),
	/**
	 * Is this stream tagged as forced?
	 */
	isForced: z.boolean(),
	/**
	 * Is this track extern to the episode's file?
	 */
	isExternal: z.boolean(),
	/**
	 * The index of this track on the episode.
	 */
	trackIndex: z.number(),
	/**
	 * A user-friendly name for this track. It does not include the track type.
	 */
	displayName: z.string(),
});
export type Track = z.infer<typeof TrackP>;

export const FontP = z.object({
	/*
	 * A human-readable identifier, used in the URL.
	 */
	slug: z.string(),
	/*
	 * The name of the font file (with the extension).
	 */
	file: z.string(),
	/*
	 * The format of this font (the extension).
	 */
	format: z.string(),
	/*
	 * The url of the font.
	 */
	link: z.string(),
});
export type Font = z.infer<typeof FontP>;

export const ChapterP = z.object({
	/**
	 * The start time of the chapter (in second from the start of the episode).
	 */
	startTime: z.number(),
	/**
	 * The end time of the chapter (in second from the start of the episode).
	 */
	endTime: z.number(),
	/**
	 * The name of this chapter. This should be a human-readable name that could be presented to the
	 * user. There should be well-known chapters name for commonly used chapters. For example, use
	 * "Opening" for the introduction-song and "Credits" for the end chapter with credits.
	 */
	name: z.string(),
});
export type Chapter = z.infer<typeof ChapterP>;

const WatchMovieP = z.preprocess(
	(x: any) => {
		if (!x) return x;

		x.name = x.title;
		x.link = {
			direct: `/api/video/${x.slug}`,
		};
		x.fonts = x.fonts?.map((y: Font) => {
			y.link = `/api/watch/${x.slug}/font/${y.slug}.${y.format}`;
			return y;
		})
		return x;
	},
	ImagesP.extend({
		/**
		 * The slug of this episode.
		 */
		slug: z.string(),
		/**
		 * The title of this episode.
		 */
		name: z.string(),
		/**
		 * The release date of this episode. It can be null if unknown.
		 */
		releaseDate: zdate().nullable(),
		/**
		 * The container of the video file of this episode. Common containers are mp4, mkv, avi and so on.
		 */
		container: z.string(),
		/**
		 * The video track. See Track for more information.
		 */
		video: TrackP,
		/**
		 * The list of audio tracks. See Track for more information.
		 */
		audios: z.array(TrackP),
		/**
		 * The list of subtitles tracks. See Track for more information.
		 */
		subtitles: z.array(TrackP),
		/**
		 * The list of fonts that can be used to display subtitles.
		 */
		fonts: z.array(FontP),
		/**
		 * The list of chapters. See Chapter for more information.
		 */
		chapters: z.array(ChapterP),
		/**
		 * The links to the videos of this watch item.
		 */
		link: z.object({
			direct: z.string(),
		}),
	}),
);

const WatchEpisodeP = WatchMovieP.and(
	z.object({
		/**
		 * The ID of the episode associated with this item.
		 */
		episodeID: z.number(),
		/**
		 * The title of the show containing this episode.
		 */
		showTitle: z.string(),
		/**
		 * The slug of the show containing this episode
		 */
		showSlug: z.string(),
		/**
		 * The season in witch this episode is in.
		 */
		seasonNumber: z.number(),
		/**
		 * The number of this episode is it's season.
		 */
		episodeNumber: z.number(),
		/**
		 * The absolute number of this episode. It's an episode number that is not reset to 1 after a new season.
		 */
		absoluteNumber: z.number(),
		/**
		 * The episode that come before this one if you follow usual watch orders. If this is the first
		 * episode or this is a movie, it will be null.
		 */
		previousEpisode: EpisodeP.nullable(),
		/**
		 * The episode that come after this one if you follow usual watch orders. If this is the last
		 * aired episode or this is a movie, it will be null.
		 */
		nextEpisode: EpisodeP.nullable(),
	}),
);

export const WatchItemP = z.union([
	WatchMovieP.and(z.object({ isMovie: z.literal(true) })),
	WatchEpisodeP.and(z.object({ isMovie: z.literal(false) })),
]);

/**
 * A watch item for a movie or an episode
 */
export type WatchItem = z.infer<typeof WatchItemP>;