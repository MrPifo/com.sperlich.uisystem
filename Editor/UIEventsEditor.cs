using Sperlich.EditorKit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Editor {
	using FlexDirection = UnityEngine.UIElements.FlexDirection;

	[CustomEditor(typeof(UIEvents))]
	[CanEditMultipleObjects]
	public class UIEventsEditor : UnityEditor.Editor {

		private static readonly Color BadgeOn = new(0.18f, 0.55f, 0.34f, 0.35f);
		private static readonly Color BadgeOff = new(1f, 1f, 1f, 0.06f);

		public override VisualElement CreateInspectorGUI() {
			var root = new VisualElement();

			SerializedProperty eventsProp = serializedObject.FindProperty("events");

			// ---- Runtime -------------------------------------------------------------------------
			var runtimeRow = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					paddingLeft = 8,
					paddingRight = 8,
					paddingTop = 5,
					paddingBottom = 5,
					marginBottom = 4,
					backgroundColor = SperlichEditorTheme.BgStep
				}
			};
			SperlichEditorWidgets.SetRadius(runtimeRow, 4);
			AddStateBadge(root, runtimeRow, "isHovered", "HOVERED");
			AddStateBadge(root, runtimeRow, "isPressing", "PRESSING");
			AddStateBadge(root, runtimeRow, "isClicking", "CLICKING");
			AddStateBadge(root, runtimeRow, "isDragging", "DRAGGING");
			root.Add(runtimeRow);

			// ---- Signals -------------------------------------------------------------------------
			var signalsSec = Section(root, "SIGNALS", true);
			signalsSec.Add(SperlichFieldColumn.Raw(eventsProp));

			// ---- Callbacks -----------------------------------------------------------------------
			var callbacksSec = Section(root, "CALLBACKS", true);
			var callbacks = new (EventSignal flag, VisualElement field)[] {
				(EventSignal.Click, Callback("onClick")),
				(EventSignal.PointerEnter, Callback("onPointerEnter")),
				(EventSignal.PointerExit, Callback("onPointerExit")),
				(EventSignal.PointerDown, Callback("onPointerDown")),
				(EventSignal.PointerUp, Callback("onPointerUp")),
				(EventSignal.PointerMove, Callback("onPointerMove")),
				(EventSignal.DragBegin, Callback("onDragBegin")),
				(EventSignal.DragEnd, Callback("onDragEnd")),
				(EventSignal.Drag, Callback("onDrag")),
			};
			foreach (var (_, field) in callbacks) {
				callbacksSec.Add(field);
			}

			void RefreshCallbacks(SerializedProperty prop) {
				var mask = (EventSignal)prop.intValue;
				foreach (var (flag, field) in callbacks) {
					field.style.display = (mask & flag) == flag ? DisplayStyle.Flex : DisplayStyle.None;
				}
			}
			RefreshCallbacks(eventsProp);
			root.TrackPropertyValue(eventsProp, RefreshCallbacks);

			SperlichInspectorScroll.Preserve(root, target);

			return root;
		}

		private VisualElement Callback(string propertyName) {
			return SperlichFieldColumn.Raw(serializedObject.FindProperty(propertyName));
		}

		private void AddStateBadge(VisualElement root, VisualElement row, string propertyName, string text) {
			SerializedProperty prop = serializedObject.FindProperty(propertyName);

			var badge = SperlichEditorWidgets.CreateBadge(text, prop.boolValue ? BadgeOn : BadgeOff, prop.boolValue ? SperlichUIEditorStyle.ActiveColor : SperlichEditorTheme.TextMuted);
			badge.style.marginRight = 4;
			row.Add(badge);

			root.TrackPropertyValue(prop, p => {
				badge.style.backgroundColor = p.boolValue ? BadgeOn : BadgeOff;
				badge.style.color = p.boolValue ? SperlichUIEditorStyle.ActiveColor : SperlichEditorTheme.TextMuted;
			});
		}

		private static VisualElement Section(VisualElement parent, string title, bool expanded) {
			var (header, body, _) = SperlichEditorWidgets.CreateChevronSection(title, expanded, SperlichEditorTheme.BgStep, null, nameof(UIEventsEditor));
			body.style.paddingLeft = 6;
			body.style.paddingRight = 6;
			body.style.paddingTop = 4;
			body.style.paddingBottom = 6;
			var wrap = new VisualElement { style = { marginBottom = 4 } };
			wrap.Add(header);
			wrap.Add(body);
			parent.Add(wrap);
			return body;
		}
	}
}
