using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraProvider : MonoBehaviour {
    public abstract void init(out int _width, out int _height, out float[] _camParams);
    public abstract Color32[] getImage();
}
