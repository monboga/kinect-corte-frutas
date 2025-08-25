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
    private int maxLoops = 4;

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
        }
    }

    // Forma 1: desactiva cubos por índice
    void ApplyForm1()
    {
        int[] cubesToDisable = new int[]
        {
            9, 10, 11,
            18, 19, 20,
            39, 40, 41,
            48, 49, 50,
            69, 70, 71,
            78, 79, 80,
            100, 101, 102,
            107, 108, 109,
            130, 131, 132,
            137, 138, 139,
            160, 161, 162,
            167, 168, 169,
            191, 192, 193,
            196, 197, 198,
            221, 222, 223, 224, 225,
            226, 227, 228,
            251, 252, 253,
            254, 255,
            256, 257, 258,
            282, 283, 284, 285, 286, 287,
            312, 313, 314, 315, 316, 317,
            343,
            344, 345,
            346,
            373,
            374, 375,
            376,
            403,
            404, 405,
            406,
            394, 395, 396, 397, 398, 399,
            400, 401, 402, 403, 404, 405,
            406, 407, 408, 409, 410, 411,
            412, 413, 414, 415,
            424, 425, 426, 427, 428, 429,
            430, 431, 432, 433, 434, 435,
            436, 437, 438, 439, 440, 441,
            442, 443, 444, 445,
            454, 455, 456, 457, 458, 459,
            460, 461, 462, 463, 464, 465,
            466, 467, 468, 469, 470, 471,
            472, 473, 474, 475,
            494, 495,
            195, 194, 166, 163, 136, 133, 106, 103, 77, 72, 47, 42, 17, 12,
            524, 525,
            554, 555,
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

    void ApplyForm2()
    {
        int[] cubesToDisable = new int[]
        {
            16, 15, 14, 13,
            46, 45, 44, 43,
            76, 75, 74, 73,
            106, 105, 104, 103,
            136, 135, 134, 133,
            166, 165, 164, 163,
            196, 195, 194, 193,
            226, 225, 224, 223,
            256, 255, 254, 253,
            286, 285, 284, 283,
            316, 315, 314, 313,
            346, 345, 344, 343,
            376, 375, 374, 373,
            406, 405, 404, 403,
            436, 435, 434, 433,
            466, 465, 464, 463,
            495, 494,
            524, 525,
            554, 555,
            437, 432, 460, 546, 516, 459, 469, 501, 531, 563, 517, 487, 515, 532, 533,
            518, 488,
            500, 499,
            468, 467,
            561, 562, 564,
            529, 530,
            497, 498,
            534, 502, 470,
            489, 490,
            461, 462,
            545, 547, 548,
            519, 520,
            491, 492,
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
    void ApplyForm3()
    {
        int[] cubesToDisable = new int[]
        {
            196, 195, 194, 193,
            226, 225, 224, 223,
            256, 255, 254, 253,
            286, 285, 284, 283,
            316, 315, 314, 313,
            346, 345, 344, 343,
            376, 375, 374, 373,
            405, 404,
            434, 435,
            464, 465,
            304, 305, 306, 307, 308, 309,
            310, 311, 312, 317, 318, 319, 320, 321,
            322, 323, 324, 325,
            334, 335, 336, 337, 338, 339,
            340, 341, 342, 347, 348, 349, 350, 351,
            352, 353, 354, 355,
            364, 365, 366, 367, 368, 369,
            370, 371, 372, 377, 378, 379, 380, 381,
            382, 383, 384, 385,
            222, 227,
            192, 197,
            138, 131, 130, 51, 50, 41, 40, 21, 20, 11, 10,
            190, 191,
            160, 161, 162, 163,
            132,
            100, 101, 102,
            70, 71, 72,
            42,
            12,
            166, 167, 168,
            137, 139,
            108, 109, 110,
            79, 80, 81,
            52,
            22,
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
    void ApplyForm4()
    {
        int columns = 30;

        int[] originalIndices = new int[]
        {
            196, 195, 194, 193,
            226, 225, 224, 223,
            256, 255, 254, 253,
            286, 285, 284, 283,
            316, 315, 314, 313,
            346, 345, 344, 343,
            376, 375, 374, 373,
            405, 404,
            434, 435,
            464, 465,
            304, 305, 306, 307, 308, 309,
            310, 311, 312, 317, 318, 319, 320, 321,
            322, 323, 324, 325,
            334, 335, 336, 337, 338, 339,
            340, 341, 342, 347, 348, 349, 350, 351,
            352, 353, 354, 355,
            364, 365, 366, 367, 368, 369,
            370, 371, 372, 377, 378, 379, 380, 381,
            382, 383, 384, 385,
            222, 227,
            192, 197,
            138, 131, 130, 51, 50, 41, 40, 21, 20, 11, 10,
            190, 191,
            160, 161, 162, 163,
            132,
            100, 101, 102,
            70, 71, 72,
            42,
            12,
            166, 167, 168,
            137, 139,
            108, 109, 110,
            79, 80, 81,
            52,
            22,
        };

        List<int> flippedIndices = new List<int>();

        foreach (int index in originalIndices)
        {
            int row = index / columns;
            int col = index % columns;
            int flippedCol = columns - 1 - col;
            int flippedIndex = row * columns + flippedCol;

            flippedIndices.Add(flippedIndex);
        }

        foreach (int index in flippedIndices)
        {
            if (index >= 0 && index < wallParts.Count)
            {
                wallParts[index].gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Índice fuera de rango en ApplyForm4: " + index);
            }
        }
    }
}