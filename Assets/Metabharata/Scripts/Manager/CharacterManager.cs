using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Manager;
    public List<CharacterCollect> characterDatas;
    public List<CharacterPrice> characterPrice;

    private void Awake()
    {
        if (Manager == null)
        {
            Manager = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}

[System.Serializable]
public class CharacterCollect
{
    public string nama;

    public CharacterData data;

    [Header("Upgrade")]
    public int characterLevel;
    public int punchLevel;
    public int kickLevel;
    public int weaponLevel;
    public int specialSkillLevel;
    public int defendLevel;

    [Header("Owned")]
    public bool owned;
}

[System.Serializable]
public class CharacterPrice
{
    public CharacterType characterType;

    [Header("Buy")]
    public float buyPrice;

    [Header("Buy")]
    public float upgradePunchLevel2Price;
    public float upgradePunchLevel3Price;
    public float upgradePunchLevel4Price;

    public float upgradeKickLevel2Price;
    public float upgradeKickLevel3Price;
    public float upgradeKickLevel4Price;

    public float upgradeWeaponLevel2Price;
    public float upgradeWeaponLevel3Price;
    public float upgradeWeaponLevel4Price;

    public float upgradeDefenseLevel2Price;
    public float upgradeDefenseLevel3Price;
    public float upgradeDefenseLevel4Price;

    public float upgradeSpecialSkillLevel2Price;
    public float upgradeSpecialSkillLevel3Price;
    public float upgradeSpecialSkillLevel4Price;
}

public enum CharacterType
{
    Prajurit,
    Elite,
    Epic,
    Rare,
    Legendary
}