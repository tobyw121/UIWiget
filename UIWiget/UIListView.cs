using System.Collections.Generic;
using UnityEngine;

namespace YourGame.UI.Widgets
{
    public class UIListView : UIMenu
    {
        public void BindDataSource<T>(IEnumerable<T> dataSource)
        {
            ClearItems(); // Annahme: UIMenu/UIScrollList hat Pooling implementiert

            foreach (var dataItem in dataSource)
            {
                // AddWidget erstellt ein neues UI-Element aus dem Template
                var widgetInstance = AddWidget(dataItem.ToString(), dataItem);
                if (widgetInstance != null)
                {
                    // Finde die Komponente im neuen Widget, die die Daten binden kann
                    var dataBoundComponent = widgetInstance.GetComponent<IDataBound<T>>();
                    if (dataBoundComponent != null)
                    {
                        dataBoundComponent.Bind(dataItem);
                    }
                    else
                    {
                        Debug.LogWarning($"Das Prefab '{_itemTemplate.name}' für die UIListView hat keine Komponente, die IDataBound<{typeof(T).Name}> implementiert.");
                    }
                }
            }
        }
    }
}