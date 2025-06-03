using UnityEngine;

public class BulletAttackParticle : MonoBehaviour
{
    [SerializeField] BaseAttackParticle baseAttackParticle;
    private GameObject caster;
    private GameObject target;
    private float damage;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animatorController;
    [SerializeField] private float bulletTime;
    [SerializeField] private float currentBulletTime;
    [SerializeField] private float bulletDelayTime;
    [SerializeField] private float currentBulletDelayTime;
    [SerializeField] private Vector2 bulletPosition;

    [SerializeField] private float speed;
    [SerializeField] private Vector2 startBulletPosition;
    [SerializeField] private Vector2 direction = Vector2.right; // default arah ke kanan
    
    public bool isSpecialAttack;
    [SerializeField] private float specialAttackRange;
    public bool isAnimated;

    private Rigidbody2D rb;

    void Start()
    {
        animatorController = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();

        if (baseAttackParticle == null)
        {
            baseAttackParticle = GetComponentInParent<BaseAttackParticle>();
        }

        baseAttackParticle.initBulletParticle += InitData;
        baseAttackParticle.InvokeSubParticle();

        baseAttackParticle.showParticle += Show;
    }

    public void InitData(GameObject tmpCaster, GameObject tmpTarget, float tmpDamge, float tmpBulletTime, Vector2 tmpBulletPosition)
    {
        caster = tmpCaster;
        target = tmpTarget;
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
        if (isFlip)
        {
            transform.rotation = new Quaternion(0,180,0,0);
            //spriteRenderer.flipX = true;
            direction = Vector2.left;

            if (isSpecialAttack)
            {
                bulletPosition = new Vector3(target.transform.position.x + specialAttackRange, caster.transform.position.y, 0);
                transform.position = bulletPosition;
            }
            else
            {
                transform.localPosition = bulletPosition;
            }
            
        }
        else
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
            //spriteRenderer.flipX = false;
            direction = Vector2.right;

            if (isSpecialAttack)
            {
                bulletPosition = new Vector3(target.transform.position.x - specialAttackRange, caster.transform.position.y , 0);
                transform.position = bulletPosition;
            }
            else
            {
                transform.localPosition = new Vector2(-bulletPosition.x, bulletPosition.y);
            }
        }

        gameObject.SetActive(true);

        currentBulletTime = bulletTime;
        currentBulletDelayTime = bulletDelayTime;

        Invoke("PlayAnimate", currentBulletDelayTime);
    }

    void PlayAnimate()
    {
        if (isAnimated && animatorController != null)
        {
            animatorController.SetTrigger("Play");
        }
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
