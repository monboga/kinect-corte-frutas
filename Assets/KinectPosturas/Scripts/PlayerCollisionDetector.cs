using UnityEngine;

public class PlayerCollisionDetector : MonoBehaviour
{
    [Header("Collision Settings")]
    public bool enableDebugMode = true;

    [Header("Collision Statistics")]
    public int totalCollisions = 0;
    public int topWallCollisions = 0;
    public int centerWallCollisions = 0;
    public int leftWallCollisions = 0;
    public int rightWallCollisions = 0;

    [Header("UI Settings")]
    public int fontSize = 40;
    public Color textColor = Color.red;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entro en contacto con: " + other.name);
        if (other.CompareTag("WallPart"))//other.gameObject.name.Contains("NewWall")
        {
            collisionCount++;
            Debug.Log("Toque detectado con muro. Total de toques: " + collisionCount);
            //Debug.Log("COLISION detectada en: " + gameObject.name + " con " + other.name);
        }
    }

    void OnGUI()
    {
        // Configurar estilo de texto
        GUIStyle style = new GUIStyle();
        style.fontSize = 80;//40
        style.normal.textColor = Color.red;
        GUI.Label(new Rect(40, 40, 1000, 200), "Toques: " + collisionCount, style);//20, 20, 500, 100
    }
}