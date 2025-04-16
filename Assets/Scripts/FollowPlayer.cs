using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FollowPlayer : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public float followSpeed = 2f; // Speed at which the camera follows the player
    public Camera mainCamera; // Reference to the main camera
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Get the main camera if not assigned
        }
    }
    // Update is called once per frame
    void Update()
    {
        // follow the player just vertically
        if (player != null)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, player.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }
}
