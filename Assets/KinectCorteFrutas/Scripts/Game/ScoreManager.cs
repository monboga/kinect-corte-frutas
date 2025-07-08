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

    [Header("Componentes de Kinect")]
    public GameObject kinecInputController; // referencia al objeto que tiene el cursos.
    public GameObject handCursor; // objeto visual del cursor


    [Header("Configuracion del juego")]
    public int maxFruits = 20;
    public int pointsPerFruit = 5;

    // referencia al gestor del Kinect
    private BodySourceManager bodySourceManager;
    private BodySourceView bodyView; // referencia al script que dibuja las manos 3D.

    private int score = 0;
    private int fruitsRemaining; // Contador de frutas.
    public float timeRemaining = 30; // Tiempo inicial en segundos

    private float initialTime; // guardaremos el tiempo inicial para poder resetearlo

    // Awake se llama antes de cualquier metodo start
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // guardamos el tiempo inicial una sola vez
        initialTime = timeRemaining;

        // buscamos el BodySourceManager al inciar.
        // bodySourceManager = FindObjectOfType<BodySourceManager>();
        // ahora

        bodySourceManager = BodySourceManager.instance;

        // usando esa instancia, buscamos el componente BodySourceView en sus hijos.
        if(bodySourceManager != null)
        {
            bodyView = bodySourceManager.bodyView;

        }

        if(bodyView == null)
        {
            Debug.LogError("ERROR: la variable 'bodyview' no ha sido asignada en elprefab de KinectManager");
            return;

        }

        // estado inicial del juego: Mostrar las instrucciones
        SetupInitialState();

    }

    private void SetupInitialState()
    {
        currentState = GameState.Instructions;

        // resetear las variables
        score = 0;
        fruitsRemaining = maxFruits;
        timeRemaining = initialTime;

        // resetar el UI
        scoreText.text = "Score: 0";
        DisplayTime(timeRemaining);
        instructionsPanel.SetActive(true);
        waitingText.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);

        // configurar visibilidad de manos/cursor para el menu de instrucciones
        bodyView.enabled = false;
        kinecInputController.SetActive(true);
        handCursor.SetActive(true);

        // llamamos alnuevo metodo Reset del FruitManager
        FindObjectOfType<FruitManager>().Reset();
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

        // al empezar el juego, invertimos los papeles.
        bodyView.enabled = true;
        kinecInputController.SetActive(false);
        handCursor.SetActive(false); // ocultamos el objeto del cursor

        // Le damos la orden al FruitManager de que empiece a crear las frutas
        FindObjectOfType<FruitManager>()?.StartCoroutine("CreateFruitsGradually");
    }

    public void FruitCut()
    {
        if (currentState != GameState.Playing) return;

        // logica de puntaje
        AddScore(pointsPerFruit);

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

        // al final del juego, apagamos todo
        bodyView.enabled = false;
        // reactivamos el cursor 2D para el menu de Game Over
        kinecInputController.SetActive(true);
        handCursor.SetActive(true);

        FindObjectOfType<FruitManager>()?.DestroyAllFruits();
        resultText.text = hasWon ? "¡GANASTE!" : "¡SE ACABO EL TIEMPO!";
    }

    public void RestartGame()
    {
        SetupInitialState();
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
        Debug.Log("Valor del score: " + score + " y valor del points: " + points);
        score += points;
        scoreText.text = "Score: " + score.ToString();
    }

}
