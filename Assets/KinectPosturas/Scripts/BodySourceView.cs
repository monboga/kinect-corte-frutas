using UnityEngine;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

namespace KinectPosturas
{
    public class BodySourceView : MonoBehaviour
    {
        public Material BoneMaterial;
        public GameObject BodySourceManager;

        [Range(0f, 1f)]
        public float jointSmoothFactor = 0.5f;

        private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
        private BodySourceManager _BodyManager;
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
            if (BodySourceManager == null) return;

            _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
            if (_BodyManager == null) return;

            Kinect.Body[] data = _BodyManager.GetData();
            if (data == null) return;

            List<ulong> trackedIds = new List<ulong>();
            foreach (var body in data)
            {
                if (body != null && body.IsTracked)
                {
                    trackedIds.Add(body.TrackingId);
                }
            }

            List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
            foreach (ulong trackingId in knownIds)
            {
                if (!trackedIds.Contains(trackingId))
                {
                    Destroy(_Bodies[trackingId]);
                    _Bodies.Remove(trackingId);
                    _BodyScaleFactors.Remove(trackingId);
                }
            }

            foreach (var body in data)
            {
                if (body == null || !body.IsTracked) continue;

                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }

        private GameObject CreateBodyObject(ulong id)
        {
            GameObject body = new GameObject("Body:" + id);

            // Agregar el Rigidbody general del cuerpo
            Rigidbody rb = body.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Agregar el BoxCollider general
            BoxCollider collider = body.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(1.5f, 2f, 0.5f);

            // Agregar script de detección general si lo necesitas/////////////////////////
            //body.AddComponent<PlayerCollisionDetector>();

            // Verificar existencia de la capa "Joint"
            int jointLayer = LayerMask.NameToLayer("Joint");
            if (jointLayer == -1)
            {
                Debug.LogWarning("La capa 'Joint' no existe. Ve a Edit > Project Settings > Tags and Layers para crearla.");
            }

            // Crear los cubos de articulaciones
            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                jointObj.name = jt.ToString();
                jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                jointObj.transform.parent = body.transform;

                // Asignar layer "Joint" si existe
                if (jointLayer != -1)
                {
                    jointObj.layer = jointLayer;
                }

                // Eliminar collider primitivo original
                GameObject.Destroy(jointObj.GetComponent<Collider>());

                // Añadir collider y rigidbody para detección de colisiones individuales
                BoxCollider jointCollider = jointObj.AddComponent<BoxCollider>();
                jointCollider.isTrigger = true;

                Rigidbody jointRb = jointObj.AddComponent<Rigidbody>();
                jointRb.isKinematic = true;
                jointRb.useGravity = false;

                // Agregar script de colisión por articulación
                jointObj.AddComponent<JointCollisionDetector>();

                // Agregar línea para dibujar huesos
                LineRenderer lr = jointObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.material = BoneMaterial;
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
            }

            return body;
        }

        private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
        {
            float scaleFactor;

            if (_BodyScaleFactors.ContainsKey(body.TrackingId))
            {
                scaleFactor = _BodyScaleFactors[body.TrackingId];
            }
            else
            {
                var head = body.Joints[Kinect.JointType.Head].Position;
                var footLeft = body.Joints[Kinect.JointType.FootLeft].Position;
                var footRight = body.Joints[Kinect.JointType.FootRight].Position;

                float footY = Mathf.Min(footLeft.Y, footRight.Y);
                float realHeight = head.Y - footY;
                float desiredHeight = 1.7f;

                scaleFactor = realHeight > 0.1f ? desiredHeight / realHeight : 1f;
                _BodyScaleFactors[body.TrackingId] = scaleFactor;
            }

            var baseJoint = body.Joints[Kinect.JointType.SpineBase];
            Vector3 bodyWorldPos = new Vector3(
                -baseJoint.Position.X * 10f * scaleFactor,
                baseJoint.Position.Y * 10f * scaleFactor,
                baseJoint.Position.Z * 10f * scaleFactor
            );
            bodyObject.transform.position = bodyWorldPos;

            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                Kinect.Joint sourceJoint = body.Joints[jt];
                Kinect.Joint? targetJoint = _BoneMap.ContainsKey(jt) ? (Kinect.Joint?)body.Joints[_BoneMap[jt]] : null;

                Transform jointObj = bodyObject.transform.Find(jt.ToString());
                if (jointObj == null) continue;

                Vector3 currentPos = jointObj.localPosition;
                Vector3 targetPos = GetScaledLocalPositionFromJoint(sourceJoint, baseJoint, scaleFactor);
                jointObj.localPosition = Vector3.Lerp(currentPos, targetPos, jointSmoothFactor);

                LineRenderer lr = jointObj.GetComponent<LineRenderer>();
                if (lr != null && targetJoint.HasValue)
                {
                    Vector3 targetJointPos = GetScaledLocalPositionFromJoint(targetJoint.Value, baseJoint, scaleFactor);
                    lr.SetPosition(0, jointObj.localPosition);
                    lr.SetPosition(1, targetJointPos);
                    lr.SetColors(GetColorForState(sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
                }
                else if (lr != null)
                {
                    lr.enabled = false;
                }
            }
        }

        private static Vector3 GetScaledLocalPositionFromJoint(Kinect.Joint joint, Kinect.Joint reference, float scale)
        {
            Vector3 jointPos = new Vector3(-joint.Position.X, joint.Position.Y, joint.Position.Z);
            Vector3 refPos = new Vector3(-reference.Position.X, reference.Position.Y, reference.Position.Z);
            return (jointPos - refPos) * 10f * scale;
        }

        private static Color GetColorForState(Kinect.TrackingState state)
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