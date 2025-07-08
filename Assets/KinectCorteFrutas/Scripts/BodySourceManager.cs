using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class BodySourceManager : MonoBehaviour 
{
    // singleton nuevo
    public static BodySourceManager instance = null;
    public BodySourceView bodyView; // linea agregada

    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    // awake se ejecuta antes que start. es ideal para configurar un singleton
    void Awake()
    {
        // si no existe ninguna instancia de este script, esta se convierte en la instancia
        if(instance == null)
        {
            instance = this;
            // le decimos a unity que no destruya este objeto al cargar otra escena.
            DontDestroyOnLoad(gameObject);
        }
        // si ya existe una instancia y no es esta, entonces esta es un duplicado y la destruimos.
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public Body[] GetData()
    {
        return _Data;
    }
    

    void Start () 
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();
            
            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }   
    }
    
    void Update () 
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                }
                
                frame.GetAndRefreshBodyData(_Data);
                
                frame.Dispose();
                frame = null;
            }
        }    
    }
    
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }
        
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            
            _Sensor = null;
        }
    }
}
