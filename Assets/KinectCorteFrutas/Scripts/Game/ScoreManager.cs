using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // necesario para el boton
using UnityEngine.SceneManagement; // necesario para reinciar la escena
using TMPro; // importante para usar TextMeshPro
using System.Linq; // importante para la deteccion del cuerpo

public class ScoreManager : MonoBehaviour
{

    // Enum para controlar el estado del juego
    public enum GameState { Instructions, WaitingForPlayer, Playing, GameOver }
    public GameState currentState;

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
    public GameObject instructionsPanel; // Panel de Instrucciones
    public TextMeshProUGUI waitingText; // texto de espera.

    [Header("Configuracion del juego")]
    public int maxFruits = 20;

    // referencia al gestor del Kinect
    private BodySourceManager bodySourceManager;

    private int score = 0;
    private int fruitsRemaining; // Contador de frutas.
    public float timeRemaining = 30; // Tiempo inicial en segundos

    // Awake se llama antes de cualquier metodo start
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // buscamos el BodySourceManager al inciar.
        bodySourceManager = FindObjectOfType<BodySourceManager>();

        // estado inicial del juego: Mostrar las instrucciones
        currentState = GameState.Instructions;
        instructionsPanel.SetActive(true);
        waitingText.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);

        // preparamos los contadores pero no los iniciamos
        fruitsRemaining = maxFruits;
        // inicializa el texto del puntaje
        scoreText.text = "Score: 0";
        DisplayTime(timeRemaining);

    }

    // Update is called once per frame
    void Update()
    {
        // Solo revisamos el estado si el juego no ha termiando
        if(currentState == GameState.WaitingForPlayer)
        {
            // Primero, nos aseguramos de que el manager exista.
            if(bodySourceManager != null )
            {
                // obtenemos los datos del cuerpo en una variable
                var bodies = bodySourceManager.GetData();

                // ahora, ANTES de usar los datos, nos aseguramos de que no sean nulos.
                // esta es la comprobacion que previene el error.
                if(bodies != null && bodies.Any(b => b.IsTracked))
                {
                    // si todo es valido y hay un cuerpo, iniciamos el juego.
                    StartGame();

                }


            }
            
        }
        else if (currentState == GameState.Playing)
        {
            // la logica del timer solo corre cuando estamos jugando
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                DisplayTime(timeRemaining);
                EndGame(false);
            }
        }

    }

    // se llama con el boton de "¡Entendido!" del panel de instrucciones
    public void DismissInstructions()
    {
        instructionsPanel.SetActive(false);
        waitingText.gameObject.SetActive(true);
        currentState = GameState.WaitingForPlayer;
    }

    // inicia la logia principal del juego
    void StartGame()
    {
        currentState = GameState.Playing;
        waitingText.gameObject.SetActive(false);

        // Le damos la orden al FruitManager de que empiece a crear las frutas
        FruitManager fruitManager = FindObjectOfType<FruitManager>();
        if (fruitManager != null)
        {
            StartCoroutine(fruitManager.CreateFruitsGradually());
        }
    }

    public void FruitCut()
    {
        if (currentState != GameState.Playing) return;
        fruitsRemaining--;

        if(fruitsRemaining <= 0)
        {
            EndGame(true);
        }
    }

    void EndGame(bool hasWon)
    {
        currentState = GameState.GameOver;
        gameOverPanel.SetActive(true);
        FindObjectOfType<FruitManager>()?.DestroyAllFruits();
        resultText.text = hasWon ? "¡GANASTE!" : "¡SE ACABO EL TIEMPO!";
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
