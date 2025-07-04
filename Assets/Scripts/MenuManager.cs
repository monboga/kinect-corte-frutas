using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // muy importnate para cambiar de escena

public class MenuManager : MonoBehaviour
{
    [Header("Paneles de menu")]
    public GameObject mainMenuPanel;
    public GameObject gameSelectionPanel;

    // se llama desde el boton "Iniciar"
    public void ShowGameSelectionPanel()
    {
        mainMenuPanel.SetActive(false);
        gameSelectionPanel.SetActive(true);
    }

    // se llama desde un boton "Atras" que se podria añadir mas adelante
    public void ShowMainMenuPanel()
    {
        mainMenuPanel.SetActive(true);
        gameSelectionPanel.SetActive(false);
    }

    // Carga de escenas del juego

    // se llama desde el boton "Corte de frutas"

    public void LoadFruitCutterScene()
    {
        // Asegurate de que el nombre "MainScene" coincida exactamente
        // con el nombre de tu escena en la carpeta KinectView
        SceneManager.LoadScene("MainScene");
    }

    public void LoadPostureScene()
    {
        // Como la escena aun no existe, mostramos un mensaje en consola
        Debug.Log("Cargando escena de Deteccion de posturas (aun no implementada) ... ");
    }

    // funciones generales
    public void ExitGame()
    {
        Debug.Log("Saliendo del juego ...");
        Application.Quit(); // esto solo funciona en una build no en el editr.
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
