using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sperlich.UISystem.Scroll.Testing
{
    public class DummyCardData
    {
        public string Title;
        public Color BgColor;
    }

    /// <summary>
    /// Test-Komponente für die UI-Prefabs im Test
    /// </summary>
    public class DummyCardItem : UIBehaviour
    {
        public Text TitleText;
        public Image Background;
    }

    /// <summary>
    /// Dummy-Implementierung des GenericScrollAdapters zum Testen.
    /// In echten Szenarien nutzt du Sperlich.PrefabManager.IRecycle in deinem UI-Item.
    /// </summary>
    public class DummyScrollAdapter : GenericScrollAdapter<DummyCardItem, DummyCardData>
    {
        public VirtualScrollView TargetView;
        
        [Header("Test Generation")]
        public int CountToGenerate = 1000;

        private void Start()
        {
            // Dummy-Daten generieren
            var testData = new List<DummyCardData>();
            for (int i = 0; i < CountToGenerate; i++)
            {
                testData.Add(new DummyCardData {
                    Title = "Item " + i,
                    BgColor = new Color(Random.value, Random.value, Random.value)
                });
            }

            SetData(testData);

            if (TargetView != null)
            {
                TargetView.SetAdapter(this);
            }
        }

        protected override void BindItem(DummyCardItem item, DummyCardData data)
        {
            if (item.TitleText != null) item.TitleText.text = data.Title;
            if (item.Background != null) item.Background.color = data.BgColor;
        }

        // Test-Methode, um das fließende Löschen zu demonstrieren
        public void RemoveItemAtIndex(int index)
        {
            if (index >= 0 && index < DataList.Count)
            {
                DataList.RemoveAt(index);
                TargetView.RebuildLayout(); // Triggert VirtualScrollAnimator (Lerp)
            }
        }
    }
}
