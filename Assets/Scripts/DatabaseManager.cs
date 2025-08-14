using Mono.Data.Sqlite; // Importamos la libreria para interactuar con la base de datos
using System.IO; // para trabajar con la ruta del archivo de la base de datos.
// importamos System.Collections para usar listas Genericas
using System.Collections.Generic;
using UnityEngine;
using System;

// Definimos la clase que gestionara la base de datos.
public class DatabaseManager : MonoBehaviour
{
    // Nombre del archivo de la base de datos.
    private string dbName = "kinect_terapia_scores.db";

    // objeto de conexion a la base de datos
    private SqliteConnection dbConnection;

    // Instancia estatica para acceder a la base de datos desde cualquier script
    public static DatabaseManager instance;

    // El metodo awake se llama antes de cualquier otro metodo Start
    private void Awake()
    {
        // Creamos la instancia unica de este manager
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Llamamos al metodo para iniciar la base de datos
        InitializeDatabase();
    }

    // Metodo que maneja la creacion de la base de datos
    public void InitializeDatabase()
    {
        // Path de la base de datos. Usamos Application.dataPath
        string dbPath = Path.Combine(Application.dataPath, dbName);
        Debug.Log("Ruta de la base de datos: " + dbPath);

        // Creamos la cadena de conexion
        dbConnection = new SqliteConnection("URI=file:" + dbPath);

        try
        {
            // Abrimos la conexion
            dbConnection.Open();
            Debug.Log("Conexion exitosa a la base de datos.");

            // Creamos la tabla 'fruit_cut_score' si no existe
            CreateScoreTable();
        }
        catch (SqliteException e)
        {
            Debug.LogError("Error al conectar a la base de datos: " + e.Message);
        }
    }

    // Metodo para crear la tabla de puntuaciones
    private void CreateScoreTable()
    {
        // Definimos la consulta SQL para crear la tabla.
        string sql = "CREATE TABLE IF NOT EXISTS fruit_cut_score(" +
            "id INTEGER PRIMARY KEY AUTOINCREMENT," +
            "score INTEGER NOT NULL," +
            "date TEXT NOT NULL);";

        // Creamos el comando SQL
        SqliteCommand command = new SqliteCommand(sql, dbConnection);

        try
        {
            // Ejecutamos la consulta
            command.ExecuteNonQuery();
            Debug.Log("Tabla 'fruit_cut_score' fue creada exitosamente o ya existe");
        }
        catch (SqliteException e)
        {
            Debug.LogError("Error al creat la tabla: " + e.Message);
        }
    }

    // Metodo publico para guardar una nueva puntuacion
    public void SaveScore(int score)
    {
        // La consulta SQL para insertar un nuevo regsitro
        string sql = "INSERT INTO fruit_cut_score (score, date) VALUES (@score, @date);";

        // Creamos un nuevo comando
        SqliteCommand command = new SqliteCommand(sql, dbConnection);

        // Añadimos los parametros a la consulta para prevenir inyeccion SQL
        command.Parameters.AddWithValue("@score", score);
        command.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        try
        {
            // Ejecutamos el comando
            int rowsAffected = command.ExecuteNonQuery();
            Debug.Log("El score fue guardado. Filas afectadas: " + rowsAffected);
        }
        catch (SqliteException e)
        {
            Debug.LogError("Error al guardar el score: " + e.Message);
        }
    }

    // El metodo OnApplicationQuit se llama cuando la apliacion se cierra
    private void OnApplicationQuit()
    {
        // Nos aseguramos de cerrar la conexion a la base de datos.
        if(dbConnection != null && dbConnection.State == System.Data.ConnectionState.Open)
        {
            dbConnection.Close();
            Debug.Log("Se cerro la conexion a la base de datos.");
        }
    }

    // Metodo publico para obtener el puntaje mas alto de la base de datos
    public int GetHighScore()
    {
        // La consulta SQL para obtener el puntaje maximo de la tabla
        string sql = "SELECT MAX(score) FROM fruit_cut_score;";

        // Creamos un nuevo objeto de comando
        SqliteCommand command = new SqliteCommand(sql, dbConnection);

        try
        {
            // ExecuteScalar() se usa para obtener un solo valor de la base de datos
            // Si no hay registros, devolvera DBNull
            object result = command.ExecuteScalar();

            // Verificamos si el resultado no es nulo y lo ocnvertimos a un entero
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }
        }
        catch (SqliteException e)
        {
            Debug.LogError("Error al obtener el high score: " + e.Message);
        }

        // Si no se pudo obtener el puntaje devolvemos 0
        return 0;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
