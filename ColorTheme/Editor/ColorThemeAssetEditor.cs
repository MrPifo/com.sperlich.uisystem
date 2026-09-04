using Sperlich.EditorKit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Themes.Editor {
	using FlexDirection = UnityEngine.UIElements.FlexDirection;

	[CustomEditor(typeof(ColorThemeAsset))]
	public class ColorThemeAssetEditor : UnityEditor.Editor {

		public override VisualElement CreateInspectorGUI() {
			var root = new VisualElement {
				style = {
					paddingTop = 2,
					paddingBottom = 4,
					marginLeft = -15,
					marginRight = -4
				}
			};

			SerializedProperty themeProp = serializedObject.FindProperty("theme");

			// Section mit Chevron
			var (header, body, _) = SperlichEditorWidgets.CreateChevronSection("THEME COLORS", true, SperlichEditorTheme.BgStep, null, nameof(ColorThemeAssetEditor));
			body.style.paddingLeft = 8;
			body.style.paddingRight = 8;
			body.style.paddingTop = 6;
			body.style.paddingBottom = 8;

			// Context Menu (Copy/Paste)
			header.AddManipulator(new ContextualMenuManipulator(evt => {
				evt.menu.AppendAction("Copy Colors", _ => {
					ColorThemeUtility.Clipboard = ColorThemeUtility.ReadColors(themeProp);
					ColorThemeUtility.HasClipboard = true;
				});
				evt.menu.AppendAction("Paste Colors", _ => {
					ColorThemeUtility.WriteColors(themeProp, ColorThemeUtility.Clipboard);
					EditorUtility.SetDirty(target);
				}, ColorThemeUtility.HasClipboard ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			}));

			// Reset Button in Header oder Body
			var actionsRow = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					justifyContent = Justify.FlexEnd,
					marginBottom = 6
				}
			};
			var resetBtn = SperlichEditorWidgets.MakeButton("Reset to Defaults", 120, () => {
				ColorThemeUtility.ResetToDefaults(themeProp);
				EditorUtility.SetDirty(target);
			});
			actionsRow.Add(resetBtn);
			body.Add(actionsRow);

			// Farbraster (2 Spalten fr 6 Farben)
			body.Add(ColorThemeUtility.CreateColorGrid(themeProp));

			var wrap = new VisualElement { style = { marginBottom = 4 } };
			wrap.Add(header);
			wrap.Add(body);
			root.Add(wrap);

			// Scroll-Position beibehalten
			SperlichInspectorScroll.Preserve(root, target);

			return root;
		}
	}
}
