using NyxMachina.Shared.EventFramework;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;


public class MainRoomPage : Page
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button kickButton;
    [SerializeField] private Button backButton;

    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Text publicityStatusText;
    [SerializeField] private TMP_Text hostName;
    [SerializeField] private TMP_Text clientName;

    [SerializeField] private WidgetManager widgetManager;

    protected override void Start()
    {
        base.Start();

        startButton.onClick.AddListener(() => { OnClickStart(); });
        kickButton.onClick.AddListener(() => { OnClickKick(); });
        backButton.onClick.AddListener(() => { OnClickBack(); });

        roomNameText.text = GameManager.Instance.nameRoom;
        roomCodeText.text = GameManager.Instance.joinCode;
        //publicityStatusText
        //hostName
        //clientName
    }

    void OnClickStart()
    {
        //Check Client sudah join atau tidak
        //pageManager.OpenPage(PageType.SelectCharacterRoom);
    }

    void OnClickKick()
    {
        //Remove Client
    }

    async void OnClickBack()
    {
        await LobbySystemInitiator.Instance.System.LeaveLobby();
        pageManager.OpenPage(PageType.OnlinePage);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EventMessenger.Main.Subscribe<LobbyChangedEvent>(OnLobbyChanged);
        EventMessenger.Main.Subscribe<LobbyCreatedEvent>(OnLobbyCreated);
    }

    private void OnLobbyCreated(LobbyCreatedEvent obj)
    {
        roomCodeText.text = obj.CurrentLobby.Lobby.LobbyCode;
        roomNameText.text = obj.CurrentLobby.Lobby.Name;
        publicityStatusText.text = obj.CurrentLobby.LobbySetting.IsLocked ? "Private" : "Public";

        //var hostNameString = LobbySystemInitiator.Instance.System.CurrentLobby.Lobby?.GetHostPlayerData()?.Profile.Name;
        //var clientNameString = LobbySystemInitiator.Instance.System.CurrentLobby.Lobby?.GetNonHostPlayer()?.Profile.Name;

        hostName.text = "Host";
        clientName.text = "Client";
    }

    private void OnLobbyChanged(LobbyChangedEvent obj)
    {
        if (obj.CurrentLobby == null) return;
        roomCodeText.text = obj.CurrentLobby.Lobby.LobbyCode;
        roomNameText.text = obj.CurrentLobby.Lobby.Name;
        publicityStatusText.text = obj.CurrentLobby.LobbySetting.IsLocked ? "Private" : "Public";

        //var hostNameString = LobbySystemInitiator.Instance.System.CurrentLobby.Lobby?.GetHostPlayerData()?.Profile.Name;
        //var clientNameString = LobbySystemInitiator.Instance.System.CurrentLobby.Lobby?.GetNonHostPlayer()?.Profile.Name;

        hostName.text = "Host";
        clientName.text = "Client";
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventMessenger.Main.Unsubscribe<LobbyChangedEvent>(OnLobbyChanged);
    }

    protected override void Update()
    {
        base.Update();
    }
    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }
}