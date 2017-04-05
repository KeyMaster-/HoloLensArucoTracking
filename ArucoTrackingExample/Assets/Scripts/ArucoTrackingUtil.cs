using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PoseData {
    public Vector3 pos;
    public Quaternion rot;
}

public class ArucoTrackingUtil {

    public static Dictionary<int, PoseData> createUnityPoseData(int marker_count, int[] ids, double[] rvecs, double[] tvecs) {
        Dictionary<int, PoseData> out_dict = new Dictionary<int, PoseData>();
        if (marker_count == 0) return out_dict;

        Vector3 rvec = new Vector3();
        for(int i=0; i<marker_count; i++) {
            PoseData data = new PoseData();
            data.pos.Set((float)tvecs[i * 3], (float)tvecs[i * 3 + 1], (float)tvecs[i * 3 + 2]);
            

            rvec.Set((float)rvecs[i * 3], (float)rvecs[i * 3 + 1], (float)rvecs[i * 3 + 2]);

            float theta = rvec.magnitude;
            rvec.Normalize();

            //the rvec from OpenCV is a compact axis-angle format. The direction of the vector is the axis, and the length of it is the angle to rotate about (i.e. theta)
            //From this stackoverflow answer: http://stackoverflow.com/questions/12933284/rodrigues-into-eulerangles-and-vice-versa
            data.rot = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, rvec);

            out_dict[ids[i]] = data;
        }

        return out_dict;
    }

        //Applies the given offset to the positions of all markers in the dictionary, in the coordinate space of the markers (i.e. camera space)
        //Modifies the dictionary values directly
    public static void addCamSpaceOffset(Dictionary<int, PoseData> dict, Vector3 offset) {
        List<int> keys = new List<int>(dict.Keys);
        foreach (int key in keys) {
            PoseData data = dict[key];
            
            data.pos += offset;
            dict[key] = data;
        }
    }
      
        //Performs a lowpass check on the position and rotation of each marker in newDict, comparing them to those in oldDict
        //If a marker moved less than posThreshold, its old position is copied to newDict
        //If a marker rotated less than rotThreshold degrees, its old rotation is copied to newDict
    public static void posRotLowpass(Dictionary<int, PoseData> oldDict, Dictionary<int, PoseData> newDict, float posThreshold, float rotThreshold) {
        posThreshold *= posThreshold; //We'll compare square distances so adjust the threshold accordingly

        //Check all keys and copy over previous pose values if our new changes were not significant enough
        //We are using the newDict here since that already made sure that new markers exist, and old ones were removed.
        //Updating the old dict would mean adding missing ones, and then looping over its keys to find and remove markers that disappeared.
        List<int> keys = new List<int>(newDict.Keys);
        foreach (int key in keys) {
            if (!oldDict.ContainsKey(key)) continue;
            
            PoseData oldPose = oldDict[key];
            PoseData newPose = newDict[key];

            float posDiff = (newPose.pos - oldPose.pos).sqrMagnitude;
            float rotDiff = Quaternion.Angle(newPose.rot, oldPose.rot);

            //If our changes didn't go over the low pass, copy over our previous values
            if (posDiff < posThreshold) {
                newPose.pos = oldPose.pos;
            }

            if (rotDiff < rotThreshold) {
                newPose.rot = oldPose.rot;
            }

            newDict[key] = newPose;
        }
    }
}
