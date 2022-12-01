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

import { styled } from "@mui/material";
import { ComponentProps, forwardRef } from "react";

type CastProps = { class?: string };

declare global {
	namespace JSX {
		interface IntrinsicElements {
			"google-cast-launcher": CastProps;
		}
	}
}

export const _CastButton = forwardRef<HTMLDivElement, ComponentProps<"div">>(function Cast(
	{ className, ...props },
	ref,
) {
	return <google-cast-launcher ref={ref} class={className} {...props} />;
});
export const CastButton = styled(_CastButton)({});