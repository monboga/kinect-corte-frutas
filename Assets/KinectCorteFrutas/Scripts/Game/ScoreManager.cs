using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // necesario para el boton
using UnityEngine.SceneManagement; // necesario para reinciar la escena
using TMPro; // importante para usar TextMeshPro
using System.Linq; // importante para la deteccion del cuerpo
using KinectCorteFrutas; // agregado para referencias bien a las clases
public class ScoreManager : MonoBehaviour
{

    // Enum para controlar el estado del juego
    public enum GameState { Instructions, WaitingForPlayer, Playing, GameOver, Paused }
    public GameState currentState;

    // Instancia estatica para acceder facilmente desde otros scripts
    public static ScoreManager instance;

    [Header("Componentes de UI")]
    public TextMeshProUGUI scoreText; // referencia al texto de la UI que mostrará el puntaje.
    // variables del temporizador
    public GameObject playerLostPanel; // referencia al panel de juego en pausa.
    public TextMeshProUGUI timerText; // referencia al texto del timer
    public GameObject gameOverPanel; // refrencia a nuestro panel
    public TextMeshProUGUI resultText; // El texto de "Ganaste" o "Perdiste"
    public Button restartButton; // el boton de reinicio
    public GameObject instructionsPanel; // Panel de Instrucciones
    public TextMeshProUGUI waitingText; // texto de espera.
    public Button backToMenuButtonInstructions; // boton en panel de instrucciones
    public Button backToMenuButtonGameOver; // Boton en panel de game over.
    public TextMeshProUGUI highScoreText; // componente de texto para mostrar el high score.

    // Texto para mostrar el nivel actual
    public TextMeshProUGUI levelText;

    [Header("Componentes de Kinect")]
    public GameObject kinecInputController; // referencia al objeto que tiene el cursos.
    public GameObject handCursor; // objeto visual del cursor

    [Header("Recompensas por completar")]
    public int timeBonus = 15; // Segundos extra al completar todas las frutas

    [Header("Configuracion del juego")]
    public int maxFruits = 20;
    public int pointsPerFruit = 5;

    // Referencia al gestor de frutas para actualizar los limites de rebote
    private FruitManager fruitManager;

    // Variables para la logica de niveles
    public int currentLevel = 1;
    // Valores de margen de rebote para cada nivel
    public float[] bounceMargins = { 0.5f, 0.65f, 0.8f };

    // referencia al gestor del Kinect
    private BodySourceManager bodySourceManager;
    private BodySourceView bodyView; // referencia al script que dibuja las manos 3D.

    private int score = 0;
    private int fruitsRemaining; // Contador de frutas.
    public float timeRemaining = 40; // Tiempo inicial en segundos

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
        // agregamos esta linea para encontrar la referencia al FruitManager
        fruitManager = FindObjectOfType<FruitManager>();

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

        // Comprobamos si el FruitManager se encontro correctamente
        if (fruitManager == null)
        {
            Debug.LogError("Error: no se encontro el FruitManager en la escena");
            return;
        }

        // estado inicial del juego: Mostrar las instrucciones
        SetupInitialState();

        // Llamamos al metodo para el puntaje mas alto al inicio del juego.
        UpdateHighScoreDisplay();

    }

    private void SetupInitialState()
    {
        currentState = GameState.Instructions;

        // resetear las variables
        score = 0;
        currentLevel = 1; // Reiniciamos el nivel a 1 al inicio del juego.
        fruitsRemaining = maxFruits;
        timeRemaining = initialTime;

        // resetar el UI
        scoreText.text = "Score: 0";
        DisplayTime(timeRemaining);
        levelText.text = "Nivel: " + currentLevel; // Mostramos el nivel inicial.
        instructionsPanel.SetActive(true);
        waitingText.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);

        // Detenemos todas las coroutines en el FruitManager
        if (fruitManager != null )
        {
            fruitManager.StopAllCoroutines();
            fruitManager.Reset();
        }

        // configurar visibilidad de manos/cursor para el menu de instrucciones
        bodyView.enabled = false;
        kinecInputController.SetActive(true);
        handCursor.SetActive(true);

        // mostrar boton en instrucciones
        backToMenuButtonInstructions.gameObject.SetActive(true);
        backToMenuButtonGameOver.gameObject.SetActive(true);

        // llamamos alnuevo metodo Reset del FruitManager
        fruitManager.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (bodySourceManager != null)
        {
            var bodies = bodySourceManager.GetData();
            bool isBodyTracked = bodies != null && bodies.Any(b => b.IsTracked);

            if (currentState == GameState.WaitingForPlayer && isBodyTracked)
            {
                StartGame();
            }
            else if (currentState == GameState.Playing && !isBodyTracked)
            {
                PauseGame(); // si se pierde el cuerpo, pausamos.
            }
            else if(currentState == GameState.Paused && isBodyTracked)
            {
                ResumeGame(); // si se vuelve a detectar, reanudamos
            }
        }

        if (currentState == GameState.Playing)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                DisplayTime(timeRemaining);
                EndGame();
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
        fruitManager?.StartCoroutine("CreateFruitsGradually");
    }

    public void FruitCut()
    {
        if (currentState != GameState.Playing) return;

        // logica de puntaje
        AddScore(pointsPerFruit);
        fruitsRemaining--;

        if(fruitsRemaining <= 0)
        {
            // Verificamos si hay mas niveles o si el juego ha terminado
            if (currentLevel < bounceMargins.Length)
            {
                currentLevel++; // Incrementamos el nivel
                StartCoroutine(HandleLevelCompletion());
            }
            else
            {
                // Si ya pasamos todos los niveles, ganamos.
                EndGame();
                resultText.text = "¡GANASTE!";
            }
            
        }
    }

    // metodo que maneja la finalizacion del nivel
    private IEnumerator HandleLevelCompletion()
    {
        // se reproduce el sonido de victoria
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.fruitCutCompletedSound);
        }
        // mostramos mensaje de exito
        resultText.text = $"¡NIVEL {currentLevel-1} COMPLETADO! + {timeBonus}s";
        resultText.gameObject.SetActive(true);

        // Pausamos brevemente el juego para feedback
        Time.timeScale = 0.5f; // reduce la velocidad del juego momentaneamente


        yield return new WaitForSeconds(1.0f);

        // restauramos la velocidad normal
        Time.timeScale = 1f;
        resultText.gameObject.SetActive(false);

        // añadimos tiempo extra
        timeRemaining += timeBonus;
        DisplayTime(timeRemaining);

        // Preparamos el siguiente nivel
        PrepareForNewLevel();
    }
    // Prepara las variables para un nuevo nivel
    private void PrepareForNewLevel()
    {
        levelText.text = "Level: " + currentLevel; // actualizamos el texto del nivel.
        fruitsRemaining = maxFruits; // reiniciamos el contador de frutas.

        // Obtenemos el nuevo margen de rebote del arreglo
        float newBounceMargin = bounceMargins[currentLevel - 1];

        // Llamamos al FruitManager para que actualice sus limites y reinicie la generacion de frutas.
        fruitManager.UpdateBounceMargin(newBounceMargin);
    }

    void EndGame()
    {
        // aqui reproducimos el sonido de game over
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.fruitGameOverSound);
        }
        currentState = GameState.GameOver;
        backToMenuButtonGameOver.gameObject.SetActive(true);
        gameOverPanel.SetActive(true);

        // al final del juego, apagamos todo
        bodyView.enabled = false;
        // reactivamos el cursor 2D para el menu de Game Over
        kinecInputController.SetActive(true);
        handCursor.SetActive(true);

        fruitManager?.DestroyAllFruits();
        resultText.text = "¡SE ACABO EL TIEMPO!";
        resultText.gameObject.SetActive(true);

        // Llamamos a nuestro gestor de base de datos para guardar la puntuacion final
        // Solo guardamos la puntuacion si tenemos un gesto de base de datos en la escena
        if (DatabaseManager.instance != null)
        {
            DatabaseManager.instance.SaveScore(score);

            // Despues de guardar, actualizamos la interfaz de usuario con el nuego High Score
            UpdateHighScoreDisplay();
        }
        else
        {
            Debug.LogError("No se encuentra la instancia de DatabaseManager en la escena");
        }
    }

    public void RestartGame()
    {
        SetupInitialState();
    }

    public void ReturnToMainMenu()
    {
        // esto carga la escena llamada "MainMenu"
        SceneManager.LoadScene("MainMenu");

        // importante: reactiviar el cursor Kinect para el menu principal
        if(bodySourceManager != null)
        {
            bodySourceManager.bodyView.enabled = false;
        }

        kinecInputController.SetActive(true);
        handCursor.SetActive(true);
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

    // metodo para actualizar el highscore
    public void UpdateHighScoreDisplay()
    {
        // verificamos si la instancia del DatabaseManager existe
        if (DatabaseManager.instance != null && highScoreText != null)
        {
            // Obtenemos el puntaje mas alto de la base de datos
            int highScore = DatabaseManager.instance.GetHighScore();

            // Actualizamos el texto de la UI con el puntaje mas alto
            highScoreText.text = "High Score: " + highScore.ToString();
        }
    }

    // metodo de juego pausado
    void PauseGame()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Paused;

        // detenemos el tiempo
        Time.timeScale = 0;

        // Mostramos UI de pausa
        playerLostPanel.SetActive(true);

        // Apagar las manos 3D
        bodyView.enabled = false;
    }

    // metodo de reanudar el juego
    void ResumeGame()
    {
        if (currentState != GameState.Paused) return;

        currentState = GameState.Playing;

        // Restauramos el tiempo
        Time.timeScale = 1;

        // ocultamos la UI de pausa.
        playerLostPanel.SetActive(false);

        // volver a acticar las manos 3D
        bodyView.enabled = true;
    }

}
