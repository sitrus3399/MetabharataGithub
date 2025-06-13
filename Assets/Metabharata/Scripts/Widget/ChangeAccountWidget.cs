using GooglePlayGames;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ChangeAccountWidget : Widget
{
    [SerializeField] private Button openProfileButton;
    [SerializeField] private Button openNotificationButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private Image openProfileImage;
    [SerializeField] private Image openNotificationImage;

    [SerializeField] private Sprite activeButtonSprite;
    [SerializeField] private Sprite nonActiveButtonSprite;

    [SerializeField] private GameObject profilPanel;
    [SerializeField] private GameObject notifPanel;

    [Header("ChangeAccount")]
    [SerializeField] private Button changeAccountButton;
    [SerializeField] private Image avatarImage;

    private string namePlayer;
    private string idPlayer;
    private string emailPlayer;
    private string avatarPlayer;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text idText;
    [SerializeField] private TMP_Text emailText;

    [Header("Notification")]
    [SerializeField] private NotificationCard notificationCard;
    [SerializeField] private List<NotificationCard> notificationCardList;
    [SerializeField] private Transform notificationCardLocation;

    void Start()
    {
        openProfileButton.onClick.AddListener(() => { OpenProfil(); });
        openNotificationButton.onClick.AddListener(() => { OpenNotif(); });
        closeButton.onClick.AddListener(() => { widgetManager.OpenWidget(WidgetType.Empty); });

        namePlayer = PlayGamesPlatform.Instance.GetUserDisplayName();
        idPlayer = PlayGamesPlatform.Instance.GetUserId();
        avatarPlayer = PlayGamesPlatform.Instance.GetUserImageUrl();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Show()
    {
        base.Show();

        if (namePlayer == null)
        {
            namePlayer = PlayGamesPlatform.Instance.GetUserDisplayName();
        }

        if (idPlayer == null)
        {
            idPlayer = PlayGamesPlatform.Instance.GetUserId();
        }

        if (avatarPlayer == null)
        {
            avatarPlayer = PlayGamesPlatform.Instance.GetUserImageUrl();
        }

        nameText.text = namePlayer;
        idText.text = idPlayer;

        OpenProfil();
    }

    public override void Hide()
    {
        base.Hide();
    }

    void OpenProfil()
    {
        profilPanel.SetActive(true);
        notifPanel.SetActive(false);

        openProfileImage.sprite = activeButtonSprite;
        openNotificationImage.sprite = nonActiveButtonSprite;
    }

    void OpenNotif()
    {
        profilPanel.SetActive(false);
        notifPanel.SetActive(true);

        openProfileImage.sprite = nonActiveButtonSprite;
        openNotificationImage.sprite = activeButtonSprite;
    }
}