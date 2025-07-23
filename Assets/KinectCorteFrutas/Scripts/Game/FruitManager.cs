using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitManager : MonoBehaviour
{

    public GameObject mFruitPrefab;
    public int maxFruits = 20; // numero maximo de frutas a generar
    public float spawnInterval = 0.75f; // Tiempo entre la generacion de cada fruta


    private List<Fruit> mAllFruits = new List<Fruit>();
    private Vector2 mBottomLeft = Vector2.zero;
    private Vector2 mTopRight = Vector2.zero;
    private Vector2 mSpawnBottomLeft; // Esquina inferior izquierda para generación
    private Vector2 mSpawnTopRight;   // Esquina superior derecha para generación

    private void Awake()
    {
        // Calculamos los límites de la cámara como antes
        float distanceToPlane = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, distanceToPlane));
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, distanceToPlane));

        // Definimos un margen para la zona de rebote (zona roja)
        float bounceMargin = 0.8f; // 80% del área visible

        // Límites de rebote (más pequeños que la pantalla completa)
        mBottomLeft = new Vector2(
            bottomLeft.x * bounceMargin,
            bottomLeft.y * bounceMargin
        );
        mTopRight = new Vector2(
            topRight.x * bounceMargin,
            topRight.y * bounceMargin
        );

        // Límites de generación (en los bordes, zona azul)
        mSpawnBottomLeft = bottomLeft;
        mSpawnTopRight = topRight;
    }

    private Vector3 GetSpawnPositionFromCorner()
    {
        int cornerIndex = Random.Range(0, 4);
        Vector3 spawnPosition = Vector3.zero;

        switch (cornerIndex)
        {
            case 0: // Bottom-Left (Generación en borde real)
                spawnPosition = new Vector3(mSpawnBottomLeft.x, mSpawnBottomLeft.y, 0);
                break;
            case 1: // Top-Left (Generación en borde real)
                spawnPosition = new Vector3(mSpawnBottomLeft.x, mSpawnTopRight.y, 0);
                break;
            case 2: // Top-Right (Generación en borde real)
                spawnPosition = new Vector3(mSpawnTopRight.x, mSpawnTopRight.y, 0);
                break;
            case 3: // Bottom-Right (Generación en borde real)
                spawnPosition = new Vector3(mSpawnTopRight.x, mSpawnBottomLeft.y, 0);
                break;
        }

        return spawnPosition;
    }

    private void Start()
    {
        // se comenta ya que no se utiliza y se usara otro metodo.
        // StartCoroutine(CreateFruitsGradually());
    }

    // funcion de creacion modificada
    public IEnumerator CreateFruitsGradually()
    {
        while(mAllFruits.Count < maxFruits)
        {
            // creamos la fruta en una esquina
            Vector3 spawnPosition = GetSpawnPositionFromCorner();
            GameObject newFruitObject = Instantiate(mFruitPrefab, GetSpawnPositionFromCorner(), Quaternion.identity, transform);
            Fruit newFruit = newFruitObject.GetComponent<Fruit>();

            // IMPORTANTE: Pasamos los limites a la fruta
            newFruit.mFruitManager = this;
            newFruit.mBottomLeft = this.mBottomLeft;
            newFruit.mTopRight = this.mTopRight;

            // logica de direccion hacia el centro
            Vector3 directionToCenter = (Vector3.zero - spawnPosition).normalized;
            newFruit.SetInitialDirection(directionToCenter);

            mAllFruits.Add(newFruit);

            // esperamos un intervalo antes de generar la siguiente
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // nuevo metodo
    // destruye todas las frutas que quedan en la pantalla.
    public void DestroyAllFruits()
    {
        // iteramos sobre todos los objetos de fruta que hemos creado
        foreach(GameObject fruitObject in GameObject.FindGameObjectsWithTag("Fruit"))
        {
            // verificamos que el objeto no haya sido destruido ya.
            if(fruitObject != null)
            {
                Destroy(fruitObject);
            }
        }
    }

    public void Reset()
    {

        StopAllCoroutines(); // Detiene la generacion actual de frutas.

        // destruimos cualquier fruta que haya quedado en pantalla
        DestroyAllFruits();

        // limpiamos la lista interna para que pueda volver a llenarse
        mAllFruits.Clear();
    }

    // solo para ayudar a debugear.
    private void OnDrawGizmos()
    {
        // Dibuja la zona de rebote (roja)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            new Vector3((mBottomLeft.x + mTopRight.x) / 2, (mBottomLeft.y + mTopRight.y) / 2, 0),
            new Vector3(mTopRight.x - mBottomLeft.x, mTopRight.y - mBottomLeft.y, 0.1f)
        );

        // Dibuja las zonas de generación (azul)
        Gizmos.color = Color.blue;
        // Esquina inferior izquierda
        Gizmos.DrawSphere(new Vector3(mSpawnBottomLeft.x, mSpawnBottomLeft.y, 0), 0.3f);
        // Esquina superior izquierda
        Gizmos.DrawSphere(new Vector3(mSpawnBottomLeft.x, mSpawnTopRight.y, 0), 0.3f);
        // Esquina superior derecha
        Gizmos.DrawSphere(new Vector3(mSpawnTopRight.x, mSpawnTopRight.y, 0), 0.3f);
        // Esquina inferior derecha
        Gizmos.DrawSphere(new Vector3(mSpawnTopRight.x, mSpawnBottomLeft.y, 0), 0.3f);
    }
}
