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

    public GameObject existingDisabledPlayer; // Reference to the disabled player

    private Vector3 newPlayerDefaultPosition;
    private Quaternion newPlayerDefaultRotation;

    private Vector3 agentDefaultPosition;
    private Quaternion agentDefaultRotation;
    private Vector3 switchDefaultPosition;
    private Quaternion switchDefaultRotation;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_MyArea = area.GetComponent<PyramidArea>();
        m_SwitchLogic = areaSwitch.GetComponent<PyramidSwitch>();

        // Store default positions and rotations
        agentDefaultPosition = transform.position;
        agentDefaultRotation = transform.rotation;
        switchDefaultPosition = areaSwitch.transform.position;
        switchDefaultRotation = areaSwitch.transform.rotation;

        // Set default position and rotation for the new player
        newPlayerDefaultPosition = existingDisabledPlayer.transform.position;
        newPlayerDefaultRotation = existingDisabledPlayer.transform.rotation;
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
        m_AgentRb.AddForce(dirToGo * 2f, ForceMode.VelocityChange);
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
        // Only respawn if the switch and agent exist
        if (areaSwitch != null && gameObject != null)
        {
            RespawnAgentAndSwitch();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("switchOn"))
        {
            SetReward(2f);
            EnableExistingPlayer();
            DestroyAgentAndSwitch();
            EndEpisode();
        }
    }

    public void OnSwitchActivated()
    {
        SetReward(1f);
        EnableExistingPlayer();
        DestroyAgentAndSwitch();
        EndEpisode();
    }

    private void RespawnAgentAndSwitch()
    {
        m_AgentRb.velocity = Vector3.zero;
        m_AgentRb.angularVelocity = Vector3.zero;

        // Reset agent position and rotation
        transform.position = agentDefaultPosition;
        transform.rotation = agentDefaultRotation;

        // Reset switch position and rotation
        areaSwitch.transform.position = switchDefaultPosition;
        areaSwitch.transform.rotation = switchDefaultRotation;

        m_SwitchLogic.ResetSwitchToDefault();
    }

    private void DestroyAgentAndSwitch()
    {
        // Destroy the agent and switch
        Destroy(gameObject);
        Destroy(areaSwitch);
    }

    private void EnableExistingPlayer()
    {
        existingDisabledPlayer.transform.position = newPlayerDefaultPosition;
        existingDisabledPlayer.transform.rotation = newPlayerDefaultRotation;
        existingDisabledPlayer.SetActive(true);
    }
}
