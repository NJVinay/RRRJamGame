using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    
    // Movespeed. Editable in the editor directly for prototyping
    public float moveSpeed = 5f;
    public float aimMoveSpeedMultiplier = 0.5f;
    public bool isAiming = false;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void OnMove(InputValue cc)
    {
        moveInput = cc.Get<Vector2>();
    }
    public void OnAim(InputValue cc)
    {
        isAiming = cc.Get<float>() > 0;
        Debug.Log(cc.Get<float>());
    }
    // Directly applies Move Direction to the RigidBody's velocity.
    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * (isAiming ? moveSpeed * 0.5f : moveSpeed);
    }
}
