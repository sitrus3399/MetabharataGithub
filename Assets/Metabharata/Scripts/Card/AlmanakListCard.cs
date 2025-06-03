using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlmanakListCard : MonoBehaviour
{
    [SerializeField] private Image frameImage;
    [SerializeField] private Sprite frameActiveSprite;
    [SerializeField] private Sprite frameNonActiveSprite;
    [SerializeField] private Image nameBarImage;
    [SerializeField] private Sprite nameBarActiveSprite;
    [SerializeField] private Sprite nameBarNonActiveSprite;
    [SerializeField] private Image characterImage;

    [SerializeField] private Button openCharacterButton;
    [SerializeField] private TMP_Text characterName;

    [SerializeField] private int indexCharacter;

    public Button OpenCharacterButton { get { return openCharacterButton; } } 

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void InitData(int index, bool isLegendary = false)
    {
        indexCharacter = index;

        CharacterCollect characterData = CharacterManager.Manager.characterDatas[indexCharacter];

        characterName.text = characterData.data.characterNickName;

        if (isLegendary)
        {
            characterImage.sprite = characterData.data.characterLegendarySprite;
        }
        else
        {
            characterImage.sprite = characterData.data.characterSprite;
        }
        
        if (!characterData.owned)
        {
            frameImage.sprite = frameNonActiveSprite;
            nameBarImage.sprite = nameBarNonActiveSprite;
            //buttonImage.sprite = buttonNonActiveSprite;
        }
        else
        {
            frameImage.sprite = frameActiveSprite;
            nameBarImage.sprite = nameBarActiveSprite;
            //buttonImage.sprite = buttonActiveSprite;
        }
    }

    public void SetOwned(bool isOwned)
    {
        switch (isOwned)
        {
            case true:
                break;
            case false:
                break;
            default:
                break;
        }
    }
}
