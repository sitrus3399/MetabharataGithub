using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectStoryCard : MonoBehaviour
{
    [SerializeField] private StoryModePage storyModePage;
    [SerializeField] private StoryData storyData;
    [SerializeField] private Image storyIcon;
    [SerializeField] private TMP_Text storyNumber;
    [SerializeField] private Button interactButton;
    [SerializeField] private Sprite storyActive;
    [SerializeField] private Sprite storyNonActive;

    public Button InteractButton { get { return interactButton; } }
    public StoryData StoryData { get { return storyData; } }

    void Start()
    {
        interactButton.onClick.AddListener(() => { InteractButtonFunction(); });
    }

    void InteractButtonFunction()
    {
        GameManager.Instance.SelectStoryData(storyData);
    }

    public void InitData(StoryModePage newStoryModePage,int indexCharacter)
    {
        storyModePage = newStoryModePage;
        storyData = StoryManager.Manager.Story[indexCharacter].data;
        storyIcon.sprite = storyData.storyIcon;
        storyNumber.text = storyData.storyNumber.ToString();
        interactButton.onClick.AddListener(() => { storyModePage.InitData(storyData); });
    }

    public void SetUnlock()
    {
        interactButton.interactable = true;
        storyIcon.sprite = storyActive;
    }   
    
    public void SetLock()
    {
        interactButton.interactable = false;
        storyIcon.sprite = storyNonActive;
    }

    void Update()
    {
        
    }
}
