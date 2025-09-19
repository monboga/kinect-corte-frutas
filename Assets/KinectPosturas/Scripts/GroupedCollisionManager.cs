using System.Collections.Generic;
using UnityEngine;

public class GroupedCollisionManager : MonoBehaviour
{
    public static GroupedCollisionManager Instance { get; private set; }

    private Dictionary<BodyRegion, Collider> activeCollisions = new Dictionary<BodyRegion, Collider>();
    private int groupedCollisionCount = 0;

    [SerializeField] private int vidas = 10; // Vidas iniciales ahora son 10
    private bool isGameOver = false;

    private bool gameStarted = false; // Control para verificar si el juego ha comenzado

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            vidas = 10; // Forzar valor por seguridad
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método público para comenzar el juego cuando se presiona el botón
    public void StartGame()
    {
        if (!gameStarted && !isGameOver)
        {
            gameStarted = true; // Cambiar el estado de juego a iniciado
            Debug.Log("¡El juego ha comenzado!");
        }
    }

    public void RegisterCollision(BodyRegion region, Collider wallPart)
    {
        if (!isGameOver && !activeCollisions.ContainsKey(region))
        {
            activeCollisions[region] = wallPart;
            groupedCollisionCount++;

            vidas--;
            Debug.Log("¡Colisión agrupada! Vidas restantes: " + vidas);

            if (vidas <= 0)
            {
                isGameOver = true;
                StopWallMovement();
            }
        }
    }

    public void UnregisterCollision(BodyRegion region, Collider wallPart)
    {
        if (activeCollisions.ContainsKey(region) && activeCollisions[region] == wallPart)
        {
            activeCollisions.Remove(region);
        }
    }

    public void StopWallMovement()
    {
        SingleWallManager wallManager = FindObjectOfType<SingleWallManager>();
        if (wallManager != null)
            wallManager.StopWallMovement();
    }

    void OnGUI()
    {
        // Solo mostrar GUI si el juego ha comenzado
        if (gameStarted)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 40;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(20, 20, 400, 50), "Vidas: " + vidas, style);
            GUI.Label(new Rect(20, 80, 400, 50), "Colisiones agrupadas: " + groupedCollisionCount, style);

            if (isGameOver)
            {
                GUIStyle gameOverStyle = new GUIStyle(style);
                gameOverStyle.fontSize = 100;
                gameOverStyle.alignment = TextAnchor.MiddleCenter;
                gameOverStyle.normal.textColor = Color.red;

                GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 100, 600, 200), "GAME OVER", gameOverStyle);
            }
        }
    }
}