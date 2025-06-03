using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitRoomWidget : Widget
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private Button closeButton;

    void Start()
    {
        yesButton.onClick.AddListener(() => { OnClickYes(); });
        noButton.onClick.AddListener(() => { OnClickNo(); });
        closeButton.onClick.AddListener(() => { OnClickNo(); });
    }

    void OnClickYes()
    {
        SceneManager.LoadScene("MainMenu");
        //Close Network Room
    }

    void OnClickNo()
    {
        widgetManager.OpenWidget(WidgetType.Empty);
    }
}
