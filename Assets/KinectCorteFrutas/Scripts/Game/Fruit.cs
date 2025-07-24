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
        if (isCut) return;

        // Lógica de rebote con los nuevos límites (mBottomLeft y mTopRight)
        if (transform.position.x > mTopRight.x || transform.position.x < mBottomLeft.x)
        {
            // Invertimos dirección en X y añadimos un pequeño aleatorio para variar
            mMovementDirection.x = -mMovementDirection.x * Random.Range(0.9f, 1.1f);
        }

        if (transform.position.y > mTopRight.y || transform.position.y < mBottomLeft.y)
        {
            // Invertimos dirección en Y y añadimos un pequeño aleatorio
            mMovementDirection.y = -mMovementDirection.y * Random.Range(0.9f, 1.1f);
        }

        // Aseguramos que la fruta no salga de la zona de rebote
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, mBottomLeft.x, mTopRight.x),
            Mathf.Clamp(transform.position.y, mBottomLeft.y, mTopRight.y),
            transform.position.z
        );

        // Movimiento con la velocidad actual
        transform.position += mMovementDirection * Time.deltaTime * moveSpeed;

        // Rotación basada en la dirección
        transform.Rotate(Vector3.forward * Time.deltaTime * mMovementDirection.x * 20, Space.Self);
    }

    public IEnumerator Pop()
    {
        if (isCut) yield break; // Si ya fue cortada, no hacemos nada
        isCut = true;

        // llamamos al audio de corte
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.fruitCutSound);
        }

        
        ScoreManager.instance.FruitCut();

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
