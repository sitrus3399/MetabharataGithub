using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Data/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public string characterNickName;
    public string characterDescription;
    public string characterQuotes;
    public Sprite characterSprite;
    public Sprite characterLegendarySprite;
    public Sprite characterPortrait;
    public VideoClip characterVideo;
    public VideoClip characterVideoLegendary;
    public AnimatorOverrideController characterAnimator;
    public AnimatorOverrideController characterLegendaryAnimator;
    public AnimatorOverrideController previewAnimator;
    public CharacterType characterType;
    public List<int> damagePukul;
    public List<int> damagePukulLegendary;
    public List<int> damageTendang;
    public List<int> damageTendangLegendary;
    public List<int> damageSenjata;
    public List<int> damageSenjataLegendary;
    public List<int> damageSpecialSkill;
    public List<int> damageSpecialLegendary;
    public List<int> defend;
    public List<int> defendLegendary;

    public Vector2 punchHitBoxLocation;
    public Vector2 punchComboHitBoxLocation;
    public Vector2 kickHitBoxLocation;
    public Vector2 weaponHitBoxLocation;
    public Vector2 specialAttackHitBoxLocation;

    public BaseAttackParticle punchBullet;
    //public AudioClip punchSound;
    public BaseAttackParticle punchBulletLegendary;
    //public AudioClip punchLegendarySound;
    public BaseAttackParticle punchComboBullet;
    //public AudioClip punchComboSound;
    public BaseAttackParticle punchComboBulletLegendary;
    //public AudioClip punchComboLegendarySound;
    public BaseAttackParticle kickBullet;
    //public AudioClip kickSound;
    public BaseAttackParticle kickBulletLegendary;
    //public AudioClip kickLegendarySound;
    public BaseAttackParticle weaponBullet;
    public AudioClip weaponSound;
    public BaseAttackParticle weaponBulletLegendary;
    public AudioClip weaponLegendarySound;
    public BaseAttackParticle specialAttackBullet;
    public AudioClip specialSound;
    public BaseAttackParticle specialAttackBulletLegendary;
    public AudioClip specialLegendarySound;

    public int hargaBeli;
    public List<int> hargaEquip;
}