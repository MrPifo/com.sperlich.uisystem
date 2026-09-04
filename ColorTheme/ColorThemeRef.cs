using System;
using UnityEngine;

namespace Sperlich.UISystem.Themes {
	/// <summary>
	/// Wraps a color theme that can either point to a persisted <see cref="ColorThemeAsset"/>
	/// or hold its own inline, serialized <see cref="ColorTheme"/> instance. Lets a component field
	/// switch between a shared/reusable theme and a one-off theme without changing its type.
	/// </summary>
	[Serializable]
	public class ColorThemeRef {

		public enum Source {
			Asset,
			Custom
		}

		public Source source = Source.Asset;
		public ColorThemeAsset asset;
		public ColorTheme custom = new ColorTheme();

		public bool HasTheme => source == Source.Asset ? asset != null : custom != null;

		/// <summary>The theme currently in effect (the asset's theme, or the inline one). Null only if Source.Asset with no asset assigned.</summary>
		public ColorTheme ActiveTheme => source == Source.Asset ? asset != null ? asset.theme : null : custom;

		public Color GetColor(ComponentState state) {
			ColorTheme theme = ActiveTheme;
			return theme != null ? theme.GetColor(state) : Color.white;
		}

		/// <summary>Writes to whichever theme is active. In Source.Asset mode this edits the shared asset.</summary>
		public void SetColor(ComponentState state, Color color) {
			ActiveTheme?.SetColor(state, color);
		}

		public Color this[ComponentState state] {
			get => GetColor(state);
			set => SetColor(state, value);
		}

		public Color Normal { get => GetColor(ComponentState.Normal); set => SetColor(ComponentState.Normal, value); }
		public Color Hovered { get => GetColor(ComponentState.Hovered); set => SetColor(ComponentState.Hovered, value); }
		public Color Pressed { get => GetColor(ComponentState.Pressed); set => SetColor(ComponentState.Pressed, value); }
		public Color Selected { get => GetColor(ComponentState.Selected); set => SetColor(ComponentState.Selected, value); }
		public Color Disabled { get => GetColor(ComponentState.Disabled); set => SetColor(ComponentState.Disabled, value); }
		public Color ReadOnly { get => GetColor(ComponentState.ReadOnly); set => SetColor(ComponentState.ReadOnly, value); }

		/// <summary>Switches to Source.Asset and points at the given (possibly null) asset.</summary>
		public void UseAsset(ColorThemeAsset asset) {
			source = Source.Asset;
			this.asset = asset;
		}

		/// <summary>Switches to Source.Custom, keeping the existing inline theme unless one is supplied.</summary>
		public void UseCustom(ColorTheme theme = null) {
			source = Source.Custom;
			custom = theme ?? custom ?? new ColorTheme();
		}

		public ColorThemeRef() { }
		private ColorThemeRef(Source source, ColorThemeAsset asset, ColorTheme custom) {
			this.source = source;
			this.asset = asset;
			this.custom = custom ?? new ColorTheme();
		}

		public static ColorThemeRef FromAsset(ColorThemeAsset asset) => new(Source.Asset, asset, null);
		public static ColorThemeRef FromCustom(ColorTheme theme) => new(Source.Custom, null, theme);

		public static implicit operator bool(ColorThemeRef reference) => reference != null && reference.HasTheme;
	}
}
