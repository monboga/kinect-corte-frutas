using UnityEngine;

public class WallCollisionTracker : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        GlobalCollisionCounter.Instance.AddCollision();
    }
}