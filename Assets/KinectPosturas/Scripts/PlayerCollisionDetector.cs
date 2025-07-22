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
        // Debug mejorado para ver exactamente qué está colisionando
        if (enableDebugMode)
        {
            Debug.Log($"[COLLISION] Trigger detectado con: {other.name}");
            Debug.Log($"[COLLISION] Tag del objeto: {other.tag}");
            Debug.Log($"[COLLISION] Posición del jugador: {transform.position}");
            Debug.Log($"[COLLISION] Posición del objeto: {other.transform.position}");
            Debug.Log($"[COLLISION] Bounds del collider: {other.bounds}");
        }

        // Verificar si el objeto tiene el tag correcto
        if (other.CompareTag("WallPart"))
        {
            totalCollisions++;

            // Clasificar la colisión según el nombre del objeto
            string objectName = other.name.ToLower();

            if (objectName.Contains("top"))
            {
                topWallCollisions++;
                if (enableDebugMode) Debug.Log("[COLLISION] ¡Colisión con muro SUPERIOR!");
            }
            else if (objectName.Contains("center"))
            {
                centerWallCollisions++;
                if (enableDebugMode) Debug.Log("[COLLISION] ¡Colisión con muro CENTRAL!");
            }
            else if (objectName.Contains("left"))
            {
                leftWallCollisions++;
                if (enableDebugMode) Debug.Log("[COLLISION] ¡Colisión con muro IZQUIERDO!");
            }
            else if (objectName.Contains("right"))
            {
                rightWallCollisions++;
                if (enableDebugMode) Debug.Log("[COLLISION] ¡Colisión con muro DERECHO!");
            }

            Debug.Log($"[STATS] Total de colisiones: {totalCollisions}");
        }
        else
        {
            if (enableDebugMode)
            {
                Debug.Log($"[WARNING] Objeto sin tag 'WallPart': {other.name} (Tag: {other.tag})");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (enableDebugMode && other.CompareTag("WallPart"))
        {
            Debug.Log($"[COLLISION] Salió del trigger: {other.name}");
        }
    }

    void OnGUI()
    {
        // Configurar estilo de texto
        GUIStyle style = new GUIStyle();
        style.fontSize = fontSize;
        style.normal.textColor = textColor;
        style.fontStyle = FontStyle.Bold;

        // Mostrar estadísticas detalladas
        int yOffset = 40;
        int lineHeight = fontSize + 10;

        GUI.Label(new Rect(40, yOffset, 1000, 200), $"COLISIONES TOTALES: {totalCollisions}", style);
        yOffset += lineHeight;

        GUI.Label(new Rect(40, yOffset, 1000, 200), $"Superior: {topWallCollisions}", style);
        yOffset += lineHeight;

        GUI.Label(new Rect(40, yOffset, 1000, 200), $"Central: {centerWallCollisions}", style);
        yOffset += lineHeight;

        GUI.Label(new Rect(40, yOffset, 1000, 200), $"Izquierdo: {leftWallCollisions}", style);
        yOffset += lineHeight;

        GUI.Label(new Rect(40, yOffset, 1000, 200), $"Derecho: {rightWallCollisions}", style);

        // Mostrar información de debug si está activado
        if (enableDebugMode)
        {
            yOffset += lineHeight * 2;
            style.fontSize = fontSize - 10;
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(40, yOffset, 1000, 200), "DEBUG MODE ACTIVADO", style);
        }
    }
}