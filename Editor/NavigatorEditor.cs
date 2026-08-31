using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Editor {
	[CustomEditor(typeof(Navigator), true)]
	[CanEditMultipleObjects]
	public class NavigatorEditor : UnityEditor.Editor {

		public override VisualElement CreateInspectorGUI() {
			var root = new VisualElement();

			SerializedProperty interactableProp = serializedObject.FindProperty("_interactable");

			SerializedProperty selectOnUpProp = serializedObject.FindProperty("selectOnUp");
			SerializedProperty selectOnDownProp = serializedObject.FindProperty("selectOnDown");
			SerializedProperty selectOnLeftProp = serializedObject.FindProperty("selectOnLeft");
			SerializedProperty selectOnRightProp = serializedObject.FindProperty("selectOnRight");
			SerializedProperty enableLoopProp = serializedObject.FindProperty("enableLoop");

			SerializedProperty isSelectedProp = serializedObject.FindProperty("isSelected");

			SerializedProperty onSelectProp = serializedObject.FindProperty("onSelect");
			SerializedProperty onDeselectProp = serializedObject.FindProperty("onDeselect");
			SerializedProperty onSubmitProp = serializedObject.FindProperty("onSubmit");
			SerializedProperty onCancelProp = serializedObject.FindProperty("onCancel");

			VisualElement selectedRow = SperlichUIEditorStyle.CreateStateRow("Is Selected", out VisualElement selectedDot, isSelectedProp.boolValue);
			root.Add(selectedRow);
			root.TrackPropertyValue(isSelectedProp, prop => SperlichUIEditorStyle.SetDotState(selectedDot, prop.boolValue));

			root.Add(SperlichUIEditorStyle.CreateSectionHeader("Core"));
			root.Add(new PropertyField(interactableProp, "Interactable"));

			Foldout navigationFoldout = SperlichUIEditorStyle.CreateFoldoutSection("Navigator.Navigation", "Navigation");
			root.Add(navigationFoldout);
			navigationFoldout.Add(new PropertyField(enableLoopProp, "Enable Loop"));

			VisualElement crossLayout = CreateDirectionalCross(selectOnUpProp, selectOnDownProp, selectOnLeftProp, selectOnRightProp);
			navigationFoldout.Add(crossLayout);

			Foldout eventsFoldout = SperlichUIEditorStyle.CreateFoldoutSection("Navigator.Events", "Events");
			root.Add(eventsFoldout);
			eventsFoldout.Add(new PropertyField(onSelectProp));
			eventsFoldout.Add(new PropertyField(onDeselectProp));
			eventsFoldout.Add(new PropertyField(onSubmitProp));
			eventsFoldout.Add(new PropertyField(onCancelProp));

			return root;
		}

		private VisualElement CreateDirectionalCross(SerializedProperty up, SerializedProperty down, SerializedProperty left, SerializedProperty right) {
			const float fieldWidth = 130f;

			var container = new VisualElement();
			container.style.marginTop = 4;

			var upRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center } };
			upRow.Add(CreateDirectionField("Up", up, fieldWidth));
			container.Add(upRow);

			var midRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center } };
			midRow.Add(CreateDirectionField("Left", left, fieldWidth));
			midRow.Add(new VisualElement { style = { width = fieldWidth } });
			midRow.Add(CreateDirectionField("Right", right, fieldWidth));
			container.Add(midRow);

			var downRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center } };
			downRow.Add(CreateDirectionField("Down", down, fieldWidth));
			container.Add(downRow);

			return container;
		}

		private VisualElement CreateDirectionField(string label, SerializedProperty prop, float width) {
			var column = new VisualElement { style = { width = width, marginLeft = 2, marginRight = 2 } };

			var lbl = new Label(label);
			lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
			lbl.style.fontSize = 10;
			lbl.style.color = SperlichUIEditorStyle.HeaderTextColor;
			column.Add(lbl);

			var field = new ObjectField { objectType = typeof(Navigator), label = string.Empty };
			field.BindProperty(prop);
			field.labelElement.style.display = DisplayStyle.None;
			field.style.marginLeft = 0;
			column.Add(field);

			return column;
		}
	}
}
