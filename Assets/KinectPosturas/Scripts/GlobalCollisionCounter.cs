using UnityEngine;
using UnityEngine.UI;

public class GlobalCollisionCounter : MonoBehaviour
{
    public static GlobalCollisionCounter Instance;

    private int totalCollisions = 0;

    [Header("UI")]
    public Text collisionText;  // Asigna desde el Inspector

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCollision()
    {
        totalCollisions++;
        Debug.Log("Colision detectada, numero de colisiones: " + totalCollisions);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (collisionText != null)
            collisionText.text = "Total de colisiones: " + totalCollisions;
    }
}