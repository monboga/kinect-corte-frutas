using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

namespace KinectPosturas
{

    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using Kinect = Windows.Kinect;

    namespace KinectPosturas
    {
        public class BodySourceView : MonoBehaviour
        {
            [Header("Materiales y Referencias")]
            public Material BoneMaterial;
            public GameObject BodySourceManager;

            [Header("Configuración de Suavizado")]
            [Range(0f, 1f)]
            public float jointSmoothFactor = 0.5f; // 0 = sin movimiento, 1 = sin suavizado

            [Header("Configuración de Colisiones")]
            public bool enableMultipleColliders = true; // Activar múltiples colliders
            public bool showDebugColliders = false; // Mostrar colliders visuales
            public Vector3 mainColliderSize = new Vector3(1.5f, 2f, 0.5f);
            public Vector3 headColliderSize = new Vector3(0.8f, 0.8f, 0.5f);
            public Vector3 armColliderSize = new Vector3(1.2f, 0.3f, 0.3f);
            public Vector3 legColliderSize = new Vector3(0.4f, 1.5f, 0.3f);

            // Diccionarios privados para manejo interno
            private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
            private BodySourceManager _BodyManager;
            private Dictionary<ulong, float> _BodyScaleFactors = new Dictionary<ulong, float>();

            // Mapeo de huesos para las líneas del esqueleto
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
                // Verificar que el BodySourceManager esté asignado
                if (BodySourceManager == null)
                {
                    Debug.LogWarning("BodySourceManager no está asignado");
                    return;
                }

                // Obtener el componente BodySourceManager
                _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
                if (_BodyManager == null)
                {
                    Debug.LogWarning("No se encontró el componente BodySourceManager");
                    return;
                }

                // Obtener los datos del cuerpo
                Kinect.Body[] data = _BodyManager.GetData();
                if (data == null)
                {
                    return;
                }

                // Crear lista de IDs rastreados
                List<ulong> trackedIds = new List<ulong>();
                foreach (var body in data)
                {
                    if (body != null && body.IsTracked)
                    {
                        trackedIds.Add(body.TrackingId);
                    }
                }

                // Obtener IDs conocidos
                List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

                // Eliminar cuerpos no rastreados
                foreach (ulong trackingId in knownIds)
                {
                    if (!trackedIds.Contains(trackingId))
                    {
                        Debug.Log($"Eliminando cuerpo con ID: {trackingId}");
                        Destroy(_Bodies[trackingId]);
                        _Bodies.Remove(trackingId);

                        // Limpiar el factor de escala guardado
                        if (_BodyScaleFactors.ContainsKey(trackingId))
                        {
                            _BodyScaleFactors.Remove(trackingId);
                        }
                    }
                }

                // Procesar cuerpos rastreados
                foreach (var body in data)
                {
                    if (body != null && body.IsTracked)
                    {
                        // Crear nuevo cuerpo si no existe
                        if (!_Bodies.ContainsKey(body.TrackingId))
                        {
                            Debug.Log($"Creando nuevo cuerpo con ID: {body.TrackingId}");
                            _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                        }

                        // Actualizar el cuerpo existente
                        RefreshBodyObject(body, _Bodies[body.TrackingId]);
                    }
                }
            }

            private GameObject CreateBodyObject(ulong id)
            {
                // Crear el GameObject principal del cuerpo
                GameObject body = new GameObject("Body:" + id);

                // Agregar Rigidbody configurado para Kinematic
                Rigidbody rb = body.AddComponent<Rigidbody>();
                rb.isKinematic = true; // No afectado por física
                rb.useGravity = false; // Sin gravedad

                // Crear sistema de colliders múltiples si está activado
                if (enableMultipleColliders)
                {
                    CreateMultipleColliders(body);
                }
                else
                {
                    // Crear collider principal único
                    CreateMainCollider(body);
                }

                // Agregar el script de detección de colisiones
                body.AddComponent<PlayerCollisionDetector>();

                // Crear los cubos de articulaciones para visualización
                CreateJointObjects(body);

                return body;
            }

            private void CreateMainCollider(GameObject body)
            {
                // Crear el BoxCollider principal como trigger
                BoxCollider collider = body.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = mainColliderSize;
                collider.center = new Vector3(0f, 1f, 0f); // Centrado en el cuerpo

                // Crear visualización del collider si está activado
                if (showDebugColliders)
                {
                    CreateDebugCollider(body, collider.size, collider.center, Color.yellow, "MainCollider");
                }

                Debug.Log($"Collider principal creado - Tamaño: {collider.size}, Centro: {collider.center}");
            }

            private void CreateMultipleColliders(GameObject body)
            {
                // Collider para cabeza
                GameObject headColliderObj = new GameObject("HeadCollider");
                headColliderObj.transform.SetParent(body.transform);
                BoxCollider headCollider = headColliderObj.AddComponent<BoxCollider>();
                headCollider.isTrigger = true;
                headCollider.size = headColliderSize;
                headCollider.center = new Vector3(0f, 1.7f, 0f);
                headColliderObj.AddComponent<PlayerCollisionDetector>();

                // Collider para brazo izquierdo
                GameObject leftArmColliderObj = new GameObject("LeftArmCollider");
                leftArmColliderObj.transform.SetParent(body.transform);
                BoxCollider leftArmCollider = leftArmColliderObj.AddComponent<BoxCollider>();
                leftArmCollider.isTrigger = true;
                leftArmCollider.size = armColliderSize;
                leftArmCollider.center = new Vector3(-0.8f, 1.2f, 0f);
                leftArmColliderObj.AddComponent<PlayerCollisionDetector>();

                // Collider para brazo derecho
                GameObject rightArmColliderObj = new GameObject("RightArmCollider");
                rightArmColliderObj.transform.SetParent(body.transform);
                BoxCollider rightArmCollider = rightArmColliderObj.AddComponent<BoxCollider>();
                rightArmCollider.isTrigger = true;
                rightArmCollider.size = armColliderSize;
                rightArmCollider.center = new Vector3(0.8f, 1.2f, 0f);
                rightArmColliderObj.AddComponent<PlayerCollisionDetector>();

                // Collider para pierna izquierda
                GameObject leftLegColliderObj = new GameObject("LeftLegCollider");
                leftLegColliderObj.transform.SetParent(body.transform);
                BoxCollider leftLegCollider = leftLegColliderObj.AddComponent<BoxCollider>();
                leftLegCollider.isTrigger = true;
                leftLegCollider.size = legColliderSize;
                leftLegCollider.center = new Vector3(-0.3f, 0.3f, 0f);
                leftLegColliderObj.AddComponent<PlayerCollisionDetector>();

                // Collider para pierna derecha
                GameObject rightLegColliderObj = new GameObject("RightLegCollider");
                rightLegColliderObj.transform.SetParent(body.transform);
                BoxCollider rightLegCollider = rightLegColliderObj.AddComponent<BoxCollider>();
                rightLegCollider.isTrigger = true;
                rightLegCollider.size = legColliderSize;
                rightLegCollider.center = new Vector3(0.3f, 0.3f, 0f);
                rightLegColliderObj.AddComponent<PlayerCollisionDetector>();

                // Crear visualizaciones de debug si están activadas
                if (showDebugColliders)
                {
                    CreateDebugCollider(headColliderObj, headCollider.size, headCollider.center, Color.red, "HeadDebug");
                    CreateDebugCollider(leftArmColliderObj, leftArmCollider.size, leftArmCollider.center, Color.blue, "LeftArmDebug");
                    CreateDebugCollider(rightArmColliderObj, rightArmCollider.size, rightArmCollider.center, Color.blue, "RightArmDebug");
                    CreateDebugCollider(leftLegColliderObj, leftLegCollider.size, leftLegCollider.center, Color.green, "LeftLegDebug");
                    CreateDebugCollider(rightLegColliderObj, rightLegCollider.size, rightLegCollider.center, Color.green, "RightLegDebug");
                }

                Debug.Log("Sistema de colliders múltiples creado");
            }

            private void CreateDebugCollider(GameObject parent, Vector3 size, Vector3 center, Color color, string name)
            {
                // Crear cubo visual para debug
                GameObject debugCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
                debugCollider.name = name;
                debugCollider.transform.SetParent(parent.transform);
                debugCollider.transform.localPosition = center;
                debugCollider.transform.localScale = size;

                // Crear material semi-transparente
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(color.r, color.g, color.b, 0.3f);
                mat.SetFloat("_Mode", 3); // Modo transparente
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;

                debugCollider.GetComponent<Renderer>().material = mat;

                // Eliminar el collider del objeto visual (solo es para debug)
                Destroy(debugCollider.GetComponent<Collider>());
            }

            private void CreateJointObjects(GameObject body)
            {
                // Crear los cubos de articulaciones para visualización del esqueleto
                for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
                {
                    GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    // Configurar LineRenderer para las líneas del esqueleto
                    LineRenderer lr = jointObj.AddComponent<LineRenderer>();
                    lr.positionCount = 2;
                    lr.material = BoneMaterial;
                    lr.startWidth = 0.05f;
                    lr.endWidth = 0.05f;

                    // Configurar el cubo de la articulación
                    jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    jointObj.name = jt.ToString();
                    jointObj.transform.parent = body.transform;

                    // Eliminar collider del cubo individual (no necesario para colisiones)
                    Destroy(jointObj.GetComponent<Collider>());
                }
            }

            private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
            {
                // Calcular o recuperar el factor de escala
                float scaleFactor = GetOrCalculateScaleFactor(body);

                // Actualizar colliders con el factor de escala
                UpdateColliders(bodyObject, scaleFactor);

                // Actualizar posiciones de articulaciones y líneas del esqueleto
                UpdateJointPositions(body, bodyObject, scaleFactor);
            }

            private float GetOrCalculateScaleFactor(Kinect.Body body)
            {
                // Usar el factor guardado si ya existe
                if (_BodyScaleFactors.ContainsKey(body.TrackingId))
                {
                    return _BodyScaleFactors[body.TrackingId];
                }

                // Calcular la altura real del cuerpo
                var head = body.Joints[Kinect.JointType.Head].Position;
                var footLeft = body.Joints[Kinect.JointType.FootLeft].Position;
                var footRight = body.Joints[Kinect.JointType.FootRight].Position;

                float footY = Mathf.Min(footLeft.Y, footRight.Y);
                float realHeight = head.Y - footY;

                // Definir altura deseada (en metros)
                float desiredHeight = 1.7f;

                // Calcular factor de escala con protección mínima
                float scaleFactor = realHeight > 0.1f ? desiredHeight / realHeight : 1f;

                // Guardar el factor para evitar recalcularlo
                _BodyScaleFactors[body.TrackingId] = scaleFactor;

                Debug.Log($"Factor de escala calculado para cuerpo {body.TrackingId}: {scaleFactor} (Altura real: {realHeight}m)");

                return scaleFactor;
            }

            private void UpdateColliders(GameObject bodyObject, float scaleFactor)
            {
                if (enableMultipleColliders)
                {
                    // Actualizar múltiples colliders
                    UpdateMultipleColliders(bodyObject, scaleFactor);
                }
                else
                {
                    // Actualizar collider principal
                    UpdateMainCollider(bodyObject, scaleFactor);
                }
            }

            private void UpdateMainCollider(GameObject bodyObject, float scaleFactor)
            {
                BoxCollider collider = bodyObject.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    Vector3 baseSize = mainColliderSize;
                    Vector3 baseCenter = new Vector3(0f, 1f, 0f);

                    Vector3 currentSize = collider.size;
                    Vector3 targetSize = baseSize * scaleFactor;
                    collider.size = Vector3.Lerp(currentSize, targetSize, jointSmoothFactor);

                    Vector3 currentCenter = collider.center;
                    Vector3 targetCenter = baseCenter * scaleFactor;
                    collider.center = Vector3.Lerp(currentCenter, targetCenter, jointSmoothFactor);
                }
            }

            private void UpdateMultipleColliders(GameObject bodyObject, float scaleFactor)
            {
                // Actualizar cada collider hijo
                Transform[] childColliders = {
                bodyObject.transform.Find("HeadCollider"),
                bodyObject.transform.Find("LeftArmCollider"),
                bodyObject.transform.Find("RightArmCollider"),
                bodyObject.transform.Find("LeftLegCollider"),
                bodyObject.transform.Find("RightLegCollider")
            };

                Vector3[] baseSizes = {
                headColliderSize,
                armColliderSize,
                armColliderSize,
                legColliderSize,
                legColliderSize
            };

                Vector3[] baseCenters = {
                new Vector3(0f, 1.7f, 0f),
                new Vector3(-0.8f, 1.2f, 0f),
                new Vector3(0.8f, 1.2f, 0f),
                new Vector3(-0.3f, 0.3f, 0f),
                new Vector3(0.3f, 0.3f, 0f)
            };

                for (int i = 0; i < childColliders.Length; i++)
                {
                    if (childColliders[i] != null)
                    {
                        BoxCollider collider = childColliders[i].GetComponent<BoxCollider>();
                        if (collider != null)
                        {
                            Vector3 currentSize = collider.size;
                            Vector3 targetSize = baseSizes[i] * scaleFactor;
                            collider.size = Vector3.Lerp(currentSize, targetSize, jointSmoothFactor);

                            Vector3 currentCenter = collider.center;
                            Vector3 targetCenter = baseCenters[i] * scaleFactor;
                            collider.center = Vector3.Lerp(currentCenter, targetCenter, jointSmoothFactor);
                        }
                    }
                }
            }

            private void UpdateJointPositions(Kinect.Body body, GameObject bodyObject, float scaleFactor)
            {
                // Actualizar posiciones de articulaciones y líneas del esqueleto
                for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
                {
                    Kinect.Joint sourceJoint = body.Joints[jt];
                    Kinect.Joint? targetJoint = null;

                    if (_BoneMap.ContainsKey(jt))
                    {
                        targetJoint = body.Joints[_BoneMap[jt]];
                    }

                    Transform jointObj = bodyObject.transform.Find(jt.ToString());
                    if (jointObj != null)
                    {
                        // Posición escalada con interpolación
                        Vector3 currentPos = jointObj.localPosition;
                        Vector3 targetPos = GetScaledVector3FromJoint(sourceJoint, scaleFactor);
                        jointObj.localPosition = Vector3.Lerp(currentPos, targetPos, jointSmoothFactor);

                        // Actualizar línea entre articulaciones
                        LineRenderer lr = jointObj.GetComponent<LineRenderer>();
                        if (lr != null)
                        {
                            if (targetJoint.HasValue)
                            {
                                Vector3 targetJointPos = GetScaledVector3FromJoint(targetJoint.Value, scaleFactor);
                                lr.SetPosition(0, jointObj.localPosition);
                                lr.SetPosition(1, targetJointPos);
                                lr.SetColors(GetColorForState(sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
                                lr.enabled = true;
                            }
                            else
                            {
                                lr.enabled = false;
                            }
                        }
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
                // Convertir coordenadas de Kinect a Unity y aplicar escala
                return new Vector3(
                    -joint.Position.X * 10 * scale,
                    joint.Position.Y * 10 * scale,
                    joint.Position.Z * 10 * scale
                );
            }
        }
    }
}

