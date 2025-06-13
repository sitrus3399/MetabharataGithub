using TMPro;
using Unity.Services.Lobbies.Models;
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

    private readonly LobbySetting _lobbySetting = new();

    void Start()
    {
        privateToggle.onValueChanged.AddListener(PrivateToggleFunction);
        pinWidgetCloseButton.onClick.AddListener(() => privateToggle.isOn = false);

        createRoomRoomButton.onClick.AddListener(CreateRoom);
    }

    void PrivateToggleFunction(bool isOn)
    {
        widgetManager.OpenWidget(isOn ? WidgetType.PinRoom : WidgetType.Empty);
        _lobbySetting.IsLocked = isOn;
    }

    void CreateRoom()
    {
        GameManager.Instance.stageType = StageType.Online;
        GameManager.Instance.nameRoom = nameRoom.text;
        _lobbySetting.LobbyName = nameRoom.text;
        CreateLobby();
    }

    async void CreateLobby()
    {
        await LobbySystemInitiator.Instance.System.CreateLobby(_lobbySetting);
        PageManager.Instance.OpenPage(PageType.OnlineRoom);
    }
}
