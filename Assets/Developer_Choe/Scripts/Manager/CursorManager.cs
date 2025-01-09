using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : PivotalManager
{
    public static void CursurChange(Texture2D texture2D)
    {
        Cursor.SetCursor(texture2D, Vector2.zero, CursorMode.Auto);
    }
}
