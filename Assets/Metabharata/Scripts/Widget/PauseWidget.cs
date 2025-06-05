using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseWidget : Widget
{
    [SerializeField] Button exitButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button settingButton;
    [SerializeField] Button returnButton;
    [SerializeField] Button closeButton;

    void Start()
    {
        exitButton.onClick.AddListener(() => { ExitFunction(); });
        restartButton.onClick.AddListener(() => { RestartFunction(); });
        settingButton.onClick.AddListener(() => { SettingFunction(); });
        returnButton.onClick.AddListener(() => { ReturnFunction(); });
        closeButton.onClick.AddListener(() => { ReturnFunction(); });
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
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

    void SettingFunction()
    {
        widgetManager.OpenWidget(WidgetType.Setting);
    }

    void ReturnFunction()
    {
        widgetManager.OpenWidget(WidgetType.Empty);
        Time.timeScale = 1f;
    }
}
