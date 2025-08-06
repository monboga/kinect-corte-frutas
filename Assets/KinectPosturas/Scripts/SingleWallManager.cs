using UnityEngine;
using System.Collections;

public class SingleWallManager : MonoBehaviour
{
    public GameObject wall; // Asignar el objeto "Wall" en el inspector
    public float speed = 10f;

    private Vector3 startPos = new Vector3(15.5f, -15f, -50f);
    private Vector3 endPos = new Vector3(15.5f, -15f, 60f);

    private int loopCount = 0;
    private int maxLoops = 1;//4

    void Start()
    {
        StartCoroutine(MoveWallLoop());
    }

    IEnumerator MoveWallLoop()
    {
        while (loopCount < maxLoops)
        {
            wall.transform.position = startPos;
            wall.SetActive(true);

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

            wall.transform.position = endPos;
            wall.SetActive(false);

            loopCount++;
            yield return new WaitForSeconds(0.5f); // Pausa opcional entre repeticiones
        }

        Debug.Log("Movimiento de pared completado "+ maxLoops + " veces.");
    }
}