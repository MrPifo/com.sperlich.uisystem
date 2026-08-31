using UnityEngine;

namespace Sperlich.UISystem {
    [CreateAssetMenu(fileName = "InputSystemResources", menuName = "UISystem/InputSystemResources")]
    public class InputSystemResources : ScriptableObject {

        public ModalBase messageModal;
        public ModalBase questionModal;

        public ModalBase GetInstance(ModalType type) {
            ModalBase modalBase = null;

            switch (type) {
                case ModalType.Question:
                    modalBase = Instantiate(questionModal.gameObject, null).GetComponent<ModalBase>();
                    break;
                case ModalType.Message:
					modalBase = Instantiate(messageModal.gameObject, null).GetComponent<ModalBase>();
					break;
            }

            return modalBase;
        }
    }
}