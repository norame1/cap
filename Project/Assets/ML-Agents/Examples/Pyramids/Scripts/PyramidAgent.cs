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
    public GameObject crossTheRoadAgent; // Reference to Cross the Road Agent
    public GameObject crossTheRoadGoal; // Reference to Cross the Road Goal
    public bool useVectorObs;
    public Vector3 agentLocalSpawnPosition;
    public Vector3 switchLocalSpawnPosition;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_MyArea = area.GetComponent<PyramidArea>();
        m_SwitchLogic = areaSwitch.GetComponent<PyramidSwitch>();

        // Ensure Cross the Road Agent is initially deactivated
        crossTheRoadAgent.SetActive(false);
        crossTheRoadGoal.SetActive(false);
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
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        m_AgentRb.AddForce(dirToGo * 0.75f, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        AddReward(-1f / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    public override void OnEpisodeBegin()
    {
        RespawnAgentAndSwitch();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("switchOn"))
        {
            SetReward(2f);
            ActivateCrossTheRoadAgent();
            EndEpisode();
        }
    }

    public void OnSwitchActivated()
    {
        SetReward(1f);
        RespawnAgentAndSwitch();
    }

    private void RespawnAgentAndSwitch()
    {
        m_AgentRb.velocity = Vector3.zero;
        m_AgentRb.angularVelocity = Vector3.zero;

        // Set local spawn position for the agent
        transform.localPosition = agentLocalSpawnPosition;
        transform.localRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360f), 0f);

        // Set local spawn position for the switch
        m_SwitchLogic.ResetSwitch(switchLocalSpawnPosition);
    }

    private void ActivateCrossTheRoadAgent()
    {
        // Deactivate Pyramid Agent and its goal
        gameObject.SetActive(false);
        areaSwitch.SetActive(false);

        // Activate Cross the Road Agent and its goal
        crossTheRoadAgent.SetActive(true);
        crossTheRoadGoal.SetActive(true);
    }
}
