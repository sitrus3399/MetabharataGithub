using System.Collections.Generic;
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

    protected override void Start()
    {
        base.Start();

        backButton.onClick.AddListener(() => ClosePage());

        createRoomButton.onClick.AddListener(() => { OpenOnlineSubPage(1); });
        searchRoomButton.onClick.AddListener(() => { OpenOnlineSubPage(2); });
    }

    public async void StartHost()
    {
        await HostSingleton.Instance.gameManager.StartHostAsync();
    }

    public async void StartClient(string joinCode)
    {
        await ClientSingleton.Instance.gameManager.StartClientAsync(joinCode);
    }

    void OpenOnlineSubPage(int index)
    {
        if (index == 1)
        {
            createRoomPage.gameObject.SetActive(true);
            searchRoomPage.gameObject.SetActive(false);
        }
        else if (index == 2)
        {
            createRoomPage.gameObject.SetActive(false);
            searchRoomPage.gameObject.SetActive(true);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Update()
    {
        base.Update();
    }
    public override void Show()
    {
        base.Show();

        createRoomPage.gameObject.SetActive(true);
        searchRoomPage.gameObject.SetActive(false);

        leaderboardPage.CreadeListCard();
    }

    public override void Hide()
    {
        base.Hide();
    }

    public void ClosePage()
    {
        pageManager.OpenPage(PageType.MainMenu);
    }
}
