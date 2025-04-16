using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float jumpForceY = 5f;
    public float moveSpeedX = 5f;
    private int moveDirection = 1; // 1 for right, -1 for left
    private Rigidbody2D rb;
    private ObstacleGenerator obstacleGenerator;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        obstacleGenerator = FindObjectOfType<ObstacleGenerator>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Jump();
        }
        obstacleGenerator.GenerateAheadOfPlayer(transform.position.y, Camera.main.orthographicSize * 2);
        // Clean up obstacles below player
        obstacleGenerator.CleanupObstaclesBelowPlayer(transform.position.y, 10f);
    }
    private void Jump()
    {
        ApplyJumpVelocity();
        ApplyRotation();
    }
    private void ApplyJumpVelocity()
    {
        if (rb == null) return;
        rb.velocity = new Vector2(moveSpeedX * moveDirection, jumpForceY);
    }
    private void ApplyRotation()
    {
        float zAngle = moveDirection == 1 ? -30f : 30f;
        transform.rotation = Quaternion.Euler(0, 0, zAngle);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("WallLeft"))
        {
            SetMoveDirection(1);
        }
        else if (collision.gameObject.CompareTag("WallRight"))
        {
            SetMoveDirection(-1);
        }
    }
    private void SetMoveDirection(int direction)
    {
        moveDirection = direction;
    }
}
