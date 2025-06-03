using Unity.Netcode;
using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CharacterBase characterBase;

    [SerializeField] private Vector2 punchHitBoxLocation;
    [SerializeField] private Vector2 punchComboHitBoxLocation;
    [SerializeField] private Vector2 kickHitBoxLocation;
    [SerializeField] private Vector2 weaponHitBoxLocation;
    [SerializeField] private Vector2 specialAttackHitBoxLocation;

    [SerializeField] private BaseAttackParticle punchBullet;
    [SerializeField] private BaseAttackParticle punchBulletLegendary;
    [SerializeField] private BaseAttackParticle punchComboBullet;
    [SerializeField] private BaseAttackParticle punchComboBulletLegendary;
    [SerializeField] private BaseAttackParticle kickBullet;
    [SerializeField] private BaseAttackParticle kickBulletLegendary;
    [SerializeField] private BaseAttackParticle weaponBullet;
    [SerializeField] private BaseAttackParticle weaponBulletLegendary;
    [SerializeField] private BaseAttackParticle specialAttackBullet;
    [SerializeField] private BaseAttackParticle specialAttackBulletLegendary;

    public void Init(CharacterData tmpData)
    {
        GameObject target = characterBase.Target.gameObject;

        punchHitBoxLocation = tmpData.punchHitBoxLocation;
        punchComboHitBoxLocation = tmpData.punchComboHitBoxLocation;
        kickHitBoxLocation = tmpData.kickHitBoxLocation;
        weaponHitBoxLocation = tmpData.weaponHitBoxLocation;
        specialAttackHitBoxLocation = tmpData.specialAttackHitBoxLocation;

        if (GameManager.Instance.stageType == StageType.Online)
        {
            if (punchBullet != null) { punchBullet.GetComponent<NetworkObject>().Despawn(); ; punchBullet = null; }
            if (punchComboBullet != null) { punchComboBullet.GetComponent<NetworkObject>().Despawn(); punchComboBullet = null; }
            if (kickBullet != null) { kickBullet.GetComponent<NetworkObject>().Despawn(); kickBullet = null; }
            if (weaponBullet != null) { weaponBullet.GetComponent<NetworkObject>().Despawn(); weaponBullet = null; }
            if (specialAttackBullet != null) { specialAttackBullet.GetComponent<NetworkObject>().Despawn(); specialAttackBullet = null; }

            if (punchBulletLegendary != null) { punchBulletLegendary.GetComponent<NetworkObject>().Despawn(); punchBulletLegendary = null; }
            if (punchComboBulletLegendary != null) { punchComboBulletLegendary.GetComponent<NetworkObject>().Despawn(); punchComboBulletLegendary = null; }
            if (kickBulletLegendary != null) { kickBulletLegendary.GetComponent<NetworkObject>().Despawn(); kickBulletLegendary = null; }
            if (weaponBulletLegendary != null) { weaponBulletLegendary.GetComponent<NetworkObject>().Despawn(); weaponBulletLegendary = null; }
            if (specialAttackBulletLegendary != null) { specialAttackBulletLegendary.GetComponent<NetworkObject>().Despawn(); specialAttackBulletLegendary = null; }

            punchBullet = Instantiate(tmpData.punchBullet, this.gameObject.transform);
            punchBullet.GetComponent<NetworkObject>().Spawn(true);
            punchBullet.gameObject.SetActive(false);

            punchComboBullet = Instantiate(tmpData.punchComboBullet, this.gameObject.transform);
            punchComboBullet.GetComponent<NetworkObject>().Spawn(true);
            punchComboBullet.gameObject.SetActive(false);

            kickBullet = Instantiate(tmpData.kickBullet, this.gameObject.transform);
            kickBullet.GetComponent<NetworkObject>().Spawn(true);
            kickBullet.gameObject.SetActive(false);

            weaponBullet = Instantiate(tmpData.weaponBullet, this.gameObject.transform);
            weaponBullet.GetComponent<NetworkObject>().Spawn(true);
            weaponBullet.gameObject.SetActive(false);

            if (characterBase.BaseData.characterType != CharacterType.Prajurit)
            {
                specialAttackBullet = Instantiate(tmpData.specialAttackBullet, this.gameObject.transform);
                specialAttackBullet.GetComponent<NetworkObject>().Spawn(true);
                specialAttackBullet.gameObject.SetActive(false);
            }
        }
        else
        {
            if (punchBullet != null) { Destroy(punchBullet.gameObject); punchBullet = null; }
            if (punchComboBullet != null) { Destroy(punchComboBullet.gameObject); punchComboBullet = null; }
            if (kickBullet != null) { Destroy(kickBullet.gameObject); kickBullet = null; }
            if (weaponBullet != null) { Destroy(weaponBullet.gameObject); weaponBullet = null; }
            if (specialAttackBullet != null) { Destroy(specialAttackBullet.gameObject); specialAttackBullet = null; }

            if (punchBulletLegendary != null) { Destroy(punchBulletLegendary.gameObject); punchBulletLegendary = null; }
            if (punchComboBulletLegendary != null) { Destroy(punchComboBulletLegendary.gameObject); punchComboBulletLegendary = null; }
            if (kickBulletLegendary != null) { Destroy(kickBulletLegendary.gameObject); kickBulletLegendary = null; }
            if (weaponBulletLegendary != null) { Destroy(weaponBulletLegendary.gameObject); weaponBulletLegendary = null; }
            if (specialAttackBulletLegendary != null) { Destroy(specialAttackBulletLegendary.gameObject); specialAttackBulletLegendary = null; }

            punchBullet = Instantiate(tmpData.punchBullet, this.gameObject.transform);
            punchBullet.gameObject.SetActive(false);
            punchComboBullet = Instantiate(tmpData.punchComboBullet, this.gameObject.transform);
            punchComboBullet.gameObject.SetActive(false);
            kickBullet = Instantiate(tmpData.kickBullet, this.gameObject.transform);
            kickBullet.gameObject.SetActive(false);
            weaponBullet = Instantiate(tmpData.weaponBullet, this.gameObject.transform);
            weaponBullet.gameObject.SetActive(false);

            if (characterBase.BaseData.characterType != CharacterType.Prajurit)
            {
                specialAttackBullet = Instantiate(tmpData.specialAttackBullet, this.gameObject.transform);
                specialAttackBullet.gameObject.SetActive(false);
            }
        }
        
        punchBullet.InitData(this.gameObject, target, tmpData.damagePukul[characterBase.PunchLevel], punchHitBoxLocation);
        punchComboBullet.InitData(this.gameObject, target, tmpData.damagePukul[characterBase.PunchLevel],  punchComboHitBoxLocation);
        kickBullet.InitData(this.gameObject, target, tmpData.damageTendang[characterBase.KickLevel],  kickHitBoxLocation);
        weaponBullet.InitData(this.gameObject, target, tmpData.damageSenjata[characterBase.KickLevel], weaponHitBoxLocation);

        if (characterBase.BaseData.characterType != CharacterType.Prajurit)
        {
            specialAttackBullet.InitData(this.gameObject, target, tmpData.damageSpecialSkill[characterBase.KickLevel], specialAttackHitBoxLocation);
            specialAttackBullet.SetSpecialAttack();
        }

        if (characterBase.BaseData.characterType == CharacterType.Legendary)
        {
            if (GameManager.Instance.stageType == StageType.Online)
            {
                punchBulletLegendary = Instantiate(tmpData.punchBulletLegendary, this.gameObject.transform);
                punchBulletLegendary.GetComponent<NetworkObject>().Spawn(true);
                punchBulletLegendary.gameObject.SetActive(false);

                punchComboBulletLegendary = Instantiate(tmpData.punchComboBulletLegendary, this.gameObject.transform);
                punchComboBulletLegendary.GetComponent<NetworkObject>().Spawn(true);
                punchComboBulletLegendary.gameObject.SetActive(false);
                
                kickBulletLegendary = Instantiate(tmpData.kickBulletLegendary, this.gameObject.transform);
                kickBulletLegendary.GetComponent<NetworkObject>().Spawn(true);
                kickBulletLegendary.gameObject.SetActive(false);

                weaponBulletLegendary = Instantiate(tmpData.weaponBulletLegendary, this.gameObject.transform);
                weaponBulletLegendary.GetComponent<NetworkObject>().Spawn(true);
                weaponBulletLegendary.gameObject.SetActive(false);

                specialAttackBulletLegendary = Instantiate(tmpData.specialAttackBulletLegendary, this.gameObject.transform);
                specialAttackBulletLegendary.GetComponent<NetworkObject>().Spawn(true);
                specialAttackBulletLegendary.gameObject.SetActive(false);
            }
            else
            {
                punchBulletLegendary = Instantiate(tmpData.punchBulletLegendary, this.gameObject.transform);
                punchBulletLegendary.gameObject.SetActive(false);
                punchComboBulletLegendary = Instantiate(tmpData.punchComboBulletLegendary, this.gameObject.transform);
                punchComboBulletLegendary.gameObject.SetActive(false);
                kickBulletLegendary = Instantiate(tmpData.kickBulletLegendary, this.gameObject.transform);
                kickBulletLegendary.gameObject.SetActive(false);
                weaponBulletLegendary = Instantiate(tmpData.weaponBulletLegendary, this.gameObject.transform);
                weaponBulletLegendary.gameObject.SetActive(false);
                specialAttackBulletLegendary = Instantiate(tmpData.specialAttackBulletLegendary, this.gameObject.transform);
                specialAttackBulletLegendary.gameObject.SetActive(false);
            }
            
            punchBulletLegendary.InitData(this.gameObject, target, tmpData.damagePukulLegendary[characterBase.PunchLevel], punchHitBoxLocation);
            punchComboBulletLegendary.InitData(this.gameObject, target, tmpData.damagePukulLegendary[characterBase.PunchLevel], punchComboHitBoxLocation);
            kickBulletLegendary.InitData(this.gameObject, target, tmpData.damageTendangLegendary[characterBase.KickLevel], kickHitBoxLocation);
            weaponBulletLegendary.InitData(this.gameObject, target, tmpData.damageSenjataLegendary[characterBase.WeaponLevel], weaponHitBoxLocation);
            specialAttackBulletLegendary.InitData(this.gameObject, target, tmpData.damageSpecialLegendary[characterBase.SpecialAttackLevel], specialAttackHitBoxLocation);
            specialAttackBulletLegendary.SetSpecialAttack();
        }
    }

    public void Punch()
    {
        punchBullet.gameObject.SetActive(false);

        if (characterBase.BaseData.characterType == CharacterType.Legendary)
            punchBulletLegendary.gameObject.SetActive(false);

        bool isFlip = characterController.IsTargetOnLeft;

        if (characterController.IsTransform)
        {
            punchBulletLegendary.Show(isFlip);
        }
        else
        {
            punchBullet.Show(isFlip);
        }
        
    }

    public void PunchCombo()
    {
        punchComboBullet.gameObject.SetActive(false);

        if (characterBase.BaseData.characterType == CharacterType.Legendary)
            punchComboBulletLegendary.gameObject.SetActive(false);

        bool isFlip = characterController.IsTargetOnLeft;

        if (characterController.IsTransform)
        {
            punchComboBulletLegendary.Show(isFlip);
        }
        else
        {
            punchComboBullet.Show(isFlip);
        }
    }

    public void Kick()
    {
        kickBullet.gameObject.SetActive(false);

        if (characterBase.BaseData.characterType == CharacterType.Legendary)
            kickBulletLegendary.gameObject.SetActive(false);

        bool isFlip = characterController.IsTargetOnLeft;

        if (characterController.IsTransform)
        {
            kickBulletLegendary.Show(isFlip);
        }
        else
        {
            kickBullet.Show(isFlip);
        }
    }

    public void WeaponAttack()
    {
        weaponBullet.gameObject.SetActive(false);

        if (characterBase.BaseData.characterType == CharacterType.Legendary)
            weaponBulletLegendary.gameObject.SetActive(false);

        bool isFlip = characterController.IsTargetOnLeft;

        if (characterController.IsTransform)
        {
            weaponBulletLegendary.Show(isFlip);

            if (characterBase.BaseData.weaponLegendarySound != null)
            {
                characterBase.PlaySound(characterBase.BaseData.weaponLegendarySound);
            }
        }
        else
        {
            weaponBullet.Show(isFlip);

            if (characterBase.BaseData.weaponSound != null)
            {
                characterBase.PlaySound(characterBase.BaseData.weaponSound);
            }
        }
    }

    public void SpecialAttack()
    {
        specialAttackBullet.gameObject.SetActive(false);

        if (characterBase.BaseData.characterType == CharacterType.Legendary)
            specialAttackBulletLegendary.gameObject.SetActive(false);

        bool isFlip = characterController.IsTargetOnLeft;

        if (characterController.IsTransform)
        {
            specialAttackBulletLegendary.Show(isFlip);

            if (characterBase.BaseData.specialLegendarySound != null)
            {
                characterBase.PlaySound(characterBase.BaseData.specialLegendarySound);
            }
        }
        else
        {
            specialAttackBullet.Show(isFlip);

            if (characterBase.BaseData.specialSound != null)
            {
                characterBase.PlaySound(characterBase.BaseData.specialSound);
            }
        }
    }
}
