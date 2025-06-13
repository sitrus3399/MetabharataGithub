using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryModePage : Page
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Image enemyBossImage;
    [SerializeField] private TMP_Text enemyBossName;
    [SerializeField] private TMP_Text enemyBossQuotes;
    [SerializeField] private Image storyIcon;
    [SerializeField] private TMP_Text storySelectedNumber;
    [SerializeField] private TMP_Text storyTitle;
    [SerializeField] private TMP_Text storyDescription;
    [SerializeField] private SelectStoryCard prefabSelectStoryCard;
    [SerializeField] private Transform selectStoryCardLocation;
    [SerializeField] private List<SelectStoryCard> prefabSelectStoryCardList;

    [SerializeField] private StoryData selectedStoryData;

    protected override void Start()
    {
        base.Start();

        backButton.onClick.AddListener(() => { pageManager.OpenPage(PageType.MainMenu); });
        playButton.onClick.AddListener(() => { Play(); });

        for (int i = 0; i < StoryManager.Manager.Story.Count; i++)
        {
            SelectStoryCard newCard = Instantiate(prefabSelectStoryCard, selectStoryCardLocation);
            newCard.InitData(this, i);
            prefabSelectStoryCardList.Add(newCard);
        }

        if (prefabSelectStoryCardList.Count > 0)
        {
            InitData(prefabSelectStoryCardList[0].StoryData, prefabSelectStoryCardList[0]);
            GameManager.Instance.SelectStoryData(prefabSelectStoryCardList[0].StoryData);
        }
    }

    void Play()
    {
        GameManager.Instance.storyDataSelected = selectedStoryData;
        GameManager.Instance.stageType = StageType.Story;
        SceneManager.LoadScene("GamePlay");
    }

    public void InitData(StoryData tmpStoryData, SelectStoryCard selectedCard)
    {
        selectedStoryData = tmpStoryData;
        enemyBossImage.sprite = tmpStoryData.enemyBossData.characterSprite;
        enemyBossName.text = tmpStoryData.enemyBossData.characterName;
        enemyBossQuotes.text = tmpStoryData.enemyBossData.characterQuotes;
        storyIcon.sprite = tmpStoryData.storyIcon;
        storyTitle.text = tmpStoryData.storyName;
        storyDescription.text = tmpStoryData.storyDescription;
        storySelectedNumber.text = selectedCard.StoryData.storyNumber.ToString();

        foreach (SelectStoryCard card in prefabSelectStoryCardList)
        {
            if (card.isActive)
            {
                card.SetDeselected();
            }

            if (card == selectedCard)
            {
                card.SetSelected();
            }
        }
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

        foreach (SelectStoryCard card in prefabSelectStoryCardList)
        {
            card.SetLock();
        }

        for (int i = 0; i < PlayerManager.Manager.StoryUnlock; i++)
        {
            prefabSelectStoryCardList[i].SetUnlock();
        }

        InitData(prefabSelectStoryCardList[0].StoryData, prefabSelectStoryCardList[0]);
    }

    public override void Hide()
    {
        base.Hide();
    }
}