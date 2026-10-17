# Sperlich UISystem — Workspace Rules & Notes

## Planungs- & Umsetzungsregeln
- **Keine unnötigen Pläne für kleine Aufgaben**: Für Bugfixes, UI-Anpassungen, Felder hinzufügen oder klare/eindeutige Instruktionen **keinen Plan erstellen**, sondern direkt chirurgisch umsetzen.
- **Planungs-Pflicht nur bei**: Großen Architektur-Entscheidungen, tiefgreifenden Refactorings oder unklaren Anforderungen.
- **Minimal-invasive Änderungen**: Nur die betroffenen Zeilen ändern.
- **XML-Dokumentation**: `/// <summary>` für Methoden/Properties/Klassen.

## Visual State (UIBase)
- Elements do not wire Pointer/Select handlers for visuals. Override `OnVisualStateChanged(previous, current)` only.
- `VisualState` is one value: Disabled > Pressed > Selected (Directional only) > Hovered (Pointer only) > Normal. Disabled counts only for the element itself, not a closed CanvasGroup.
- Special rules: override `EvaluateVisualState()`. Trigger a re-check with `RefreshVisualState()` after logic changes.
- Elements without a click subscription that still need hover call `EnsureEvents()` in `OnAwake`.
- Mouse press selects only if `SelectOnPointerDown` is overridden to `true`.
- `Button.OnSubmit` is sealed. Submit runs through `OnClick`.
