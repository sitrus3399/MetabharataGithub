using UnityEngine;
using UnityEngine.UI;

public class CreditWidget : Widget
{
    [SerializeField] private Button closeButton;

    private void Start()
    {
        closeButton.onClick.AddListener(() => { widgetManager.OpenWidget(WidgetType.Setting); });
    }
}
