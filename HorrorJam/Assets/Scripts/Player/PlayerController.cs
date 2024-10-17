using UnityEngine;
using VInspector.Libs;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float playerSpeed = 5.0f;
    [SerializeField]
    private float sprintSpeed = 10.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private InputManager inputManager;
    private Transform cameraTransform;

    public float maxPlayerStamina = 10.0f;
    public float playerStamina;

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
        //Checks for Ground
        GroundCheck();
        //Handles Player Movement/Sprint
        PlayerMovement();
        //Handles Player Jumping
        PlayerJump();
    }

    public void PlayerMovement()
    {
        Vector2 movement = inputManager.GetPlayerMovement();

        Vector3 move = new Vector3(movement.x, 0f, movement.y);
        move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
        move.y = 0f;

        if (inputManager.PlayerSprint() && playerStamina > 0)
        {
            controller.Move(move * Time.deltaTime * sprintSpeed);
            if(move.magnitude > 0f)
            {
                playerStamina -= Time.deltaTime;
                Debug.Log("Player Sprint Stamina: " + Mathf.Round(playerStamina));
            }
        }
        else
        {
            controller.Move(move * Time.deltaTime * playerSpeed);
        }
    }

    public void PlayerJump()
    {
        // Changes the height position of the player..
        if (inputManager.PlayerJumped() && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    /*public void StaminaRegen()
    {
        if(playerStamina < maxPlayerStamina)
        {
            new WaitForSeconds(2);
            playerStamina += Time.deltaTime;
            Debug.Log("Current Player Stamina: " + Mathf.Round(playerStamina));
        }
    }*/

    public void GroundCheck()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
    }
}
