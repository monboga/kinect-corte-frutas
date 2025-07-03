using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // importante para usar TextMeshPro

public class ScoreManager : MonoBehaviour
{

    // Instancia estatica para acceder facilmente desde otros scripts
    public static ScoreManager instance;

    // referencia al texto de la UI que mostrará el puntaje.
    public TextMeshProUGUI scoreText;

    private int score = 0;

    // variables del temporizador
    public TextMeshProUGUI timerText; // referencia al texto del timer
    public float timeRemaining = 30; // Tiempo inicial en segundos
    public bool timerIsRunning = true; // Para conocer si el timer debe correr

    // Awake se llama antes de cualquier metodo start
    private void Awake()
    {
        // configuracion del Singleton
        if(instance == null)
        {
            instance = this;

        } else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // inicializa el texto del puntaje
        scoreText.text = "Score: " + score.ToString();

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
                timerIsRunning = false;

                // aqui se podria agregar la logica del "Game Over"
            }
        }
    }

    // metodo para mostrar el tiempo formateado
    void DisplayTime(float timeToDisplay)
    {
        // Sumamos 1 para que el display no muestre 0 cuando aún queda una fraccion de segundo
        timeToDisplay += 1;

        // Usamos Mathf.FloorToInt para obtener solo el numero de entero de segundos
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
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
