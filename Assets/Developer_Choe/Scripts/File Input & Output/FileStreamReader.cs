using System.IO;
using UnityEngine;

public class FileStreamReader
{
    public static StreamReader _StreamReader;

    public static StreamReader ReadTextFile(string path, string extension)
    {
        _StreamReader = new StreamReader(string.Format("{0}.{1}", path, extension));
        return _StreamReader;
    }
    //»ç¿ë¹ý
    //    if (reader != null)
    //            {
    //                string text = reader.ReadToEnd();
    //    string[] texts = text.Split('\n');

    //                for (int j = 0; j<texts.Length; j++)
    //                {
    //                    string[] tempText = new string[2];
    //                    if (texts[j].Contains(TitleText))
    //                    {
    //                        tempText = texts[j].Split(TitleText);
    //    videoInfo.TitleText = tempText[1];
    //                    }
    //                    else if (texts[j].Contains(TitleText_en))
    //{
    //    tempText = texts[j].Split(TitleText_en);
    //    videoInfo.TitleText_EN = tempText[1];
    //}
    //else if (texts[j].Contains(SubText))
    //{
    //    tempText = texts[j].Split(SubText);
    //    videoInfo.SubText = tempText[1];
    //}
    //else if (texts[j].Contains(Subext_en))
    //{
    //    tempText = texts[j].Split(Subext_en);
    //    videoInfo.SubText_EN = tempText[1];
    //}
    //else if (texts[j].Contains(VideoLength))
    //{
    //    tempText = texts[j].Split(VideoLength);
    //    videoInfo.VideoLength = int.Parse(tempText[1]);
    //}
    //                }
    //                reader.Close();
    //            }
}
