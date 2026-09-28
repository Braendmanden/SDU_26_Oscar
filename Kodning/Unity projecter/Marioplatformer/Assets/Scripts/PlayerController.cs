using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;

    public float speed = 10;
    public float maxSpeed = 10;
    public float jumpForce = 10;
    bool jump = false;
    public bool onGround;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && onGround)
        {
            jump = true;
        }
    }

    private void FixedUpdate()
    {
        float direction = Input.GetAxis("Horizontal"); // [-1, 1] left/right
        rb.AddForce(Vector2.right * direction * speed);

        if (rb.linearVelocity.x > maxSpeed)
        {
            rb.linearVelocity = new Vector2(maxSpeed, rb.linearVelocity.y);
        }
        if (rb.linearVelocity.x < -maxSpeed)
        {
            rb.linearVelocity = new Vector2(-maxSpeed, rb.linearVelocity.y);
        }

        if (jump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jump = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DeathTrigger"))
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            onGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            onGround = false;
        }
    }

}
