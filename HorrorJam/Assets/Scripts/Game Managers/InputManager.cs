using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;

    public static InputManager Instance { get { return _instance; } }

    private PlayerControls playerControls;
    private LookControls lookControls;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        playerControls = new PlayerControls();
        lookControls = new LookControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        lookControls.Enable();
    }

    private void OnDisable()
    {
        playerControls?.Disable();
        lookControls?.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return playerControls.Player.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta()
    {
        return lookControls.Mouse.MouseLook.ReadValue<Vector2>();
    }

    public bool PlayerJumped()
    {
        return playerControls.Player.Jump.IsPressed();
    }

    public bool PlayerSprint()
    {
        return playerControls.Player.Sprint.IsPressed();
    }

    public bool PlayerInteraction()
    {
        return playerControls.Player.Interact.IsPressed();
    }
}
