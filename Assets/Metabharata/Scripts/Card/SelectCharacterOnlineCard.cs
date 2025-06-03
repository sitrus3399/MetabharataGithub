using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacterOnlineCard : MonoBehaviour
{
    [SerializeField] private Image cardIcon;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Button cardButton;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private CharacterData characterData;
    [SerializeField] private SelectCharacterRoomPage onlinePage;
    [SerializeField] private bool isOwned;

    void Start()
    {
        cardButton.onClick.AddListener(() => { SelectFunction(); });
    }

    void SelectFunction()
    {
        onlinePage.InitCharacter(characterData);
        onlinePage.SelectIcon().SetActive(true);
        onlinePage.SelectIcon().transform.position = transform.position;
    }

    public void Init(CharacterData tmpCharacterData, bool tmpOwned, SelectCharacterRoomPage tmpOnlinePage)
    {
        characterData = tmpCharacterData;
        cardIcon.sprite = characterData.characterPortrait;
        cardName.text = characterData.characterName;
        isOwned = tmpOwned;
        onlinePage = tmpOnlinePage;

        if (tmpOwned)
        {
            cardButton.interactable = true;
            lockIcon.gameObject.SetActive(false);
        }
        else
        {
            cardButton.interactable = false;
            lockIcon.gameObject.SetActive(true);
        }
    }
}
