using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;
using MetabharataAudio;

public class SettingWidget : Widget
{
    [SerializeField] private Image toggleEnglish;
    [SerializeField] private Image toggleBahasa;
    [SerializeField] private Image toggleJawa;

    [SerializeField] private Button buttonEnglish;
    [SerializeField] private Button buttonBahasa;
    [SerializeField] private Button buttonJawa;
    [SerializeField] private Button creditButton;

    [SerializeField] private Button closeButton;

    [SerializeField] private Slider volumeSlider;

    [SerializeField] private bool isGamePlay;

    void Start()
    {
        ChangeLanguage(GameManager.Instance.bahasaType);
        buttonEnglish.onClick.AddListener(() => { ChangeLanguage(BahasaType.English); });
        buttonBahasa.onClick.AddListener(() => { ChangeLanguage(BahasaType.Indonesia); });
        buttonJawa.onClick.AddListener(() => { ChangeLanguage(BahasaType.Jawa); });
        closeButton.onClick.AddListener(() =>
        {
            if (isGamePlay)
            {
                widgetManager.OpenWidget(WidgetType.Pause);
            }
            else
            {
                widgetManager.OpenWidget(WidgetType.Empty);
            }
        });

        creditButton.onClick.AddListener(() => { widgetManager.OpenWidget(WidgetType.Credit); });

        volumeSlider.onValueChanged.AddListener(UpdateValueSlider);
    }

    public override void Show()
    {
        base.Show();

        volumeSlider.value = GameManager.Instance.volume;
    }

    private void ChangeLanguage(BahasaType tmpType)
    {
        LeanLocalization.SetCurrentLanguageAll(tmpType.ToString());
        UpdateToggle(tmpType);
    }

    void UpdateValueSlider(float value)
    {
        AudioManager.Manager.SetVolume(SoundType.MASTER, value);
    }

    void UpdateToggle(BahasaType tmpType)
    {
        toggleEnglish.gameObject.SetActive(false);
        toggleBahasa.gameObject.SetActive(false);
        toggleJawa.gameObject.SetActive(false);

        switch (tmpType)
        {
            case BahasaType.English:
                toggleEnglish.gameObject.SetActive(true);
                GameManager.Instance.GantiBahasa(BahasaType.English);
                break;
            case BahasaType.Indonesia:
                toggleBahasa.gameObject.SetActive(true);
                GameManager.Instance.GantiBahasa(BahasaType.Indonesia);
                break;
            case BahasaType.Jawa:
                toggleJawa.gameObject.SetActive(true);
                GameManager.Instance.GantiBahasa(BahasaType.Jawa);
                break;
            default:
                break;
        }
    }
}
