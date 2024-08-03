using UnityEngine;

public class CrossTheRoadObstacle : MonoBehaviour
{
    public enum ObstacleType
    {
        Wall,
        Tree
    }

    [SerializeField]
    private ObstacleType obstacleType = ObstacleType.Wall;

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

            Debug.Log($"Collision with {obstacleType}");
            agent.TakeAwayPoints();
        }
    }

    private void CacheAgent()
    {
        agent = transform.parent.parent.GetComponentInChildren<CrossTheRoadAgent>();
    }
}
