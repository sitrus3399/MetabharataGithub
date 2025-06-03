using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmBuyWidget : Widget
{
    [SerializeField] private Button buyButton;
    [SerializeField] private Image buyImageButton;
    [SerializeField] private Sprite activeButtonImage;
    [SerializeField] private Sprite nonActiveButtonImage;
    [SerializeField] private Button cancelBuyButton;
    [SerializeField] private Button closeBuyButton;
    private CharacterData characterData;

    [SerializeField] private List<AcademyDetailCharacter> academyDetailCharacter;
    [SerializeField] private AcademyPage academyPage;
    [SerializeField] private AlmanakPage almanakPage;

    [SerializeField] private int indexCharacter;
    [SerializeField] private int buyPrice;

    [SerializeField] private TMP_Text priceText;

    void Start()
    {
        cancelBuyButton.onClick.AddListener(() => { Hide(); });
        closeBuyButton.onClick.AddListener(() => { Hide(); });
    }

    public void InitData(CharacterCollect data, int index)
    {
        characterData = data.data;

        indexCharacter = index;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => { Buy(data); Hide(); });

        switch (CharacterManager.Manager.characterDatas[index].data.characterType)
        {
            case CharacterType.Prajurit:
                buyPrice = (int)CharacterManager.Manager.characterPrice[0].buyPrice;
                break;
            case CharacterType.Elite:
                buyPrice = (int)CharacterManager.Manager.characterPrice[1].buyPrice;
                break;
            case CharacterType.Epic:
                buyPrice = (int)CharacterManager.Manager.characterPrice[2].buyPrice;
                break;
            case CharacterType.Rare:
                buyPrice = (int)CharacterManager.Manager.characterPrice[3].buyPrice;
                break;
            case CharacterType.Legendary:
                buyPrice = (int)CharacterManager.Manager.characterPrice[4].buyPrice;
                break;
            default:
                break;
        }

        if (PlayerManager.Manager.Coin >= buyPrice)
        {
            buyImageButton.sprite = activeButtonImage;
            buyButton.interactable = true;
        }
        else
        {
            buyImageButton.sprite = nonActiveButtonImage;
            buyButton.interactable= false;
        }

        priceText.text = buyPrice.ToString();
    }

    void Buy(CharacterCollect characterData)
    {
        if (PlayerManager.Manager.Coin >= buyPrice)
        {
            PlayerManager.Manager.ReduceCoin(buyPrice);

            characterData.owned = true;

            foreach (AcademyDetailCharacter academyDetail in academyDetailCharacter)
            {
                academyDetail.ReInit();
            }
            
            academyPage.InitShowData();
            almanakPage.InitShowData();
        }
    }
}
