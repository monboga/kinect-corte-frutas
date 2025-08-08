using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleWallManager : MonoBehaviour
{
    public GameObject wall;
    public float speed = 10f;

    private Vector3 startPos = new Vector3(15.5f, -15f, -50f);
    private Vector3 endPos = new Vector3(15.5f, -15f, 60f);

    private int loopCount = 0;
    private int maxLoops = 5;

    private List<Transform> wallParts = new List<Transform>();

    void Start()
    {
        // Obtener todos los cubos hijos de wall que tienen la tag WallPart
        foreach (Transform child in wall.transform)
        {
            if (child.CompareTag("WallPart"))
            {
                wallParts.Add(child);
            }
        }

        StartCoroutine(MoveWallLoop());
    }

    IEnumerator MoveWallLoop()
    {
        while (loopCount < maxLoops)
        {
            // Reinicia la posición y activa el muro
            wall.transform.position = startPos;
            wall.SetActive(true);

            // Activa todos los cubos (por si fueron desactivados antes)
            foreach (Transform part in wallParts)
            {
                part.gameObject.SetActive(true);
            }

            // Aplica la forma correspondiente
            ApplyForm(loopCount);

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

            // Asegura posición final y desactiva
            wall.transform.position = endPos;
            wall.SetActive(false);

            loopCount++;
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Movimiento de pared completado " + maxLoops + " veces.");
    }

    // Aplica la forma según el número de loop
    void ApplyForm(int formIndex)
    {
        switch (formIndex)
        {
            case 0:
                ApplyForm1(); break;
            case 1:
                ApplyForm2(); break;
            case 2:
                ApplyForm3(); break;
            case 3:
                ApplyForm4(); break;
            case 4:
                ApplyForm5(); break;
        }
    }

    // Forma 1: desactiva cubos por índice
    void ApplyForm1()
    {
        int[] cubesToDisable = new int[]
        {
            41, 71, 77, 78, 192, 196, 283, 284, 285,
            313, 314, 315, 404, 455, 456, 457, 459,
            462, 464, 466, 469, 471, 472, 473, 485,
            486, 487, 494, 554
        };

        foreach (int index in cubesToDisable)
        {
            if (index >= 0 && index < wallParts.Count)
            {
                wallParts[index].gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Índice fuera de rango: " + index);
            }
        }
    }

    // Formas vacías que puedes implementar luego
    void ApplyForm2() { /* otros índices */ }
    void ApplyForm3() { /* otros índices */ }
    void ApplyForm4() { /* otros índices */ }
    void ApplyForm5() { /* otros índices */ }
}