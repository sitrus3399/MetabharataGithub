using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterListCard : MonoBehaviour
{
    [SerializeField] private Button openCharacterButton;
    [SerializeField] private Image characterImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Sprite frameActiveSprite;
    [SerializeField] private Sprite frameNonActiveSprite;
    [SerializeField] private Image nameBarImage;
    [SerializeField] private Sprite nameBarActiveSprite;
    [SerializeField] private Sprite nameBarNonActiveSprite;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite buttonActiveSprite;
    [SerializeField] private Sprite buttonNonActiveSprite;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject priceBar;
    [SerializeField] private GameObject notifSold;
    [SerializeField] private int indexCharacter;
    [SerializeField] private int buyPrice;

    public Button OpenCharacterButton { get { return openCharacterButton; } }
    public int IndexCharacter { get { return indexCharacter; } }

    public void InitData(CharacterCollect characterData, int index)
    {
        characterImage.sprite = characterData.data.characterSprite;

        indexCharacter = index;

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

        priceText.text = buyPrice.ToString();
        nameText.text = characterData.data.characterNickName;

        if (characterData.owned)
        {
            frameImage.sprite = frameNonActiveSprite;
            nameBarImage.sprite = nameBarNonActiveSprite;
            buttonImage.sprite = buttonNonActiveSprite;
        }
        else
        {
            frameImage.sprite = frameActiveSprite;
            nameBarImage.sprite = nameBarActiveSprite;
            buttonImage.sprite = buttonActiveSprite;
        }
    }

    public void SetTerjual()
    {
        priceBar.SetActive(false);
        notifSold.SetActive(true);
    }

    public void SetDijual()
    {
        priceBar.SetActive(true);
        notifSold.SetActive(false);
    }
}
