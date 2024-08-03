using System;
using System.Collections;
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

    public GameObject agentPrefab; // Reference to the agent prefab to instantiate

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
        RespawnAgentAndSwitch();
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("switchOn"))
        {
            Debug.Log("Collision with switchOn detected.");
            SetReward(2f);
            EndEpisode();

            // Instantiate the new agent
            if (agentPrefab != null)
            {
                Instantiate(agentPrefab, agentDefaultPosition, agentDefaultRotation);
                Debug.Log("New agent instantiated.");

                // Schedule the destruction of the current agent and switch after a short delay
                StartCoroutine(DestroyAfterDelay());
            }
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
        Destroy(areaSwitch);
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

        // Reset agent position and rotation
        transform.position = agentDefaultPosition;
        transform.rotation = agentDefaultRotation;

        // Reset switch position and rotation
        areaSwitch.transform.position = switchDefaultPosition;
        areaSwitch.transform.rotation = switchDefaultRotation;

        m_SwitchLogic.ResetSwitchToDefault();
    }
}
