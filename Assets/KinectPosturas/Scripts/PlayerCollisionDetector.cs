using UnityEngine;

public class PlayerCollisionDetector : MonoBehaviour
{
    private int collisionCount = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WallPart"))//other.gameObject.name.Contains("NewWall")
        {
            collisionCount++;
            Debug.Log("Toque detectado con muro. Total de toques: " + collisionCount);
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 40;
        style.normal.textColor = Color.red;
        GUI.Label(new Rect(20, 20, 500, 100), "Toques: " + collisionCount, style);
    }
}