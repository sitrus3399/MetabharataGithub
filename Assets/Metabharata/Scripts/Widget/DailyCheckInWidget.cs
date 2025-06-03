using UnityEngine;
using UnityEngine.UI;

public class DailyCheckInWidget : Widget
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button[] dailyLoginButton;
    [SerializeField] private Image[] dailyLoginImage;
    [SerializeField] private Sprite dailyLoginActive;
    [SerializeField] private Sprite dailyLoginClaimed;
    [SerializeField] private Image[] dailyLoginIcon;
    [SerializeField] private Sprite dailyLoginCoinIcon;
    [SerializeField] private Sprite dailyLoginClaimedIcon;
    [SerializeField] private DailyCheckInManager dailyCheckInManager;

    private void Start()
    {
        closeButton.onClick.AddListener(() => { Hide(); });
    }

    public override void Show()
    {
        base.Show();

        int currentReward = PlayerPrefs.GetInt(dailyCheckInManager.rewardOpenKey, 0);

        SetAllButtonFalse();

        CheckDailyActive();

        for (int i = 0; i < dailyLoginButton.Length; i++)
        {
            int j = i;
            dailyLoginButton[j].onClick.AddListener(() => { dailyCheckInManager.ClaimDailyReward(j); CheckDailyActive(); });
        }
    }

    void SetAllButtonFalse()
    {
        for (int i = 0; i < dailyLoginButton.Length; i++)
        {
            dailyLoginButton[i].interactable = false;
        }
    }

    void CheckDailyActive()
    {
        for (int i = 0; i < dailyLoginButton.Length; i++)
        {
            if (dailyCheckInManager.CheckRewardOpen(i) && !dailyCheckInManager.CheckRewardClaimed(i))
            {
                dailyLoginButton[i].interactable = true;
            }

            if (dailyCheckInManager.CheckRewardClaimed(i))
            {
                dailyLoginImage[i].sprite = dailyLoginClaimed;
                dailyLoginIcon[i].sprite = dailyLoginClaimedIcon;
            }
            else
            {
                dailyLoginImage[i].sprite = dailyLoginActive;
                dailyLoginIcon[i].sprite = dailyLoginCoinIcon;
            }
        }
    }

    public override void Hide()
    {
        base.Hide();
    }
}
