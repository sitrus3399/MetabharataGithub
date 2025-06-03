using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPage : MonoBehaviour
{
    [SerializeField] private OnlinePage onlinePage;

    [SerializeField] private Toggle privateToggle;
    [SerializeField] private Button changeNameRoomButton;

    [SerializeField] private Button createRoomRoomButton;

    [SerializeField] private WidgetManager widgetManager;
    [SerializeField] private PinWidget pinWidget;
    [SerializeField] private Button pinWidgetCloseButton;

    [SerializeField] private TMP_Text nameRoom;

    void Start()
    {
        privateToggle.onValueChanged.AddListener(PrivateToggleFunction);
        pinWidgetCloseButton.onClick.AddListener(() => privateToggle.isOn = false);

        createRoomRoomButton.onClick.AddListener(() => { CreateRoom(); });
    }

    void PrivateToggleFunction(bool isOn)
    {
        if (isOn)
        {
            widgetManager.OpenWidget(WidgetType.PinRoom);
        }
        else
        {
            widgetManager.OpenWidget(WidgetType.Empty);
        }
    }

    void CreateRoom()
    {
        GameManager.Instance.stageType = StageType.Online;
        GameManager.Instance.nameRoom = nameRoom.text;
        onlinePage.StartHost();
    }

    void Update()
    {
        
    }
}
