using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ObstacleGenerator : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject obstaclePrefab;
    public GameObject movingObstaclePrefab;
    [Header("Generation Parameters")]
    public float gameWidth = 10f;          // Width of the game area (from 0 to gameWidth)
    public float gameStartPosition = 0f;     // Starting position of the game area
    public float jumpWidth = 2f;           // Maximum horizontal jump distance
    public float rowSpacing = 4f;          // Vertical space between obstacle rows
    public float minObstacleWidth = 1f;    // Minimum width of obstacles
    public float maxObstacleWidth = 1.5f;  // Maximum width of obstacles
    public float obstacleHeight = 0.5f;    // Height of each obstacle
    public Transform obstacleParent;       // Parent transform for obstacles
    [Header("Difficulty Settings")]
    public float initialDifficulty = 1f;
    public float difficultyIncreaseRate = 0.1f;
    public int rowsPerDifficultyIncrease = 10;
    // Internal variables
    private float lastGeneratedY = 0f;
    private float currentDifficulty;
    private Dictionary<int, bool> generatedRows = new Dictionary<int, bool>();
    void Start()
    {
        currentDifficulty = initialDifficulty;
        // Create parent if not assigned
        if (obstacleParent == null)
        {
            GameObject parent = new GameObject("Obstacles");
            obstacleParent = parent.transform;
        }
    }
    // Call this from your main game controller to generate obstacles ahead of the player
    public void GenerateAheadOfPlayer(float playerY, float viewportHeight)
    {
        // Calculate how far ahead we need to generate
        float targetY = playerY + viewportHeight * 2;
        // Start generating from the last generated Y or just ahead of player
        float currentY = lastGeneratedY == 0 ? playerY + rowSpacing : lastGeneratedY + rowSpacing;
        while (currentY <= targetY)
        {
            // Generate a row if we haven't already at this height
            int rowIndex = Mathf.FloorToInt(currentY / rowSpacing);
            if (!generatedRows.ContainsKey(rowIndex))
            {
                GenerateRow(currentY);
                generatedRows[rowIndex] = true;
            }
            currentY += rowSpacing;
        }
        lastGeneratedY = Mathf.Max(lastGeneratedY, targetY - rowSpacing);
    }
    // Generates a single row of obstacles at the specified y coordinate
    private void GenerateRow(float y)
    {
        // Update difficulty based on height
        int rowNumber = Mathf.FloorToInt(y / rowSpacing);
        currentDifficulty = initialDifficulty + (rowNumber / rowsPerDifficultyIncrease) * difficultyIncreaseRate;
        // Different obstacle patterns based on row number and randomness
        int patternType = (rowNumber % 3) + Random.Range(0, 2);
        switch (patternType)
        {
            case 0:
                GenerateGapPattern(y, rowNumber);
                break;
            case 1:
                GenerateRandomPattern(y, rowNumber);
                break;
            case 2:
                GenerateAlternatingPattern(y, rowNumber);
                break;
            case 3:
                GenerateZigZagPattern(y, rowNumber);
                break;
            default:
                GenerateRandomPattern(y, rowNumber);
                break;
        }
        // Add a moving obstacle with increasing probability based on difficulty
        if (currentDifficulty > 2 && Random.value < 0.3f * (currentDifficulty - 2) / 3)
        {
            SpawnMovingObstacle(y, rowNumber);
        }
    }
    // Pattern 1: A random gap in a line of obstacles
    private void GenerateGapPattern(float y, int rowNumber)
    {
        // Ensure we have at least one gap (of jump width size)
        float gapStart = Random.Range(gameStartPosition, gameWidth - jumpWidth);
        // Create obstacles on either side of the gap
        if (gapStart > gameStartPosition)
        {
            CreateObstacle(gameStartPosition, y, gapStart, obstacleHeight, rowNumber, "Left");
        }
        if (gapStart + jumpWidth < gameWidth)
        {
            CreateObstacle(gapStart + jumpWidth, y, gameWidth - (gapStart + jumpWidth), obstacleHeight, rowNumber, "Right");
        }
    }
    // Pattern 2: Random obstacles with jumpable gaps between them
    private void GenerateRandomPattern(float y, int rowNumber)
    {
        List<Rect> obstacles = new List<Rect>();
        // Determine number of obstacles based on difficulty and game width
        int minObstacles = Mathf.Max(1, Mathf.FloorToInt(currentDifficulty));
        int maxObstacles = Mathf.FloorToInt(gameWidth / (minObstacleWidth + jumpWidth));
        int numObstacles = Mathf.Min(maxObstacles, minObstacles + Random.Range(0, 6));
        // Try to place obstacles randomly but ensure jumpable gaps
        for (int i = 0; i < numObstacles * 3; i++) // Multiple attempts to place obstacles
        {
            if (obstacles.Count >= numObstacles)
                break;
            float width = Random.Range(minObstacleWidth, Mathf.Min(maxObstacleWidth, gameWidth / 4));
            float x = Random.Range(gameStartPosition, gameWidth - width);
            Rect newObstacle = new Rect(x, y, width, obstacleHeight);
            // Check if there's enough space to jump around this obstacle
            bool validPlacement = true;
            foreach (Rect existing in obstacles)
            {
                // Check if there's not enough gap between obstacles
                float gap = Mathf.Min(
                    Mathf.Abs(existing.x - (newObstacle.x + newObstacle.width)),
                    Mathf.Abs(newObstacle.x - (existing.x + existing.width))
                );
                if (gap < jumpWidth && !(newObstacle.x >= existing.x + existing.width + jumpWidth ||
                                        newObstacle.x + newObstacle.width + jumpWidth <= existing.x))
                {
                    validPlacement = false;
                    break;
                }
            }
            if (validPlacement)
            {
                obstacles.Add(newObstacle);
            }
        }
        // Create all obstacles in the list
        for (int i = 0; i < obstacles.Count; i++)
        {
            Rect obs = obstacles[i];
            CreateObstacle(obs.x, obs.y, obs.width, obs.height, rowNumber, $"Random_{i}");
        }
        // If we couldn't place any obstacles, ensure at least one with a gap
        if (obstacles.Count == 0)
        {
            GenerateGapPattern(y, rowNumber);
        }
    }
    // Pattern 3: Alternating obstacles from left and right
    private void GenerateAlternatingPattern(float y, int rowNumber)
    {
        // Determine the gap size (player needs at least jumpWidth to pass)
        float gapSize = jumpWidth + Random.Range(0, 1f);
        // Place an obstacle on either the left or right
        bool obstacleOnLeft = Random.value > 0.5f;
        if (obstacleOnLeft)
        {
            float width = gameWidth - gapSize;
            CreateObstacle(gameStartPosition, y, width, obstacleHeight, rowNumber, "Left");
        }
        else
        {
            float width = gameWidth - gapSize;
            CreateObstacle(gapSize, y, width, obstacleHeight, rowNumber, "Right");
        }
    }
    // Pattern 4: ZigZag pattern with obstacles
    private void GenerateZigZagPattern(float y, int rowNumber)
    {
        // Decide number of sections based on difficulty and game width
        int sections = Mathf.FloorToInt(currentDifficulty) + 1;
        sections = Mathf.Min(sections, Mathf.FloorToInt(gameWidth / (jumpWidth * 2)));
        float sectionWidth = gameWidth / sections;
        // Alternate obstacles from left and right in each section
        for (int i = 0; i < sections; i++)
        {
            float startX = gameStartPosition + i * sectionWidth;
            float obstacleWidth = sectionWidth - jumpWidth;
            if (obstacleWidth <= 0)
                continue;
            // Place obstacle on left or right of section
            bool placeOnLeft = (i % 2 == 0);
            if (placeOnLeft)
            {
                CreateObstacle(startX, y, obstacleWidth, obstacleHeight, rowNumber, $"ZigLeft_{i}");
            }
            else
            {
                CreateObstacle(startX + jumpWidth, y, obstacleWidth, obstacleHeight, rowNumber, $"ZigRight_{i}");
            }
        }
    }
    // Helper method to create an obstacle
    private void CreateObstacle(float x, float y, float width, float height, int rowNumber, string label)
    {
        GameObject obstacle = Instantiate(obstaclePrefab, new Vector3(x + width / 2, y, 0), Quaternion.identity, obstacleParent);
        obstacle.transform.localScale = new Vector3(width, height, 1);
        // For obstacles with center pivot
        obstacle.transform.position = new Vector3(x + width / 2, y, 0);
        obstacle.name = $"Obstacle_Row{rowNumber}_{label}";
    }
    // Creates a moving obstacle that travels back and forth
    private void SpawnMovingObstacle(float y, int rowNumber)
    {
        float width = minObstacleWidth + (maxObstacleWidth - minObstacleWidth) * 0.5f;
        float xPos = Random.Range(gameStartPosition, gameStartPosition + gameWidth - width);
        GameObject obstacle = Instantiate(movingObstaclePrefab, new Vector3(xPos, y, 0), Quaternion.identity, obstacleParent);
        obstacle.transform.localScale = new Vector3(width, obstacleHeight, 1);
        obstacle.name = $"MovingObstacle_Row{rowNumber}";
        // Add a movement script component
        MovingObstacle mover = obstacle.AddComponent<MovingObstacle>();
        mover.speed = 1f + Random.value * currentDifficulty * 0.5f;
        mover.minX = gameStartPosition;
        mover.maxX = gameStartPosition + gameWidth;
    }
    // Cleanup obstacles that are below the player to save memory
    public void CleanupObstaclesBelowPlayer(float playerY, float buffer)
    {
        float threshold = playerY - buffer;
        // Using a separate list to avoid modifying collection during enumeration
        List<Transform> toDestroy = new List<Transform>();
        foreach (Transform child in obstacleParent)
        {
            if (child.position.y < threshold)
            {
                toDestroy.Add(child);
                // Also remove from our tracking dictionary
                int rowIndex = Mathf.FloorToInt(child.position.y / rowSpacing);
                generatedRows.Remove(rowIndex);
            }
        }
        foreach (Transform child in toDestroy)
        {
            Destroy(child.gameObject);
        }
    }
    // Visualize game boundaries in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(gameStartPosition, -10, 0), new Vector3(gameStartPosition, 30, 0));
        Gizmos.DrawLine(new Vector3(gameWidth, -10, 0), new Vector3(gameWidth, 30, 0));
        // Draw row spacing indicators
        Gizmos.color = Color.green;
        for (int i = 0; i < 10; i++)
        {
            float y = i * rowSpacing;
            Gizmos.DrawLine(new Vector3(gameStartPosition, y, 0), new Vector3(gameWidth, y, 0));
        }
    }
}
// Companion script for moving obstacles
public class MovingObstacle : MonoBehaviour
{
    public float speed = 2f;
    public float minX = 0f;
    public float maxX = 10f;
    private int direction = 1;
    void Update()
    {
        // Get the width of the obstacle
        float width = transform.localScale.x;
        // Move the obstacle
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);
        // Check boundaries and change direction
        if (transform.position.x - width / 2 <= minX)
        {
            transform.position = new Vector3(minX + width / 2, transform.position.y, transform.position.z);
            direction = 1;
        }
        else if (transform.position.x + width / 2 >= maxX)
        {
            transform.position = new Vector3(maxX - width / 2, transform.position.y, transform.position.z);
            direction = -1;
        }
    }
}