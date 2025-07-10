using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using KinectCorteFrutas;

public class KinectMenuController : MonoBehaviour
{
    [Header("Configuracion de Interaccion")]
    public Image handCursor; // Imagen UI que representa la mano.
    public Image handCursorFill; // imagen adicional para mostrar progreso de seleccion
    public float dwellTime = 3.0f; // Tiempo requerido para seleccionar el boton
    public float handSensitivity = 0.4f; // sensibilidad del movimiento (mas bajo = mas movimiento).

    [Header("Feedback visual")]
    public float hoverScale = 1.2f; // Escala cuando está sobre un botón
    public Color normalColor = Color.white; // Color normal del cursor
    public Color hoverColor = Color.yellow; // Color cuando está sobre boton
    public Color activeColor = Color.green; // Color cuando esta seleccionado

    private BodySourceManager bodyManager;
    private Button lastButtonOver;
    private float currenDwellTime;

    // Start is called before the first frame update
    void Start()
    {

        // ocultamos el cursor de la mano al inicio.
        handCursor.gameObject.SetActive(false);
        handCursorFill.gameObject.SetActive(false);

        // Obtenemos la instancia dell BodySourceManager (singleton)
        bodyManager = BodySourceManager.instance;

        handCursorFill.fillAmount = 0;

        SceneManager.sceneLoaded += OnSceneLoaded;
        
    }

    void OnDestroy()
    {
        // nos desuscribimos del evento para evitar errores
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // resetamos el estado para evitar intentar usar un boton que ya fue destruido.
        ResetDwell();
    }

    // Update is called once per frame
    void Update()
    {
        // si no hay gestor de Kinect, no hacemos nada.
        if (bodyManager == null) return;

        // buscamos el primer cuerpo que el Kinect esté siguiendo.
        var trackedBody = bodyManager.GetData()?.FirstOrDefault(b => b.IsTracked);

        if(trackedBody == null)
        {
            // si no hay cuerpo, ocultamos el cursor y reseteamos el estado.
            handCursor.gameObject.SetActive(false);
            ResetDwell();
            return;
        }

        // si hay un cuerpo, mostramos el cursor.
        handCursor.gameObject.SetActive(true);
        handCursorFill.gameObject.SetActive(true);

        // mover el cursor con la mano.
        // obtenemos la posicion de la mano derecha en el espacio 3D del kinect.
        var handPosition = trackedBody.Joints[Windows.Kinect.JointType.HandRight].Position;

        // mapeamos la posicion 3d de la mano a la posicion 2d de la pantalla
        float screenX = (handPosition.X + handSensitivity) / (handSensitivity * 2) * Screen.width;
        float screenY = ((handPosition.Y + handSensitivity) / (handSensitivity * 2)) * Screen.height;

        // Actualizamos posicion del cursor
        handCursor.transform.position = new Vector2(screenX, screenY);
        handCursorFill.transform.position = handCursor.transform.position;

        // logica para "hacer clic"
        CheckForButtonInteraction();
        
    }

    private void CheckForButtonInteraction()
    {
        // creamos un "puntero" virtual en la posicion del cursor.
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = handCursor.transform.position;

        // lanzamos un rayo para ver que elementos de UI hay debajo del cursor.
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // Buscamos el primer boton en los resultados
        Button currentButton = results.Select(r => r.gameObject.GetComponent<Button>()).FirstOrDefault(b => b != null);


        // si estamos sobre un boton
        if(currentButton != null)
        {
            if(lastButtonOver == currentButton)
            {
                ResetDwell();
                lastButtonOver = currentButton;

                // Feedback visual inicial al entrar al boton
                handCursor.transform.localScale = Vector3.one * hoverScale;
                handCursor.color = hoverColor;
            }

            // Incrementamos ell tiempo de seleccion
            currenDwellTime += Time.deltaTime;


            // Actualizamos el fill amount (progreso de seleccion)
            float fillAmount = currenDwellTime / dwellTime;
            handCursorFill.fillAmount = fillAmount;

            // cambiamos color gradualmente segun el progreso
            handCursor.color = Color.Lerp(hoverColor, activeColor, fillAmount);

            // Si se completo el tiempo de seleccion
            if(currenDwellTime >= dwellTime)
            {
                // ejecutamo el clic del boton
                currentButton.onClick.Invoke();
                ResetDwell();

                // pequeño feedback de confirmacion
                StartCoroutine(SelectionFeedback());
            }
        }
        else // si no estamos sobre ningun boton
        {
            ResetDwell();
        }
    }

    private void ResetDwell()
    {
        currenDwellTime = 0;
        lastButtonOver = null;
        // reseteamos el feedback visual
        handCursor.fillAmount = 0;

        // restauramos apariencia normal del cursor.
        handCursor.transform.localScale = Vector3.one;
        handCursor.color = normalColor;

    }

    private IEnumerator SelectionFeedback()
    {
        // pequeña animacion de confirmacion
        handCursor.transform.localScale = Vector3.one * (hoverScale * 1.3f);
        yield return new WaitForSeconds(0.1f);
        handCursor.transform.localScale = Vector3.one * hoverScale;
    }
}
