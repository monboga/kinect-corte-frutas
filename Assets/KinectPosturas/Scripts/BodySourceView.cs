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

        private ulong? _activeTrackingId = null;

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

            // Validar si el esqueleto activo sigue siendo rastreado
            if (_activeTrackingId.HasValue && !trackedIds.Contains(_activeTrackingId.Value))
            {
                if (_Bodies.ContainsKey(_activeTrackingId.Value))
                {
                    Destroy(_Bodies[_activeTrackingId.Value]);
                    _Bodies.Remove(_activeTrackingId.Value);
                    _BodyScaleFactors.Remove(_activeTrackingId.Value);
                }

                _activeTrackingId = null;
            }

            if (!_activeTrackingId.HasValue)
            {
                foreach (var body in data)
                {
                    if (body != null && body.IsTracked)
                    {
                        _activeTrackingId = body.TrackingId;
                        _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                        RefreshBodyObject(body, _Bodies[body.TrackingId]);
                        break;
                    }
                }
            }
            else
            {
                foreach (var body in data)
                {
                    if (body != null && body.IsTracked && body.TrackingId == _activeTrackingId.Value)
                    {
                        RefreshBodyObject(body, _Bodies[body.TrackingId]);
                        break;
                    }
                }
            }
        }

        private GameObject CreateBodyObject(ulong id)
        {
            GameObject body = new GameObject("Body:" + id);

            int jointLayer = LayerMask.NameToLayer("Joint");
            if (jointLayer == -1)
            {
                Debug.LogWarning("La capa 'Joint' no existe. Ve a Edit > Project Settings > Tags and Layers para crearla.");
            }

            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                jointObj.name = jt.ToString();
                jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                jointObj.transform.parent = body.transform;

                if (jointLayer != -1)
                {
                    jointObj.layer = jointLayer;
                }

                GameObject.Destroy(jointObj.GetComponent<Collider>());

                BoxCollider jointCollider = jointObj.AddComponent<BoxCollider>();
                jointCollider.isTrigger = true;

                Rigidbody jointRb = jointObj.AddComponent<Rigidbody>();
                jointRb.isKinematic = true;
                jointRb.useGravity = false;

                JointCollisionDetector detector = jointObj.AddComponent<JointCollisionDetector>();
                detector.region = GetRegionForJoint(jt);

                LineRenderer lr = jointObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.material = BoneMaterial;
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lr.useWorldSpace = true;
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

            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                Kinect.Joint sourceJoint = body.Joints[jt];
                Kinect.Joint? targetJoint = _BoneMap.ContainsKey(jt) ? (Kinect.Joint?)body.Joints[_BoneMap[jt]] : null;

                Transform jointObj = bodyObject.transform.Find(jt.ToString());
                if (jointObj == null) continue;

                Vector3 currentPos = jointObj.position;
                Vector3 targetPos = GetScaledWorldPositionFromJoint(sourceJoint, scaleFactor);
                jointObj.position = Vector3.Lerp(currentPos, targetPos, jointSmoothFactor);

                LineRenderer lr = jointObj.GetComponent<LineRenderer>();
                if (lr != null && targetJoint.HasValue)
                {
                    Transform targetTransform = bodyObject.transform.Find(_BoneMap[jt].ToString());
                    if (targetTransform != null)
                    {
                        lr.useWorldSpace = true;
                        lr.SetPosition(0, jointObj.position);
                        lr.SetPosition(1, targetTransform.position);
                        lr.SetColors(GetColorForState(sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
                        lr.enabled = true;
                    }
                    else
                    {
                        lr.enabled = false;
                    }
                }
                else if (lr != null)
                {
                    lr.enabled = false;
                }
            }
        }

        private static Vector3 GetScaledWorldPositionFromJoint(Kinect.Joint joint, float scale)
        {
            return new Vector3(-joint.Position.X, joint.Position.Y, joint.Position.Z) * 10f * scale;
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

        private BodyRegion GetRegionForJoint(Kinect.JointType jt)
        {
            switch (jt)
            {
                case Kinect.JointType.HandLeft:
                case Kinect.JointType.WristLeft:
                case Kinect.JointType.ElbowLeft:
                case Kinect.JointType.ShoulderLeft:
                case Kinect.JointType.ThumbLeft:
                case Kinect.JointType.HandTipLeft:
                    return BodyRegion.LeftArm;

                case Kinect.JointType.HandRight:
                case Kinect.JointType.WristRight:
                case Kinect.JointType.ElbowRight:
                case Kinect.JointType.ShoulderRight:
                case Kinect.JointType.ThumbRight:
                case Kinect.JointType.HandTipRight:
                    return BodyRegion.RightArm;

                case Kinect.JointType.FootLeft:
                case Kinect.JointType.AnkleLeft:
                case Kinect.JointType.KneeLeft:
                case Kinect.JointType.HipLeft:
                    return BodyRegion.LeftLeg;

                case Kinect.JointType.FootRight:
                case Kinect.JointType.AnkleRight:
                case Kinect.JointType.KneeRight:
                case Kinect.JointType.HipRight:
                    return BodyRegion.RightLeg;

                case Kinect.JointType.Head:
                case Kinect.JointType.Neck:
                    return BodyRegion.Head;

                default:
                    return BodyRegion.Torso;
            }
        }
    }
}