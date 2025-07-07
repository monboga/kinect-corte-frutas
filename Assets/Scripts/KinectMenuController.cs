using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class KinectMenuController : MonoBehaviour
{
    [Header("Configuracion de Interaccion")]
    public Image handCursor; // arrastra aqui tu UI Image del cursor.
    public float dwellTime = 3.0f; // Tiempo en segundos para hacer "clic"
    public float handSensitivity = 0.4f; // sensibilidad del movimiento (mas bajo = mas movimiento).

    private BodySourceManager bodyManager;
    private Button lastButtonOver;
    private float currenDwellTime;

    // Start is called before the first frame update
    void Start()
    {

        // ocultamos el cursor de la mano al inicio.
        handCursor.gameObject.SetActive(false);

        // buscamos el gestor del Kinect en la escena.
        bodyManager = FindObjectOfType<BodySourceManager>();

        
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

        // mover el cursor con la mano.
        // obtenemos la posicion de la mano derecha en el espacio 3D del kinect.
        var handPosition = trackedBody.Joints[Windows.Kinect.JointType.HandRight].Position;

        // mapeamos la posicion 3d de la mano a la posicion 2d de la pantalla
        float screenX = (handPosition.X + handSensitivity) / (handSensitivity * 2) * Screen.width;
        float screenY = ((handPosition.Y + handSensitivity) / (handSensitivity * 2)) * Screen.height;

        handCursor.transform.position = new Vector2(screenX, screenY);

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

        Button currentButton = null;
        if (results.Count > 0)
        {
            // buscamos el primer boton en los resultados.
            currentButton = results.Select(r => r.gameObject.GetComponent<Button>()).FirstOrDefault(b => b != null);

        }

        // si estamos sobre un boton
        if(currentButton != null)
        {
            if(lastButtonOver == currentButton)
            {
                // si seguimos sobre el mismo boton,aumentamos el tiempo de espera.
                currenDwellTime += Time.deltaTime;

                // añadimos feedback visual
                handCursor.fillAmount = currenDwellTime / dwellTime;

                if(currenDwellTime >= dwellTime)
                {
                    // tiempo cumplido, hacemos clic
                    currentButton.onClick.Invoke();
                    ResetDwell();
                }
            }
            else
            {
                // si es un boton nuevo, reiniciamos el contador
                ResetDwell();
                lastButtonOver = currentButton;
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

    }
}
