using UnityEngine;

namespace Sperlich.UISystem {
    /// <summary>
    /// Eine generische Schnittstelle für UI-Inputs, 
    /// um das Sperlich.UISystem von spezifischen Input-Lösungen (z.B. Rewired oder Unity Input System) zu entkoppeln.
    /// </summary>
    public interface IUIInputProvider {
        bool GetButtonDown(string action);
        bool GetButtonUp(string action);
        bool GetButton(string action);
        float GetAxis(string action);
        Vector2 GetMouseDelta();
        
        bool GetButtonDown(NavAction action);
        bool GetButtonUp(NavAction action);
        bool GetButton(NavAction action);
        float GetAxis(NavAction action);
    }
}
