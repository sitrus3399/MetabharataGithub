using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacterRoomPage : Page
{
    [Header("Character Select")]
    [SerializeField] private List<SelectCharacterOnlineCard> roomCards;
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

        selectCharacterButton.onClick.AddListener(() => { OnClickSelectCharacter(); });
        backButton.onClick.AddListener(() => { OnClickBack(); });
        selectArenaButton.onClick.AddListener(() => { OnClickSelectArena(); });
        backArenaButton.onClick.AddListener(() => { OnClickBackArena(); });
        prevArenaButton.onClick.AddListener(() => { OnClickPrev(); });
        NextArenaButton.onClick.AddListener(() => { OnClickNext(); });
    }

    void OnClickSelectCharacter()
    {
        
    }

    public void InitCharacter(CharacterData tmpCharacterData)
    {
        if (LobbySystemInitiator.Instance.System.IsHost)
        {
            characterImage1.sprite = tmpCharacterData.characterSprite;
            characterImage1.color = Color.white;
            characterText1.text = tmpCharacterData.characterName;
            characterAnimator1.runtimeAnimatorController = tmpCharacterData.previewAnimator;
            GameManager.Instance.characterDataFreeBattle1 = tmpCharacterData;
        }
        else if (LobbySystemInitiator.Instance.System.IsClient)
        {
            characterImage2.sprite = tmpCharacterData.characterSprite;
            characterImage2.color = Color.white;
            characterText2.text = tmpCharacterData.characterName;
            characterAnimator2.runtimeAnimatorController = tmpCharacterData.previewAnimator;
            GameManager.Instance.characterDataFreeBattle2 = tmpCharacterData;
        }
    }

    public GameObject SelectIcon()
    {
        if (LobbySystemInitiator.Instance.System.IsHost)
        {
            return selectIconPlayer1.gameObject;
        }

        if (LobbySystemInitiator.Instance.System.IsClient)
        {
            return selectIconPlayer2.gameObject;
        }

        // Cek tidak pakai login
        Debug.LogWarning($"Belum masuk Server / Test Tanpa Login");
        return selectIconPlayer1.gameObject;
    }

    void OnClickBack()
    {
        pageManager.OpenPage(PageType.MainRoom);
    }

    void OnClickSelectArena()
    {
        selectCharacterPanel.SetActive(false);
        selectArenaPanel.SetActive(true);
    }

    void OnClickPrev()
    {
        if (backgroundSelectIndex > 0)
        {
            backgroundSelectIndex -= 1;

            mapListPanel.transform.localPosition = new Vector2(4993.5f - ((backgroundSelectIndex * (2329 / 2)) + 160), 0);
            arenaNameText.text = listBackgroundName[backgroundSelectIndex];

            GameManager.Instance.backgroundFreeBattle = listBackgroundSprite[backgroundSelectIndex];
        }
    }

    void OnClickNext()
    {
        if (backgroundSelectIndex < listBackgroundImage.Count - 1)
        {
            backgroundSelectIndex += 1;

            mapListPanel.transform.localPosition = new Vector2(4993.5f - ((backgroundSelectIndex * (2329 / 2)) + 160), 0);
            arenaNameText.text = listBackgroundName[backgroundSelectIndex];

            GameManager.Instance.backgroundFreeBattle = listBackgroundSprite[backgroundSelectIndex];
        }
    }

    void OnClickBackArena()
    {
        selectCharacterPanel.gameObject.SetActive(true);
        selectArenaPanel.gameObject.SetActive(false);
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

    void InitData()
    {
        for (int i = 0; i < CharacterManager.Manager.characterDatas.Count; i++)
        {
            roomCards[i].Init(CharacterManager.Manager.characterDatas[i].data, CharacterManager.Manager.characterDatas[i].owned, this);
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
}
