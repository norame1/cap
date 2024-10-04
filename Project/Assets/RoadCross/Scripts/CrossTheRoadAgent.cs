using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections;

public class CrossTheRoadAgent : Agent
{
    [SerializeField]
    private float speed = 5.0f;

    [SerializeField, Tooltip("This is the offset amount from the local agent position the agent will move on every step")]
    private float stepAmount = 5.0f;

    [SerializeField]
    private TextMeshProUGUI rewardValue = null;

    [SerializeField]
    private TextMeshProUGUI episodesValue = null;

    [SerializeField]
    private TextMeshProUGUI stepValue = null;

    [SerializeField]
    private Material successMaterial;

    [SerializeField]
    private Material failureMaterial;

    [SerializeField]
    private Animator agentAnimator; // Reference to the Animator component for controlling animations

    private CrossTheRoadGoal goal = null;

    private float overallReward = 0;

    private float overallSteps = 0;

    private Vector3 moveTo = Vector3.zero;

    private Vector3 originalPosition = Vector3.zero;

    private Rigidbody agentRigidbody;

    private bool moveInProgress = false;

    private int direction = 0;

    private float[] spawnPositionsX = { 0.01f, 5.01f, 10.01f, -5.01f, -10.01f };

    public enum MoveToDirection
    {
        Idle,
        Left,
        Right,
        Forward
    }

    // Commented out since it's not being used, preventing a warning
    public MoveToDirection moveToDirection = MoveToDirection.Idle;

    // Use the "new" keyword to avoid the CS0114 error
    private new void Awake()
    {
        goal = transform.parent.GetComponentInChildren<CrossTheRoadGoal>();
        originalPosition = new Vector3(0f, transform.localPosition.y, transform.localPosition.z);
        agentRigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Randomly select a spawn position
        float randomX = spawnPositionsX[Random.Range(0, spawnPositionsX.Length)];

        // Set the new spawn position
        Vector3 newSpawnPosition = new Vector3(randomX, originalPosition.y, originalPosition.z);

        // Update the agent's position and moveTo
        transform.localPosition = moveTo = newSpawnPosition;
        transform.localRotation = Quaternion.identity;
        agentRigidbody.velocity = Vector3.zero;

        // Reset the animation state
        agentAnimator.SetFloat("ver", 0f);
        agentAnimator.SetFloat("hor", 0f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 3 observations - x, y, z
        sensor.AddObservation(transform.localPosition);

        // 3 observations - x, y, z
        sensor.AddObservation(goal.transform.localPosition);
    }

    void Update()
    {
        if (!moveInProgress)
            return;

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, moveTo, Time.deltaTime * speed);

        if (Vector3.Distance(transform.localPosition, moveTo) <= 0.00001f)
        {
            moveInProgress = false;
            agentAnimator.SetFloat("ver", 0f);
            agentAnimator.SetFloat("hor", 0f); // Stop the movement in animation
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (moveInProgress)
            return;

        direction = actionBuffers.DiscreteActions[0];

        // Check if the agent is on the road
        bool isOnRoad = transform.localPosition.z > 5;

        if (isOnRoad)
        {
            // On the road, only allow forward movement or idle
            switch (direction)
            {
                case 0: // idle
                    moveTo = transform.localPosition;
                    // moveToDirection = MoveToDirection.Idle;
                    agentAnimator.SetFloat("ver", 0f); // Idle animation
                    break;
                case 3: // forward
                    moveTo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + stepAmount);
                    // moveToDirection = MoveToDirection.Forward;
                    moveInProgress = true;
                    agentAnimator.SetFloat("ver", 1f); // Forward movement animation
                    break;
            }
        }
        else
        {
            // Off the road, allow all movements
            switch (direction)
            {
                case 0: // idle
                    moveTo = transform.localPosition;
                    moveToDirection = MoveToDirection.Idle;
                    agentAnimator.SetFloat("ver", 0f); // Idle animation
                    break;
                case 1: // left
                    moveTo = new Vector3(transform.localPosition.x - stepAmount, transform.localPosition.y, transform.localPosition.z);
                    moveToDirection = MoveToDirection.Left;
                    moveInProgress = true;
                    agentAnimator.SetFloat("hor", -1f); // Left movement animation
                    break;
                case 2: // right
                    moveTo = new Vector3(transform.localPosition.x + stepAmount, transform.localPosition.y, transform.localPosition.z);
                    moveToDirection = MoveToDirection.Right;
                    moveInProgress = true;
                    agentAnimator.SetFloat("hor", 1f); // Right movement animation
                    break;
                case 3: // forward
                    moveTo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + stepAmount);
                    moveToDirection = MoveToDirection.Forward;
                    moveInProgress = true;
                    agentAnimator.SetFloat("ver", 1f); // Forward movement animation
                    break;
            }
        }
    }

    public void GivePoints()
    {
        AddReward(1.0f);
        UpdateStats();
        EndEpisode();
        StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f));
    }

    public void TakeAwayPoints()
    {
        AddReward(-0.025f);
        UpdateStats();
        EndEpisode();
        StartCoroutine(SwapGroundMaterial(failureMaterial, 0.5f));
    }

    private void UpdateStats()
    {
        overallReward += GetCumulativeReward();
        overallSteps += StepCount;
        rewardValue.text = $"{overallReward.ToString("F2")}";
        episodesValue.text = $"{CompletedEpisodes}";
        stepValue.text = $"{overallSteps}";
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //idle
        discreteActionsOut[0] = 0;

        //move left
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 1;
        }

        //move right
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 2;
        }

        //move forward
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 3;
        }
    }

    private IEnumerator SwapGroundMaterial(Material material, float duration)
    {
        // Implement this method based on your requirements
        yield return new WaitForSeconds(duration);
    }
}
