using Lean.Localization;
using MetabharataAudio;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameNotifWidget : Widget
{
    [SerializeField] private LeanLocalizedTextMeshProUGUI label;
    [SerializeField] private Button exitButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI exitText;
    [SerializeField] private Button restartButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI restartText;
    [SerializeField] private Button nextButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI nextText;
    [SerializeField] private Image iconEndGame;
    [SerializeField] private Sprite winIcon;
    [SerializeField] private Sprite loseIcon;

    public bool isWin;

    void Start()
    {
        Hide();

        exitButton.onClick.AddListener(() => { ExitFunction(); });
        restartButton.onClick.AddListener(() => { RestartFunction(); });
        nextButton.onClick.AddListener(() => { NextFunction(); });
    }

    public override void Show()
    {
        base.Show();

        int score1;
        int score2;

        score1 = GameplayManager.Manager.playerWin.FindAll(n => n == 1).Count;
        score2 = GameplayManager.Manager.playerWin.FindAll(n => n == 2).Count;

        if (GameplayManager.Manager.CharacterOnGameplay[0].isOwned)
        {
            if (score1 > score2)
            {
                isWin = true;
                AudioManager.Manager.PlaySFX("WinGame");
            }
            else if (score1 < score2)
            {
                isWin = false;
                AudioManager.Manager.PlaySFX("GameOver");
            }
        }
        else if (GameplayManager.Manager.CharacterOnGameplay[1].isOwned)
        {
            if (score1 > score2)
            {
                isWin = false;
                AudioManager.Manager.PlaySFX("GameOver");
            }
            else if (score1 < score2)
            {
                isWin = true;
                AudioManager.Manager.PlaySFX("WinGame");
            }
        }

        InitData();
    }

    void ExitFunction()
    {
        widgetManager.OpenWidget(WidgetType.Empty);
        SceneManager.LoadScene("MainMenu");
    }

    void RestartFunction()
    {
        widgetManager.OpenWidget(WidgetType.Empty);
        GameplayManager.Manager.RestartGame();
    }

    void NextFunction()
    {
        widgetManager.OpenWidget(WidgetType.Empty);
        GameplayManager.Manager.NextStory();
    }

    void InitData()
    {
        if (GameManager.Instance.stageType == StageType.Story)
        {
            if (GameplayManager.Manager.StoryNumber >= GameManager.Instance.storyDataSelected.stageList.Count || !isWin)
            {
                exitButton.gameObject.SetActive(true);
                restartButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(false);
            }
            else
            {
                exitButton.gameObject.SetActive(true);
                restartButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
            }
        }
        else if (GameManager.Instance.stageType == StageType.FreeBattle)
        {
            exitButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(false);
        }
        else if (GameManager.Instance.stageType == StageType.Online)
        {

        }

        if (isWin)
        {
            label.TranslationName = "EndGameNotifWidget/LabelEndBattleWin";
            iconEndGame.sprite = winIcon;
        }
        else if (!isWin)
        {
            label.TranslationName = "EndGameNotifWidget/LabelEndBattleLose";
            iconEndGame.sprite = loseIcon;
        }
    }
}
