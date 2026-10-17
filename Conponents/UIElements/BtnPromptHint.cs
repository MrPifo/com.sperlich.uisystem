using System.Globalization;
using Sperlich.Event;
using Sperlich.Text;
using Sperlich.UISystem.Themes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Sperlich.UISystem {
	[DefaultExecutionOrder(1000)]
	public class ButtonPromptHint : UIBase {

		[Header("Action")]
		[SerializeField, TextArea(2, 4)]
		private string text = "";
		[SerializeField]
		private InputActionReference action = new();

		[Header("Visuals")]
		[SerializeField, FormerlySerializedAs("_text")]
		private SText label;
		[SerializeField, Min(0f)]
		private float promptSize = 1f;
		[SerializeField]
		private ColorThemeRef textColor = new();
		[SerializeField]
		private float fadeSpeed = 0.12f;

		[Header("Events")]
		[SerializeField]
		private SEvent onPressEvent = new();

		public SText Label => label;
		public InputActionReference Action => action;
		public SEvent OnPressEvent => onPressEvent;
		public float FadeSpeed => fadeSpeed;

		/// <summary>Glyph size as a multiple of the label font size. Re-renders on change.</summary>
		public float PromptSize {
			get => promptSize;
			set {
				promptSize = Mathf.Max(0f, value);
				SetText(text);
			}
		}

		protected override void FetchComponents() {
			base.FetchComponents();

			if (label == null) {
				label = GetComponentInChildren<SText>();
			}
			if (label != null) {
				if (textColor.HasTheme) {
					label.color = textColor.GetColor(ComponentState.Normal);
				}
				SetText(text);
			}
		}

		protected internal override void OnSubmit(EventData evt) => onPressEvent.Invoke();

		/// <summary>
		/// Sets the prompt text. The token <c>@action@</c> is replaced with an SText glyph tag
		/// (<c>&lt;glyph:ActionName size="x"&gt;</c>) that resolves to the icon of the active input device.
		/// </summary>
		public void SetText(string value) {
			text = value;
			if (label == null) {
				return;
			}

			string actionKey = action?.ActionEnum.ToString() ?? string.Empty;
			string size = promptSize.ToString("0.###", CultureInfo.InvariantCulture);
			string glyphTag = $"<glyph:{actionKey} size=\"{size}\">";

			label.SetText(value.Replace("@action@", glyphTag));
		}

		public void SetControlAction(ControlAction controlAction) {
			action = controlAction.action;
			name = controlAction.action.ActionEnum.ToString();
			onPressEvent = controlAction.onPressEvent ?? new SEvent();

			SetText(controlAction.text);
		}

		public void SetDisabled() {
			State = ComponentState.Disabled;
			if (label != null && textColor.HasTheme) {
				label.color = textColor.GetColor(ComponentState.Disabled);
			}
		}

		public void SetEnabled() {
			State = ComponentState.Normal;
			if (label != null && textColor.HasTheme) {
				label.color = textColor.GetColor(ComponentState.Normal);
			}
		}
	}
}
