using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Sperlich.EditorKit;

namespace Sperlich.UISystem.Editor {

	/// <summary>
	/// Forces the <see cref="Menu"/> inspector into UI Toolkit so nested UITK property drawers
	/// (e.g. <see cref="ControlActionDrawer"/> in the Available Actions list) are hosted natively
	/// instead of bridged from IMGUI. Layout stays the default field-by-field inspector — unless the concrete
	/// subclass carries <see cref="SInspectorAttribute"/>, in which case <see cref="SInspectorGate"/> renders it
	/// with <c>SperlichInspectorEngine</c> instead; this editor being a non-fallback <c>[CustomEditor(typeof(Menu),
	/// true)]</c> would otherwise always win over editorkit's own fallback SInspector editor for every Menu
	/// subclass, silently ignoring the attribute.
	/// </summary>
	[CustomEditor(typeof(Menu), true)]
	[CanEditMultipleObjects]
	public class MenuEditor : UnityEditor.Editor {

		public override VisualElement CreateInspectorGUI() {
			if (SInspectorGate.IsEnabledFor(target?.GetType())) {
				return SInspectorGate.Build(this);
			}
			var root = new VisualElement();
			InspectorElement.FillDefaultInspector(root, serializedObject, this);
			return root;
		}
	}
}
