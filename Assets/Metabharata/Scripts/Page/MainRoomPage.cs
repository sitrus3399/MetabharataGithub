using NyxMachina.Shared.EventFramework;
using TMPro;
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
        EventMessenger.Main.Subscribe<PlayerLeftLobbyEvent>(OnPlayerLeftLobby);
        EventMessenger.Main.Subscribe<PlayerJoinedLobbyEvent>(OnPlayerJoinLobby);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventMessenger.Main.Unsubscribe<LobbyChangedEvent>(OnLobbyChanged);
        EventMessenger.Main.Unsubscribe<LobbyCreatedEvent>(OnLobbyCreated);
        EventMessenger.Main.Unsubscribe<PlayerLeftLobbyEvent>(OnPlayerLeftLobby);
        EventMessenger.Main.Unsubscribe<PlayerJoinedLobbyEvent>(OnPlayerJoinLobby);
    }

    private void OnPlayerJoinLobby(PlayerJoinedLobbyEvent obj)
    {
        Set(obj.JoinedLobby);
    }

    private void OnPlayerLeftLobby(PlayerLeftLobbyEvent obj)
    {
        if (obj.LastJoinedLobby is not null)
        {
            if (string.Equals(obj.PlayerId, obj.LastJoinedLobby.Lobby.HostId))
            {
                OnClickBack();
                return;
            }
        }

        if (obj.PlayerId == LobbySystemInitiator.Instance.System.CurrentPlayerId)
        {
            OnClickBack();
            return;
        }

        Set(LobbySystemInitiator.Instance.System.CurrentLobby);
    }

    private void OnLobbyCreated(LobbyCreatedEvent obj)
    {
        Set(obj.CurrentLobby);
    }

    private void OnLobbyChanged(LobbyChangedEvent obj)
    {
        if (obj.CurrentLobby == null) return;

        Set(obj.CurrentLobby);
    }

    private void Set(LobbyWrapper currentLobby)
    {
        roomCodeText.text = currentLobby.Lobby.LobbyCode;
        roomNameText.text = currentLobby.Lobby.Name;
        publicityStatusText.text = currentLobby.LobbySetting.IsLocked ? "Private" : "Public";

        var hostNameString = currentLobby.GetHostPlayerData().PlayerName;
        var clientNameString = currentLobby.GetNonHostPlayer() != null ? currentLobby.GetNonHostPlayer().PlayerName : "...";

        hostName.text = hostNameString;
        clientName.text = clientNameString;
    }
}