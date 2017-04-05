using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageDisplay : MonoBehaviour {
    public MeshRenderer displayMesh;
    public GeneralCameraProvider cameraProvider;

	// Use this for initialization
	void Start () {
            //Expects that something called runner.init() during Awake, so that the provider has been initialised by now
        displayMesh.material.mainTexture = cameraProvider.getTexture();
	}
}
