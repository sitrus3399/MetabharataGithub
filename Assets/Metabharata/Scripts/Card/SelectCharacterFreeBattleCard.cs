using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacterFreeBattleCard : MonoBehaviour
{
    [SerializeField] private Image cardIcon;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Button cardButton;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private CharacterData characterData;
    [SerializeField] private FreeBattlePage freeBattlePage;
    [SerializeField] private bool isOwned;

    private void Start()
    {
        cardButton.onClick.AddListener(() => { SelectFunction(); });
    }

    void SelectFunction()
    {
        freeBattlePage.InitCharacter(characterData);
        freeBattlePage.SelectIcon().SetActive(true);
        freeBattlePage.SelectIcon().transform.position = transform.position;
    }

    public void Init(CharacterData tmpCharacterData, bool tmpOwned, FreeBattlePage tmpFreeBattlePage)
    {
        characterData = tmpCharacterData;
        cardIcon.sprite = characterData.characterPortrait;
        cardName.text = characterData.characterName;
        isOwned = tmpOwned;
        freeBattlePage = tmpFreeBattlePage;

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