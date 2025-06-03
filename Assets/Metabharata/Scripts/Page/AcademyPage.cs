using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class AcademyPage : Page
{
    [SerializeField] private CharacterListCard characterListCardPrefab;
    [SerializeField] private List<CharacterListCard> characterListCards;
    [SerializeField] private List<CharacterListCard> characterListCardsFiltered;
    [SerializeField] private Transform cardLocation;

    [Header ("TopBar")]
    [SerializeField] private Button backButton;
    private bool isMainPage;

    [Header("Search")]
    [SerializeField] private Button epicButton;
    [SerializeField] private Button rareButton;
    [SerializeField] private Button legendaryButton;
    [SerializeField] private TMP_InputField searchInput;

    [Header ("Main")]
    [SerializeField] private Button prajuritButton;
    [SerializeField] private Button upgradeCharacterButton;
    [SerializeField] private Button upgradeEquipmentButton;
    [SerializeField] private Image upgradeCharacterSelectImage;
    [SerializeField] private Image upgradeEquipmentSelectImage;
   

    [Header("UpgradeCharacter")]
    [SerializeField] private AcademyDetailCharacter academyDetailCharacter;
    [SerializeField] private GameObject cardListPanel;

    [Header("UpgradeEquipment")]
    [SerializeField] private AcademyUpgradeEquipment academyUpgradeEquipment;

    public bool IsMainPage { get { return isMainPage; } set { isMainPage = value; } }
    public List<CharacterListCard> CharacterListCards { get { return characterListCards; } }
    
    protected override void Start()
    {
        base.Start();

        characterListCards = new List<CharacterListCard>();
        backButton.onClick.AddListener(() => { BackButtonFunction(); });
        prajuritButton.onClick.AddListener(() => { FilterWithType(CharacterType.Prajurit); });
        epicButton.onClick.AddListener(() => { FilterWithType(CharacterType.Epic); });
        rareButton.onClick.AddListener(() => { FilterWithType(CharacterType.Rare); });
        legendaryButton.onClick.AddListener(() => { FilterWithType(CharacterType.Legendary); });
        searchInput.onValueChanged.AddListener(SearchObjects);

        upgradeCharacterButton.onClick.AddListener(() => { SetUpgradeCharacter(); });
        upgradeEquipmentButton.onClick.AddListener(() => { SetUpgradeEquipment(); });
    }

    void BackButtonFunction()
    {
        if (isMainPage)
        {
            pageManager.OpenPage(PageType.MainMenu);
        }
        else
        {
            academyDetailCharacter.DetailCharacterPage.gameObject.SetActive(false);
            academyDetailCharacter.ConfirmBuyWidget.Hide() ;
            isMainPage = true;
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

        isMainPage = true;

        InitShowData();
    }

    public void InitShowData()
    {
        foreach (CharacterListCard cards in characterListCards)
        {
            cards.gameObject.SetActive(false);
        }

        characterListCardsFiltered.Clear();

        for (int i = 0; i < CharacterManager.Manager.characterDatas.Count; i++)
        {
            CharacterListCard tmpCard = GetObjectPooledCard();
            tmpCard.gameObject.SetActive(true);
            tmpCard.OpenCharacterButton.onClick.RemoveAllListeners();

            int j = i;
            tmpCard.OpenCharacterButton.onClick.AddListener(() => { academyDetailCharacter.InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[j],*/ j); isMainPage = false; });
            tmpCard.InitData(CharacterManager.Manager.characterDatas[j], j);

            characterListCardsFiltered.Add(tmpCard);

            if (!CharacterManager.Manager.characterDatas[i].owned)
            {
                tmpCard.SetDijual();
            }
            else
            {
                tmpCard.SetTerjual();
            }
        }

        InitUpgradeEquipment();

        SetUpgradeCharacter();
    }

    void SetUpgradeCharacter()
    {
        prajuritButton.gameObject.SetActive(false);

        //cardListPanel.gameObject.SetActive(true);
        RectTransform rt = cardListPanel.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(245,0); 
        rt.offsetMax = new Vector2(0, -245); 


        upgradeCharacterSelectImage.gameObject.SetActive(true);
        upgradeEquipmentSelectImage.gameObject.SetActive(false);
        academyUpgradeEquipment.gameObject.SetActive(false);

        academyDetailCharacter.DetailCharacterPage.gameObject.SetActive(false);
        academyDetailCharacter.ConfirmBuyWidget.Hide();
    }

    void SetUpgradeEquipment()
    {
        prajuritButton.gameObject.SetActive(true);
        academyDetailCharacter.DetailCharacterPage.gameObject.SetActive(false);
        academyDetailCharacter.ConfirmBuyWidget.Hide();

        //cardListPanel.gameObject.SetActive(false);
        RectTransform rt = cardListPanel.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(2450, 0);
        rt.offsetMax = new Vector2(0, 2450);

        upgradeCharacterSelectImage.gameObject.SetActive(false);
        upgradeEquipmentSelectImage.gameObject.SetActive(true);
        academyUpgradeEquipment.gameObject.SetActive(true);

    }

    void InitUpgradeEquipment()
    {
        if (characterListCardsFiltered.Count > 0)
        {
            academyUpgradeEquipment.InitCharacter(characterListCardsFiltered);
        }
        else
        {
            academyUpgradeEquipment.SetBlank();
        }
    }

    private void FilterWithType(CharacterType tmpType)
    {
        foreach (CharacterListCard tmpCard in characterListCards)
        {
            tmpCard.gameObject.SetActive(false);
        }

        characterListCardsFiltered.Clear();

        for (int i = 0; i < CharacterManager.Manager.characterDatas.Count; i++)
        {
            if (CharacterManager.Manager.characterDatas[i].data.characterType == tmpType /* && !CharacterManager.Manager.characterDatas[i].owned*/)
            {
                CharacterListCard tmpCard = GetObjectPooledCard();
                tmpCard.gameObject.SetActive(true);
                tmpCard.OpenCharacterButton.onClick.RemoveAllListeners();

                int j = i;
                tmpCard.OpenCharacterButton.onClick.AddListener(() => { academyDetailCharacter.InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[j],*/ j); isMainPage = false; });
                tmpCard.InitData(CharacterManager.Manager.characterDatas[j], j);

                characterListCardsFiltered.Add(tmpCard);

                if (!CharacterManager.Manager.characterDatas[i].owned)
                {
                    tmpCard.SetDijual();
                }
                else
                {
                    tmpCard.SetTerjual();
                }
            }

            InitUpgradeEquipment();
        }

        switch (tmpType)
        {
            case CharacterType.Prajurit:
                break;
            case CharacterType.Elite:
                break;
            case CharacterType.Rare:
                break;
            case CharacterType.Epic:
                break;
            case CharacterType.Legendary:
                break;
            default:
                break;
        }
    }

    private void SearchObjects(string query)
    {
        foreach (CharacterListCard tmpCard in characterListCards)
        {
            tmpCard.gameObject.SetActive(false);
        }

        characterListCardsFiltered.Clear();

        if (string.IsNullOrEmpty(query)) return;

        for (int i = 0; i < CharacterManager.Manager.characterDatas.Count; i++)
        {
            bool match = CharacterManager.Manager.characterDatas[i].data.characterName.ToLower().Contains(query.ToLower());

            if (match)
            {
                CharacterListCard tmpCard = GetObjectPooledCard();
                tmpCard.gameObject.SetActive(true);
                tmpCard.OpenCharacterButton.onClick.RemoveAllListeners();

                int j = i;
                tmpCard.OpenCharacterButton.onClick.AddListener(() => { academyDetailCharacter.InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[j],*/ j); isMainPage = false;});
                tmpCard.InitData(CharacterManager.Manager.characterDatas[j], j);

                characterListCardsFiltered.Add(tmpCard);
            }
        }

        InitUpgradeEquipment();
    }

    public override void Hide()
    {
        base.Hide();

        foreach (CharacterListCard tmpCard in characterListCards)
        {
            tmpCard.gameObject.SetActive(false);
        }
    }

    private CharacterListCard GetObjectPooledCard()
    {
        for (int i = 0; i < characterListCards.Count; i++)
        {
            if (!characterListCards[i].gameObject.activeInHierarchy)
            {
                return characterListCards[i];
            }
        }

        return CreateCharacterListCard();
    }

    private CharacterListCard CreateCharacterListCard()
    {
        CharacterListCard newCard = Instantiate(characterListCardPrefab, cardLocation);
        newCard.gameObject.SetActive(false);
        characterListCards.Add(newCard);
        return newCard;
    }
}
