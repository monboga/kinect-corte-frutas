using System.Collections;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public GameObject[] walls;

    // Posiciones fijas en X y Y, solo se moverán en Z
    private float fixedX = 0f;//-5.607446f
    private float fixedY = 3f;//2.588154f //0
    private float startZ = -50f;//-40f;//50 
    private float endZ = 50f;//38//70f;//38  

    public float speed = 10f;

    void Start()
    {
        StartCoroutine(MoverMurosUnoPorUno());
    }

    IEnumerator MoverMurosUnoPorUno()
    {
        yield return new WaitForSeconds(1f); // Esperar al iniciar

        foreach (GameObject wall in walls)
        {
            // Establecer la posición inicial exacta antes de activar
            Vector3 startPos = new Vector3(fixedX, fixedY, startZ);
            Vector3 endPos = new Vector3(fixedX, fixedY, endZ);

            wall.transform.position = startPos;
            wall.SetActive(true);

            // Mover en línea recta por el eje Z
            while (Mathf.Abs(wall.transform.position.z - endZ) > 0.01f)
            {
                Vector3 nuevaPos = Vector3.MoveTowards(
                    wall.transform.position,
                    endPos,
                    speed * Time.deltaTime
                );

                // Fijar X y Y en cada frame, por si algo externo las cambia
                wall.transform.position = new Vector3(fixedX, fixedY, nuevaPos.z);

                yield return null;
            }

            // Asegura que quede exactamente en el destino final
            wall.transform.position = endPos;

            wall.SetActive(false);
            yield return new WaitForSeconds(0.2f); // Pausa opcional entre muros
        }
    }
}