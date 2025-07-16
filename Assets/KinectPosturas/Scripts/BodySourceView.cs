using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

namespace KinectPosturas
{
    public class BodySourceView : MonoBehaviour
    {
        public Material BoneMaterial;
        public GameObject BodySourceManager;
        //define velocidad de interpolacion:
        [Range(0f, 1f)]
        public float jointSmoothFactor = 0.5f; // 0 = sin movimiento, 1 = sin suavizado

        private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
        private BodySourceManager _BodyManager;
        //diccionario para guardar los factores:
        private Dictionary<ulong, float> _BodyScaleFactors = new Dictionary<ulong, float>();

        private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
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
            if (BodySourceManager == null)
            {
                return;
            }

            _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
            if (_BodyManager == null)
            {
                return;
            }

            Kinect.Body[] data = _BodyManager.GetData();
            if (data == null)
            {
                return;
            }

            List<ulong> trackedIds = new List<ulong>();
            foreach (var body in data)
            {
                if (body == null)
                {
                    continue;
                }

                if (body.IsTracked)
                {
                    trackedIds.Add(body.TrackingId);
                }
            }

            List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

            // First delete untracked bodies
            foreach (ulong trackingId in knownIds)
            {
                if (!trackedIds.Contains(trackingId))
                {
                    Destroy(_Bodies[trackingId]);
                    _Bodies.Remove(trackingId);

                    // Limpiar el factor de escala guardado
                    if (_BodyScaleFactors.ContainsKey(trackingId))
                    {
                        _BodyScaleFactors.Remove(trackingId);
                    }
                }
            }

            foreach (var body in data)
            {
                if (body == null)
                {
                    continue;
                }

                if (body.IsTracked)
                {
                    if (!_Bodies.ContainsKey(body.TrackingId))
                    {
                        _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                    }

                    RefreshBodyObject(body, _Bodies[body.TrackingId]);
                }
            }
        }

        private GameObject CreateBodyObject(ulong id)
        {
            GameObject body = new GameObject("Body:" + id);

            // Agregar el Rigidbody
            Rigidbody rb = body.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Agregar el BoxCollider como "Is Trigger"
            BoxCollider collider = body.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(1.5f, 2f, 0.5f); // Ajusta según el tamaño del cuerpo

            /*
            // Crear una caja visual para depuración del collider
            GameObject debugCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugCollider.transform.SetParent(body.transform);
            debugCollider.transform.localPosition = collider.center;
            debugCollider.transform.localScale = collider.size;

            // Hacerla semi-transparente
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 1f, 0f, 0.3f); // amarillo semi-transparente
            mat.SetFloat("_Mode", 3); // transparente
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            debugCollider.GetComponent<Renderer>().material = mat;
            

            // Eliminar colisionador del objeto visual
            Destroy(debugCollider.GetComponent<Collider>());
            */

            // Agregar el script de detección de colisiones
            body.AddComponent<PlayerCollisionDetector>();

            // Crear los cubos de articulaciones
            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                LineRenderer lr = jointObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.material = BoneMaterial;
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;

                jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                jointObj.name = jt.ToString();
                jointObj.transform.parent = body.transform;

                // Opcional: Desactiva collider del cubo individual
                GameObject.Destroy(jointObj.GetComponent<Collider>());
            }

            return body;
        }

        private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
        {
            float scaleFactor;

            // 1. Usar el factor guardado si ya existe
            if (_BodyScaleFactors.ContainsKey(body.TrackingId))
            {
                scaleFactor = _BodyScaleFactors[body.TrackingId];
            }
            else
            {
                // 2. Calcular la altura real del cuerpo
                var head = body.Joints[Kinect.JointType.Head].Position;
                var footLeft = body.Joints[Kinect.JointType.FootLeft].Position;
                var footRight = body.Joints[Kinect.JointType.FootRight].Position;

                float footY = Mathf.Min(footLeft.Y, footRight.Y);
                float realHeight = head.Y - footY;

                // 3. Definir altura deseada (en metros)
                float desiredHeight = 1.7f;

                // 4. Calcular factor de escala con protección mínima
                scaleFactor = realHeight > 0.1f ? desiredHeight / realHeight : 1f;

                // 5. Guardar el factor para evitar recalcularlo cada frame
                _BodyScaleFactors[body.TrackingId] = scaleFactor;
            }

            // 6. Ajustar el BoxCollider del cuerpo
            BoxCollider collider = bodyObject.GetComponent<BoxCollider>();
            if (collider != null)
            {
                Vector3 baseSize = new Vector3(1.5f, 2f, 0.5f);
                Vector3 baseCenter = new Vector3(0f, 1f, 0f);

                Vector3 currentSize = collider.size;
                Vector3 targetSize = baseSize * scaleFactor;
                collider.size = Vector3.Lerp(currentSize, targetSize, jointSmoothFactor);

                Vector3 currentCenter = collider.center;
                Vector3 targetCenter = baseCenter * scaleFactor;
                collider.center = Vector3.Lerp(currentCenter, targetCenter, jointSmoothFactor);
            }

            // 7. Posicionar las articulaciones y actualizar líneas
            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                Kinect.Joint sourceJoint = body.Joints[jt];
                Kinect.Joint? targetJoint = null;

                if (_BoneMap.ContainsKey(jt))
                {
                    targetJoint = body.Joints[_BoneMap[jt]];
                }

                Transform jointObj = bodyObject.transform.Find(jt.ToString());

                // Posición escalada con interpolacion
                Vector3 currentPos = jointObj.localPosition;
                Vector3 targetPos = GetScaledVector3FromJoint(sourceJoint, scaleFactor);
                jointObj.localPosition = Vector3.Lerp(currentPos, targetPos, jointSmoothFactor);

                // Línea entre articulaciones
                LineRenderer lr = jointObj.GetComponent<LineRenderer>();
                if (targetJoint.HasValue)
                {
                    Vector3 targetJointPos = GetScaledVector3FromJoint(targetJoint.Value, scaleFactor);
                    lr.SetPosition(0, jointObj.localPosition);
                    lr.SetPosition(1, targetJointPos);
                    lr.SetColors(GetColorForState(sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
                }
                else
                {
                    lr.enabled = false;
                }
            }
        }

        private static Color GetColorForState(Kinect.TrackingState state)
        {
            switch (state)
            {
                case Kinect.TrackingState.Tracked:
                    return Color.green;

                case Kinect.TrackingState.Inferred:
                    return Color.red;

                default:
                    return Color.black;
            }
        }

        private static Vector3 GetScaledVector3FromJoint(Kinect.Joint joint, float scale)
        {
            // Aplica escala personalizada después de convertir a espacio Unity
            return new Vector3(
                -joint.Position.X * 10 * scale,
                joint.Position.Y * 10 * scale,
                joint.Position.Z * 10 * scale
            );
        }
    }
}

