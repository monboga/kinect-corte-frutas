using UnityEngine;

public class PlayerCollisionDetector : MonoBehaviour
{
    private int collisionCount = 0;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entro en contacto con: " + other.name);
        if (other.CompareTag("WallPart"))//other.gameObject.name.Contains("NewWall")
        {
            collisionCount++;
            Debug.Log("Toque detectado con muro. Total de toques: " + collisionCount);
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 80;//40
        style.normal.textColor = Color.red;
        GUI.Label(new Rect(40, 40, 1000, 200), "Toques: " + collisionCount, style);//20, 20, 500, 100
    }
}