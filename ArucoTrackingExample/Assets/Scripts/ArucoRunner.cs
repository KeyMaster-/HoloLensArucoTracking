using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Diagnostics;

public class ArucoRunner : MonoBehaviour {
    public CameraProvider camProvider;
    public float markerSize;
    public int sizeReduce;

    public Vector3 offset;

        //Changes in pos/rot below these thresholds are ignored
    public float positionLowPass = 0.005f; //Value in meters
    public float rotationLowPass = 3; //Value in degrees

    public int avgFilterMemoryLength = 5;
    PoseRunningAverage average;

    public Dictionary<int, PoseData> poseDict;

    public event Action onDetectionRun;

    // Use this for initialization
    public void init () {
        int imgWidth;
        int imgHeight;
        float[] camParams;

        camProvider.init(out imgWidth, out imgHeight, out camParams);
            //Test if we got a valid image
        if(imgWidth > 0 && imgHeight > 0) {
            ArucoTracking.init(imgWidth, imgHeight, markerSize, camParams, sizeReduce);
        }

        average = new PoseRunningAverage(avgFilterMemoryLength);
        poseDict = new Dictionary<int, PoseData>();
	}

    public virtual void runDetect() {
        if (!ArucoTracking.lib_inited) return;

        //Stopwatch timer = Stopwatch.StartNew();

        trackNewFrame();

        //timer.Stop();
        //UnityEngine.Debug.Log(timer.ElapsedMilliseconds);

        Dictionary<int, PoseData> newDict = ArucoTrackingUtil.createUnityPoseData(ArucoTracking.marker_count, ArucoTracking.ids, ArucoTracking.rvecs, ArucoTracking.tvecs);

        ArucoTrackingUtil.addCamSpaceOffset(newDict, offset); //Doing this first is important, since PoseDict also has positions with added offset
        ArucoTrackingUtil.posRotLowpass(poseDict, newDict, positionLowPass, rotationLowPass);
        average.averageNewState(newDict);

        poseDict = newDict;

        invokeOnDetectionRun();
    }

        //Necessary since only the declaring class can invoke an event
    protected void invokeOnDetectionRun() {
        onDetectionRun.Invoke();
    }

    protected void trackNewFrame() {
        Color32[] img_data = camProvider.getImage();
        ArucoTracking.detect_markers(img_data);
    }
}
