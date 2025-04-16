using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WallManager : MonoBehaviour
{
    [Header("Wall References")]
    public Transform leftWall;            // Reference to the left wall
    public Transform rightWall;           // Reference to the right wall
    [Header("Wall Settings")]
    public float gameWidth = 10f;         // Width of the game area
    public float wallThickness = 0.5f;    // Thickness of the walls
    public float initialWallHeight = 30f; // Initial height of walls
    public float extraHeightBuffer = 20f; // Extra height beyond camera view
    [Header("Level References")]
    public ObstacleGenerator obstacleGenerator;  // Reference to obstacle generator
    public Camera gameCamera;                    // Reference to main camera
    // Internal variables
    private float highestObstacleY = 0f;
    private float lowestVisibleY = 0f;
    private float highestVisibleY = 0f;
    void Start()
    {
        // Find camera if not assigned
        if (gameCamera == null)
            gameCamera = Camera.main;
        // Set up initial wall positions and scales
        SetupWalls();
    }
    void LateUpdate()
    {
        // Calculate visible area bounds based on camera
        CalculateVisibleAreaBounds();
        // Find the highest obstacle
        UpdateHighestObstaclePosition();
        // Update wall heights based on visible area and obstacles
        UpdateWallHeights();
        // Update wall positions based on visible area
        UpdateWallPositions();
    }
    // Initial wall setup
    private void SetupWalls()
    {
        if (leftWall == null || rightWall == null)
        {
            Debug.LogError("Walls not assigned to WallManager!");
            return;
        }
        // Position the left wall at the left edge of the game area
        leftWall.position = new Vector3(obstacleGenerator.gameStartPosition - wallThickness / 2, 0, 0);
        leftWall.localScale = new Vector3(wallThickness, initialWallHeight, 1f);
        // Position the right wall at the right edge of the game area
        rightWall.position = new Vector3(gameWidth + wallThickness / 2, 0, 0);
        rightWall.localScale = new Vector3(wallThickness, initialWallHeight, 1f);
    }
    // Calculate the visible area bounds based on camera position
    private void CalculateVisibleAreaBounds()
    {
        if (gameCamera == null)
            return;
        float verticalExtent = gameCamera.orthographicSize;
        // Calculate the top and bottom of visible area
        lowestVisibleY = gameCamera.transform.position.y - verticalExtent - 5f; // 5 units below camera view
        highestVisibleY = gameCamera.transform.position.y + verticalExtent + extraHeightBuffer;
    }
    // Find the highest obstacle in the scene
    private void UpdateHighestObstaclePosition()
    {
        if (obstacleGenerator == null || obstacleGenerator.obstacleParent == null)
            return;
        // Start with the current highestVisibleY as minimum
        highestObstacleY = Mathf.Max(highestVisibleY, highestObstacleY);
        foreach (Transform obstacle in obstacleGenerator.obstacleParent)
        {
            float obstacleTop = obstacle.position.y + obstacle.localScale.y / 2;
            if (obstacleTop > highestObstacleY)
            {
                highestObstacleY = obstacleTop;
            }
        }
    }
    // Update wall heights based on the highest obstacle
    private void UpdateWallHeights()
    {
        if (leftWall == null || rightWall == null)
            return;
        // Calculate target height based on the highest point that needs to be covered
        float topPoint = Mathf.Max(highestObstacleY, highestVisibleY) + extraHeightBuffer;
        float targetHeight = topPoint - lowestVisibleY;
        // Update wall scales
        Vector3 leftScale = leftWall.localScale;
        leftScale.y = targetHeight;
        leftWall.localScale = leftScale;
        Vector3 rightScale = rightWall.localScale;
        rightScale.y = targetHeight;
        rightWall.localScale = rightScale;
    }
    // Update wall positions to cover the visible area and beyond
    private void UpdateWallPositions()
    {
        if (leftWall == null || rightWall == null)
            return;
        // Position walls to extend from below the camera to above the highest obstacle
        float midPoint = lowestVisibleY + (leftWall.localScale.y / 2);
        leftWall.position = new Vector3(obstacleGenerator.gameStartPosition - wallThickness / 2, midPoint, 0);
        rightWall.position = new Vector3(gameWidth + wallThickness / 2, midPoint, 0);
    }
    // Draw gizmos for debugging
    private void OnDrawGizmos()
    {
        // Draw game boundaries
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(obstacleGenerator.gameStartPosition, -50, 0), new Vector3(obstacleGenerator.gameStartPosition, 50, 0));
        Gizmos.DrawLine(new Vector3(gameWidth, -50, 0), new Vector3(gameWidth, 50, 0));
        // Draw visible area if in play mode
        if (Application.isPlaying && gameCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(-5, lowestVisibleY, 0), new Vector3(gameWidth + 5, lowestVisibleY, 0));
            Gizmos.DrawLine(new Vector3(-5, highestVisibleY, 0), new Vector3(gameWidth + 5, highestVisibleY, 0));
        }
    }
}