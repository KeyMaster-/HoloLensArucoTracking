using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PoseRunningAverage {
    Dictionary<int, PoseData>[] previousStates;
    int stateMemoryLength;
    int nextStateIdx = 0;
    
    public PoseRunningAverage(int _stateMemoryLength) {
        stateMemoryLength = _stateMemoryLength;
        previousStates = new Dictionary<int, PoseData>[stateMemoryLength];
        for(int i=0; i<stateMemoryLength; i++) {
            previousStates[i] = new Dictionary<int, PoseData>();
        }
    }

        //Updates newDict to the running-average values for each of the markers poses, taking this new data into account
    public void averageNewState(Dictionary<int, PoseData> newDict) {
        List<int> newDictKeys = new List<int>(newDict.Keys);
        previousStates[nextStateIdx].Clear();

        Vector3 totalPos = new Vector3();

        int statesSeen;
        foreach(int key in newDictKeys) {
            PoseData newPose = newDict[key];
            previousStates[nextStateIdx][key] = newPose; //PoseData is a struct, so it will be copied to our previousStates dict. This allows us to modify newDict later

            int i = nextStateIdx;
            statesSeen = 0;
            totalPos.Set(0, 0, 0);

            do {
                if (!previousStates[i].ContainsKey(key)) break; //Only iterate while the dictionaries still contain this marker
                statesSeen++;
                PoseData previousPose = previousStates[i][key];
                totalPos += previousPose.pos;
                
                i = positiveMod(i - 1, stateMemoryLength);
            } while (i != nextStateIdx);

            totalPos /= statesSeen;
            newPose.pos = totalPos;
            
                //Averaging multiple quaternions is not easy or well defined, and the simple "sum and average" method fails on larger steps between quaternions
                //Instead, we just average between the previous and new frame with slerp to get at least some nice smooth rotation
            int previousDictIdx = positiveMod(nextStateIdx - 1, stateMemoryLength);
            if(previousStates[previousDictIdx].ContainsKey(key)) {
                newPose.rot = Quaternion.Slerp(newPose.rot, previousStates[previousDictIdx][key].rot, 0.5f);
            }

            newDict[key] = newPose;
        }

        nextStateIdx = (nextStateIdx + 1) % stateMemoryLength;
    }

        //Computes x mod m, guaranteeing that the result is positive (i.e. -1 % 5 would be -1, but positiveMod(-1, 5) is 4
        //Taken from this SO answer: http://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
    int positiveMod(int x, int m) {
        return (x % m + m) % m;
    }
}
