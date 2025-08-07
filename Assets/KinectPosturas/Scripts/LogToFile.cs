using System.IO;
using UnityEngine;

public class LogToFile : MonoBehaviour
{
    private string logFilePath;

    void OnEnable()
    {
        // Define la ruta donde se guardará el archivo de log, con la fecha y hora en el nombre
        string logFileName = $"Log_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
        logFilePath = Path.Combine(Application.dataPath, "KinectPosturas/ConsoleLogs", logFileName);

        // Asegúrate de que la carpeta exista
        string directory = Path.GetDirectoryName(logFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Suscribirse al evento de log
        Application.logMessageReceived += HandleLog;
    }

    // Cuando el script es desactivado, cancelamos la suscripción
    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    // Manejar los mensajes de la consola
    private void HandleLog(string logString, string stackTrace, LogType logType)
    {
        // Abrimos el archivo en modo escritura, 'false' sobrescribe el archivo en cada ejecución
        using (StreamWriter writer = new StreamWriter(logFilePath, true))  // 'true' para agregar, 'false' para sobrescribir
        {
            writer.WriteLine($"[{System.DateTime.Now}] {logType}: {logString}");
            if (!string.IsNullOrEmpty(stackTrace))
            {
                writer.WriteLine($"Stack Trace: {stackTrace}");
            }
        }
    }
}