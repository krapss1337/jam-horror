using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public bool CanMove {  get; private set; } = true;
    private bool IsSprinting => canSprint && inputManager.PlayerSprint();
    private bool ShouldJump => inputManager.PlayerJumped() && groundedPlayer;
    private bool ShouldCrouch => inputManager.PlayerCrouch() && !duringCrouchAnimation && groundedPlayer;
    private bool ShouldInteract => inputManager.PlayerInteraction() && canInteract;

    [Header("Functional Options")]
    [SerializeField]
    private bool canSprint = true;
    [SerializeField]
    private bool canJump = true;
    [SerializeField]
    private bool canCrouch = true;
    [SerializeField]
    private bool canUseHeadBob = true;
    [SerializeField]
    private bool WillSlideOnSlopes = true;
    [SerializeField]
    private bool canInteract = true;
    [SerializeField]
    private bool useStamina = true;

    [Header("Movement Parameters")]
    [SerializeField]
    private float playerSpeed = 5.0f;
    [SerializeField]
    private float sprintSpeed = 10.0f;
    [SerializeField]
    private float crouchSpeed = 2.5f;
    [SerializeField]
    private float slopeSlideSpeed = 2.5f;
    private Vector3 moveDirection;

    [Header("Jump Parameters")]
    [SerializeField]
    private float jumpForce = 8.0f;
    [SerializeField]
    private float gravityValue = 30f;

    [Header("Crouch Parameters")]
    [SerializeField]
    private float crouchingHeight = 0.5f;
    [SerializeField]
    private float standingHeight = 2f;
    [SerializeField]
    private float timeToCrouch = 0.25f;
    [SerializeField]
    private Vector3 crouchingCenter = new Vector3(0,.5f,0);
    [SerializeField]
    private Vector3 standingCenter = new Vector3(0, 0f, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Headbob Parameters")]
    [SerializeField]
    private float walkBobSpeed = 14f;
    [SerializeField]
    private float walkBobAmount = .05f;
    [SerializeField]
    private float sprintBobSpeed = 18f;
    [SerializeField]
    private float sprintBobAmount = .1f;
    [SerializeField]
    private float crouchBobSpeed = 8f;
    [SerializeField]
    private float crouchBobAmount = .025f;
    private float timer;

    //Sliding Parameters
    private Vector3 hitPointNormal;

    private bool IsSliding
    {
        get
        {
            if (groundedPlayer && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > controller.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    [Header("Interaction Parameters")]
    [SerializeField]
    private Vector3 interactionWayPoint = default;
    [SerializeField]
    private float interactionDistance = default;
    [SerializeField]
    private LayerMask interactionLayer = default;
    private AInteractable currentInteractable;

    [Header("Health Parameters")]
    [SerializeField]
    private float maxHealth = 100f;
    [SerializeField]
    private float timeBeforeHealthRegenStarts = 3f;
    [SerializeField]
    private float healthValueIncrement = 1f;
    [SerializeField]
    private float healthTimeIncrement = 0.1f;
    private float currentHealth;
    private Coroutine healthRegeneration;
    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;

    [Header("Stamina Parameters")]
    [SerializeField]
    private float maxStamina = 100f;
    [SerializeField]
    private float staminaUseMultipler = 5f;
    [SerializeField]
    private float timeBeforeStaminaRegenStarts = 5f;
    [SerializeField]
    private float staminaValueIncrement = 2f;
    [SerializeField]
    private float staminaTimeIncrement = 0.1f;
    private float currentStamina;
    private Coroutine staminaRegeneration;
    public static Action<float> OnStaminaChange;

    [Header("Input Parameters")]
    private InputManager inputManager;
    private Vector2 movement;

    [Header("Camera/Cinemachine Parameters")]
    private CinemachineShake cameraShake;
    private Transform cameraTransform;

    [Header("Other Components")]
    private CharacterController controller;
    private bool groundedPlayer {
        get {
            if (controller.isGrounded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static PlayerController _instance;

    private void OnEnable()
    {
        OnTakeDamage += ApplyDamage;
    }

    private void OnDisable()
    {
        OnTakeDamage -= ApplyDamage;
    }

    void Start()
    {
        _instance = this;
        controller = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;
        cameraShake = CinemachineShake.Instance;
        cameraTransform = Camera.main.transform;

        currentHealth = maxHealth;
        currentStamina = maxStamina;
        Cursor.visible = false;
    }

    void Update()
    {
        if(CanMove){
            //Handles Player Movement/Sprint
            PlayerMovement();
            ApplyFinalMovement();
            //Handles Player Jumping
            if (canJump) {
                PlayerJump();
            }

            //Handles Player Crouch
            if (canCrouch) {  
                PlayerCrouch(); 
            }

            //Handles Camera Headbob
            if (canUseHeadBob) {
                CameraHeadbob();
            }

            //Handles Player Interaction
            if (canInteract)
            {
                PlayerInteractionCheck();
                PlayerInteractionInput();
            }

            //Handles Stamina
            if (useStamina)
            {
                PlayerStamina();
            }
        }
    }

    private void PlayerMovement()
    {
        movement = inputManager.GetPlayerMovement();

        float moveDirectionY = moveDirection.y;

        moveDirection = (cameraTransform.forward * movement.y + cameraTransform.right * movement.x);
        moveDirection.y = moveDirectionY;
    }

    private void ApplyFinalMovement()
    {
        if (!groundedPlayer)
        {
            moveDirection.y -= gravityValue * Time.deltaTime;
        }

        if (WillSlideOnSlopes && IsSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSlideSpeed;
        }

        controller.Move(moveDirection * Time.deltaTime * (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : playerSpeed));
    }

    private void PlayerJump()
    {
        if (ShouldJump)
        {
            moveDirection.y = jumpForce;
        }
    }

    private void PlayerCrouch()
    {
        if (ShouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }

    private void CameraHeadbob()
    {
        if (!groundedPlayer) { return; }

        if (Mathf.Abs(moveDirection.magnitude) > .1f)
        {
            timer = Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);

            cameraShake.ShakeCamera(isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount, Mathf.Sin(timer));
        }
    }

    private void PlayerInteractionCheck()
    {
        if (Physics.Raycast(Camera.main.ViewportPointToRay(interactionWayPoint), out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject.layer == 6 && (currentInteractable == null || 
                hit.collider.gameObject.GetInstanceID() != currentInteractable.GetInstanceID()))
            {
                hit.collider.TryGetComponent(out currentInteractable);

                if (currentInteractable)
                {
                    Debug.DrawLine(cameraTransform.transform.position, hit.collider.transform.position, Color.blue);
                    currentInteractable.OnFocus();
                }
            }
        }
        else if (currentInteractable)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    private void PlayerInteractionInput()
    {
        if (ShouldInteract && currentInteractable != null && 
            Physics.Raycast(Camera.main.ViewportPointToRay(interactionWayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            currentInteractable.OnInteract();
        }
    }

    private void PlayerStamina()
    {
        if(IsSprinting && movement != Vector2.zero)
        {
            if (staminaRegeneration != null)
            {
                StopCoroutine(staminaRegeneration);
                staminaRegeneration = null;
            }

            currentStamina -= staminaUseMultipler * Time.deltaTime;
            if(currentStamina < 0)
            {
                currentStamina = 0;
            }

            OnStaminaChange?.Invoke(currentStamina);

            if(currentStamina <= 0)
            {
                canSprint = false;
            }
        }

        if (!IsSprinting && currentStamina < maxStamina && staminaRegeneration == null)
        {
            staminaRegeneration = StartCoroutine(StaminaRegen());
        }
    }

    //When player is damaged
    private void ApplyDamage(float dmg)
    {
        currentHealth -= dmg;
        OnDamage?.Invoke(currentHealth);

        if (currentHealth <= 0) { PlayerDeath(); }
        else if(healthRegeneration != null)
            {
            StopCoroutine(healthRegeneration);
        }
    }

    //When Player Dies
    private void PlayerDeath()
    {
        currentHealth = 0;

        if (healthRegeneration != null) {
            StopCoroutine(healthRegeneration);
        }

        print("Player Died!");
    }

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(cameraTransform.position, Vector3.up,1f))
        {
            yield break;
        }

        duringCrouchAnimation = true;

        float timeElaspe = 0;
        float targetHeight = isCrouching ? standingHeight : crouchingHeight;
        float currentHeight = controller.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = controller.center;

        while(timeElaspe < timeToCrouch)
        {
            controller.height = Mathf.Lerp(currentHeight, targetHeight, timeElaspe/timeToCrouch);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, timeElaspe/timeToCrouch);
            timeElaspe += Time.deltaTime;
            yield return null;
        }

        controller.height = targetHeight;
        controller.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    private IEnumerator HealthRegen()
    {
        yield return new WaitForSeconds(timeBeforeHealthRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        while (currentHealth < maxHealth) {
            currentHealth += healthValueIncrement;

            if (currentHealth > maxHealth) {
                currentHealth = maxHealth;
            }

            OnHeal?.Invoke(currentHealth);
            yield return timeToWait;
        }

        healthRegeneration = null;
    }

    private IEnumerator StaminaRegen()
    {
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        while (currentStamina < maxStamina)
        {
            if (currentStamina > 0)
            {
                canSprint = true;
            }

            currentStamina += staminaValueIncrement;

            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }

            OnStaminaChange?.Invoke(currentStamina);
            yield return timeToWait;
        }

        staminaRegeneration = null;
    }
}
