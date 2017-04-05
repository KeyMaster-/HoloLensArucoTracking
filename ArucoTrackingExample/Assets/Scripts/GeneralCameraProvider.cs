using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralCameraProvider : CameraProvider {
    public int webcamDeviceNumber;
    public int webcamDesiredWidth;
    public int webcamDesiredHeight;
    public int webcamDesiredFPS;

    public bool useTestImg;
    public Texture2D testImg;

    public float focalX;
    public float focalY;
    public float centerX;
    public float centerY;

    public float[] distortion;

    private WebCamTexture webcamTexture;
    private Color32[] imgData;

    override public void init(out int _width, out int _height, out float[] _cam_params) {
            //Default values to signify failed init if that should happen
        _width = 0;
        _height = 0;
        if (useTestImg) {
            _width = testImg.width;
            _height = testImg.height;
            imgData = testImg.GetPixels32();
        }
        else {
            WebCamDevice[] devices = WebCamTexture.devices;
            if(devices.Length > 0) {
                webcamTexture = new WebCamTexture(devices[webcamDeviceNumber].name, webcamDesiredWidth, webcamDesiredHeight, webcamDesiredFPS);
                    //We have to play the webcam to get actual width/height information from it
                webcamTexture.Play();
                _width = webcamTexture.width;
                _height = webcamTexture.height;

                imgData = new Color32[_width * _height];
            }
            else {
                Debug.Log("No webcam found!");
            }
        }

        _cam_params = initCameraParams();
    }

    float[] initCameraParams() {
        float[]cameraParams = new float[4 + 5];

        cameraParams[0] = focalX;
        cameraParams[1] = focalY;
        cameraParams[2] = centerX;
        cameraParams[3] = centerY;

        if(distortion.Length != 5) {
            Debug.LogAssertion("Camera parameters expect 5 distorion values. Will continue with all set to 0 for now.");
        }
        else {
            for (int i = 0; i < 5; i++) {
                cameraParams[4 + i] = distortion[i];
            }
        }
        
        return cameraParams;
    }

    override public Color32[] getImage() {
        if(!useTestImg) {
            webcamTexture.GetPixels32(imgData);
        }
        return imgData;
    }

    public Texture getTexture() {
        if(useTestImg) {
            return testImg;
        }
        else {
            return webcamTexture;
        }
    }
}
