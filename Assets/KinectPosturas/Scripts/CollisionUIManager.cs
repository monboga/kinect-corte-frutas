using UnityEngine;

public class CollisionUIManager : MonoBehaviour
{
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 60;
        style.normal.textColor = Color.yellow;
        GUI.Label(new Rect(40, 40, 1000, 200), "Toques: " + JointCollisionDetector.TotalCollisions, style);
    }
}