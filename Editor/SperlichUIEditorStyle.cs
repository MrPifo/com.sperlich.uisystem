using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Editor {
	public static class SperlichUIEditorStyle {

		public static readonly Color ActiveColor = new Color(0.196f, 0.804f, 0.196f);
		public static readonly Color InactiveColor = new Color(0.5f, 0.5f, 0.5f);
		public static readonly Color HeaderTextColor = new Color(0.75f, 0.75f, 0.75f);
		public static readonly Color HeaderLineColor = new Color(1f, 1f, 1f, 0.08f);

		public static VisualElement CreateSectionHeader(string title) {
			var container = new VisualElement();
			container.style.marginTop = 8;
			container.style.marginBottom = 3;

			var label = new Label(title);
			label.style.unityFontStyleAndWeight = FontStyle.Bold;
			label.style.fontSize = 11;
			label.style.color = HeaderTextColor;
			container.Add(label);

			var line = new VisualElement();
			line.style.height = 1;
			line.style.marginTop = 2;
			line.style.backgroundColor = HeaderLineColor;
			container.Add(line);

			return container;
		}

		public static Foldout CreateFoldoutSection(string persistenceKey, string title, bool defaultExpanded = false) {
			var foldout = new Foldout {
				text = title,
				value = defaultExpanded,
				viewDataKey = $"Sperlich.UISystem.Editor.Foldout.{persistenceKey}"
			};
			foldout.style.marginTop = 8;
			foldout.style.marginBottom = 3;

			Label label = foldout.Q<Label>(className: Foldout.textUssClassName);
			if (label != null) {
				label.style.unityFontStyleAndWeight = FontStyle.Bold;
				label.style.fontSize = 11;
				label.style.color = HeaderTextColor;
			}

			return foldout;
		}

		public static VisualElement CreateStatusDot(bool active) {
			var dot = new VisualElement();
			dot.style.width = 8;
			dot.style.height = 8;
			dot.style.borderTopLeftRadius = 4;
			dot.style.borderTopRightRadius = 4;
			dot.style.borderBottomLeftRadius = 4;
			dot.style.borderBottomRightRadius = 4;
			dot.style.backgroundColor = active ? ActiveColor : InactiveColor;
			dot.style.marginRight = 6;
			return dot;
		}

		public static VisualElement CreateStateRow(string label, out VisualElement dot, bool initialActive) {
			var row = new VisualElement();
			row.style.flexDirection = FlexDirection.Row;
			row.style.alignItems = Align.Center;
			row.style.marginBottom = 4;

			dot = CreateStatusDot(initialActive);
			row.Add(dot);
			row.Add(new Label(label));

			return row;
		}

		public static void SetDotState(VisualElement dot, bool active) {
			dot.style.backgroundColor = active ? ActiveColor : InactiveColor;
		}
	}
}
