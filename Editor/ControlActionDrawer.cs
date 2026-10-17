using Sperlich.EditorKit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Editor {

	/// <summary>EditorKit-styled drawer for <see cref="ControlAction"/> (text + input action + press event).</summary>
	[CustomPropertyDrawer(typeof(ControlAction))]
	public class ControlActionDrawer : PropertyDrawer {

		public override VisualElement CreatePropertyGUI(SerializedProperty property) {
			SerializedProperty textProp = property.FindPropertyRelative("text");
			SerializedProperty actionProp = property.FindPropertyRelative("action");
			SerializedProperty eventProp = property.FindPropertyRelative("onPressEvent");

			var box = new VisualElement {
				style = {
					marginTop = 2,
					marginBottom = 4,
					paddingLeft = 6,
					paddingRight = 6,
					paddingTop = 4,
					paddingBottom = 6,
					backgroundColor = SperlichEditorTheme.BgStep
				}
			};
			SperlichEditorWidgets.SetRadius(box, 4);

			var textField = new TextField("Text");
			textField.BindProperty(textProp);
			box.Add(textField);

			box.Add(new PropertyField(actionProp));
			box.Add(new PropertyField(eventProp));

			return box;
		}
	}
}
