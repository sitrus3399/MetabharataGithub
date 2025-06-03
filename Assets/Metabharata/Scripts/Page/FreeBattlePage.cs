using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FreeBattlePage : Page
{
    [Header("Character Select")]
    [SerializeField] private List<SelectCharacterFreeBattleCard> freeBattleCards;
    [SerializeField] private int selectCharacterNumber = 1;
    [SerializeField] private Button selectCharacterButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Image characterImage1;
    [SerializeField] private Animator characterAnimator1;
    [SerializeField] private TMP_Text characterText1;
    [SerializeField] private Image characterImage2;
    [SerializeField] private Animator characterAnimator2;
    [SerializeField] private TMP_Text characterText2;
    [SerializeField] private Sprite blankCharacter;

    [Header("Arena Select")]
    [SerializeField] private Button selectArenaButton;
    [SerializeField] private Button backArenaButton;
    [SerializeField] private Button prevArenaButton;
    [SerializeField] private Button NextArenaButton;
    [SerializeField] private TMP_Text arenaNameText;
    [SerializeField] private TMP_Text selectArenaLabel;
    [SerializeField] private Image selectIconPlayer1;
    [SerializeField] private Image selectIconPlayer2;
    [SerializeField] private List<Image> listBackgroundImage;
    [SerializeField] private List<Sprite> listBackgroundSprite;
    [SerializeField] private List<string> listBackgroundName;
    [SerializeField] private int backgroundSelectIndex;
    [SerializeField] private GameObject mapListPanel;

    [Header("General")]
    [SerializeField] private GameObject selectCharacterPanel;
    [SerializeField] private GameObject selectArenaPanel;

    protected override void Start()
    {
        base.Start();

        InitData();

        backButton.onClick.AddListener(() => { BackFunction(); });
        selectCharacterButton.onClick.AddListener(() => { SelectCharacter(); });

        selectArenaButton.onClick.AddListener(() => { SelectArenaFunction(); });
        backArenaButton.onClick.AddListener(() => { BackArenaFunction(); });
        prevArenaButton.onClick.AddListener(() => { PrevFunction(); });
        NextArenaButton.onClick.AddListener(() => { NextFunction(); });

        for (int i = 0; i < listBackgroundImage.Count; i++)
        {
            listBackgroundImage[i].sprite = listBackgroundSprite[i];
        }
    }

    void BackFunction()
    {
        if (selectCharacterNumber == 2)
        {
            selectCharacterNumber = 1;
            EmptyCharacter2();
            selectIconPlayer2.gameObject.SetActive(false);
        }
        else if (selectCharacterNumber == 1)
        {
            EmptyCharacter1();
            selectIconPlayer1.gameObject.SetActive(false);
            pageManager.OpenPage(PageType.MainMenu);
        }
    }

    public GameObject SelectIcon()
    {
        if (selectCharacterNumber == 2)
        {
            return selectIconPlayer2.gameObject;
        }
        else if (selectCharacterNumber == 1)
        {
            return selectIconPlayer1.gameObject;
        }

        return null;
    }

    void PrevFunction()
    {
        if (backgroundSelectIndex > 0)
        {
            backgroundSelectIndex -= 1;

            mapListPanel.transform.localPosition = new Vector2(4993.5f - ((backgroundSelectIndex * (2329 / 2)) + 160), 0);
            arenaNameText.text = listBackgroundName[backgroundSelectIndex];

            GameManager.Instance.backgroundFreeBattle = listBackgroundSprite[backgroundSelectIndex];
        }
    }

    void NextFunction()
    {
        if (backgroundSelectIndex < listBackgroundImage.Count - 1)
        {
            backgroundSelectIndex += 1;

            mapListPanel.transform.localPosition = new Vector2(4993.5f - ((backgroundSelectIndex * (2329 / 2)) + 160), 0);
            arenaNameText.text = listBackgroundName[backgroundSelectIndex];

            GameManager.Instance.backgroundFreeBattle = listBackgroundSprite[backgroundSelectIndex];
        }
    }

    void SelectCharacter()
    {
        if (selectCharacterNumber == 2)
        {
            if (GameManager.Instance.characterDataFreeBattle2 != null)
            {
                mapListPanel.transform.localPosition = new Vector2(4993.5f, 0);
                arenaNameText.text = listBackgroundName[backgroundSelectIndex];
                GameManager.Instance.backgroundFreeBattle = listBackgroundSprite[0];

                selectCharacterPanel.SetActive(false);
                selectArenaPanel.SetActive(true);
            }
        }
        else if (selectCharacterNumber == 1)
        {
            if (GameManager.Instance.characterDataFreeBattle1 != null)
            {
                selectCharacterNumber = 2;
            }
        }
    }

    void SelectArenaFunction()
    {
        GameManager.Instance.stageType = StageType.FreeBattle;
        SceneManager.LoadScene("GamePlay");
    }

    void BackArenaFunction()
    {
        selectCharacterPanel.SetActive(true);
        selectArenaPanel.SetActive(false);
    }

    public void InitCharacter(CharacterData tmpCharacterData)
    {
        if (selectCharacterNumber == 2)
        {
            characterImage2.sprite = tmpCharacterData.characterSprite;
            characterImage2.color = Color.white;
            characterText2.text = tmpCharacterData.characterName;
            characterAnimator2.runtimeAnimatorController = tmpCharacterData.previewAnimator;
            GameManager.Instance.characterDataFreeBattle2 = tmpCharacterData;
        }
        else if (selectCharacterNumber == 1)
        {
            characterImage1.sprite = tmpCharacterData.characterSprite;
            characterImage1.color = Color.white;
            characterText1.text = tmpCharacterData.characterName;
            characterAnimator1.runtimeAnimatorController = tmpCharacterData.previewAnimator;
            GameManager.Instance.characterDataFreeBattle1 = tmpCharacterData;
        }
    }

    void EmptyCharacter1()
    {
        characterText1.text = "";
        characterAnimator1.runtimeAnimatorController = null;
        GameManager.Instance.characterDataFreeBattle1 = null;
        characterImage1.sprite = blankCharacter;
        characterImage1.color = Color.black;
    }

    void EmptyCharacter2()
    {
        characterText2.text = "";
        characterAnimator2.runtimeAnimatorController = null;
        GameManager.Instance.characterDataFreeBattle2 = null;
        characterImage2.sprite = blankCharacter;
        characterImage2.color = Color.black;
    }

    void InitData()
    {
        for (int i = 0; i < CharacterManager.Manager.characterDatas.Count; i++)
        {
            freeBattleCards[i].Init(CharacterManager.Manager.characterDatas[i].data, CharacterManager.Manager.characterDatas[i].owned, this);
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

        InitData();
        EmptyCharacter1();
        EmptyCharacter2();
        selectCharacterNumber = 1;

        selectCharacterPanel.SetActive(true);
        selectArenaPanel.SetActive(false);
    }

    public override void Hide()
    {
        base.Hide();
    }
}