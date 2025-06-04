using MetabharataAudio;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterController : NetworkBehaviour
{
    [SerializeField] private CharacterBase characterBase;
    [SerializeField] private CharacterAttack characterAttack;

    private CharacterState currentState = CharacterState.Idle;
    private StageType stageType;

    public CharacterUIController controllerPrefab;
    
    private CharacterUIController controller;
    public Joystick joystick;

    private CapsuleCollider2D capsuleCollider2D;
    [SerializeField] private float colliderSizeYCrouch;
    [SerializeField] private float colliderOffsideYCrouch;
    [SerializeField] private float colliderSizeYIdle;
    [SerializeField] private float colliderOffsideYIdle;

    public float speed = 5f;
    public float jumpForce = 7f;
    //public float rollDistance = 3f;
    public float jumpDistance = 3f;
    public float rollSpeed = 8f;
    public float rollCooldown = 1f;
    public float decelerationRate = 5f;

    private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    private bool isGrounded;
    private bool facingRight = true;
    private bool targetOnLeft;
    private bool canComboPunch = false;
    private bool canComboKick = false;
    private bool canRoll = true;

    [Header("Transform")]
    private bool isTransform = false;
    [SerializeField] private float transformTime;
    private float currentTransformTime;
    [SerializeField] private Animator transformLegendaryParticle;

    public CharacterState CurrentState { get { return currentState; } }
    public bool IsGrounded { get { return isGrounded; } }
    public bool IsTargetOnLeft { get { return targetOnLeft; } }
    public bool TargetOnLeft { get { return targetOnLeft; } }
    public bool CanComboPunch {  get { return canComboPunch; } }
    public bool CanRoll { get { return canRoll; } }
    public bool IsTransform { get { return isTransform; } }

    void Start()
    {
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        stageType = GameManager.Instance.stageType;

        if (joystick == null)
        {
            if (stageType == StageType.Online)
            {
                Debug.Log($"Online {IsOwner}");
                if (IsOwner)
                {
                    controller = Instantiate(controllerPrefab, GameplayManager.Manager.MainCanvas.transform);
                    joystick = controller.joystick;
                    controller.SetPlayer(this, characterBase);
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"OnNetworkSpawn");
        base.OnNetworkSpawn();

        characterBase.isOwned = IsOwner;
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log($"OnNetworkDespawn");
        base.OnNetworkDespawn();

        characterBase.isOwned = IsOwner;
    }

    void Update()
    {
        if (GameplayManager.Manager == null) { return; }
        if (GameplayManager.Manager.VideoOn) { return; }
        if (GameplayManager.Manager.GameEnd) { return; }

        if (currentState == CharacterState.Dead || characterBase.isDead) {return;}

        if (isTransform)
        {
            currentTransformTime -= Time.deltaTime;

            if (currentTransformTime <= 0)
            {
                InitDeTransform();
            }
        }

        if (currentState == CharacterState.GetHit || currentState == CharacterState.Knock) { return; }

        if (characterBase.isAI) { return; }

        if (!characterBase.isOwned) { return; }

        if (currentState != CharacterState.Idle && currentState != CharacterState.Walk && currentState != CharacterState.Crouch) { return; }

        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;

        if (isGrounded)
        {
            if (currentState == CharacterState.Idle)
            {
                Steady();
            }

            if (currentState != CharacterState.RollForward || currentState != CharacterState.RollBackward)
            {
                if (vertical < -0.7f)
                {
                    SetColliderSize(CharacterState.Crouch);
                    Crouch();
                }
                else if (horizontal > 0.3f && vertical > -0.7f)
                {
                    SetColliderSize(CharacterState.Idle);
                    Move(horizontal);
                }
                else if (horizontal < -0.3f && vertical > -0.7f)
                {
                    SetColliderSize(CharacterState.Idle);
                    Move(horizontal);
                }
                else
                {
                    SetColliderSize(CharacterState.Idle);
                    Idle();
                }
            }
            
            if (vertical > 0.5f)
            {
                if (horizontal > 0.3f)
                {
                    JumpMove(Vector2.right);
                }
                else if (horizontal < -0.3f)
                {
                    JumpMove(Vector2.left);
                }
                else
                {
                    Jump();
                }
            }

            if (canRoll)
            {
                if (vertical < -0.7f && horizontal > 0.3f)
                {
                    RollMove(Vector2.right);
                }
                else if (vertical < -0.7f && horizontal < -0.3f)
                {
                    RollMove(Vector2.left);
                }
            }
        }
    }

    public void SetAnimatorController(AnimatorOverrideController newAnimatorController)
    {
        animator.runtimeAnimatorController = newAnimatorController;
    }

    public void InitController()
    {
        if (characterBase.isOwned)
        {
            controller = Instantiate(controllerPrefab, GameplayManager.Manager.MainCanvas.transform);
            joystick = controller.joystick;
            controller.SetPlayer(this, characterBase);
        }

        if (stageType == StageType.Online)
        {
            Debug.Log($"Online {IsOwner}");
            if (IsOwner)
            {
                controller = Instantiate(controllerPrefab, GameplayManager.Manager.MainCanvas.transform);
                joystick = controller.joystick;
                controller.SetPlayer(this, characterBase);
            }
        }
    }    

    public void Steady()
    {
        if (rb.linearVelocity.magnitude > 0)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, decelerationRate * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    #region Move
    public void Move(float direction)
    {
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        animator.SetBool("isWalking", true);

        ChangeState(CharacterState.Walk);

        //FaceEnemy();
        FlipIfNeeded(direction);

        if (stageType == StageType.Online)
        {
            if (!IsOwner) { return; }

            RequestMoveServerRpc(direction);
        }
    }

    [ServerRpc]
    void RequestMoveServerRpc(float direction)
    {
        PerformMoveClientRpc(direction);
    }

    [ClientRpc]
    void PerformMoveClientRpc(float direction)
    {
        if (IsOwner) return;

        animator.SetBool("isWalking", true);

        ChangeState(CharacterState.Walk);

        FlipIfNeeded(direction);
    }

    #endregion

    #region Jump
    public void Jump()
    {
        ChangeState(CharacterState.Jump);
        animator.SetTrigger("JumpUp");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        AudioManager.Manager.PlaySFX("Jump");

        if (stageType == StageType.Online)
        {
            if (!IsOwner) { return; }

            RequestJumpServerRpc();
        }
    }

    [ServerRpc]
    void RequestJumpServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformJumpClientRpc();
    }

    [ClientRpc]
    void PerformJumpClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        ChangeState(CharacterState.Jump);
        animator.SetTrigger("JumpUp");
        AudioManager.Manager.PlaySFX("Jump");
    }

    public void JumpMove(Vector2 direction)
    {
        StartCoroutine(JumpMoveCoroutine(direction));

        if (stageType == StageType.Online)
        {
            if (!IsOwner) { return; }

            RequestJumpMoveServerRpc(direction);
        }
    }    

    public IEnumerator JumpMoveCoroutine(Vector2 direction)
    {
        ChangeState(direction.x > 0 ? CharacterState.JumpForward : CharacterState.JumpBackward);
        animator.SetTrigger("JumpSide");
        FlipIfNeeded(direction.x);
        rb.linearVelocity = new Vector2(direction.x * jumpDistance, jumpForce);
        AudioManager.Manager.PlaySFX("Jump");
        yield return new WaitForSeconds(1f);
        Idle();
    }

    [ServerRpc]
    void RequestJumpMoveServerRpc(Vector2 direction)
    {
        PerformJumpMoveClientRpc(direction);
    }

    [ClientRpc]
    void PerformJumpMoveClientRpc(Vector2 direction)
    {
        if (IsOwner) return;

        StartCoroutine(JumpMoveCoroutine(direction));
    }

    #endregion

    #region Roll
    public void RollMove(Vector2 direction)
    {
        StartCoroutine(RollMoveCoroutine(direction));

        if (stageType == StageType.Online)
        {
            if (!IsOwner) { return; }

            RequestRollServerRpc(direction);
        }
    }

    public IEnumerator RollMoveCoroutine(Vector2 direction)
    {
        SetColliderSize(CharacterState.Crouch);

        canRoll = false;
        ChangeState(direction.x > 0 ? CharacterState.RollForward : CharacterState.RollBackward);
        animator.SetTrigger("Roll");
        FlipIfNeeded(direction.x);
        rb.linearVelocity = new Vector2(direction.x * rollSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(0.75f);
        Idle();
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;

        SetColliderSize(CharacterState.Idle);
    }

    [ServerRpc]
    void RequestRollServerRpc(Vector2 direction)
    {
        PerformRollClientRpc(direction);
    }

    [ClientRpc]
    void PerformRollClientRpc(Vector2 direction)
    {
        if (IsOwner) return;

        StartCoroutine(RollMoveCoroutine(direction));
    }
    #endregion

    #region Hit
    public void GetHit(bool isKnock)
    {
        Hit(isKnock);

        if (stageType == StageType.Online)
        {
            if (!IsOwner) { return; }

            RequestHitServerRpc(isKnock);
        }
    }

    void Hit(bool isKnock)
    {
        AudioManager.Manager.PlaySFX("Hit");

        if (isKnock)
        {
            if (currentState == CharacterState.Jump || currentState == CharacterState.JumpForward || currentState == CharacterState.JumpBackward || currentState == CharacterState.RollForward || currentState == CharacterState.RollBackward)
            {
                ChangeState(CharacterState.Knock);
                animator.SetTrigger("Knock");
                rb.linearVelocity = Vector2.zero;
                StartCoroutine(Knock());
            }
            else if (currentState != CharacterState.Dead && !characterBase.isDead)
            {
                ChangeState(CharacterState.Knock);
                animator.SetTrigger("Knock");
                StartCoroutine(Knock());
            }
        }
        else
        {
            if (currentState == CharacterState.Jump || currentState == CharacterState.JumpForward || currentState == CharacterState.JumpBackward || currentState == CharacterState.RollForward || currentState == CharacterState.RollBackward)
            {
                ChangeState(CharacterState.GetHit);
                animator.SetTrigger("GetHit");
                rb.linearVelocity = Vector2.zero;
                StartCoroutine(FallToGround());
            }
            else if (currentState != CharacterState.Dead && !characterBase.isDead)
            {
                ChangeState(CharacterState.GetHit);
                animator.SetTrigger("GetHit");
                StartCoroutine(FallToGround());
            }
        }
    }

    [ServerRpc]
    void RequestHitServerRpc(bool isKnock)
    {
        PerformHitClientRpc(isKnock);
    }

    [ClientRpc]
    void PerformHitClientRpc(bool isKnock)
    {
        if (IsOwner) return;

        Hit(isKnock);
    }

    #endregion

    IEnumerator FallToGround()
    {
        yield return new WaitForSeconds(0.5f);
        rb.linearVelocity = new Vector2(0, -jumpForce);
        yield return new WaitForSeconds(0.5f);
        Idle();
    }

    IEnumerator Knock()
    {
        rb.linearVelocity = new Vector2(0, -jumpForce);
        SetColliderSize(CharacterState.Crouch);
        yield return new WaitForSeconds(1f);

        SetColliderSize(CharacterState.Idle);
        Idle();

    }

    #region Punch
    public void Punch()
    {
        if (currentState == CharacterState.Idle || currentState == CharacterState.Walk)
        {
            ChangeState(CharacterState.Punch);
            animator.SetTrigger("Punch");
            canComboPunch = true;
            StartCoroutine(ResetPunchCombo());
            characterAttack.Punch();

            if (stageType == StageType.Online)
            {
                if (!IsOwner) { return; }

                RequestPunchServerRpc();
            }
        }
        else if (canComboPunch)
        {
            ChangeState(CharacterState.PunchCombo);
            animator.SetTrigger("PunchCombo");
            canComboPunch = false;
            characterAttack.PunchCombo();

            if (stageType == StageType.Online)
            {
                if (!IsOwner) { return; }

                RequestPunchComboServerRpc();
            }
        }
    }

    [ServerRpc]
    void RequestPunchServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformPunchClientRpc();
    }

    [ClientRpc]
    void PerformPunchClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        ChangeState(CharacterState.Punch);
        animator.SetTrigger("Punch");
        canComboPunch = true;
        StartCoroutine(ResetPunchCombo());
        characterAttack.Punch();
    }

    [ServerRpc]
    void RequestPunchComboServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformPunchComboClientRpc();
    }

    [ClientRpc]
    void PerformPunchComboClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        ChangeState(CharacterState.PunchCombo);
        animator.SetTrigger("PunchCombo");
        canComboPunch = false;
        characterAttack.PunchCombo();
    }

    IEnumerator ResetPunchCombo()
    {
        yield return new WaitForSeconds(0.75f);
        canComboPunch = false;
        Idle();
    }

    #endregion

    #region Kick
    public void Kick()
    {
        if (currentState == CharacterState.Idle || currentState == CharacterState.Walk)
        {
            ChangeState(CharacterState.Kick);
            animator.SetTrigger("Kick");
            canComboKick = true;
            StartCoroutine(ResetKickCombo());
            characterAttack.Kick();

            if (stageType == StageType.Online)
            {
                if (!IsOwner) { return; }

                RequestKickServerRpc();
            }
        }
    }

    [ServerRpc]
    void RequestKickServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformKickClientRpc();
    }

    [ClientRpc]
    void PerformKickClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        ChangeState(CharacterState.Kick);
        animator.SetTrigger("Kick");
        canComboKick = true;
        StartCoroutine(ResetKickCombo());
        characterAttack.Kick();
    }

    IEnumerator ResetKickCombo()
    {
        yield return new WaitForSeconds(0.5f);
        canComboKick = false;
        Idle();
    }

    #endregion

    #region SpecialAttack
    public void SpecialAttack()
    {
        if (currentState == CharacterState.Idle || currentState == CharacterState.Walk)
        {
            if (characterBase.SkillPoint >= 3)
            {
                characterBase.UseSkillPoint(3);

                ChangeState(CharacterState.SpecialAttack);
                animator.SetTrigger("SpecialAttack");
                StartCoroutine(ResetState(1f));

                characterAttack.SpecialAttack();

                if (isTransform)
                {
                    GameplayManager.Manager.PlaySpecialSkillVideo(characterBase.BaseData.characterVideoLegendary);
                }
                else
                {
                    GameplayManager.Manager.PlaySpecialSkillVideo(characterBase.BaseData.characterVideo);
                }

                if (stageType == StageType.Online)
                {
                    if (!IsOwner) { return; }

                    RequestSpecialAttackServerRpc();
                }
            }
        }
    }

    [ServerRpc]
    void RequestSpecialAttackServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformSpecialAttackClientRpc();
    }

    [ClientRpc]
    void PerformSpecialAttackClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        characterBase.UseSkillPoint(3);

        ChangeState(CharacterState.SpecialAttack);
        animator.SetTrigger("SpecialAttack");
        StartCoroutine(ResetState(1f));

        characterAttack.SpecialAttack();

        if (isTransform)
        {
            GameplayManager.Manager.PlaySpecialSkillVideo(characterBase.BaseData.characterVideoLegendary);
        }
        else
        {
            GameplayManager.Manager.PlaySpecialSkillVideo(characterBase.BaseData.characterVideo);
        }
    }

    #endregion

    #region WeaponAttack
    public void WeaponAttack()
    {
        if (currentState == CharacterState.Idle || currentState == CharacterState.Walk)
        {
            if (characterBase.SkillPoint >= 1)
            {
                characterBase.UseSkillPoint(1);

                ChangeState(CharacterState.WeaponAttack);
                animator.SetTrigger("WeaponAttack");
                StartCoroutine(ResetState(1f));

                characterAttack.WeaponAttack();

                if (stageType == StageType.Online)
                {
                    if (!IsOwner) { return; }

                    RequestWeaponAttackServerRpc();
                }
            }
        }
    }

    [ServerRpc]
    void RequestWeaponAttackServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformWeaponAttackClientRpc();
    }

    [ClientRpc]
    void PerformWeaponAttackClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        characterBase.UseSkillPoint(1);

        ChangeState(CharacterState.WeaponAttack);
        animator.SetTrigger("WeaponAttack");
        StartCoroutine(ResetState(1f));

        characterAttack.WeaponAttack();
    }

    #endregion

    #region Dead
    public void Dead()
    {
        ChangeState(CharacterState.Dead);
        animator.SetTrigger("Dead");

        for (int i = 0; i < GameplayManager.Manager.CharacterOnGameplay.Count; i++) //Cari player lain dan jadikan dia yang menang
        {
            if (GameplayManager.Manager.CharacterOnGameplay[i] != characterBase)
            {
                GameplayManager.Manager.InitWinner(GameplayManager.Manager.CharacterOnGameplay[i], i + 1);
            }
        }

        if (stageType == StageType.Online)
        {
            if (!IsOwner) { return; }

            RequestDeadServerRpc();
        }
    }

    [ServerRpc]
    void RequestDeadServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformDeadClientRpc();
    }

    [ClientRpc]
    void PerformDeadClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        ChangeState(CharacterState.Dead);
        animator.SetTrigger("Dead");

        for (int i = 0; i < GameplayManager.Manager.CharacterOnGameplay.Count; i++) //Cari player lain dan jadikan dia yang menang
        {
            if (GameplayManager.Manager.CharacterOnGameplay[i] != characterBase)
            {
                GameplayManager.Manager.InitWinner(GameplayManager.Manager.CharacterOnGameplay[i], i + 1);
            }
        }
    }

    #endregion

    #region Idle
    public void Idle()
    {
        if (currentState != CharacterState.Dead || !characterBase.isDead)
        {
            ChangeState(CharacterState.Idle);
            animator.SetTrigger("Idle");

            if (stageType == StageType.Online)
            {
                if (!IsOwner) { return; }

                RequestIdleServerRpc();
            }
        }
    }

    [ServerRpc]
    void RequestIdleServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformIdleClientRpc();
    }

    [ClientRpc]
    void PerformIdleClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        ChangeState(CharacterState.Dead);
        animator.SetTrigger("Dead");

        for (int i = 0; i < GameplayManager.Manager.CharacterOnGameplay.Count; i++) //Cari player lain dan jadikan dia yang menang
        {
            if (GameplayManager.Manager.CharacterOnGameplay[i] != characterBase)
            {
                GameplayManager.Manager.InitWinner(GameplayManager.Manager.CharacterOnGameplay[i], i + 1);
            }
        }
    }

    #endregion

    #region Transform Detransform
    public void TransformLegend()
    {
        if (characterBase.BaseData.characterType == CharacterType.Legendary && characterBase.SkillPoint >= 3)
        {
            InitTransform();

            if (stageType == StageType.Online)
            {
                if (!IsOwner) { return; }

                RequestTransformServerRpc();
            }
        }
    }

    [ServerRpc]
    void RequestTransformServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformTransformClientRpc();
    }

    [ClientRpc]
    void PerformTransformClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        InitTransform();
    }

    void InitTransform()
    {
        isTransform = true;
        currentTransformTime = transformTime;
        animator.runtimeAnimatorController = characterBase.BaseData.characterLegendaryAnimator;
        transformLegendaryParticle.gameObject.SetActive(true);
        transformLegendaryParticle.SetTrigger("Play");
    }

    public void InitDeTransform()
    {
        isTransform = false;
        currentTransformTime = 0;
        animator.runtimeAnimatorController = characterBase.BaseData.characterAnimator;
        transformLegendaryParticle.SetTrigger("Play");
        Invoke("TransformParticleOff", 1f);

        if (stageType == StageType.Online)
        {
            if (!IsOwner) { return; }

            RequestDeTransformServerRpc();
        }
    }

    [ServerRpc]
    void RequestDeTransformServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformDeTransformClientRpc();
    }

    [ClientRpc]
    void PerformDeTransformClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        isTransform = false;
        currentTransformTime = 0;
        animator.runtimeAnimatorController = characterBase.BaseData.characterAnimator;
        transformLegendaryParticle.SetTrigger("Play");
        Invoke("TransformParticleOff", 1f);
    }

    void TransformParticleOff()
    {
        transformLegendaryParticle.gameObject.SetActive(false);
    }

    #endregion

    IEnumerator ResetState(float delay)
    {
        yield return new WaitForSeconds(delay);
        Idle();
    }

    void FaceEnemy()
    {
        if (targetOnLeft)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }

    public void SetColliderSize(CharacterState tmpState)
    {
        if (tmpState == CharacterState.Idle)
        {
            capsuleCollider2D.size = new Vector2(capsuleCollider2D.size.x, colliderSizeYIdle);
            capsuleCollider2D.offset = new Vector2(capsuleCollider2D.offset.x, colliderOffsideYIdle);
        }
        else if (tmpState == CharacterState.Crouch)
        {
            capsuleCollider2D.size = new Vector2(capsuleCollider2D.size.x, colliderSizeYCrouch);
            capsuleCollider2D.offset = new Vector2(capsuleCollider2D.offset.x, colliderOffsideYCrouch);
        }
    }

    void FlipIfNeeded(float direction)
    {
        if (characterBase.Target != null)
        {
            float enemyDirection = characterBase.Target.transform.position.x - transform.position.x;

            if (enemyDirection > 0)
            {
                targetOnLeft = false;
            }
            else if (enemyDirection < 0)
            {
                targetOnLeft = true;
            }
        }

        if (direction > 0 && targetOnLeft)
        {
            Flip(true);
        }
        else if (direction < 0 && !targetOnLeft)
        {
            Flip(false);
        }
    }

    void Flip(bool value)
    {
        spriteRenderer.flipX = value;
    }

    #region Crounch
    public void Crouch()
    {
        ChangeState(CharacterState.Crouch);

        if (stageType == StageType.Online)
        {
            if (!IsOwner) { return; }

            RequestCrouchServerRpc(); 
        }
    }

    [ServerRpc]
    void RequestCrouchServerRpc(ServerRpcParams rpcParams = default)
    {
        PerformCrouchClientRpc();
    }

    [ClientRpc]
    void PerformCrouchClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return;

        ChangeState(CharacterState.Crouch);
    }

    #endregion

    #region ChangeState
    void ChangeState(CharacterState newState)
    {
        if (!characterBase.isDead || newState == CharacterState.Dead)
        {
            currentState = newState;
            animator.SetBool("isWalking", newState == CharacterState.Walk);
            animator.SetBool("isCrouching", newState == CharacterState.Crouch);

            if (stageType == StageType.Online)
            {
                if (!IsOwner) { return; }

                RequestChangeStateServerRpc(newState);
            }
        }

        FaceEnemy();
    }

    [ServerRpc]
    void RequestChangeStateServerRpc(CharacterState newState)
    {
        PerformChangeStateClientRpc(newState);
    }

    [ClientRpc]
    void PerformChangeStateClientRpc(CharacterState newState)
    {
        if (IsOwner) return;

        currentState = newState;
        animator.SetBool("isWalking", newState == CharacterState.Walk);
        animator.SetBool("isCrouching", newState == CharacterState.Crouch);
    }

    #endregion

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            
            if (animator != null)
            {
                Idle();
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}

public enum CharacterState { Idle, Walk, Crouch, Jump, JumpForward, JumpBackward, RollForward, RollBackward, Punch, PunchCombo, Kick, KickCombo, SpecialAttack, WeaponAttack, GetHit, Dead, Knock }