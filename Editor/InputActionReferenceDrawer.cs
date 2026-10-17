using System;
using System.Linq;
using Sperlich.EditorKit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Sperlich.UISystem {

	/// <summary>EditorKit drawer: category dropdown + an alphabetically sorted action dropdown from the matching action enum.</summary>
	[CustomPropertyDrawer(typeof(InputActionReference))]
	public class InputActionReferenceDrawer : PropertyDrawer {

		public override VisualElement CreatePropertyGUI(SerializedProperty property) {
			SerializedProperty categoryProp = property.FindPropertyRelative("category");
			SerializedProperty selectedProp = property.FindPropertyRelative("selectedAction");

			string[] ActionNames() {
				string[] cats = categoryProp.enumNames;
				int idx = categoryProp.enumValueIndex;
				Type enumType = cats != null && idx >= 0 && idx < cats.Length ? FindEnumType(cats[idx]) : null;
				return enumType != null ? Enum.GetNames(enumType) : Array.Empty<string>();
			}

			// original enum indices, ordered alphabetically by display name
			int[] SortedOrder() {
				string[] names = ActionNames();
				return Enumerable.Range(0, names.Length)
					.OrderBy(i => ObjectNames.NicifyVariableName(names[i]), StringComparer.OrdinalIgnoreCase)
					.ToArray();
			}

			string SlotLabel(int slot) {
				string[] names = ActionNames();
				int[] order = SortedOrder();
				return slot >= 0 && slot < order.Length ? ObjectNames.NicifyVariableName(names[order[slot]]) : "—";
			}

			int SelectedSlot() {
				int[] order = SortedOrder();
				int slot = Array.IndexOf(order, selectedProp.intValue);
				return slot >= 0 ? slot : (order.Length > 0 ? 0 : -1);
			}

			var root = new VisualElement { style = { marginTop = 2, marginBottom = 2 } };
			root.Add(new Label(property.displayName) {
				style = { fontSize = 11, color = SperlichEditorTheme.TextSecondary, marginBottom = 3 }
			});

			VisualElement actionDropdown = null;
			void RefreshLabel() {
				Label lbl = actionDropdown?.Q<Label>();
				if (lbl != null) {
					lbl.text = SlotLabel(SelectedSlot());
				}
			}

			actionDropdown = SperlichEditorWidgets.BuildDropdown(
				() => SortedOrder().Length,
				SlotLabel,
				SelectedSlot,
				slot => {
					int[] order = SortedOrder();
					if (slot >= 0 && slot < order.Length) {
						selectedProp.intValue = order[slot];
						selectedProp.serializedObject.ApplyModifiedProperties();
					}
				});

			var categoryDropdown = SperlichEditorWidgets.CreateEnumDropdown(categoryProp, null, _ => {
				selectedProp.intValue = 0;
				selectedProp.serializedObject.ApplyModifiedProperties();
				RefreshLabel();
			});

			root.Add(Row("Category", categoryDropdown));
			root.Add(Row("Action", actionDropdown));

			root.TrackPropertyValue(selectedProp, _ => RefreshLabel());
			root.TrackPropertyValue(categoryProp, _ => RefreshLabel());
			RefreshLabel();

			return root;
		}

		static VisualElement Row(string label, VisualElement control) {
			var row = new VisualElement {
				style = { flexDirection = UnityEngine.UIElements.FlexDirection.Row, alignItems = Align.Center, marginBottom = 2 }
			};
			row.Add(new Label(label) {
				style = { width = 70, fontSize = 11, color = SperlichEditorTheme.TextSecondary }
			});
			control.style.flexGrow = 1;
			row.Add(control);
			return row;
		}

		static Type FindEnumType(string enumName) {
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				Type type = assembly.GetType($"Sperlich.UISystem.{enumName}");
				if (type != null && type.IsEnum) {
					return type;
				}
			}
			return null;
		}
	}
}
