using Cinemachine;
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
    private bool canInteract = true;
    [SerializeField]
    private bool canFootsteps = true;

    [Header("Movement Parameters")]
    [SerializeField]
    private float playerSpeed = 5.0f;
    [SerializeField]
    private float sprintSpeed = 10.0f;
    [SerializeField]
    private float crouchSpeed = 2.5f;
    private Vector3 playerVelocity;

    [Header("Jump Parameters")]
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;

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

    [Header("Interaction")]
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
    private float timeBeforeRegenStarts = 3f;
    [SerializeField]
    private float healthValueIncrement = 1f;
    [SerializeField]
    private float healthTimeIncrement = 0.1f;
    private float currentHealth;
    private Coroutine healthRegeneration;

    [Header("Stamina Parameters")]
    public float maxPlayerStamina = 10.0f;
    public float playerStamina;

    [Header("Components")]
    private CharacterController controller;
    private CinemachineShake cameraShake;
    private bool groundedPlayer;
    private InputManager inputManager;
    private Transform cameraTransform;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;
        cameraShake = CinemachineShake.Instance;
        cameraTransform = Camera.main.transform;

        playerStamina = maxPlayerStamina;
        Cursor.visible = false;
    }

    void Update()
    {
        if(CanMove){
            //Checks for Ground
            GroundCheck();
            //Handles Player Movement/Sprint
            PlayerMovement();
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

            if (canInteract)
            {
                PlayerInteractionCheck();
                PlayerInteractionInput();
            }

            /*
            //Handles Stamina Regen
            if (playerStamina < maxPlayerStamina)
            {
                StartCoroutine(StaminaRegen());
            }*/
        }
    }

    private void PlayerMovement()
    {
        Vector2 movement = inputManager.GetPlayerMovement();

        Vector3 move = new Vector3(movement.x, 0f, movement.y);
        playerVelocity = move;
        move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
        move.y = 0f;

        controller.Move(move * Time.deltaTime * (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : playerSpeed));
    }

    private void PlayerJump()
    {
        if (ShouldJump)
        {
            // Changes the height position of the player..
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void PlayerCrouch()
    {
        if (ShouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }

    private void GroundCheck()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
    }

    private void CameraHeadbob()
    {
        if (!groundedPlayer) { return; }

        if (Mathf.Abs(playerVelocity.magnitude) > .1f)
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

    /*
    private IEnumerator StaminaRegen()
    {
        yield return new WaitForSeconds(2);
        playerStamina += Time.deltaTime;
        Debug.Log("Current Player Stamina: " + Mathf.Round(playerStamina));
    }*/
}
