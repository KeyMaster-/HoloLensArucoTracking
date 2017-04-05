using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedObject : MonoBehaviour {
    public int markerId;

    Quaternion baseRotation;

    public ArucoRunner trackingRunner;
        //If true, the object will not be deactivated even when the marker is not being detected
    public bool persist = false;
        //If this is specified, the marker will be placed in world space based on this camera. Otherwise, the object's local position is simply set to the pose data
    public Camera parentCamera;
    
	void Start () {
        trackingRunner.onDetectionRun += onDetectionRun;
        baseRotation = transform.localRotation;
	}

    private void onDetectionRun() {
        if (trackingRunner.poseDict.ContainsKey(markerId)) {
            if(!persist) gameObject.SetActive(true);
            PoseData pose = trackingRunner.poseDict[markerId];
            if(parentCamera == null) {
                gameObject.transform.localPosition = pose.pos;
            }
            else {
                Vector3 posePos = pose.pos;
                posePos.z = -posePos.z;
                gameObject.transform.position = parentCamera.cameraToWorldMatrix.MultiplyPoint(posePos);
            }
            
            gameObject.transform.localRotation = pose.rot * baseRotation;
        }
        else {
            if (!persist) gameObject.SetActive(false);
        }
    }
}
