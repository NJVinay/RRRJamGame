using UnityEngine;

public class Player : MonoBehaviour
{

    private Rigidbody2D rb;

    [SerializeField]private float moveSpeed;
    [SerializeField] private float SpeedX, SpeedY;
    [SerializeField]private float jumpForce;

    private float xInput;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        SpeedX = Input.GetAxis("Horizontal") * moveSpeed;
        SpeedY = Input.GetAxis("Vertical") * moveSpeed;
        rb.linearVelocity = new Vector2(SpeedX, SpeedY);
    }
}
