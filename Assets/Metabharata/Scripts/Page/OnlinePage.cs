using UnityEngine;
using UnityEngine.UI;

public class OnlinePage : Page
{
    [SerializeField] private LeaderboardPage leaderboardPage;
    [SerializeField] private Button backButton;

    [SerializeField] private CreateRoomPage createRoomPage;
    [SerializeField] private SearchRoomPage searchRoomPage;

    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button searchRoomButton;

    [SerializeField] private string roomPassword = "123";
    [SerializeField] private bool isRoomPrivate;

    protected override void Start()
    {
        base.Start();

        backButton.onClick.AddListener(ClosePage);

        createRoomButton.onClick.AddListener(() => { OpenOnlineSubPage(1); });
        searchRoomButton.onClick.AddListener(() => { OpenOnlineSubPage(2); });
    }

    public async void JoinLobby(string joinCode)
    {
        await LobbySystemInitiator.Instance.System.JoinLobbyByCodeAsync(joinCode, roomPassword);
        pageManager.OpenPage(PageType.OnlineRoom);
    }

    public async void LeaveLobby()
    {
        await LobbySystemInitiator.Instance.System.LeaveLobby();
    }

    private void OpenOnlineSubPage(int index)
    {
        createRoomPage.gameObject.SetActive(index == 1);
        searchRoomPage.gameObject.SetActive(index == 2);
    }

    public override void Show()
    {
        base.Show();

        createRoomPage.gameObject.SetActive(true);
        searchRoomPage.gameObject.SetActive(false);

        leaderboardPage.CreadeListCard();
    }

    public void ClosePage()
    {
        LeaveLobby();
        pageManager.OpenPage(PageType.MainMenu);
    }
}
