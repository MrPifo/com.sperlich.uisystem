using System.Collections.Generic;
using UnityEngine;
using Sperlich.PrefabManager;

namespace Sperlich.UISystem.Scroll
{
    /// <summary>
    /// Eine optionale Convenience-Basisklasse für eine VirtualScrollView.
    /// Nutzt den com.sperlich.prefabmanager, um das Pooling (Instanziieren und Recyceln) von UI-Items automatisch zu übernehmen.
    /// Du musst lediglich eine Datenliste bereitstellen und die BindItem-Methode implementieren.
    /// </summary>
    /// <typeparam name="TItem">Die Komponenten-Klasse des Prefabs (z.B. InventoryCardUI).</typeparam>
    /// <typeparam name="TData">Die Daten-Klasse (z.B. ItemData).</typeparam>
    public abstract class GenericScrollAdapter<TItem, TData> : MonoBehaviour, IVirtualScrollAdapter 
        where TItem : Component // Component als kleinster gemeinsamer Nenner für UI-Skripte
    {
        [Header("Prefab Manager Settings")]
        [Tooltip("Der Enum-Wert des Prefabs, das vom PrefabManager für die Liste erzeugt werden soll.")]
        public Prefabs PrefabType;

        [Header("Runtime Data")]
        protected List<TData> DataList = new List<TData>();

        /// <summary>
        /// Gibt die aktuelle Anzahl an Elementen an die ScrollView weiter.
        /// </summary>
        public virtual int GetItemCount()
        {
            return DataList != null ? DataList.Count : 0;
        }

        /// <summary>
        /// Holt ein Element aus dem PrefabManager-Pool und bindet es.
        /// </summary>
        public virtual RectTransform GetItem(int index)
        {
            // Spawn holt ein bestehendes oder erzeugt ein neues Objekt über den Pool.
            TItem instance = PrefabManager.PrefabManager.Spawn<TItem>(PrefabType, transform);
            
            // Führe dein benutzerdefiniertes Daten-Binding durch
            BindItem(instance, DataList[index]);
            
            return instance.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Gibt das Element wieder an den PrefabManager-Pool zurück.
        /// </summary>
        public virtual void ReleaseItem(int index, RectTransform item)
        {
            // Nutze die IRecycle Schnittstelle vom PrefabManager, um das Objekt in den Pool zu packen
            if (item.TryGetComponent(out IRecycle recycleItem))
            {
                recycleItem.Recycle();
            }
            else
            {
                Debug.LogWarning($"[GenericScrollAdapter] Das Element {item.gameObject.name} implementiert kein IRecycle und kann nicht korrekt an den PrefabManager Pool zurückgegeben werden!");
                item.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Diese Methode musst du in deiner abgeleiteten Klasse überschreiben, 
        /// um die Daten (z.B. Text, Icons) auf das UI-Prefab zu mappen.
        /// </summary>
        protected abstract void BindItem(TItem item, TData data);

        /// <summary>
        /// Aktualisiert die zugrunde liegende Datenliste.
        /// (Du musst danach manuell scrollView.RebuildLayout() aufrufen, damit sich die Anzeige aktualisiert).
        /// </summary>
        public void SetData(List<TData> newData)
        {
            DataList = newData;
        }
    }
}
