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

    private void Awake()
    {
        // --- MÉTODO DE CÁLCULO MÁS ROBUSTO ---
        float distanceToPlane = Mathf.Abs(Camera.main.transform.position.z);

        // Usamos ViewportToWorldPoint, que es más fiable.
        // Viewport va de (0,0) en la esquina inferior izquierda a (1,1) en la superior derecha.
        mBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, distanceToPlane));
        mTopRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, distanceToPlane));
    }

    private void Start()
    {
        // se comenta ya que no se utiliza y se usara otro metodo.
        // StartCoroutine(CreateFruitsGradually());
    }

    // metodo de spawn modificado
    private Vector3 GetSpawnPositionFromCorner()
    {
        int cornerIndex = Random.Range(0, 4);
        Vector3 spawnPosition = Vector3.zero;

        switch (cornerIndex)
        {
            case 0: // Bottom-Left
                spawnPosition = new Vector3(mBottomLeft.x, mBottomLeft.y, 0);
                break;
            case 1: // Top-Left
                spawnPosition = new Vector3(mBottomLeft.x, mTopRight.y, 0);
                break;
            case 2: // Top-Right
                spawnPosition = new Vector3(mTopRight.x, mTopRight.y, 0);
                break;
            case 3: // Bottom-Right
                spawnPosition = new Vector3(mTopRight.x, mBottomLeft.y, 0);
                break;
        }

        return spawnPosition;
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

    /* 
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.farClipPlane)), 0.5f);
        Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, Camera.main.farClipPlane)), 0.5f);

    }

    public Vector3 GetPlanePosition()
    {
        // get a random value
        float targetX = Random.Range(mBottomLeft.x, mTopRight.x);
        float targetY = Random.Range(mBottomLeft.y, mTopRight.y);

        return new Vector3(targetX, targetY, 0);
    }

    private IEnumerator CreateBubbles()
    {

        while(mAllFruits.Count < 20)
        {
            // Create and add
            GameObject newBubbleObject = Instantiate(mFruitPrefab, GetPlanePosition(), Quaternion.identity, transform);
            Fruit newBubble = newBubbleObject.GetComponent<Fruit>();

            // Setup bubble
            newBubble.mFruitManager = this;
            mAllFruits.Add(newBubble);

            yield return new WaitForSeconds(0.5f);
        }

    }
    */
}
