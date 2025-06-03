using UnityEngine;

public class SubAttackParticle : MonoBehaviour
{
    [SerializeField] AttackParticle attackParticle;
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

    public bool isSpecialAttack;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        attackParticle.initSubParticle += InitData;
        attackParticle.InvokeSubParticle();

        attackParticle.showParticle += Show;
    }

    public void InitData(GameObject tmpCaster, float tmpDamge, float tmpBulletTime, Vector2 tmpBulletPosition)
    {
        caster = tmpCaster;
        damage = tmpDamge;
        currentBulletTime = bulletTime;
        currentBulletDelayTime = bulletDelayTime;
    }

    void Update()
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
            transform.localPosition = bulletPosition;
        }
        else
        {
            spriteRenderer.flipX = false;
            direction = Vector2.right;
            transform.localPosition = new Vector2(-bulletPosition.x, bulletPosition.y);
        }

        currentBulletTime = bulletTime;
        currentBulletDelayTime = bulletDelayTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.gameObject != caster)
            {
                Debug.Log($"Got Hit Enemy");
                CharacterHealth characterHealth = collision.GetComponent<CharacterHealth>();

                characterHealth.GotHit(damage, isSpecialAttack);
            }
        }
    }
}
