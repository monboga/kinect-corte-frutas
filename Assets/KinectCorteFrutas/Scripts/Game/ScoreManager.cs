using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // necesario para el boton
using UnityEngine.SceneManagement; // necesario para reinciar la escena
using TMPro; // importante para usar TextMeshPro

public class ScoreManager : MonoBehaviour
{

    // Instancia estatica para acceder facilmente desde otros scripts
    public static ScoreManager instance;

    [Header("Componentes de UI")]
    // referencia al texto de la UI que mostrará el puntaje.
    public TextMeshProUGUI scoreText;
    // variables del temporizador
    public TextMeshProUGUI timerText; // referencia al texto del timer
    public GameObject gameOverPanel; // refrencia a nuestro panel
    public TextMeshProUGUI resultText; // El texto de "Ganaste" o "Perdiste"
    public Button restartButton; // el boton de reinicio

    [Header("Configuracion del juego")]
    public int maxFruits = 20;

    private int score = 0;
    private int fruitsRemaining; // Contador de frutas.
    public float timeRemaining = 30; // Tiempo inicial en segundos
    public bool timerIsRunning = true; // Para conocer si el timer debe correr

    // Awake se llama antes de cualquier metodo start
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // tomamos el valor de nuestra variable de maxFruits
        fruitsRemaining = maxFruits;
        // inicializa el texto del puntaje
        scoreText.text = "Score: " + score.ToString();
        gameOverPanel.SetActive(false); // nos aseguramos que el panel este oculto

        // inicializa el estado del timer
        timerIsRunning = true;

    }

    // Update is called once per frame
    void Update()
    {
        // logica del Temporizador
        if(timerIsRunning)
        {
            if(timeRemaining > 0)
            {
                // restamos el tiempo que ha pasado desde el ultimo frame
                timeRemaining -= Time.deltaTime;
                // actualizamos el texto en pantalla
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("¡El tiempo se ha acabado!");
                timeRemaining = 0;
                DisplayTime(timeRemaining);
                EndGame(false);

                // aqui se podria agregar la logica del "Game Over"
            }
        }
    }

    public void FruitCut()
    {
        if (!timerIsRunning) return;
        fruitsRemaining--;

        if(fruitsRemaining <= 0)
        {
            EndGame(true);
        }
    }

    void EndGame(bool hasWon)
    {
        timerIsRunning = false;
        gameOverPanel.SetActive(true);

        FruitManager fruitManager = FindObjectOfType<FruitManager>();
        if(fruitManager != null)
        {
            fruitManager.DestroyAllFruits();
        }

        if(hasWon)
        {
            resultText.text = "¡GANASTE!";

        }
        else
        {
            resultText.text = "SE ACABO EL TIEMPO";
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // metodo para mostrar el tiempo formateado
    void DisplayTime(float timeToDisplay)
    {
        // Sumamos 1 para que el display no muestre 0 cuando aún queda una fraccion de segundo
        timeToDisplay = Mathf.Max(0, timeToDisplay);

        // Usamos Mathf.FloorToInt para obtener solo el numero de entero de segundos
        int seconds = Mathf.FloorToInt(timeToDisplay);

        // actualizamos el texto
        timerText.text = "Tiempo: " + seconds.ToString();
    }

    // metodo publico para añadir puntos
    public void AddScore(int points)
    {
        score += points;
        scoreText.text = "Score: " + score.ToString();
    }

}
