/// <summary>
/// Movement that moves towards player using an offset position. Can rotate towards player and also
/// optionally lock on when alight with player. Derived from MoveTowardsPlayer.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowardsPlayerOffset : MoveTowardsPlayer
{
    [Header("Player Offset")]

    [SerializeField]
    [Tooltip("Move towards the player using offset direction and distance.")]
    private Vector3 offSet;

    [SerializeField]
    private float offSetDistance = 0.5f;

    [SerializeField]
    [Tooltip("Dampens speed as object approaches offSet position if dampenDistance > 1.0")]
    private float dampenDistance = 1.0f;
    [SerializeField]
    private float minSpeed = 0.5f;
    [SerializeField]
    [Tooltip("Time delay to update offset position.")]
    private float updateOffsetPositionDelay = 1.0f;

    private float originalSpeed;
    private Vector3 moveToPosition;
    private float updateOffsetTimer;
    protected override void Start()
    {
        base.Start();
        offSet.Normalize();
        originalSpeed = speed;
        // Set original offset position to move towards.
        moveToPosition = GameManager.PlayerTransform.position + (offSet * offSetDistance);
        updateOffsetTimer = 0.0f;
    }
    public override void UpdatePosition()
    {
        float distance = 0.0f;

        // Move towards the offset position.
        transform.position = Vector3.MoveTowards(transform.position, moveToPosition, speed * Time.deltaTime);

        distance = Vector3.Distance(transform.position, moveToPosition);

        // Adjust speed based on distance if desired (dampenDistance > 1.0f)
        if (dampenDistance > 1.0f && distance < dampenDistance)
        {
           speed = Mathf.Max(speed * distance / dampenDistance, minSpeed);
        }
        else
        {
            moveToPosition = GameManager.PlayerTransform.position + (offSet * offSetDistance);
            speed = originalSpeed;
        }

        if (distance < 1f) // dampending the distance
        {
            updateOffsetTimer += Time.deltaTime;
            // Reset the position to move towards.
            moveToPosition = GameManager.PlayerTransform.position + (offSet * offSetDistance);
            updateOffsetTimer = 0.0f;
        }

    }
}
