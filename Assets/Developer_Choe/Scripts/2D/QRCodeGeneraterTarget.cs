using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QRCodeGeneraterTarget : MonoBehaviour
{
    public string URL;

    private RawImage _RawImage;

    private void Awake()
    {
        _RawImage = GetComponent<RawImage>();
    }

    private void Start()
    {
        _RawImage.texture = QrCodeUtility.GenerateQR(URL);
    }
}
