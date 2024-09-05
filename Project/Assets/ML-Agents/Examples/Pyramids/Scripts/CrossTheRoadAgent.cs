using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections;

public class CrossTheRoadAgent : Agent
{
    [SerializeField]
    private float speed = 50.0f;

    [SerializeField, Tooltip("This is the offset amount from the local agent position the agent will move on every step")]
    private float stepAmount = 1.0f;

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
    private GameObject agentPrefab; // Reference to the agent prefab

    [SerializeField]
    private Animator agentAnimator; // Reference to the Animator component

    private CrossTheRoadGoal goal = null;

    private float overallReward = 0;

    private float overallSteps = 0;

    private Vector3 moveTo = Vector3.zero;

    private Vector3 originalPosition = Vector3.zero;

    private Rigidbody agentRigidbody;

    private bool moveInProgress = false;

    private int direction = 0;

    public enum MoveToDirection
    {
        Idle,
        Left,
        Right,
        Forward
    }

    protected override void Awake()
    {
        originalPosition = transform.localPosition;
        agentRigidbody = GetComponent<Rigidbody>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        goal = transform.parent.GetComponentInChildren<CrossTheRoadGoal>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = moveTo = originalPosition;
        transform.localRotation = Quaternion.identity;
        agentRigidbody.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
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
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (moveInProgress)
            return;

        direction = actionBuffers.DiscreteActions[0];

        switch (direction)
        {
            case 0: // idle
                moveTo = transform.localPosition;
                agentAnimator.SetFloat("ver", 0f);
                agentAnimator.SetFloat("hor", 0f);
                break;
            case 1: // left
                moveTo = new Vector3(transform.localPosition.x - stepAmount, transform.localPosition.y, transform.localPosition.z);
                agentAnimator.SetFloat("hor", -1f);
                agentAnimator.SetFloat("ver", 0f);
                moveInProgress = true;
                break;
            case 2: // right
                moveTo = new Vector3(transform.localPosition.x + stepAmount, transform.localPosition.y, transform.localPosition.z);
                agentAnimator.SetFloat("hor", 1f);
                agentAnimator.SetFloat("ver", 0f);
                moveInProgress = true;
                break;
            case 3: // forward
                moveTo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + stepAmount);
                agentAnimator.SetFloat("ver", 1f);
                agentAnimator.SetFloat("hor", 0f);
                moveInProgress = true;
                break;
        }
    }

    public void GivePoints()
    {
        AddReward(1.0f);
        UpdateStats();
        StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f));
        InstantiateNewAgent();
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
        discreteActionsOut[0] = 0; // idle

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 1; // move left
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 2; // move right
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 3; // move forward
        }
    }

    private IEnumerator SwapGroundMaterial(Material material, float duration)
    {
        // Implement this method based on your requirements
        yield return new WaitForSeconds(duration);
    }

    private void InstantiateNewAgent()
    {
        Instantiate(agentPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
