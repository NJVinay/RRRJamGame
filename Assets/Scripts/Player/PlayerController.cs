using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
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
    public bool isHoldingFire = false;
    public WeaponsManager weaponsManager;

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

    public void OnAim(InputValue cc)
    {
        isAiming = cc.Get<float>() > 0;
    }

    public void OnFire(InputValue cc)
    {
        isHoldingFire = cc.Get<float>() > 0;
        weaponsManager.Fire(isHoldingFire);
    }

    public void OnDebug01(InputValue cc)
    {
        weaponsManager.weaponCheck();
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.linearVelocity = moveInput * (isAiming ? moveSpeed * aimMoveSpeedMultiplier : moveSpeed);
        }
    }

    private void Update()
    {
        if (isHoldingFire)
        {
            weaponsManager.Fire(isHoldingFire);
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
}
