using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PyramidAgent : Agent
{
    public GameObject area;
    private PyramidArea m_MyArea;
    private Rigidbody m_AgentRb;
    private PyramidSwitch m_SwitchLogic;
    public GameObject areaSwitch;
    public bool useVectorObs;
    public Vector3 agentLocalSpawnPosition;
    public Vector3 switchLocalSpawnPosition;
    public GameObject roadcrossPrefab;

    [SerializeField]
    private Animator agentAnimator;

    [SerializeField]
    private float animationSpeed = 5f;

    [SerializeField]
    private GameObject newPlayerPrefab;

    private bool hasReachedGoal = false;
    private const float moveSpeed = 5f;
    private const float rotateSpeed = 150f;
    private Vector3 targetPosition;
    Color orange = new Color(1.0f, 0.5f, 0.0f);
    private Vector3 lastPosition;
    private float stuckTimeThreshold = 2.0f; // Time in seconds before considering stuck
    private float stuckTimer = 0f; // Timer to track time of being stuck
    private bool isStuck = false; // To track whether the agent is currently stuck

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_MyArea = area.GetComponent<PyramidArea>();
        m_SwitchLogic = areaSwitch.GetComponent<PyramidSwitch>();
        agentAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics;

        lastPosition = transform.position; // Initialize last known position
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            sensor.AddObservation(m_SwitchLogic.GetState());
            sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        if (hasReachedGoal) return;

        // Check if the agent is stuck
        DetectAndHandleStuck();

        // If the agent is stuck, do not try to find the switch, just focus on unsticking
        if (isStuck)
        {
            return; // Skip the rest of the movement logic while the agent is stuck
        }

        // Only if the agent is not stuck, perform the raycast to find the switch
        RaycastHit hit;
        float rayLength = 100f; // Max ray length for better detection
        int rayCount = 36; // Number of rays in 360 degrees
        float angleStep = 360f / rayCount; // Angle between each ray

        bool switchDetected = false;

        // Loop through 360 degrees and cast a ray in each direction
        for (int i = 0; i < rayCount; i++)
        {
            // Calculate the direction for the current ray
            float currentAngle = i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            // Debug.DrawRay(transform.position, rayDirection * rayLength, orange);
            // Perform the raycast
            if (Physics.Raycast(transform.position, rayDirection, out hit, rayLength))
            {
                if (hit.collider.CompareTag("switchOn"))
                {
                    // Move directly toward the detected switch
                    targetPosition = hit.point;
                    switchDetected = true;
                    break; // Stop casting further rays once a switch is detected
                }
            }
        }

        if (switchDetected)
        {
            MoveDirectlyTowardTarget(targetPosition); // Keep moving toward the detected switch
        }
        else
        {
            // Regular wandering behavior if no switch is detected
            var action = act[0];
            Vector3 moveDir = Vector3.zero;
            float rotation = 0;

            switch (action)
            {
                case 1:
                    moveDir = transform.forward * moveSpeed;
                    break;
                case 2:
                    moveDir = -transform.forward * moveSpeed;
                    break;
                case 3:
                    rotation = rotateSpeed * Time.deltaTime;
                    break;
                case 4:
                    rotation = -rotateSpeed * Time.deltaTime;
                    break;
            }

            // Move and rotate agent
            m_AgentRb.velocity = new Vector3(moveDir.x, m_AgentRb.velocity.y, moveDir.z);
            transform.Rotate(0, rotation, 0);

            // Animate agent movement
            AnimateAgent(m_AgentRb.velocity);
        }
    }

    private void MoveDirectlyTowardTarget(Vector3 targetPos)
    {
        // Calculate the direction towards the target
        Vector3 directionToTarget = (targetPos - transform.position).normalized;

        // Apply velocity directly toward the target without any rotation
        m_AgentRb.velocity = directionToTarget * moveSpeed;

        // Rotate the agent to face the target smoothly
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

        // Animate agent movement
        AnimateAgent(m_AgentRb.velocity);

        // Check if agent is stuck after detecting the target
        DetectAndHandleStuck();  // Call stuck detection while moving to target
    }

    private void AnimateAgent(Vector3 velocity)
    {
        float forwardSpeed = Vector3.Dot(velocity, transform.forward);
        float rotationSpeed = m_AgentRb.angularVelocity.y;

        agentAnimator.SetFloat("ver", Mathf.Lerp(agentAnimator.GetFloat("ver"), forwardSpeed, Time.deltaTime * animationSpeed));
        agentAnimator.SetFloat("hor", Mathf.Lerp(agentAnimator.GetFloat("hor"), rotationSpeed, Time.deltaTime * animationSpeed));
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (hasReachedGoal) return;

        AddReward(-1f / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.D)) discreteActionsOut[0] = 3;
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        if (Input.GetKey(KeyCode.A)) discreteActionsOut[0] = 4;
        if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 2;
    }

    public override void OnEpisodeBegin()
    {
        if (!hasReachedGoal)
        {
            RespawnAgentAndSwitch();
        }
        lastPosition = transform.position; // Reset last known position
        stuckTimer = 0f; // Reset stuck timer
        isStuck = false; // Reset stuck state
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("switchOn") && !hasReachedGoal)
        {
            hasReachedGoal = true;
            SetReward(3f);
            DestroyAndSpawnNewAgent();
        }
        else if (collision.gameObject.CompareTag("switchOn"))
        {
            SetReward(2f);
            EndEpisode();
        }
    }

    public void OnSwitchActivated()
    {
        if (!hasReachedGoal) // Ensure this only happens if the agent hasn't reached the goal
        {
            SetReward(1f);
            RespawnAgentAndSwitch();
        }
    }

    private void RespawnAgentAndSwitch()
    {
        m_AgentRb.velocity = Vector3.zero;
        m_AgentRb.angularVelocity = Vector3.zero;

        transform.localPosition = agentLocalSpawnPosition;
        transform.localRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360f), 0f);

        m_SwitchLogic.ResetSwitch(switchLocalSpawnPosition);
        lastPosition = transform.position; // Reset last known position after respawn
        stuckTimer = 0f; // Reset stuck timer
        isStuck = false; // Reset stuck state
    }

    private int rightRotationAttempts = 0; // Track how many right rotations have been attempted

    private void DetectAndHandleStuck()
    {
        // Calculate distance moved since last frame
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        // If distance moved is small, increase the stuck timer
        if (distanceMoved < 0.05f)
        {
            stuckTimer += Time.deltaTime;

            // If the stuck timer exceeds the threshold, try rotating to the right and moving forward
            if (stuckTimer > stuckTimeThreshold)
            {
                isStuck = true; // Mark the agent as stuck

                // Rotate the agent to the right by 90 degrees
                transform.Rotate(0, 90f, 0);  // Rotate right (90 degrees)

                // Take a step forward after rotating
                m_AgentRb.velocity = transform.forward * (moveSpeed); // Move forward with full speed for a step

                // Increment the right rotation attempts
                rightRotationAttempts++;

                // If the agent has completed 4 right rotations (full 360 degrees), reset attempts
                if (rightRotationAttempts >= 4)
                {
                    rightRotationAttempts = 0; // Reset the counter after a full turn
                }

                stuckTimer = 0f; // Reset the stuck timer after rotating and stepping forward
            }
        }
        else
        {
            // If the agent has moved, reset the stuck timer and right rotation attempts
            stuckTimer = 0f;
            isStuck = false; // The agent is no longer stuck
            rightRotationAttempts = 0; // Reset the right rotation attempts after moving
        }

        // Update the last known position
        lastPosition = transform.position;
    }


    private void DestroyAndSpawnNewAgent()
    {
        if (roadcrossPrefab != null)
        {
            GameObject newPlayer = Instantiate(newPlayerPrefab, transform.position, transform.rotation);
            newPlayer.transform.SetParent(roadcrossPrefab.transform);
        }
        else
        {
            Debug.Log("Reached!");
        }

        Destroy(gameObject);
    }
}
