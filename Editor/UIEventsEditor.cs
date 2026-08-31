using UnityEditor;

namespace Sperlich.UISystem.Editor {
	[CustomEditor(typeof(UIEvents))]
	public class UIEventsEditor : UnityEditor.Editor {
		private SerializedProperty events;
		private SerializedProperty onClick;
		private SerializedProperty onPointerEnter;
		private SerializedProperty onPointerExit;
		private SerializedProperty onPointerDown;
		private SerializedProperty onPointerUp;
		private SerializedProperty onPointerMove;
		private SerializedProperty onDragBegin;
		private SerializedProperty onDragEnd;
		private SerializedProperty onDrag;

		private void OnEnable() {
			events = serializedObject.FindProperty("events");
			onClick = serializedObject.FindProperty("onClick");
			onPointerEnter = serializedObject.FindProperty("onPointerEnter");
			onPointerExit = serializedObject.FindProperty("onPointerExit");
			onPointerDown = serializedObject.FindProperty("onPointerDown");
			onPointerUp = serializedObject.FindProperty("onPointerUp");
			onPointerMove = serializedObject.FindProperty("onPointerMove");
			onDragBegin = serializedObject.FindProperty("onDragBegin");
			onDragEnd = serializedObject.FindProperty("onDragEnd");
			onDrag = serializedObject.FindProperty("onDrag");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField(events);

			EventSignal selectedEvents = (EventSignal)events.intValue;

			if (selectedEvents != EventSignal.None) {
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Event Callbacks", EditorStyles.boldLabel);
			}

			if (HasFlag(selectedEvents, EventSignal.Click)) {
				EditorGUILayout.PropertyField(onClick);
			}

			if (HasFlag(selectedEvents, EventSignal.PointerEnter)) {
				EditorGUILayout.PropertyField(onPointerEnter);
			}

			if (HasFlag(selectedEvents, EventSignal.PointerExit)) {
				EditorGUILayout.PropertyField(onPointerExit);
			}

			if (HasFlag(selectedEvents, EventSignal.PointerDown)) {
				EditorGUILayout.PropertyField(onPointerDown);
			}

			if (HasFlag(selectedEvents, EventSignal.PointerUp)) {
				EditorGUILayout.PropertyField(onPointerUp);
			}

			if (HasFlag(selectedEvents, EventSignal.PointerMove)) {
				EditorGUILayout.PropertyField(onPointerMove);
			}

			if (HasFlag(selectedEvents, EventSignal.DragBegin)) {
				EditorGUILayout.PropertyField(onDragBegin);
			}

			if (HasFlag(selectedEvents, EventSignal.DragEnd)) {
				EditorGUILayout.PropertyField(onDragEnd);
			}

			if (HasFlag(selectedEvents, EventSignal.Drag)) {
				EditorGUILayout.PropertyField(onDrag);
			}

			serializedObject.ApplyModifiedProperties();
		}

		private bool HasFlag(EventSignal value, EventSignal flag) {
			return (value & flag) == flag;
		}
	}
}