using UnityEngine;

public class JointCollisionDetector : MonoBehaviour
{
    public static int TotalCollisions { get; private set; } = 0;

    public BodyRegion region;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WallPart"))
        {
            TotalCollisions++;
            Debug.Log("Colisión en " + gameObject.name + " con " + other.name + ". Total: " + TotalCollisions);

            GroupedCollisionManager.Instance?.RegisterCollision(region, other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WallPart"))
        {
            GroupedCollisionManager.Instance?.UnregisterCollision(region, other);
        }
    }
}