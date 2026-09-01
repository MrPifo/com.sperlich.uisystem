using UnityEngine;

namespace Sperlich.UISystem.Scroll
{
    /// <summary>
    /// Stellt die Schnittstelle zwischen dem Virtual-Scroll-Layout und den eigentlichen Daten / Prefabs her.
    /// Muss von einem externen Script implementiert werden, um der ScrollView mitzuteilen, was sie rendern soll.
    /// </summary>
    public interface IVirtualScrollAdapter
    {
        /// <summary>
        /// Liefert die Gesamtanzahl der Datensätze.
        /// Anhand dieses Wertes berechnet die VirtualScrollView die Scroll-Höhe.
        /// </summary>
        int GetItemCount();

        /// <summary>
        /// Wird aufgerufen, wenn ein Element (anhand seines logischen Index) in den sichtbaren Viewport scrollt.
        /// Das externe System sollte hier ein Objekt instanziieren (oder aus einem Pool holen) und die Daten daran binden.
        /// </summary>
        /// <param name="index">Der logische Listen-Index des Elements.</param>
        /// <returns>Das fertig gebundene RectTransform.</returns>
        RectTransform GetItem(int index);

        /// <summary>
        /// Wird aufgerufen, wenn ein Element den Viewport verlässt und nicht mehr sichtbar ist.
        /// Das externe System sollte das Objekt an dieser Stelle wieder in den Pool zurückführen.
        /// </summary>
        /// <param name="index">Der logische Listen-Index, der verschwunden ist.</param>
        /// <param name="item">Das dazugehörige RectTransform, das zuvor von GetItem zurückgegeben wurde.</param>
        void ReleaseItem(int index, RectTransform item);

        /// <summary>
        /// Optional: Aktualisiert die Datenbindung eines bereits sichtbaren RectTransforms (z. B. nach Listenänderungen oder Animationen).
        /// </summary>
        /// <param name="index">Der neue logische Listen-Index des Elements.</param>
        /// <param name="item">Das sichtbare RectTransform.</param>
        void RebindItem(int index, RectTransform item) { }
    }
}
