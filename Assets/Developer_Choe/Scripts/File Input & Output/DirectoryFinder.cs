using System.IO;
using UnityEngine;

public class DirectoryFinder
{
    public static DirectoryInfo _DirectoryInfo;


    public static DirectoryInfo GetDirectory(string path)
    {
        _DirectoryInfo = new DirectoryInfo(path);

        return _DirectoryInfo;
    }

    public static FileInfo[] GetFileInfo(string path)
    {
        FileInfo[] fileInfos = null;
        if(_DirectoryInfo != null)
        {
            fileInfos = _DirectoryInfo.GetFiles();
        }
        return fileInfos;
    }

    //»ç¿ë¹ý
    //    foreach (DirectoryInfo fi in di.GetDirectories())
    //        {
    //            byte[] tempBytes = null;
    //    string baseURL = string.Format("{0}/{1}/", Application.streamingAssetsPath, fi.Name);
    //    string texturePath = string.Format("{0}jpg.jpg", baseURL);
    //            if (File.Exists(string.Format("{0}jpg.jpg", baseURL)))
    //            {
    //                tempBytes = File.ReadAllBytes(texturePath);
    //                Texture2D texture = new Texture2D(0, 0);
    //    texture.LoadImage(tempBytes);
    //                IntroImages[imgIndex].texture = texture;
    //            }
    //texturePath = string.Format("{0}png.png", baseURL);
    //if (File.Exists(texturePath))
    //{
    //    tempBytes = File.ReadAllBytes(texturePath);
    //    Texture2D texture = new Texture2D(0, 0);
    //    texture.LoadImage(tempBytes);
    //    IntroImages[imgIndex].texture = texture;
    //}

    //if (PanelSettings.IsReverse)
    //{
    //    imgIndex--;
    //}
    //else
    //{
    //    imgIndex++;
    //}
}
