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

import { Font, Track } from "@kyoo/models";
import { IconButton, tooltip, Menu, ts, A } from "@kyoo/primitives";
import { useAtom } from "jotai";
import { useRouter } from "solito/router";
import { Platform, View } from "react-native";
import { useTranslation } from "react-i18next";
import ClosedCaption from "@material-symbols/svg-400/rounded/closed_caption-fill.svg";
import Fullscreen from "@material-symbols/svg-400/rounded/fullscreen-fill.svg";
import FullscreenExit from "@material-symbols/svg-400/rounded/fullscreen_exit-fill.svg";
import { Stylable, useYoshiki } from "yoshiki/native";
import { createParam } from "solito";
import { fullscreenAtom, subtitleAtom } from "../state";

const { useParam } = createParam<{ subtitle?: string }>();

export const RightButtons = ({
	subtitles,
	fonts,
	onMenuOpen,
	onMenuClose,
	...props
}: {
	subtitles?: Track[];
	fonts?: Font[];
	onMenuOpen: () => void;
	onMenuClose: () => void;
} & Stylable) => {
	const { css } = useYoshiki();
	const { t } = useTranslation();
	const [isFullscreen, setFullscreen] = useAtom(fullscreenAtom);
	const [selectedSubtitle, setSubtitle] = useParam("subtitle");

	const spacing = css({ marginHorizontal: ts(1) });

	subtitles = [{ id: 1, slug: "oto", displayName: "toto" }];

	return (
		<View {...css({ flexDirection: "row" }, props)}>
			{subtitles && (
				<Menu
					Triger={IconButton}
					icon={ClosedCaption}
					{...tooltip(t("player.subtitles"), true)}
					{...spacing}
					// id="sortby"
					// aria-controls={subtitleAnchor ? "subtitle-menu" : undefined}
					// aria-haspopup="true"
					// aria-expanded={subtitleAnchor ? "true" : undefined}
				>
					<Menu.Item
						label={t("player.subtitle-none")}
						selected={!selectedSubtitle}
						onPress={() => setSubtitle(undefined)}
					/>
					{subtitles.map((x) => (
						<Menu.Item
							key={x.id}
							label={x.displayName}
							selected={selectedSubtitle === x.slug}
							onPress={() => setSubtitle(x.slug)}
						/>
					))}
				</Menu>
			)}
			{Platform.OS === "web" && (
				<IconButton
					icon={isFullscreen ? FullscreenExit : Fullscreen}
					onPress={() => setFullscreen(!isFullscreen)}
					{...tooltip(t("player.fullscreen"), true)}
					{...spacing}
				/>
			)}
			{/* {subtitleAnchor && ( */}
			{/* 	<SubtitleMenu */}
			{/* 		subtitles={subtitles!} */}
			{/* 		fonts={fonts!} */}
			{/* 		anchor={subtitleAnchor} */}
			{/* 		onClose={() => { */}
			{/* 			setSubtitleAnchor(null); */}
			{/* 			onMenuClose(); */}
			{/* 		}} */}
			{/* 	/> */}
			{/* )} */}
		</View>
	);
};

const SubtitleMenu = ({
	subtitles,
	fonts,
	anchor,
	onClose,
}: {
	subtitles: Track[];
	fonts: Font[];
	anchor: HTMLElement;
	onClose: () => void;
}) => {
	const router = useRouter();
	const { t } = useTranslation("player");
	const [selectedSubtitle, setSubtitle] = useAtom(subtitleAtom);
	const { subtitle, ...queryWithoutSubs } = router.query;

	return (
		<Menu
			id="subtitle-menu"
			MenuListProps={{
				"aria-labelledby": "subtitle",
			}}
			anchorEl={anchor}
			open={!!anchor}
			onClose={onClose}
			anchorOrigin={{
				vertical: "top",
				horizontal: "center",
			}}
			transformOrigin={{
				vertical: "bottom",
				horizontal: "center",
			}}
		>
			<MenuItem
				selected={!selectedSubtitle}
				onClick={() => {
					setSubtitle(null);
					onClose();
				}}
				component={Link}
				to={{ query: queryWithoutSubs }}
				shallow
				replace
			>
				<ListItemText>{t("subtitle-none")}</ListItemText>
			</MenuItem>
			{subtitles.map((sub) => (
				<MenuItem
					key={sub.id}
					selected={selectedSubtitle?.id === sub.id}
					onClick={() => {
						setSubtitle({ track: sub, fonts });
						onClose();
					}}
					component={Link}
					to={{ query: { ...router.query, subtitle: sub.language ?? sub.id } }}
					shallow
					replace
				>
					<ListItemText>{sub.displayName}</ListItemText>
				</MenuItem>
			))}
		</Menu>
	);
};
