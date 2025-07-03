using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public Sprite mFruitSprite;
    public Sprite mCutSprite;

    // variable para la velocidad
    public float moveSpeed = 2.5f;

    [HideInInspector]
    public FruitManager mFruitManager = null;

    // establecemos los limites de la pantalla
    [HideInInspector]
    public Vector2 mBottomLeft, mTopRight;

    private Vector3 mMovementDirection = Vector3.zero;
    private SpriteRenderer mSpriteRenderer = null;
    private Coroutine mCurrentChanger = null;
    private bool isCut = false; // para evitar logica extra despues de cortar;

    private void Awake()
    {
        mSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        mCurrentChanger = StartCoroutine(DirectionChanger());
    }

    public void SetInitialDirection(Vector3 direction)
    {
        mMovementDirection = direction;
    }

    private void Update()
    {
        if (isCut) return; // si la frita ya fue cortada, no la movemos ni la rotamos.

        // logica de rebote
        // comprobar si ha llegado a los bordes horizontales.
        if(transform.position.x > mTopRight.x || transform.position.x < mBottomLeft.x)
        {
            // invertimos la direccion en el eje x
            mMovementDirection.x *= -1;
        }

        // comprobamos si ha llegado a los bordes verticales
        if(transform.position.y > mTopRight.y || transform.position.y < mBottomLeft.y)
        {
            // invertimos la direccion en el eje Y
            mMovementDirection.y *= -1;
        }

        // Clamping para asegurar que nunca se quede atascada fuera de los limites
        transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, mBottomLeft.x, mTopRight.x),
                Mathf.Clamp(transform.position.y, mBottomLeft.y, mTopRight.y),
                transform.position.z
                );

        // usamos nuestra nueva varibale en lugar de un numero fijo
        transform.position += mMovementDirection * Time.deltaTime * moveSpeed;

        // movement
        transform.position += mMovementDirection * Time.deltaTime * 0.35f;

        // rotation
        transform.Rotate(Vector3.forward * Time.deltaTime * mMovementDirection.x * 20, Space.Self);
    }

    public IEnumerator Pop()
    {
        if (isCut) yield break; // Si ya fue cortada, no hacemos nada
        isCut = true;

        // LLamada al ScoreManager y le suma 5 puntos.
        ScoreManager.instance.AddScore(5);

        // Detenemos el movimiento y la logica para que no siga interactuando
        StopCoroutine(mCurrentChanger);
        mMovementDirection = Vector3.zero;

        // hacemos el collider un trigger para que no colisione con otras cosas mientras desaparece.
        GetComponent<Collider2D>().enabled = false;

        // Cambiamos el sprite al de la fruta cortada
        mSpriteRenderer.sprite = mCutSprite;

        yield return new WaitForSeconds(0.5f);

        // transform.position = mFruitManager.GetPlanePosition();

        // mSpriteRenderer.sprite = mFruitSprite;
        // mCurrentChanger = StartCoroutine(DirectionChanger());

        // En lugar de reposicionar la fruta, la destruimos del juego
        Destroy(gameObject);
    }

    private IEnumerator DirectionChanger()
    {
        // esperamos 2 segundos antes de cambiar a un movimiento aleatorio
        yield return new WaitForSeconds(2.0f);

        while(gameObject.activeSelf)
        {
            mMovementDirection = new Vector2(Random.Range(-100, 100) * 0.01f, Random.Range(-100, 100) * 0.01f);

            // si la direccion es casi cero, le damos una pequeña fuerza para que no se quede quita.
            if(mMovementDirection.magnitude < 0.1f)
            {
                mMovementDirection = Vector2.one;
            }

            yield return new WaitForSeconds(5.0f);
        }
    }


}
