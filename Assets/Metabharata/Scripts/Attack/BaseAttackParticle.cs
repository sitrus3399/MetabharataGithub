using System.Collections;
using UnityEngine;

public class BaseAttackParticle : MonoBehaviour
{
    [Header("Base Data")]

    private GameObject caster;
    private GameObject target;
    private float damage;
    [SerializeField] private float bulletTime;
    [SerializeField] private float currentBulletTime;
    [SerializeField] private float bulletDelayTime;
    [SerializeField] private float currentBulletDelayTime;
    [SerializeField] private Vector2 bulletPosition;

    public delegate void InitParticle(GameObject caster, GameObject target, float damge, float bulletTime, Vector2 bulletPosition);
    public event InitParticle initBulletParticle;

    public delegate void ShowParticle(bool isFlip);
    public event ShowParticle showParticle;

    public bool isSpecialAttack;

    public void InitData(GameObject tmpCaster, GameObject tmpTarget, float tmpDamge, Vector2 tmpBulletPosition)
    {
        caster = tmpCaster;
        target = tmpTarget;
        damage = tmpDamge;
        currentBulletTime = bulletTime;
        currentBulletDelayTime = bulletDelayTime;
        bulletPosition = tmpBulletPosition;
    }

    void Start()
    {
        
    }

    public void Show(bool isFlip)
    {
        gameObject.SetActive(true);

        if (isFlip)
        {
            transform.localPosition = new Vector2(-bulletPosition.x, bulletPosition.y);

            showParticle?.Invoke(isFlip);

            if (showParticle == null)
            {
                StartCoroutine(ShowInvoke(isFlip));
            }
        }
        else
        {
            transform.localPosition = bulletPosition;
            showParticle?.Invoke(isFlip);

            if (showParticle == null)
            {
                StartCoroutine(ShowInvoke(isFlip));
            }
        }
    }

    IEnumerator ShowInvoke(bool isFlip)
    {
        yield return new WaitForSeconds(0.1f);

        showParticle?.Invoke(isFlip);
    }

    public void InvokeSubParticle()
    {
        initBulletParticle?.Invoke(caster, target, damage, bulletTime, bulletPosition);
    }

    public void SetSpecialAttack()
    {
        isSpecialAttack = true;
    }
}
