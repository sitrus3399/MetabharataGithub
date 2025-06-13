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
    [SerializeField] private Sprite storySelected;
    [SerializeField] private Sprite storyNonActive;

    [SerializeField] public bool isActive;

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
        interactButton.onClick.AddListener(() => { storyModePage.InitData(storyData, this); });
    }

    public void SetSelected()
    {
        storyIcon.sprite = storySelected;
    }

    public void SetDeselected()
    {
        storyIcon.sprite = storyActive;
    }

    public void SetUnlock()
    {
        storyNumber.text = storyData.storyNumber.ToString();
        interactButton.interactable = true;
        storyIcon.sprite = storyActive;
        isActive = true;
    }   
    
    public void SetLock()
    {
        storyNumber.text = "";
        interactButton.interactable = false;
        storyIcon.sprite = storyNonActive;
        isActive = false;
    }

    void Update()
    {
        
    }
}
