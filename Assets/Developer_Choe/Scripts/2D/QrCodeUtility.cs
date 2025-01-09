using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing.QrCode;
using ZXing;

public class QrCodeUtility
{
    public static Texture2D LoadQR(string url)
    {
        return GenerateQR(url);
    }

    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        //인코딩 작업    //QRcode Make..
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
        //QRcode는 적외선 센서를 통하여 인식을 하게 되는데, 인식을 하게 되면 해당 QRcode에 저장된 텍스트를 실행 시켜 그 내용을 확인 하는 것이다. 이에 따라 QR 코드를 만들 때에는 QRcode 안에 저장할 텍스트와 함께 생성시킨다.
    }

    public static Texture2D GenerateQR(string text)
    {
        //인코딩 작업을 위한 Encode 함수 호출
        var encoded = new Texture2D(256, 256);
        var color32 = Encode(text, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();

        //인코드를 완료후 PNG 파일로 만들기 위한 File 시스템.
        //byte[] bytes = encoded.EncodeToPNG();
        //File.WriteAllBytes(Application.persistentDataPath + text + ".png", bytes);
        //Application.persistentDataPath 에 파일을 저장하였으니 C:\Users\유저네임\AppData\LocalLow\의 유니티 컴퍼니 회사 이름에 저장 될 것이다.
        return encoded;
    }
}
