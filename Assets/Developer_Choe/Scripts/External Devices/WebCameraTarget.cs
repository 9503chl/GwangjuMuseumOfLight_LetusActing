using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class WebCameraTarget : MonoBehaviour
{
    public RawImage _RawImage;

    public int CameraIndex;

    private void Awake()
    {
        _RawImage = GetComponent<RawImage>();
    }
    private void Start()
    {
        _RawImage = WebCamara.WebCamInitialize(_RawImage, CameraIndex);
    }
}
