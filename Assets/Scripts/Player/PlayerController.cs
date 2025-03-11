using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    
    // Movespeed. Editable in the editor directly for prototyping
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    // Gets Move direction from the Input System
    public void OnMove(InputValue cc)
    {
        moveInput = cc.Get<Vector2>();
    }


    // Directly applies Move Direction to the RigidBody.
    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
