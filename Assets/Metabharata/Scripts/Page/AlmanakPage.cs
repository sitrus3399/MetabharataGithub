using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlmanakPage : Page
{
    [SerializeField] private AlmanakState almanakState;

    [Header("List Card")]
    [SerializeField] private AlmanakListCard almanakListCard;
    [SerializeField] private List<AlmanakListCard> almanakListCards;
    [SerializeField] private List<AlmanakListCard> almanakListCardFiltered;
    [SerializeField] private Transform cardLocation;

    [Header("TopBar")]
    [SerializeField] private Button backButton;
    private bool isMainPage;

    [Header("Search")]
    [SerializeField] private Button epicButton;
    [SerializeField] private Button rareButton;
    [SerializeField] private Button legendaryButton;
    [SerializeField] private Button allButton;
    [SerializeField] private Button ownedButton;
    [SerializeField] private Button yetOwnedButton;
    [SerializeField] private TMP_InputField searchInput;

    [Header("UpgradeCharacter")]
    [SerializeField] private AcademyDetailCharacter academyDetailCharacter;

    public List<AlmanakListCard> CharacterListCards { get { return almanakListCards; } }

    protected override void Start()
    {
        base.Start();

        almanakListCards = new List<AlmanakListCard>();

        backButton.onClick.AddListener(() => { BackButtonFunction(); });

        epicButton.onClick.AddListener(() => { FilterWithType(CharacterType.Epic); });
        rareButton.onClick.AddListener(() => { FilterWithType(CharacterType.Rare); });
        legendaryButton.onClick.AddListener(() => { FilterWithType(CharacterType.Legendary); });
        allButton.onClick.AddListener(() => { FilterWithOwned(AlmanakState.All); });
        ownedButton.onClick.AddListener(() => { FilterWithOwned(AlmanakState.Owned); });
        yetOwnedButton.onClick.AddListener(() => { FilterWithOwned(AlmanakState.YetOwned); });

        searchInput.onValueChanged.AddListener(SearchObjects);
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

        almanakState = AlmanakState.All;

        InitData(almanakState );

        InitShowData();
    }

    public void InitShowData()
    {

        foreach (AlmanakListCard cards in almanakListCards)
        {
            cards.gameObject.SetActive(false);
        }

        almanakListCardFiltered.Clear();

        for (int i = 0; i < CharacterManager.Manager.characterDatas.Count; i++)
        {
            AlmanakListCard tmpCard = GetObjectPooledCard();
            tmpCard.gameObject.SetActive(true);
            tmpCard.OpenCharacterButton.onClick.RemoveAllListeners();

            int j = i;
            tmpCard.OpenCharacterButton.onClick.AddListener(() => { academyDetailCharacter.InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[j],*/ j); isMainPage = false; });
            tmpCard.InitData(j);

            almanakListCardFiltered.Add(tmpCard);

            if (!CharacterManager.Manager.characterDatas[i].owned)
            {
                tmpCard.SetOwned(true);
            }
            else
            {
                tmpCard.SetOwned(false);
            }

            if (CharacterManager.Manager.characterDatas[j].data.characterType == CharacterType.Legendary)
            {
                CreateCardLegendary(j);
            }
        }
    }

    void InitData(AlmanakState almanakState )
    {
        switch (almanakState)
        {
            case AlmanakState.All:
                break;
            case AlmanakState.Owned:
                break;
            case AlmanakState.YetOwned:
                break;
            default:
                break;
        }
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
            academyDetailCharacter.ConfirmBuyWidget.Hide();
            isMainPage = true;
        }
    }

    private void FilterWithType(CharacterType tmpType)
    {
        foreach (AlmanakListCard tmpCard in almanakListCards)
        {
            tmpCard.gameObject.SetActive(false);
        }

        almanakListCardFiltered.Clear();

        for (int i = 0; i < CharacterManager.Manager.characterDatas.Count; i++)
        {
            if (CharacterManager.Manager.characterDatas[i].data.characterType == tmpType /* && !CharacterManager.Manager.characterDatas[i].owned*/)
            {
                AlmanakListCard tmpCard = GetObjectPooledCard();
                tmpCard.gameObject.SetActive(true);
                tmpCard.OpenCharacterButton.onClick.RemoveAllListeners();

                int j = i;
                tmpCard.OpenCharacterButton.onClick.AddListener(() => { academyDetailCharacter.InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[j],*/ j); isMainPage = false; });
                tmpCard.InitData(j);

                almanakListCardFiltered.Add(tmpCard);

                if (!CharacterManager.Manager.characterDatas[i].owned)
                {
                    tmpCard.SetOwned(true);
                }
                else
                {
                    tmpCard.SetOwned(false);
                }

                if (CharacterManager.Manager.characterDatas[j].data.characterType == CharacterType.Legendary)
                {
                    CreateCardLegendary(j);
                }
            }
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

    private void FilterWithOwned(AlmanakState tmpState)
    {
        foreach (AlmanakListCard tmpCard in almanakListCards)
        {
            tmpCard.gameObject.SetActive(false);
        }

        almanakListCardFiltered.Clear();

        for (int i = 0; i < CharacterManager.Manager.characterDatas.Count; i++)
        {
            AlmanakListCard tmpCard;
            int j;

            switch (tmpState)
            {
                case AlmanakState.All:
                    tmpCard = GetObjectPooledCard();
                    tmpCard.gameObject.SetActive(true);
                    tmpCard.OpenCharacterButton.onClick.RemoveAllListeners();

                    j = i;
                    tmpCard.OpenCharacterButton.onClick.AddListener(() => { academyDetailCharacter.InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[j],*/ j); isMainPage = false; });
                    tmpCard.InitData(j);

                    almanakListCardFiltered.Add(tmpCard);

                    if (!CharacterManager.Manager.characterDatas[i].owned)
                    {
                        tmpCard.SetOwned(true);
                    }
                    else
                    {
                        tmpCard.SetOwned(false);
                    }

                    if (CharacterManager.Manager.characterDatas[j].data.characterType == CharacterType.Legendary)
                    {
                        CreateCardLegendary(j);
                    }
                    break;
                case AlmanakState.Owned:
                    if (CharacterManager.Manager.characterDatas[i].owned)
                    {
                        tmpCard = GetObjectPooledCard();
                        tmpCard.gameObject.SetActive(true);
                        tmpCard.OpenCharacterButton.onClick.RemoveAllListeners();

                        j = i;
                        tmpCard.OpenCharacterButton.onClick.AddListener(() => { academyDetailCharacter.InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[j],*/ j); isMainPage = false; });
                        tmpCard.InitData(j);

                        almanakListCardFiltered.Add(tmpCard);

                        if (!CharacterManager.Manager.characterDatas[i].owned)
                        {
                            tmpCard.SetOwned(true);
                        }
                        else
                        {
                            tmpCard.SetOwned(false);
                        }

                        if (CharacterManager.Manager.characterDatas[j].data.characterType == CharacterType.Legendary)
                        {
                            CreateCardLegendary(j);
                        }
                    }
                    break;
                case AlmanakState.YetOwned:
                    if (!CharacterManager.Manager.characterDatas[i].owned)
                    {
                        tmpCard = GetObjectPooledCard();
                        tmpCard.gameObject.SetActive(true);
                        tmpCard.OpenCharacterButton.onClick.RemoveAllListeners();

                        j = i;
                        tmpCard.OpenCharacterButton.onClick.AddListener(() => { academyDetailCharacter.InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[j],*/ j); isMainPage = false; });
                        tmpCard.InitData(j);

                        almanakListCardFiltered.Add(tmpCard);

                        if (!CharacterManager.Manager.characterDatas[i].owned)
                        {
                            tmpCard.SetOwned(true);
                        }
                        else
                        {
                            tmpCard.SetOwned(false);
                        }

                        if (CharacterManager.Manager.characterDatas[j].data.characterType == CharacterType.Legendary)
                        {
                            CreateCardLegendary(j);
                        }
                    }
                    break;
                default:
                    break;
            }   
        }
    }

    private void SearchObjects(string query)
    {
        foreach (AlmanakListCard tmpCard in almanakListCards)
        {
            tmpCard.gameObject.SetActive(false);
        }

        almanakListCardFiltered.Clear();

        if (string.IsNullOrEmpty(query)) return;

        for (int i = 0; i < CharacterManager.Manager.characterDatas.Count; i++)
        {
            bool match = CharacterManager.Manager.characterDatas[i].data.characterName.ToLower().Contains(query.ToLower());

            if (match)
            {
                AlmanakListCard tmpCard = GetObjectPooledCard();
                tmpCard.gameObject.SetActive(true);
                tmpCard.OpenCharacterButton.onClick.RemoveAllListeners();

                int j = i;
                tmpCard.OpenCharacterButton.onClick.AddListener(() => { academyDetailCharacter.InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[j],*/ j); isMainPage = false; });
                tmpCard.InitData( j);

                almanakListCardFiltered.Add(tmpCard);

                if (CharacterManager.Manager.characterDatas[j].data.characterType == CharacterType.Legendary)
                {
                    CreateCardLegendary(j);
                }
            }
        }
    }

    void CreateCardLegendary(int index)
    {
        if (CharacterManager.Manager.characterDatas[index].data.characterType == CharacterType.Legendary)
        {
            AlmanakListCard tmpCardLegendary = GetObjectPooledCard();
            tmpCardLegendary.gameObject.SetActive(true);
            tmpCardLegendary.OpenCharacterButton.onClick.RemoveAllListeners();

            tmpCardLegendary.OpenCharacterButton.onClick.AddListener(() => { academyDetailCharacter.InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[index],*/ index, true); isMainPage = false; });
            tmpCardLegendary.InitData(index, true);

            almanakListCardFiltered.Add(tmpCardLegendary);

            if (!CharacterManager.Manager.characterDatas[index].owned)
            {
                tmpCardLegendary.SetOwned(true);
            }
            else
            {
                tmpCardLegendary.SetOwned(false);
            }
        }
    }

    private AlmanakListCard GetObjectPooledCard()
    {
        for (int i = 0; i < almanakListCards.Count; i++)
        {
            if (!almanakListCards[i].gameObject.activeInHierarchy)
            {
                return almanakListCards[i];
            }
        }

        return CreateCharacterListCard();
    }

    private AlmanakListCard CreateCharacterListCard()
    {
        AlmanakListCard newCard = Instantiate(almanakListCard, cardLocation);
        newCard.gameObject.SetActive(false);
        almanakListCards.Add(newCard);
        return newCard;
    }
}

[System.Serializable]
public enum AlmanakState
{
    All,
    Owned,
    YetOwned
}