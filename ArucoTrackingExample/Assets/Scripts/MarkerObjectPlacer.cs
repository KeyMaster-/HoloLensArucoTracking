using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class MarkerObjectPlacer : MonoBehaviour {
    public ArucoRunner trackingRunner;
    public Camera cam;
    
        //The object should be oriented such that the direction pointing out of the marker image aligns with Z+.
    public GameObject markerObjectPrefab;

    protected List<GameObject> quadInstances;
    
    virtual protected void Start () {
        trackingRunner.onDetectionRun += onDetectionRun;

        quadInstances = new List<GameObject>();
    }

    virtual protected void onDetectionRun() {

        //Add/remove quads to match how many we saw
        if (quadInstances.Count > ArucoTracking.marker_count) {
            //Clear out any instances we don't need anymore
            for (int i = quadInstances.Count - 1; i >= ArucoTracking.marker_count; i--) {
                GameObject.Destroy(quadInstances[i]);
                quadInstances.RemoveAt(i);
            }
        }
        else if (ArucoTracking.marker_count > quadInstances.Count) {
            int to_add = ArucoTracking.marker_count - quadInstances.Count;
            for (int i = 0; i < to_add; i++) {
                quadInstances.Add(makeMarkerObject());
            }
        }

        for (int i = 0; i < ArucoTracking.marker_count; i++) {
            PoseData pose = trackingRunner.poseDict[ArucoTracking.ids[i]];
            quadInstances[i].transform.localPosition = pose.pos;
            quadInstances[i].transform.localRotation = pose.rot;
        }
    }

    GameObject makeMarkerObject()
    {
        GameObject quad = GameObject.Instantiate(markerObjectPrefab);
        quad.transform.localScale = new Vector3(trackingRunner.markerSize, trackingRunner.markerSize, trackingRunner.markerSize);
        quad.transform.parent = cam.transform;
        return quad;
    }
}
