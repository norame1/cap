using UnityEngine;

public class CrossTheRoadGoal : MonoBehaviour
{
    private CrossTheRoadAgent agent = null;

    void Awake()
    {
        CacheAgent();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.tag.ToLower() == "player")
        {
            if (agent == null)
            {
                CacheAgent();
            }

            Debug.Log("Points earned as road was crossed");
            agent.GivePoints();
        }
    }

    private void CacheAgent()
    {
        agent = transform.parent.GetComponentInChildren<CrossTheRoadAgent>();
    }
}
