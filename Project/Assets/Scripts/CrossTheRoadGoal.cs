using UnityEngine;

public class CrossTheRoadGoal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player"))
        {
            other.gameObject.GetComponent<CrossTheRoadAgent>().GivePoints();
        }
        else if (other.CompareTag("enemy"))
        {
            other.gameObject.GetComponent<CrossTheRoadAgent>().TakeAwayPoints();
        }
    }
}
