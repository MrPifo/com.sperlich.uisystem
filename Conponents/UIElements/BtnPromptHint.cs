using Sperlich.UISystem.Themes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Sperlich.UISystem {
	[DefaultExecutionOrder(1000)]
	public class ButtonPromptHint : UIBase {

		[Header("Action")]
		[SerializeField]
		private string text = "";
		[SerializeField]
		public InputActionReference action = new();

		[Header("Visuals")]
		public float fadeSpeed = 0.12f;
		public ColorThemeAsset txtColor;
		[SerializeField]
		private TMP_Text _text;
		[SerializeField]
		private UnityEvent onPressEvent;

		public TMP_Text Text => _text;
		public UnityEvent OnPressEvent => onPressEvent;

		protected override void FetchComponents() {
			base.FetchComponents();

			if (_text == null) {
				_text = GetComponentInChildren<TMP_Text>();
			}
			if (_text != null) {
				if (txtColor != null) {
					_text.color = txtColor.GetColor(ComponentState.Normal);
				}
				SetText(text);
			}
		}

		public void SetText(string text) {
			this.text = text;
			string actionKey = action.ActionEnum.ToString();
			string replaceText = "[" + actionKey + "]";
			
			string output = text.Replace("@action@", replaceText);
			if (_text != null) {
			    _text.SetText(output);
			}
		}
		public void SetControlAction(ControlAction action) {
			this.action = action.action;
			this.onPressEvent = action.onPressEvent;
			this.name = action.action.ActionEnum.ToString();

			SetText(action.text);
		}
		public void SetDisabled() {
			State = ComponentState.Disabled;
			if (_text != null && txtColor != null) {
				_text.color = txtColor.GetColor(ComponentState.Disabled);
			}
		}
		public void SetEnabled() {
			State = ComponentState.Normal;
			if (_text != null && txtColor != null) {
				_text.color = txtColor.GetColor(ComponentState.Normal);
			}
		}
	}
}