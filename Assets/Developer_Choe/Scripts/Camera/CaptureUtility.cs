using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CaptureUtility
{
    private static Camera TargetCamera;
    private static byte[] bytes;
    /// <summary>
    /// ȭ�� ĸ��
    /// </summary>
    /// <param name="texture2D">Ÿ�� �ؽ���</param>
    /// <param name="width">�ڸ����� �ϴ� ũ�� width</param>
    /// <param name="height">�ڸ����� �ϴ� ũ�� height</param>
    /// <param name="x">�ڸ����� �ϴ� ��ġ x</param>
    /// <param name="y">�ڸ����� �ϴ� ��ġ y</param>
    public static void CaptureScreen(Texture2D texture2D, int width, int height, int x, int y)
    {
        TargetCamera = Camera.main;

        RenderTexture renderTexture = new RenderTexture(TargetCamera.pixelWidth, TargetCamera.pixelHeight, 24);
        RenderTexture targetTexture = TargetCamera.targetTexture;
        RenderTexture activeTexture = RenderTexture.active;
        TargetCamera.targetTexture = renderTexture;
        TargetCamera.Render();
        RenderTexture.active = renderTexture;
        
        texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture2D.ReadPixels(new Rect((1080 - width) / 2, (1920 - height) / 2, width, height), 0, 0);
        texture2D.Apply();
        TargetCamera.targetTexture = targetTexture;
        RenderTexture.active = activeTexture;

        bytes = texture2D.EncodeToJPG();

        File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath + "/Test.jpg"), bytes);
    }
}
