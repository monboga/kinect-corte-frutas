using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Fuentes de Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips de Musica")]
    public AudioClip menuMusic;
    public AudioClip fruitCutGameMusic;
    // public AudioClip postureGameMusic;

    [Header("Clirps de efectos de sonido (SFX)")]
    public AudioClip fruitCutSound;
    public AudioClip fruitCutCompletedSound;
    public AudioClip fruitGameOverSound;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Al inciiar el juego por primera vez, ponemos la musica del menu.
        PlayMusic(menuMusic);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // buena practica desuscribirse del evento al destruir el objeto    
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // cambiamos la musica de fondo dependiendo de la escena cargada
        if(scene.name == "FruitCutScene")
        {
            PlayMusic(fruitCutGameMusic);
        }
        else if (scene.name == "MainMenu")
        {
            PlayMusic(menuMusic);
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource.clip == musicClip) return;

        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // metodo para reproducir un efecto de sonido una sola vez
    public void PlaySFX(AudioClip sfxClip)
    {
        sfxSource.PlayOneShot(sfxClip);
    }
}
