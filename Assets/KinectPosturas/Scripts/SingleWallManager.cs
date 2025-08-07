using System.Collections;
using UnityEngine;

public class SingleWallManager : MonoBehaviour
{
    public GameObject wall; // Asignar el objeto "Wall" en el inspector
    public float speed = 10f;

    private Vector3 startPos = new Vector3(15.5f, -15f, -50f);
    private Vector3 endPos = new Vector3(15.5f, -15f, 60f);

    private int loopCount = 0;
    private int maxLoops = 2;

    void Start()
    {
        // Inicia el movimiento de la pared
        StartCoroutine(MoveWallLoop());
    }

    IEnumerator MoveWallLoop()
    {
        while (loopCount < maxLoops)
        {
            // Establece la pared en la posición inicial
            wall.transform.position = startPos;
            wall.SetActive(true);  // Asegúrate de que la pared esté activa

            // Mueve la pared hacia la posición final
            while (Mathf.Abs(wall.transform.position.z - endPos.z) > 0.01f)
            {
                Vector3 newPos = Vector3.MoveTowards(
                    wall.transform.position,
                    endPos,
                    speed * Time.deltaTime
                );

                wall.transform.position = new Vector3(startPos.x, startPos.y, newPos.z);
                yield return null;
            }

            // Asegura que la pared esté exactamente en la posición final
            wall.transform.position = endPos;
            wall.SetActive(false);  // Desactiva la pared al llegar a la meta

            loopCount++;  // Incrementa el contador de bucles
            yield return new WaitForSeconds(0.5f);  // Pausa antes de comenzar el siguiente movimiento
        }

        Debug.Log("Movimiento de pared completado " + maxLoops + " veces.");
    }
}