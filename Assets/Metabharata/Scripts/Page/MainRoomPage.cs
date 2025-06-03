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

        roomNameText.text = GameManager.Instance.nameRoom;
        roomCodeText.text = GameManager.Instance.joinCode;
        //publicityStatusText
        //hostName
        //clientName
    }

    void OnClickStart()
    {
        //Check Client sudah join atau tidak
        pageManager.OpenPage(PageType.SelectCharacterRoom);
    }

    void OnClickKick()
    {
        //Remove Client
    }

    void OnClickBack()
    {
        widgetManager.OpenWidget(WidgetType.ExitRoom);
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
    }

    public override void Hide()
    {
        base.Hide();
    }
}