using UnityEngine;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

namespace KinectPosturas
{
    public class BodySourceView : MonoBehaviour
    {
        // CORRECCIÓN: Variables públicas para asignar desde el Inspector de Unity.
        public BodySourceManager _BodyManager;
        public Material BoneMaterial;
        public GameObject JointPrefab; // Un prefab para la articulación, ej: una esfera pequeña.

        [Range(0f, 1f)]
        public float jointSmoothFactor = 0.5f;
        public float desiredHeight = 1.7f; // Altura deseada del avatar en metros.

        // Diccionarios para gestionar los cuerpos y sus datos
        private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
        private Dictionary<ulong, float> _BodyScaleFactors = new Dictionary<ulong, float>();

        // Mapa de huesos que conecta las articulaciones
        private readonly Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
        {
            { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
            { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
            { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
            { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
            { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
            { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
            { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
            { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
            { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
            { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
            { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
            { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
            { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
            { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
            { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
            { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
            { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
            { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
            { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
            { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
            { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
            { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
            { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
            { Kinect.JointType.Neck, Kinect.JointType.Head },
        };

        void Update()
        {
            // CORRECCIÓN: Comprobación de la variable _BodyManager asignada
            if (_BodyManager == null)
            {
                Debug.LogError("BodySourceManager no está asignado.");
                return;
            }

            Kinect.Body[] data = _BodyManager.GetData();
            if (data == null) return;

            // CORRECCIÓN: LÓGICA REESTRUCTURADA
            // 1. Identificar todos los cuerpos que están siendo rastreados en este frame.
            List<ulong> trackedIds = new List<ulong>();
            foreach (var body in data)
            {
                if (body != null && body.IsTracked)
                {
                    trackedIds.Add(body.TrackingId);
                }
            }

            // 2. Eliminar los GameObjects de los cuerpos que ya no se rastrean.
            List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
            foreach (ulong trackingId in knownIds)
            {
                if (!trackedIds.Contains(trackingId))
                {
                    Debug.Log($"Eliminando cuerpo con ID: {trackingId}");
                    Destroy(_Bodies[trackingId]);
                    _Bodies.Remove(trackingId);
                    if (_BodyScaleFactors.ContainsKey(trackingId))
                    {
                        _BodyScaleFactors.Remove(trackingId);
                    }
                }
            }

            // 3. Crear o actualizar los GameObjects para los cuerpos rastreados.
            foreach (var body in data)
            {
                if (body != null && body.IsTracked)
                {
                    // Si es un cuerpo nuevo, crea su GameObject
                    if (!_Bodies.ContainsKey(body.TrackingId))
                    {
                        _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                    }
                    // Actualiza la posición y rotación del cuerpo existente
                    RefreshBodyObject(body, _Bodies[body.TrackingId]);
                }
            }
        }

        // CORRECCIÓN: Método movido fuera de Update y limpiado
        private GameObject CreateBodyObject(ulong id)
        {
            GameObject body = new GameObject("Body:" + id);

            // Agregar componentes de física si se necesitan (ej: para colisiones)
            Rigidbody rb = body.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Crear un objeto visual para cada articulación
            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                // Usa un prefab para que sea más fácil de configurar visualmente
                GameObject jointObj = Instantiate(JointPrefab);

                jointObj.name = jt.ToString();
                jointObj.transform.parent = body.transform;

                // Añadir LineRenderer para dibujar los huesos
                LineRenderer lr = jointObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.material = BoneMaterial;
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
            }

            return body;
        }

        // CORRECCIÓN: Método movido fuera de Update y limpiado
        private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
        {
            // Calcular o recuperar el factor de escala para normalizar la altura del avatar
            float scaleFactor;
            if (!_BodyScaleFactors.TryGetValue(body.TrackingId, out scaleFactor))
            {
                var head = body.Joints[Kinect.JointType.Head].Position;
                var foot = body.Joints[Kinect.JointType.FootLeft].Position; // Usar un pie como referencia
                float realHeight = Mathf.Abs(head.Y - foot.Y);

                scaleFactor = (realHeight > 0.1f) ? desiredHeight / realHeight : 1f;
                _BodyScaleFactors[body.TrackingId] = scaleFactor;
            }

            // Actualizar la posición del objeto principal del cuerpo
            var baseJoint = body.Joints[Kinect.JointType.SpineBase];
            Vector3 bodyWorldPos = GetScaledWorldPosition(baseJoint, scaleFactor);
            bodyObject.transform.position = bodyWorldPos;

            // Actualizar cada articulación
            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                Transform jointObj = bodyObject.transform.Find(jt.ToString());
                if (jointObj == null) continue;

                Kinect.Joint sourceJoint = body.Joints[jt];

                // Interpolar la posición para un movimiento más suave
                Vector3 targetPos = GetScaledLocalPosition(sourceJoint, baseJoint, scaleFactor);
                jointObj.localPosition = Vector3.Lerp(jointObj.localPosition, targetPos, jointSmoothFactor);

                // Actualizar el LineRenderer para dibujar el hueso
                LineRenderer lr = jointObj.GetComponent<LineRenderer>();
                if (lr != null && _BoneMap.ContainsKey(jt))
                {
                    Kinect.Joint targetJoint = body.Joints[_BoneMap[jt]];
                    Vector3 targetJointPos = GetScaledLocalPosition(targetJoint, baseJoint, scaleFactor);

                    lr.SetPosition(0, jointObj.localPosition);
                    lr.SetPosition(1, targetJointPos);
                    lr.startColor = GetColorForState(sourceJoint.TrackingState);
                    lr.endColor = GetColorForState(targetJoint.TrackingState);
                }
                else if (lr != null)
                {
                    // Si la articulación no tiene un hueso conectado, oculta la línea
                    lr.enabled = false;
                }
            }
        }

        // CORRECCIÓN: Funciones de ayuda limpiadas y simplificadas
        private Vector3 GetScaledWorldPosition(Kinect.Joint joint, float scale)
        {
            // Invierte el eje X para un efecto espejo y aplica escala.
            return new Vector3(-joint.Position.X * 10f * scale, joint.Position.Y * 10f * scale, joint.Position.Z * 10f * scale);
        }

        private Vector3 GetScaledLocalPosition(Kinect.Joint joint, Kinect.Joint reference, float scale)
        {
            // Calcula la posición relativa a la articulación base (SpineBase)
            Vector3 jointPos = new Vector3(-joint.Position.X, joint.Position.Y, joint.Position.Z);
            Vector3 refPos = new Vector3(-reference.Position.X, reference.Position.Y, reference.Position.Z);
            return (jointPos - refPos) * 10f * scale;
        }

        private Color GetColorForState(Kinect.TrackingState state)
        {
            switch (state)
            {
                case Kinect.TrackingState.Tracked: return Color.green;
                case Kinect.TrackingState.Inferred: return Color.red;
                default: return Color.black;
            }
        }
    }
}