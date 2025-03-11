using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public float dashSpeed;
    public float dashTime;
    public float dashCooldown;

    public bool isDashing = false;
    public float lastDashTime;


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
        Debug.Log(cc.Get<float>());
    }
}
