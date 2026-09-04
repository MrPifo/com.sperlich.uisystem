using Sperlich.EditorKit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sperlich.UISystem.Editor {
	using FlexDirection = UnityEngine.UIElements.FlexDirection;

	[CustomEditor(typeof(Navigator), true)]
	[CanEditMultipleObjects]
	public class NavigatorEditor : UnityEditor.Editor {

		private static readonly Color Accent = SperlichEditorTheme.ButtonAccent;

		public override VisualElement CreateInspectorGUI() {
			var root = new VisualElement();
			var col = new SperlichFieldColumn(110f);

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

			// ---- Status Row ---------------------------------------------------------------------
			var statusRow = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					justifyContent = Justify.SpaceBetween,
					paddingLeft = 8,
					paddingRight = 8,
					paddingTop = 5,
					paddingBottom = 5,
					marginBottom = 4,
					backgroundColor = SperlichEditorTheme.BgStep
				}
			};
			SperlichEditorWidgets.SetRadius(statusRow, 4);

			var statusLabelWrap = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
			var statusDot = new VisualElement {
				style = {
					width = 8,
					height = 8,
					marginRight = 6,
					backgroundColor = isSelectedProp.boolValue ? SperlichUIEditorStyle.ActiveColor : SperlichEditorTheme.TextMuted
				}
			};
			SperlichEditorWidgets.SetRadius(statusDot, 4);
			statusLabelWrap.Add(statusDot);

			var titleLabel = new Label("Selection State") {
				style = {
					fontSize = 11,
					unityFontStyleAndWeight = FontStyle.Bold,
					color = SperlichEditorTheme.TextSecondary
				}
			};
			statusLabelWrap.Add(titleLabel);
			statusRow.Add(statusLabelWrap);

			var stateBadge = SperlichEditorWidgets.CreateBadge(
				isSelectedProp.boolValue ? "SELECTED" : "IDLE",
				isSelectedProp.boolValue ? new Color(0.18f, 0.55f, 0.34f, 0.35f) : new Color(1f, 1f, 1f, 0.06f),
				isSelectedProp.boolValue ? SperlichUIEditorStyle.ActiveColor : SperlichEditorTheme.TextMuted
			);
			statusRow.Add(stateBadge);

			root.TrackPropertyValue(isSelectedProp, prop => {
				bool sel = prop.boolValue;
				statusDot.style.backgroundColor = sel ? SperlichUIEditorStyle.ActiveColor : SperlichEditorTheme.TextMuted;
				stateBadge.text = sel ? "SELECTED" : "IDLE";
				stateBadge.style.backgroundColor = sel ? new Color(0.18f, 0.55f, 0.34f, 0.35f) : new Color(1f, 1f, 1f, 0.06f);
				stateBadge.style.color = sel ? SperlichUIEditorStyle.ActiveColor : SperlichEditorTheme.TextMuted;
			});
			root.Add(statusRow);

			// ---- Core ---------------------------------------------------------------------------
			var coreSec = Section(root, "CORE", true);
			coreSec.Add(col.Property(interactableProp, "Interactable"));
			coreSec.Add(col.Property(enableLoopProp, "Enable Loop"));

			// ---- Navigation ---------------------------------------------------------------------
			var navSec = Section(root, "NAVIGATION", true);
			VisualElement crossBox = SperlichEditorWidgets.CreateBox(4, SperlichEditorTheme.BorderSubtle);
			crossBox.style.backgroundColor = SperlichEditorTheme.BgDark;
			crossBox.style.paddingTop = 6;
			crossBox.style.paddingBottom = 6;
			crossBox.style.paddingLeft = 4;
			crossBox.style.paddingRight = 4;
			crossBox.style.marginTop = 2;
			crossBox.style.marginBottom = 2;
			crossBox.Add(CreateDirectionalCross(selectOnUpProp, selectOnDownProp, selectOnLeftProp, selectOnRightProp));
			navSec.Add(crossBox);

			// ---- Events -------------------------------------------------------------------------
			var eventsSec = Section(root, "EVENTS", true);
			eventsSec.Add(new PropertyField(onSelectProp));
			eventsSec.Add(new PropertyField(onDeselectProp));
			eventsSec.Add(new PropertyField(onSubmitProp));
			eventsSec.Add(new PropertyField(onCancelProp));

			// preserve scroll
			SperlichInspectorScroll.Preserve(root, target);

			return root;
		}

		private static VisualElement Section(VisualElement parent, string title, bool expanded) {
			var (header, body, _) = SperlichEditorWidgets.CreateChevronSection(title, expanded, SperlichEditorTheme.BgStep, null, nameof(NavigatorEditor));
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

		private VisualElement CreateDirectionalCross(SerializedProperty up, SerializedProperty down, SerializedProperty left, SerializedProperty right) {
			const float fieldWidth = 120f;

			var container = new VisualElement {
				style = {
					alignItems = Align.Center
				}
			};

			// Up row
			var upRow = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					justifyContent = Justify.Center,
					marginBottom = 3
				}
			};
			upRow.Add(CreateDirectionField("▲", up, fieldWidth, false));
			container.Add(upRow);

			// Middle row (Left, Hub, Right)
			var midRow = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					justifyContent = Justify.Center,
					marginBottom = 3
				}
			};
			midRow.Add(CreateDirectionField("◄", left, fieldWidth, false));

			// Center Crosshair Hub (Variante 1A mit Guide Lines & Center Box)
			var hub = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					marginLeft = 2,
					marginRight = 2
				}
			};

			var leftLine = new VisualElement {
				style = {
					width = 4,
					height = 1,
					backgroundColor = new Color(Accent.r, Accent.g, Accent.b, 0.4f)
				}
			};
			var centerBox = new VisualElement {
				style = {
					width = 22,
					height = 22,
					backgroundColor = new Color(0.12f, 0.13f, 0.16f),
					borderTopWidth = 1,
					borderBottomWidth = 1,
					borderLeftWidth = 1,
					borderRightWidth = 1,
					borderTopColor = new Color(Accent.r, Accent.g, Accent.b, 0.35f),
					borderBottomColor = new Color(Accent.r, Accent.g, Accent.b, 0.35f),
					borderLeftColor = new Color(Accent.r, Accent.g, Accent.b, 0.35f),
					borderRightColor = new Color(Accent.r, Accent.g, Accent.b, 0.35f),
					alignItems = Align.Center,
					justifyContent = Justify.Center
				}
			};
			SperlichEditorWidgets.SetRadius(centerBox, 4);

			var crosshairLabel = new Label("❖") {
				style = {
					fontSize = 11,
					color = Accent,
					unityTextAlign = TextAnchor.MiddleCenter
				}
			};
			centerBox.Add(crosshairLabel);

			var rightLine = new VisualElement {
				style = {
					width = 4,
					height = 1,
					backgroundColor = new Color(Accent.r, Accent.g, Accent.b, 0.4f)
				}
			};

			hub.Add(leftLine);
			hub.Add(centerBox);
			hub.Add(rightLine);
			midRow.Add(hub);

			midRow.Add(CreateDirectionField("►", right, fieldWidth, true));
			container.Add(midRow);

			// Down row
			var downRow = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					justifyContent = Justify.Center
				}
			};
			downRow.Add(CreateDirectionField("▼", down, fieldWidth, false));
			container.Add(downRow);

			return container;
		}

		private VisualElement CreateDirectionField(string arrow, SerializedProperty prop, float width, bool badgeAtEnd) {
			var row = new VisualElement {
				style = {
					width = width,
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center
				}
			};

			var badge = new VisualElement {
				style = {
					width = 18,
					height = 18,
					backgroundColor = SperlichEditorTheme.BgStep,
					alignItems = Align.Center,
					justifyContent = Justify.Center,
					borderTopWidth = 1,
					borderBottomWidth = 1,
					borderLeftWidth = 1,
					borderRightWidth = 1,
					flexShrink = 0
				}
			};
			SperlichEditorWidgets.SetBorderColor(badge, SperlichEditorTheme.BorderSubtle);
			SperlichEditorWidgets.SetRadius(badge, 3);

			var arrowLabel = new Label(arrow) {
				style = {
					fontSize = 9,
					unityFontStyleAndWeight = FontStyle.Bold,
					color = Accent,
					unityTextAlign = TextAnchor.MiddleCenter
				}
			};
			badge.Add(arrowLabel);

			var field = new ObjectField { objectType = typeof(Navigator), label = string.Empty };
			field.BindProperty(prop);
			field.labelElement.style.display = DisplayStyle.None;
			field.style.width = width - 22; // badge(18) + margin(3) + 1px gap
			field.style.flexShrink = 0;
			field.style.marginLeft = 0;
			field.style.marginRight = 0;

			if (badgeAtEnd) {
				badge.style.marginLeft = 3;
				row.Add(field);
				row.Add(badge);
			} else {
				badge.style.marginRight = 3;
				row.Add(badge);
				row.Add(field);
			}

			return row;
		}
	}
}
