using UnityEngine;

public class JointCollisionDetector : MonoBehaviour
{
    public static int TotalCollisions { get; private set; } = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WallPart"))
        {
            TotalCollisions++;
            Debug.Log("Colisión en " + gameObject.name + " con " + other.name + ". Total: " + TotalCollisions);
        }
    }
}