using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AcademyUpgradeEquipment : MonoBehaviour
{
    private List<CharacterListCard> characterListCardsFiltered;

    int previewNumber;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [Header("Level")]
    [SerializeField] private Image[] punchLevelImage;
    [SerializeField] private Image[] kickLevelImage;
    [SerializeField] private Image[] weaponLevelImage;
    [SerializeField] private Image[] defenseLevelImage;
    [SerializeField] private Image[] specialSkillLevelImage;
    [SerializeField] private GameObject specialSkillUpgradeBar;
    [SerializeField] private Sprite activeLevelSprite;
    [SerializeField] private Sprite nonActiveLevelSprite;
    [SerializeField] private int punchLevel;
    [SerializeField] private int kickLevel;
    [SerializeField] private int weaponLevel;
    [SerializeField] private int defenseLevel;
    [SerializeField] private int specialSkillLevel;
    [SerializeField] private float punchPrice;
    [SerializeField] private float kickPrice;
    [SerializeField] private float weaponPrice;
    [SerializeField] private float defensePrice;
    [SerializeField] private float specialSkillPrice;
    [SerializeField] private TMP_Text punchPriceText;
    [SerializeField] private TMP_Text kickPriceText;
    [SerializeField] private TMP_Text weaponPriceText;
    [SerializeField] private TMP_Text defensePriceText;
    [SerializeField] private TMP_Text specialSkillPriceText;
    [SerializeField] private Button upgradePunchButton;
    [SerializeField] private Image upgradePunchImage;
    [SerializeField] private Button upgradeKickButton;
    [SerializeField] private Image upgradeKickImage;
    [SerializeField] private Button upgradeWeaponButton;
    [SerializeField] private Image upgradeWeaponImage;
    [SerializeField] private Button upgradeDefenseButton;
    [SerializeField] private Image upgradeDefenseImage;
    [SerializeField] private Button upgradeSpecialSkillButton;
    [SerializeField] private Image upgradeSpecialSkillImage;

    [SerializeField] private Sprite upgradeActiveImage;
    [SerializeField] private Sprite upgradeNonActiveImage;

    private void Start()
    {
        prevButton.onClick.AddListener(() => { PrevFunction(); });
        nextButton.onClick.AddListener(() => { NextFunction(); });

        upgradePunchButton.onClick.AddListener(() => { UpgradePunch(); });
        upgradeKickButton.onClick.AddListener(() => { UpgradeKick(); });
        upgradeWeaponButton.onClick.AddListener(() => { UpgradeWeapon(); });
        upgradeDefenseButton.onClick.AddListener(() => { UpgradeDefense(); });
        upgradeSpecialSkillButton.onClick.AddListener(() => { UpgradeSpecialSkill(); });
    }

    void UpgradePunch()
    {
        if (PlayerManager.Manager.Coin >= punchPrice && CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].punchLevel < 4)
        {
            PlayerManager.Manager.ReduceCoin(punchPrice);

            CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].punchLevel += 1;

            InitData();
        }

        SetAddImage();
    }

    void UpgradeKick()
    {
        if (PlayerManager.Manager.Coin >= kickPrice && CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].kickLevel < 4)
        {
            PlayerManager.Manager.ReduceCoin(kickPrice);

            CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].kickLevel += 1;

            InitData();
        }

        SetAddImage();
    }

    void UpgradeWeapon()
    {
        if (PlayerManager.Manager.Coin >= weaponPrice && CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].weaponLevel < 4)
        {
            PlayerManager.Manager.ReduceCoin(weaponPrice);

            CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].weaponLevel += 1;

            InitData();
        }

        SetAddImage();
    }

    void UpgradeDefense()
    {
        if (PlayerManager.Manager.Coin >= weaponPrice && CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].defendLevel < 4)
        {
            PlayerManager.Manager.ReduceCoin(defensePrice);

            CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].defendLevel += 1;

            InitData();
        }

        SetAddImage();
    }

    void UpgradeSpecialSkill()
    {
        if (PlayerManager.Manager.Coin >= specialSkillPrice && CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].specialSkillLevel < 4)
        {
            PlayerManager.Manager.ReduceCoin(specialSkillPrice);

            CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].specialSkillLevel += 1;

            InitData();
        }

        SetAddImage();
    }

    public void InitCharacter(List<CharacterListCard> newCharacterListCardsFiltered)
    {
        previewNumber = 0;

        characterListCardsFiltered = newCharacterListCardsFiltered;

        if (characterListCardsFiltered.Count > 0)
        {
            InitData();
        }
        else
        {
            SetBlank();
        }

        SetAddImage();
    }

    void SetAddImage()
    {
        if (CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].punchLevel >= 4)
        {
            upgradePunchImage.sprite = upgradeNonActiveImage;
        }
        else
        {
            upgradePunchImage.sprite = upgradeActiveImage;
        }

        if (CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].kickLevel >= 4)
        {
            upgradeKickImage.sprite = upgradeNonActiveImage;
        }
        else
        {
            upgradeKickImage.sprite = upgradeActiveImage;
        }

        if (CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].weaponLevel >= 4)
        {
            upgradeWeaponImage.sprite = upgradeNonActiveImage;
        }
        else
        {
            upgradeWeaponImage.sprite = upgradeActiveImage;
        }

        if (CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].defendLevel >= 4)
        {
            upgradeDefenseImage.sprite = upgradeNonActiveImage;
        }
        else
        {
            upgradeDefenseImage.sprite = upgradeActiveImage;
        }

        if (CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].specialSkillLevel >= 4)
        {
            upgradeSpecialSkillImage.sprite = upgradeNonActiveImage;
        }
        else
        {
            upgradeSpecialSkillImage.sprite = upgradeActiveImage;
        }
    }

    void PrevFunction()
    {
        if (previewNumber - 1 < 0)
        {
            previewNumber = characterListCardsFiltered.Count - 1;
        }
        else
        {
            previewNumber -= 1;
        }

        InitData();
    }

    void InitData()
    {
        characterImage.sprite = CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].data.characterSprite;
        characterNameText.text = CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].data.characterName;

        punchLevel = CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].punchLevel;
        kickLevel = CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].kickLevel;
        weaponLevel = CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].weaponLevel;
        defenseLevel = CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].defendLevel;
        specialSkillLevel = CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].specialSkillLevel;

        foreach (Image levelImage in punchLevelImage)
        {
            levelImage.sprite = nonActiveLevelSprite;
        }

        for (int i = 0; i < punchLevel; i++)
        {
            punchLevelImage[i].sprite = activeLevelSprite;
        }

        foreach (Image levelImage in kickLevelImage)
        {
            levelImage.sprite = nonActiveLevelSprite;
        }

        for (int i = 0; i < kickLevel; i++)
        {
            kickLevelImage[i].sprite = activeLevelSprite;
        }

        foreach (Image levelImage in weaponLevelImage)
        {
            levelImage.sprite = nonActiveLevelSprite;
        }

        for (int i = 0; i < weaponLevel; i++)
        {
            weaponLevelImage[i].sprite = activeLevelSprite;
        }

        foreach (Image levelImage in defenseLevelImage)
        {
            levelImage.sprite = nonActiveLevelSprite;
        }

        for (int i = 0; i < defenseLevel; i++)
        {
            defenseLevelImage[i].sprite = activeLevelSprite;
        }

        foreach (Image levelImage in specialSkillLevelImage)
        {
            levelImage.sprite = nonActiveLevelSprite;
        }

        for (int i = 0; i < specialSkillLevel; i++)
        {
            specialSkillLevelImage[i].sprite = activeLevelSprite;
        }

        int indexType = 0;

        specialSkillUpgradeBar.SetActive(true);
        switch (CharacterManager.Manager.characterDatas[characterListCardsFiltered[previewNumber].IndexCharacter].data.characterType)
        {
            case CharacterType.Prajurit:
                indexType = 0;
                specialSkillUpgradeBar.SetActive(false);
                break;
            case CharacterType.Elite:
                indexType = 1;
                break;
            case CharacterType.Epic:
                indexType = 2;
                break;
            case CharacterType.Rare:
                indexType = 3;
                break;
            case CharacterType.Legendary:
                indexType = 4;
                break;
            default:
                break;
        }

        switch (punchLevel)
        {
            case 1:
                punchPrice = CharacterManager.Manager.characterPrice[indexType].upgradePunchLevel2Price;
                punchPriceText.text = punchPrice.ToString();
                break;
            case 2:
                punchPrice = CharacterManager.Manager.characterPrice[indexType].upgradePunchLevel3Price;
                punchPriceText.text = punchPrice.ToString();
                break;
            case 3:
                punchPrice = CharacterManager.Manager.characterPrice[indexType].upgradePunchLevel4Price;
                punchPriceText.text = punchPrice.ToString();
                break;
            case 4:
                punchPrice = CharacterManager.Manager.characterPrice[indexType].upgradePunchLevel4Price;
                punchPriceText.text = punchPrice.ToString();
                break;
            default:
                punchPriceText.text = "";
                break;
        }

        switch (kickLevel)
        {
            case 1:
                kickPrice = CharacterManager.Manager.characterPrice[indexType].upgradeKickLevel2Price;
                kickPriceText.text = kickPrice.ToString();
                break;
            case 2:
                kickPrice = CharacterManager.Manager.characterPrice[indexType].upgradeKickLevel3Price;
                kickPriceText.text = kickPrice.ToString();
                break;
            case 3:
                kickPrice = CharacterManager.Manager.characterPrice[indexType].upgradeKickLevel4Price;
                kickPriceText.text = kickPrice.ToString();
                break;
            case 4:
                kickPrice = CharacterManager.Manager.characterPrice[indexType].upgradeKickLevel4Price;
                kickPriceText.text = kickPrice.ToString();
                break;
            default:
                kickPriceText.text = "";
                break;
        }

        switch (weaponLevel)
        {
            case 1:
                weaponPrice = CharacterManager.Manager.characterPrice[indexType].upgradeWeaponLevel2Price;
                weaponPriceText.text = weaponPrice.ToString();
                break;
            case 2:
                weaponPrice = CharacterManager.Manager.characterPrice[indexType].upgradeWeaponLevel3Price;
                weaponPriceText.text = weaponPrice.ToString();
                break;
            case 3:
                weaponPrice = CharacterManager.Manager.characterPrice[indexType].upgradeWeaponLevel4Price;
                weaponPriceText.text = weaponPrice.ToString();
                break;
            case 4:
                weaponPrice = CharacterManager.Manager.characterPrice[indexType].upgradeWeaponLevel4Price;
                weaponPriceText.text = weaponPrice.ToString();
                break;
            default:
                weaponPriceText.text = "";
                break;
        }

        switch (defenseLevel)
        {
            case 1:
                defensePrice = CharacterManager.Manager.characterPrice[indexType].upgradeDefenseLevel2Price;
                defensePriceText.text = defensePrice.ToString();
                break;
            case 2:
                defensePrice = CharacterManager.Manager.characterPrice[indexType].upgradeDefenseLevel3Price;
                defensePriceText.text = defensePrice.ToString();
                break;
            case 3:
                defensePrice = CharacterManager.Manager.characterPrice[indexType].upgradeDefenseLevel4Price;
                defensePriceText.text = defensePrice.ToString();
                break;
            case 4:
                defensePrice = CharacterManager.Manager.characterPrice[indexType].upgradeDefenseLevel4Price;
                defensePriceText.text = defensePrice.ToString();
                break;
            default:
                defensePriceText.text = "";
                break;
        }

        switch (specialSkillLevel)
        {
            case 1:
                specialSkillPrice = CharacterManager.Manager.characterPrice[indexType].upgradeSpecialSkillLevel2Price;
                specialSkillPriceText.text = specialSkillPrice.ToString();
                break;
            case 2:
                specialSkillPrice = CharacterManager.Manager.characterPrice[indexType].upgradeSpecialSkillLevel3Price;
                specialSkillPriceText.text = specialSkillPrice.ToString();
                break;
            case 3:
                specialSkillPrice = CharacterManager.Manager.characterPrice[indexType].upgradeSpecialSkillLevel4Price;
                specialSkillPriceText.text = specialSkillPrice.ToString();
                break;
            case 4:
                specialSkillPrice = CharacterManager.Manager.characterPrice[indexType].upgradeSpecialSkillLevel4Price;
                specialSkillPriceText.text = specialSkillPrice.ToString();
                break;
            default:
                specialSkillPriceText.text = "";
                break;
        }

        SetAddImage();

        if (CharacterManager.Manager.characterDatas[previewNumber].owned)
        {
            characterImage.color = Color.white;

            upgradePunchImage.sprite = upgradeActiveImage;
            upgradeKickImage.sprite = upgradeActiveImage;
            upgradeWeaponImage.sprite = upgradeActiveImage;
            upgradeDefenseImage.sprite = upgradeActiveImage;
            upgradeSpecialSkillImage.sprite = upgradeActiveImage;

            upgradePunchButton.interactable = true;
            upgradeKickButton.interactable = true;
            upgradeWeaponButton.interactable = true;
            upgradeDefenseButton.interactable = true;
            upgradeSpecialSkillButton.interactable = true;
        }
        else
        {
            characterImage.color = Color.black;

            upgradePunchImage.sprite = upgradeNonActiveImage;
            upgradeKickImage.sprite = upgradeNonActiveImage;
            upgradeWeaponImage.sprite = upgradeNonActiveImage;
            upgradeDefenseImage.sprite = upgradeNonActiveImage;
            upgradeSpecialSkillImage.sprite = upgradeNonActiveImage;

            upgradePunchButton.interactable = false;
            upgradeKickButton.interactable = false;
            upgradeWeaponButton.interactable = false;
            upgradeDefenseButton.interactable = false;
            upgradeSpecialSkillButton.interactable = false;
        }
    }

    void NextFunction()
    {
        if (previewNumber + 1 < characterListCardsFiltered.Count - 1)
        {
            previewNumber += 1;
        }
        else
        {
            previewNumber = 0;
        }

        InitData();
    }

    public void SetBlank()
    {
        characterImage.sprite = null;
        characterNameText.text = " - ";

        foreach (Image levelImage in punchLevelImage)
        {
            levelImage.sprite = nonActiveLevelSprite;
        }

        foreach (Image levelImage in kickLevelImage)
        {
            levelImage.sprite = nonActiveLevelSprite;
        }

        foreach (Image levelImage in weaponLevelImage)
        {
            levelImage.sprite = nonActiveLevelSprite;
        }

        foreach (Image levelImage in defenseLevelImage)
        {
            levelImage.sprite = nonActiveLevelSprite;
        }

        foreach (Image levelImage in specialSkillLevelImage)
        {
            levelImage.sprite = nonActiveLevelSprite;
        }

        punchPriceText.text = "";
        kickPriceText.text = "";
        weaponPriceText.text = "";
        defensePriceText.text = "";
        specialSkillPriceText.text = "";
    }
}