using System.Collections;
using UnityEngine;
using VInspector.Libs;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public bool CanMove {  get; private set; } = true;
    private bool IsSprinting => canSprint && inputManager.PlayerSprint();
    private bool ShouldJump => inputManager.PlayerJumped() && groundedPlayer;
    private bool ShouldCrouch => inputManager.PlayerCrouch() && !duringCrouchAnimation && groundedPlayer;

    [Header("Functional Options")]
    [SerializeField]
    private bool canSprint = true;
    [SerializeField]
    private bool canJump = true;
    [SerializeField]
    private bool canCrouch = true;

    [Header("Movement Parameters")]
    [SerializeField]
    private float playerSpeed = 5.0f;
    [SerializeField]
    private float sprintSpeed = 10.0f;
    [SerializeField]
    private float crouchSpeed = 2.5f;

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
    private Vector3 crouchingCenter = new Vector3(0,0.5f,0);
    [SerializeField]
    private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;


    [Header("Stamina Parameters")]
    public float maxPlayerStamina = 10.0f;
    public float playerStamina;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private InputManager inputManager;
    private Transform cameraTransform;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;
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
        move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
        move.y = 0f;

        controller.Move(move * Time.deltaTime * (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : playerSpeed)); 
    }

    private void PlayerJump()
    {
        if ((ShouldJump))
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

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(Camera.main.transform.position, Vector3.up,1f))
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
