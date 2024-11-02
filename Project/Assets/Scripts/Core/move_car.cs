using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public float speed = 10f; // Speed of the car

    void Update()
    {
        // Move the car in the global Z direction at a constant speed
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);
    }
}
