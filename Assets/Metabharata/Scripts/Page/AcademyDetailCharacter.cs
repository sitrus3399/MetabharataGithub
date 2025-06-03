using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AcademyDetailCharacter : MonoBehaviour
{
    [SerializeField] private DetailCharacterPageType type;

    [SerializeField] private AcademyPage academyPage;
    [SerializeField] private AlmanakPage almanakPage;

    //int previewNumber;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [SerializeField] private GameObject detailCharacterPage;
    [SerializeField] private Image characterShilouette;
    [SerializeField] private Button openBuyPanelButton;
    [SerializeField] private Button videoCharacter;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text characterRarity;
    [SerializeField] private TMP_Text characterDescription;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image pukulBar;
    [SerializeField] private Image pukulLegendaryBar;
    [SerializeField] private float maxPukulBar;
    [SerializeField] private Image tendangBar;
    [SerializeField] private Image tendangLegendaryBar;
    [SerializeField] private float maxTendangBar;
    [SerializeField] private Image senjataBar;
    [SerializeField] private Image senjataLegendaryBar;
    [SerializeField] private float maxSenjataBar;
    [SerializeField] private Image specialSkillBar;
    [SerializeField] private Image specialLegendaryBar;
    [SerializeField] private float maxSkillBar;

    [SerializeField] private int indexCharacter;
    [SerializeField] private int buyPrice;
    
    [Header("Widget Confirm Buy")]
    [SerializeField] private ConfirmBuyWidget confirmBuyWidget;

    public GameObject DetailCharacterPage {  get { return detailCharacterPage; } }
    public ConfirmBuyWidget ConfirmBuyWidget { get {  return confirmBuyWidget; } }

    private void Start()
    {
        prevButton.onClick.AddListener(() => { PrevFunction(); });
        nextButton.onClick.AddListener(() => { NextFunction(); });

        openBuyPanelButton.onClick.AddListener(() => { confirmBuyWidget.Show(); });
    }

    void PrevFunction()
    {
        if (type == DetailCharacterPageType.Academy)
        {
            if (indexCharacter - 1 < 0)
            {
                if (academyPage.CharacterListCards.Count > 0)
                {
                    indexCharacter = academyPage.CharacterListCards.Count - 1;
                }
                else
                {
                    indexCharacter = 0;
                }
            }
            else
            {
                indexCharacter -= 1;
            }
        }
        else if (type == DetailCharacterPageType.Almanak)
        {
            if (indexCharacter - 1 < 0)
            {
                if (almanakPage.CharacterListCards.Count > 0)
                {
                    indexCharacter = almanakPage.CharacterListCards.Count - 1;
                }
                else
                {
                    indexCharacter = 0;
                }
            }
            else
            {
                indexCharacter -= 1;
            }
        }

        InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[indexCharacter],*/ indexCharacter);
    }

    void NextFunction()
    {
        if (type == DetailCharacterPageType.Academy)
        {
            if (indexCharacter + 1 < academyPage.CharacterListCards.Count)
            {
                indexCharacter += 1;
            }
            else
            {
                indexCharacter = 0;
            }
        }
        else if (type == DetailCharacterPageType.Almanak)
        {
            if (indexCharacter + 1 < almanakPage.CharacterListCards.Count)
            {
                indexCharacter += 1;
            }
            else
            {
                indexCharacter = 0;
            }
        }

        InitDetailCharacterPage(/*CharacterManager.Manager.characterDatas[indexCharacter],*/ indexCharacter);
    }

    public void ReInit()
    {
        switch (CharacterManager.Manager.characterDatas[indexCharacter].data.characterType)
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

        if (CharacterManager.Manager.characterDatas[indexCharacter].owned)
        {
            priceText.text = "Sold";
            openBuyPanelButton.interactable = false;
        }
        else
        {
            priceText.text = buyPrice.ToString();
            openBuyPanelButton.interactable = true;
        }
    }

    public void InitDetailCharacterPage(/*CharacterCollect characterData,*/ int index, bool isLegendary = false)
    {
        switch (index)
        {
            case 29:
                if (isLegendary)
                {
                    indexCharacter = 32;
                }
                break;
            case 30:
                if (isLegendary)
                {
                    indexCharacter = 33;
                }
                break;
            case 31:
                if (isLegendary)
                {
                    indexCharacter = 34;
                }
                break;
            case 32:
                index = 29;
                isLegendary = true;
                break;
            case 33:
                index = 30;
                isLegendary = true;
                break;
            case 34:
                index = 31;
                isLegendary = true;
                break;
            default:
                break;
        }

        CharacterCollect characterData = CharacterManager.Manager.characterDatas[index];

        confirmBuyWidget.InitData(characterData, index);

        if (!isLegendary)
        {
            indexCharacter = index;
        }

        detailCharacterPage.SetActive(true);

        videoCharacter.onClick.AddListener(() => { });

        if (isLegendary)
        {
            characterName.text = characterData.data.characterName + "Transform";
        }
        else
        {
            characterName.text = characterData.data.characterName;
        }

        characterDescription.text = characterData.data.characterDescription;
        characterRarity.text = characterData.data.characterType.ToString();

        priceText.text = characterData.data.hargaBeli.ToString();

        pukulBar.fillAmount = characterData.data.damagePukul[characterData.punchLevel] / maxPukulBar;
        pukulLegendaryBar.fillAmount = characterData.data.damagePukulLegendary[characterData.punchLevel] / maxPukulBar;
        tendangBar.fillAmount = characterData.data.damageTendang[characterData.kickLevel] / maxTendangBar;
        tendangLegendaryBar.fillAmount = characterData.data.damageTendangLegendary[characterData.kickLevel] / maxTendangBar;
        senjataBar.fillAmount = characterData.data.damageSenjata[characterData.weaponLevel] / maxSenjataBar;
        senjataLegendaryBar.fillAmount = characterData.data.damageSenjataLegendary[characterData.weaponLevel] / maxSenjataBar;
        specialSkillBar.fillAmount = characterData.data.damageSpecialSkill[characterData.specialSkillLevel] / maxSkillBar;
        specialLegendaryBar.fillAmount = characterData.data.damageSpecialLegendary[characterData.specialSkillLevel] / maxSkillBar;

        //if (type == DetailCharacterPageType.Almanak)
        //{
        //    if (isLegendary)
        //    {
        //        pukulBar.gameObject.SetActive(true);
        //        pukulLegendaryBar.gameObject.SetActive(false);
        //        tendangBar.gameObject.SetActive(true);
        //        tendangLegendaryBar.gameObject.SetActive(false);
        //        senjataBar.gameObject.SetActive(true);
        //        senjataLegendaryBar.gameObject.SetActive(false);
        //        specialSkillBar.gameObject.SetActive(true);
        //        specialLegendaryBar.gameObject.SetActive(false);
        //    }
        //    else
        //    {
        //        pukulBar.gameObject.SetActive(false);
        //        pukulLegendaryBar.gameObject.SetActive(true);
        //        tendangBar.gameObject.SetActive(false);
        //        tendangLegendaryBar.gameObject.SetActive(true);
        //        senjataBar.gameObject.SetActive(false);
        //        senjataLegendaryBar.gameObject.SetActive(true);
        //        specialSkillBar.gameObject.SetActive(false);
        //        specialLegendaryBar.gameObject.SetActive(true);
        //    }
        //}

        if (isLegendary)
        {
            characterShilouette.sprite = characterData.data.characterLegendarySprite;
        }
        else
        {
            characterShilouette.sprite = characterData.data.characterSprite;
        }
        
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

        if (CharacterManager.Manager.characterDatas[index].owned)
        {
            priceText.text = "Sold";
            openBuyPanelButton.interactable = false;
        }
        else
        {
            priceText.text = buyPrice.ToString();
            openBuyPanelButton.interactable= true;
        }
    }
}

[System.Serializable]
public enum DetailCharacterPageType
{
    Academy,
    Almanak
}