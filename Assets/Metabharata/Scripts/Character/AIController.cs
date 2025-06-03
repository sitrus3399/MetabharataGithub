using System.Collections;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] private CharacterBase characterBase;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private bool isLeft;
    [SerializeField] private float cooldownBehaviour;
    [SerializeField] private bool isMove;
    [SerializeField] private bool isMoveLeft;
    private int randomAIIndex;

    private AIState aiState ;

    void Update()
    {
        if (GameplayManager.Manager == null) { return; }
        if (GameplayManager.Manager.VideoOn){ return; }
        if (GameplayManager.Manager.GameEnd) { return; }

        if (!characterBase.isAI) { return; }
        
        if (characterController.CurrentState == CharacterState.Dead || characterBase.isDead) { return; }

        if (characterController.CurrentState == CharacterState.GetHit || characterController.CurrentState == CharacterState.Knock) { return; }

        if (characterController.IsGrounded)
        {
            if (characterController.CurrentState == CharacterState.Idle)
            {
                characterController.Steady();
            }
        }

        if (cooldownBehaviour > 0)
        {
            cooldownBehaviour -= Time.deltaTime;
        }
        else if (cooldownBehaviour <= 0)
        {
            SetRandomBehaviour();
        }

        if (isMove)
        {
            if (isMoveLeft)
            {
                Move(-1);
            }
            else
            {
                Move(1);
            }
        }

        //characterController.TransformLegend(); //Weapon Attack
    }

    void SetRandomBehaviour()
    {
        characterController.Steady();

        isMove = false;

        randomAIIndex = Random.Range(0, System.Enum.GetValues(typeof(AIState)).Length);

        float direction = characterBase.Target.transform.position.x - transform.position.x;

        characterController.SetColliderSize(CharacterState.Idle);

        switch (randomAIIndex)
        {
            case 0: //Jump Up
                Jump();
                cooldownBehaviour += 1.5f;
                break;
            case 1: //Jump Left
                JumpMove(Vector2.left);
                cooldownBehaviour += 1.5f;
                break;
            case 2: //Jump Right
                JumpMove(Vector2.right);
                cooldownBehaviour += 1.5f;
                break;
            case 3: //Move Left
                isMove = true;
                isMoveLeft = true;

                if (direction < 0 && direction > -5)
                {
                    Debug.Log("Terlalu dekat kiri");
                    cooldownBehaviour += 0.1f;
                }
                else 
                {
                    Move(-1);
                    cooldownBehaviour += 1.5f;
                }
                break;
            case 4: //Move Right
                isMove = true;
                isMoveLeft = false;

                if (direction > 0 && direction < 5)
                {
                    Debug.Log("Terlalu dekat kanan");
                    cooldownBehaviour += 0.1f;
                }
                else
                {
                    Move(1);
                    cooldownBehaviour += 1.5f;
                }

                break;
            case 5: //Punch
                Punch();
                cooldownBehaviour += 1f;
                break;
            case 6: //Kick
                Kick();
                cooldownBehaviour += 1f;
                break;
            case 7: //SpecialAttack
                if (characterBase.SkillPoint >= 3)
                {
                    SpecialAttack();
                    cooldownBehaviour += 1f;
                }
                else 
                {
                    cooldownBehaviour += 0.2f;
                }
                break;
            case 8: //WeaponAttack
                int randomChanceUsingWeapon = Random.Range(0, 3);

                if (randomChanceUsingWeapon == 1 && characterBase.SkillPoint >= 1)
                {
                    WeaponAttack();
                    cooldownBehaviour += 1f;
                }
                else
                {
                    cooldownBehaviour += 0.2f;
                }
                break;
            case 9: //TransformLegend
                if (characterBase.CharacterType == CharacterType.Legendary && characterBase.SkillPoint >= 3)
                {
                    TransformLegend();
                    cooldownBehaviour += 1f;
                }
                else
                {
                    cooldownBehaviour += 0.1f;
                }
                break;
            case 10:
                characterController.SetColliderSize(CharacterState.Crouch);
                Crouch();
                cooldownBehaviour += 1f;
                break;
            case 11:
                if (characterController.CanRoll)
                {
                    characterController.SetColliderSize(CharacterState.Crouch);
                    RollMove(Vector2.left);
                    cooldownBehaviour += 0.8f;
                }
                else
                {
                    cooldownBehaviour += 0.1f;
                }
                break;
            case 12:
                if (characterController.CanRoll)
                {
                    characterController.SetColliderSize(CharacterState.Crouch);
                    RollMove(Vector2.right);
                    cooldownBehaviour += 0.8f;
                }
                else
                {
                    cooldownBehaviour += 0.1f;
                }
                break;
            default:
                break;
        }
    }

    void Crouch()
    {
        characterController.Crouch();
    }

    void Jump()
    {
        characterController.Jump();
    }

    void JumpMove(Vector2 direction)
    {
        characterController.JumpMove(direction);
    }

    void RollMove(Vector2 direction)
    {
        characterController.RollMove(direction);
    }

    void Move(float direction)
    {
        characterController.Move(direction);
    }

    void Punch()
    {
        characterController.Punch();
    }

    void Kick()
    {
        characterController.Kick();
    }

    void SpecialAttack()
    {
        characterController.SpecialAttack();
    }

    void WeaponAttack()
    { 
        characterController.WeaponAttack();
    }

    void TransformLegend()
    { 
        characterController.TransformLegend(); 
    }
}

public enum AIState
{
    JumpUp,
    JumpLeft,
    JumpRight,
    MoveLeft,
    MoveRight,
    Punch,
    Kick,
    SpecialAttack,
    WeaponAttack,
    TransformLegend,
    Crounch,
    RollLeft,
    RollRight,
}