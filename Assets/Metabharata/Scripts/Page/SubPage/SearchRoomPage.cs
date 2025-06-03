using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchRoomPage : MonoBehaviour
{
    [SerializeField] private OnlinePage onlinePage;

    [SerializeField] private TMP_InputField searchText;
    [SerializeField] private TMP_InputField joinText;

    [SerializeField] private Button joinButton;

    void Start()
    {
        joinButton.onClick.AddListener(() => { SearchRoom(); });
    }

    void SearchRoom()
    {
        GameManager.Instance.stageType = StageType.Online;

        onlinePage.StartClient(joinText.text);
    }

    void Update()
    {
        
    }
}