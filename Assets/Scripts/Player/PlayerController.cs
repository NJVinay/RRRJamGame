using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    // Movespeed. Editable in the editor directly for prototyping
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float aimMoveSpeedMultiplier = 0.5f;
    [SerializeField] float dashSpeed = 25f;
    [SerializeField] float dashTime = 0.1f;
    [SerializeField] float dashCooldown = 2f;
    private float lastDashTime;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private bool isDashing = false;


    
    [Header("Shooting Settings")]
    public bool isAiming = false;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void OnMove(InputValue cc)
    {
        moveInput = cc.Get<Vector2>();

    }
    public void OnDash(InputValue cc)
    {
        if (!isDashing && !isAiming && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }
    }
    // Directly applies Move Direction to the RigidBody's velocity.
    private void FixedUpdate()
    {
        if(!isDashing)
        {
            rb.linearVelocity = moveInput * (isAiming ? moveSpeed * 0.5f : moveSpeed);
        }        
    }
    IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        rb.linearVelocity = moveInput * dashSpeed;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
    }
    public void OnAim(InputValue cc)
    {
        isAiming = cc.Get<float>() > 0;
    }
}
