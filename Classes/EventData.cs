using UnityEngine.EventSystems;

namespace Sperlich.UISystem {
	public readonly struct EventData {

        public readonly EventSignal type;
        public readonly BaseEventData baseData;
        public readonly PointerEventData pointerData;
        public readonly UIEvents eventElement;
        public readonly UIBase uiElement;
        public readonly Navigator navElement;

        public EventData(EventSignal type, PointerEventData data, UIBase uiElement, UIEvents eventComponent) {
            this.type = type;
            this.pointerData = data;
            this.baseData = data;
            this.eventElement = eventComponent;
            this.uiElement = uiElement;
            this.navElement = null;
			this.navElement = this.uiElement != null ? this.uiElement.Navigator : null;
		}
		public EventData(EventSignal type, BaseEventData data, Navigator navElement) {
			this.type = type;
			this.baseData = data;
			this.navElement = navElement;
            this.uiElement = navElement.UIElement;
            this.eventElement = this.uiElement != null ? this.uiElement.Events : null;
            this.pointerData = null;
		}
	}
}