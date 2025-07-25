﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

namespace KinectCorteFrutas
{
    public class BodySourceView : MonoBehaviour
    {
        public BodySourceManager mBodySourceManager;
        public GameObject mJointObject;

        private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>();
        private List<JointType> _joints = new List<JointType>
    {

        JointType.HandLeft,
        JointType.HandRight

    };

        void Update()
        {

            #region Get Kinect data
            Body[] data = mBodySourceManager.GetData();
            if (data == null)
                return;

            List<ulong> trackedIds = new List<ulong>();
            foreach (var body in data)
            {
                if (body == null)
                    continue;

                if (body.IsTracked)
                    trackedIds.Add(body.TrackingId);
            }
            #endregion

            #region Delete Kinect bodies
            List<ulong> knownIds = new List<ulong>(mBodies.Keys);
            foreach (ulong trackingId in knownIds)
            {
                if (!trackedIds.Contains(trackingId))
                {
                    // Destroy body object
                    Destroy(mBodies[trackingId]);

                    // Remove from list
                    mBodies.Remove(trackingId);
                }
            }
            #endregion

            #region Create Kinect Bodies
            foreach (var body in data)
            {
                // if no body, skip
                if (body == null)
                    continue;

                if (body.IsTracked)
                {
                    // if body isn´t tracked, create body.
                    if (!mBodies.ContainsKey(body.TrackingId))
                        mBodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

                    // update positions
                    UpdateBodyObject(body, mBodies[body.TrackingId]);
                }
            }
            #endregion

        }

        private GameObject CreateBodyObject(ulong id)
        {

            // Create body parent
            GameObject body = new GameObject("Body:" + id);

            // si no hay un prefab de articulacion asignado, no intentes crear nada.
            if (mJointObject == null)
            {
                return body; // simplemente devolvemos elobjeto padre vacio.
            }

            //Create joints
            foreach (JointType joint in _joints)
            {
                //Create object
                GameObject newJoint = Instantiate(mJointObject);
                newJoint.name = joint.ToString();

                // Parent to body
                newJoint.transform.parent = body.transform;
            }

            return body;
        }

        private void UpdateBodyObject(Body body, GameObject bodyObject)
        {
            //Update joints
            foreach (JointType _joint in _joints)
            {
                // Get new target position
                Joint sourceJoint = body.Joints[_joint];
                Vector3 targetPosition = GetVector3FromJoint(sourceJoint);
                targetPosition.z = 0;

                // Get joint, Set new position
                Transform joinObject = bodyObject.transform.Find(_joint.ToString());
                joinObject.position = targetPosition;
            }
        }

        private static Vector3 GetVector3FromJoint(Joint joint)
        {
            return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
        }
    }
}


