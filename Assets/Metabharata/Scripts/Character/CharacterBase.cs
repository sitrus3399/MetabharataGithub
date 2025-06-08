using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterBase : NetworkBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterAttack characterAttack;
    [SerializeField] private CharacterData baseData;

    [SerializeField] private int punchLevel;
    [SerializeField] private int kickLevel;
    [SerializeField] private int weaponLevel;
    [SerializeField] private int specialSkillLevel;
    [SerializeField] private int defendLevel;

    [SerializeField] private CharacterBase target;

    [SerializeField] private float maxHP;
    [SerializeField] private NetworkVariable<float> currentHP;
    [SerializeField] private float maxSkill;
    [SerializeField] private NetworkVariable<float> currentSkill;
    [SerializeField] private float maxPoint;
    [SerializeField] private float skillPoint;

    [SerializeField] private string characterName;
    [SerializeField] private string characterDescription;
    [SerializeField] private string characterQuotes;
    [SerializeField] private Sprite characterSprite;
    [SerializeField] private CharacterType characterType;
    [SerializeField] private int damagePukul;
    [SerializeField] private int damagePukulLegendary;
    [SerializeField] private int damageTendang;
    [SerializeField] private int damageTendangLegendary;
    [SerializeField] private int damageSenjata;
    [SerializeField] private int damageSenjataLegendary;
    [SerializeField] private int damageSpecialSkill;
    [SerializeField] private int damageSpecialLegendary;
    [SerializeField] private int defend;
    [SerializeField] private int defendLegendary;

    [SerializeField] private AudioSource audioSource;

    public bool isOwned; //Persiapan Multiplayer
    public bool isAI;
    public bool isDead;

    [SerializeField] private PlayerPanel playerPanel;

    public CharacterBase Target { get { return target; } }
    public CharacterType CharacterType { get { return characterType; } }

    public int PunchLevel { get { return punchLevel; } }
    public int KickLevel { get { return kickLevel; } }
    public int WeaponLevel { get { return weaponLevel; } }
    public int SpecialAttackLevel { get { return specialSkillLevel; } }
    public float SkillPoint { get { return skillPoint; } }
    public float Defend { get { return defend; } }
    public float DefendLegendary { get { return defendLegendary; } }
    public CharacterData BaseData { get { return baseData; } }

    CharacterCollect newCharacterCollect = new CharacterCollect();

    void Start()
    {
        if (IsServer)
        {
            currentHP.Value = maxHP;
            currentSkill.Value = 0;
        }
        //playerPanel = GameplayManager.Manager.GetPlayerPanel(this);

        if (GameManager.Instance.stageType == StageType.Online)
        {
            if (GameManager.Instance.characterDataOnlineHost != null && GameManager.Instance.characterDataOnlineClient != null)
            {
                if (IsHost && isOwned)
                {
                    Debug.Log($"IsHost");
                    newCharacterCollect.punchLevel = SearchCharacter(GameManager.Instance.characterDataOnlineHost).punchLevel;
                    newCharacterCollect.kickLevel = SearchCharacter(GameManager.Instance.characterDataOnlineHost).kickLevel;
                    newCharacterCollect.weaponLevel = SearchCharacter(GameManager.Instance.characterDataOnlineHost).weaponLevel;
                    newCharacterCollect.specialSkillLevel = SearchCharacter(GameManager.Instance.characterDataOnlineHost).specialSkillLevel;
                    newCharacterCollect.defendLevel = SearchCharacter(GameManager.Instance.characterDataOnlineHost).defendLevel;

                    InitPlayerPanel(GameplayManager.Manager.PlayerPanelLeft);
                    InitData(GameManager.Instance.characterDataFreeBattle1, newCharacterCollect);
                    InitSpawnLocation(GameplayManager.Manager.CharacterLeftLocation.position);
                    InitOwned(true, false);
                }
                else if (IsClient && isOwned)
                {
                    Debug.Log($"IsClient");
                    newCharacterCollect.punchLevel = SearchCharacter(GameManager.Instance.characterDataOnlineClient).punchLevel;
                    newCharacterCollect.kickLevel = SearchCharacter(GameManager.Instance.characterDataOnlineClient).kickLevel;
                    newCharacterCollect.weaponLevel = SearchCharacter(GameManager.Instance.characterDataOnlineClient).weaponLevel;
                    newCharacterCollect.specialSkillLevel = SearchCharacter(GameManager.Instance.characterDataOnlineClient).specialSkillLevel;
                    newCharacterCollect.defendLevel = SearchCharacter(GameManager.Instance.characterDataOnlineClient).defendLevel;

                    InitPlayerPanel(GameplayManager.Manager.PlayerPanelRight);
                    InitData(GameManager.Instance.characterDataFreeBattle2, newCharacterCollect);
                    InitSpawnLocation(GameplayManager.Manager.CharacterRightLocation.position);
                    InitOwned(true, false);
                }
            }
        }
    }

    CharacterCollect SearchCharacter(CharacterData tmpCharacter)
    {
        foreach (CharacterCollect charData in CharacterManager.Manager.characterDatas)
        {
            if (charData.data == tmpCharacter)
            {
                return charData;
            }
        }

        return null;
    }

    private void Update()
    {
        if (GameManager.Instance.stageType == StageType.Online)
        {
            //Debug.LogWarning($"Sementara dimatikan sampai online beres");
            return;
        }

        if (currentSkill.Value < maxSkill)
        {
            currentSkill.Value += Time.deltaTime;

            playerPanel.SetSkillBar(currentSkill.Value, maxSkill, skillPoint);
        }
        else if (currentSkill.Value >= maxSkill)
        {
            if (skillPoint < maxPoint)
            {
                skillPoint += 1;

                currentSkill.Value = 0;

                playerPanel.SetSkillBar(currentSkill.Value, maxSkill, skillPoint);
            }
        }
    }

    public void GotHit(float damage)
    {
        currentHP.Value -= damage;
        playerPanel.SetHPBar(currentHP.Value, maxHP);
        if (currentHP.Value <= 0)
        {
            isDead = true;
            characterHealth.Dead();
        }

        RequestGotHitServerRpc(currentHP.Value);
    }

    [ServerRpc]
    void RequestGotHitServerRpc(float broadcastHP)
    {
        PerformGotHitClientRpc(broadcastHP);
    }

    [ClientRpc]
    void PerformGotHitClientRpc(float broadcastHP)
    {
        if (IsOwner) return;

        //currentHP.Value = broadcastHP; Otomatis dari NetworkVariable<float>
        playerPanel.SetHPBar(currentHP.Value, maxHP);
        if (currentHP.Value <= 0)
        {
            isDead = true;
            characterHealth.Dead();
        }
    }

    public void InitData(CharacterData tmpBaseData, CharacterCollect characterCollect)
    {
        baseData = tmpBaseData;

        punchLevel = characterCollect.punchLevel;
        kickLevel = characterCollect.kickLevel;
        weaponLevel = characterCollect.weaponLevel;
        specialSkillLevel = characterCollect.specialSkillLevel;
        defendLevel = characterCollect.defendLevel;

        characterName = baseData.characterName;
        characterDescription = baseData.characterDescription;
        characterQuotes = baseData.characterQuotes;
        characterSprite = baseData.characterSprite;
        characterType = baseData.characterType;
        damagePukul = baseData.damagePukul[punchLevel - 1];
        damagePukulLegendary = baseData.damagePukulLegendary[punchLevel - 1];
        damageTendang = baseData.damageTendang[kickLevel - 1];
        damageTendangLegendary = baseData.damageTendangLegendary[kickLevel - 1];
        damageSenjata = baseData.damageSenjata[weaponLevel - 1];
        damageSenjataLegendary = baseData.damageSenjataLegendary[weaponLevel - 1];
        damageSpecialSkill = baseData.damageSpecialSkill[specialSkillLevel - 1];
        damageSpecialLegendary = baseData.damageSpecialLegendary[specialSkillLevel - 1];
        defend = baseData.defend[defendLevel - 1];
        defendLegendary = baseData.defendLegendary[defendLevel - 1];

        playerPanel.SetRestart();
        playerPanel.InitData(tmpBaseData);

        if (tmpBaseData.characterAnimator != null)
        {
            characterController.SetAnimatorController(tmpBaseData.characterAnimator);
        }
    }

    public void InitSpawnLocation(Vector3 spawnLocation)
    {
        transform.position = spawnLocation;
    }

    public void InitOwned(bool tmpIsOwned, bool tmpIsAI)
    {
        isOwned = tmpIsOwned;
        isAI = tmpIsAI;
    }

    public void InitPlayerPanel(PlayerPanel tmpPlayerPanel)
    {
        playerPanel = tmpPlayerPanel;
    }

    public void SetCharacterAttackData(CharacterBase tmpTarget)
    {
        target = tmpTarget;

        characterAttack.Init(baseData);
    }

    public void UseSkillPoint(float decreasePoint)
    {
        skillPoint -= decreasePoint;
    }

    public void SetRestart()
    {
        if (IsServer)
        {
            currentHP.Value = maxHP;
            currentSkill.Value = 0;
        }
        skillPoint = 0;

        isDead = false;

        playerPanel.SetRestart();

        characterController.Idle();

        if (baseData.characterType == CharacterType.Legendary)
        {
            characterController.InitDeTransform();
        }
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentHP.OnValueChanged += (oldValue, newValue) =>
        {
            playerPanel?.SetHPBar(newValue, maxHP);
            if (newValue <= 0)
            {
                isDead = true;
                characterHealth.Dead();
            }
        };
    }
}
