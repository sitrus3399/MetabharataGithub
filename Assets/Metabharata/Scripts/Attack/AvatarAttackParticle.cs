using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AvatarAttackParticle : MonoBehaviour
{
    [SerializeField] BaseAttackParticle baseAttackParticle;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float bulletTime;
    [SerializeField] private float currentBulletTime;
    [SerializeField] private float bulletDelayTime;
    [SerializeField] private float currentBulletDelayTime;

    [SerializeField] private Animator animator;
    [SerializeField] private Vector2 direction = Vector2.right; // default arah ke kanan

    void Start()
    {
        baseAttackParticle.showParticle += Show;
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
        }
        else
        {
            spriteRenderer.flipX = false;
            direction = Vector2.right;
        }

        currentBulletTime = bulletTime;
        currentBulletDelayTime = bulletDelayTime;

        StartCoroutine(PlayAnimator());
    }

    IEnumerator PlayAnimator()
    {
        yield return new WaitForSeconds(currentBulletDelayTime);
        animator.SetTrigger("Play");
    }
}
