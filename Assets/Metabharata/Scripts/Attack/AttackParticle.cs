using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AttackParticle : MonoBehaviour
{
    private GameObject caster;
    private float damage;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float bulletTime;
    [SerializeField] private float currentBulletTime;
    [SerializeField] private float bulletDelayTime;
    [SerializeField] private float currentBulletDelayTime;
    [SerializeField] private Vector2 bulletPosition;

    [SerializeField] private float speed;
    [SerializeField] private Vector2 startBulletPosition;
    [SerializeField] private Vector2 direction = Vector2.right; // default arah ke kanan

    public delegate void InitParticle(GameObject caster, float damge, float bulletTime, Vector2 bulletPosition);
    public event InitParticle initSubParticle;

    public delegate void ShowParticle(bool isFlip);
    public event ShowParticle showParticle;

    public bool isSpecialAttack;

    private Rigidbody2D rb;

    public void SetSpecialAttack()
    {
        isSpecialAttack = true;
    }

    public void InvokeSubParticle()
    {
        initSubParticle?.Invoke(caster, damage, bulletTime, bulletPosition);
    }

    private void Update()
    {
        if (currentBulletDelayTime > 0)
        {
            currentBulletDelayTime -= Time.deltaTime;
        }
        else
        {
            if (currentBulletTime > 0)
            {
                rb.linearVelocity = direction.normalized * speed;
                currentBulletTime -= Time.deltaTime;
            }
            if (currentBulletTime <= 0)
            {
                transform.localPosition = Vector3.zero;
                gameObject.SetActive(false);
            }
        }
    }

    public void Show(bool isFlip)
    {
        gameObject.SetActive(true);

        if (isFlip)
        {
            spriteRenderer.flipX = true;
            direction = Vector2.left;
            transform.localPosition = new Vector2(-bulletPosition.x, bulletPosition.y);
            
            showParticle?.Invoke(isFlip);

            if (showParticle == null)
            {
                StartCoroutine(ShowInvoke(isFlip));
            }
        }
        else
        {
            spriteRenderer.flipX = false;
            direction = Vector2.right;
            transform.localPosition = bulletPosition;
            showParticle?.Invoke(isFlip);

            if (showParticle == null)
            {
                StartCoroutine(ShowInvoke(isFlip));
            }
        }

        currentBulletTime = bulletTime;
        currentBulletDelayTime = bulletDelayTime;
    }

    IEnumerator ShowInvoke(bool isFlip)
    {
        yield return new WaitForSeconds(0.1f);

        showParticle?.Invoke(isFlip);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.gameObject != caster)
            {
                CharacterHealth characterHealth = collision.GetComponent<CharacterHealth>();

                characterHealth.GotHit(damage, isSpecialAttack);
            }
        }
    }
}
