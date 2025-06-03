using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private CharacterBase characterBase;
    [SerializeField] private CharacterController characterController;

    public void GotHit(float damage, bool isKnock)
    {
        if (characterBase.isDead == false)
        {
            float fixDamage = 0;

            if (characterController.IsTransform)
            {
                fixDamage = damage * (1 - (characterBase.DefendLegendary / 100));
            }
            else
            {
                fixDamage = damage * (1 - (characterBase.Defend / 100));
            }

            characterBase.GotHit(fixDamage);
            characterController.GetHit(isKnock);
        }
    }

    public void Dead()
    {
        characterController.Dead();
    }
}
