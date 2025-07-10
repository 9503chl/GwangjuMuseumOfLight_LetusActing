using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

public class ViewMenuItem : EndNameEditAction
{
    private static GameObject EnsureCanvasGameObject()
    {
        GameObject activeGameObject = Selection.activeGameObject;
        Canvas canvas = (activeGameObject != null) ? activeGameObject.GetComponentInParent<Canvas>() : null;
        if (canvas != null && canvas.gameObject.activeInHierarchy)
        {
            if (!EditorUtility.IsPersistent(canvas) && (canvas.hideFlags & HideFlags.HideInHierarchy) == 0)
            {
                return canvas.gameObject;
            }
        }
        GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.layer = LayerMask.NameToLayer("UI");
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        return go;
    }

    [MenuItem("GameObject/UI/View", false, 9001)]
    private static void CreateViewWithGameObject(MenuCommand menuCommand)
    {
        GameObject parent = menuCommand.context as GameObject;
        if (parent == null || parent.GetComponentsInParent<Canvas>(true).Length == 0)
        {
            parent = EnsureCanvasGameObject();
        }
        GameObject go = new GameObject(GameObjectUtility.GetUniqueNameForSibling(parent.transform, "View"), typeof(RectTransform), typeof(CanvasGroup), typeof(View));
        go.layer = parent.layer;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeGameObject = go;
    }

    private static string RemoveInvalidFileNameChars(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalidChars.Length; i++)
            {
                fileName = fileName.Replace(invalidChars[i], '_');
            }
        }
        return fileName;
    }

    public static void CreateCustomViewScript(string fileName)
    {
        string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (string.IsNullOrEmpty(assetPath))
        {
            assetPath = "Assets/Scripts";
        }
        if (!AssetDatabase.IsValidFolder(assetPath))
        {
            AssetDatabase.CreateFolder("Assets", "Scripts");
        }
        SessionState.SetInt("ActiveInstanceID", Selection.activeInstanceID);
        EndNameEditAction action = CreateInstance<ViewMenuItem>();
        string pathName = string.Format("{0}/{1}.cs", assetPath, RemoveInvalidFileNameChars(fileName));
        Texture2D icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, pathName, icon, null);
    }

    private static MonoScript GetMonoScript(string typeName)
    {
        string[] guids = AssetDatabase.FindAssets(string.Format("t:MonoScript"));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            if (script != null && script.GetClass() != null && string.Compare(script.GetClass().Name, typeName) == 0)
            {
                return script;
            }
        }
        return null;
    }

    private static string[] GetMonoScriptTypeNames()
    {
        List<string> result = new List<string>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:MonoScript"));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            if (script != null && script.GetClass() != null)
            {
                result.Add(script.GetClass().Name);
            }
        }
        EditorUtility.UnloadUnusedAssetsImmediate();
        return result.ToArray();
    }

    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        string fileExt = Path.GetExtension(pathName);
        if (string.Compare(fileExt, ".cs", true) == 0)
        {
            string typeName = Path.GetFileNameWithoutExtension(pathName);
            List<string> typeNames = new List<string>(GetMonoScriptTypeNames());
            int number = 0;
            typeName = typeName.Replace(" ", "").Replace(".", "_").Replace("(", "_").Replace(")", "");
            while (typeNames.Contains(typeName))
            {
                typeName = string.Format("{0}{1}", typeName, ++number);
            }
            pathName = string.Format("{0}/{1}{2}", Path.GetDirectoryName(pathName), typeName, Path.GetExtension(pathName));
            MonoScript script = GetMonoScript("CustomView");
            if (script != null)
            {
                string text = script.text.Replace("CustomView", typeName);
                File.WriteAllText(pathName, text, Encoding.UTF8);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using System.Collections;");
                sb.AppendLine("using System.Collections.Generic;");
                sb.AppendLine("using UnityEngine;");
                sb.AppendLine("using UnityEngine.UI;");
                sb.AppendLine();
                sb.AppendLine(string.Format("public class {0} : View", typeName));
                sb.AppendLine("{");
                sb.AppendLine("    private void Awake()");
                sb.AppendLine("    {");
                sb.AppendLine("        OnBeforeShow += View_BeforeShow;");
                sb.AppendLine("        OnAfterShow += View_AfterShow;");
                sb.AppendLine("        OnBeforeHide += View_BeforeHide;");
                sb.AppendLine("        OnAfterHide += View_AfterHide;");
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    private void View_BeforeShow()");
                sb.AppendLine("    {");
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    private void View_AfterShow()");
                sb.AppendLine("    {");
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    private void View_BeforeHide()");
                sb.AppendLine("    {");
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    private void View_AfterHide()");
                sb.AppendLine("    {");
                sb.AppendLine("    }");
                sb.AppendLine("}");
                File.WriteAllText(pathName, sb.ToString(), Encoding.UTF8);
            }
            SessionState.SetString("AssetPathName", pathName);
            AssetDatabase.ImportAsset(pathName);
        }
    }

    [InitializeOnLoadMethod]
    private static void OnInitialized()
    {
        AssemblyReloadEvents.afterAssemblyReload += OnAssemblyReloaded;
    }

    private static void OnAssemblyReloaded()
    {
        AssemblyReloadEvents.afterAssemblyReload -= OnAssemblyReloaded;
        Selection.activeInstanceID = SessionState.GetInt("ActiveInstanceID", 0);
        GameObject activeGameObject = Selection.activeGameObject;
        string assetPathName = SessionState.GetString("AssetPathName", null);
        SessionState.EraseInt("ActiveInstanceID");
        SessionState.EraseString("AssetPathName");
        if (activeGameObject != null && !string.IsNullOrEmpty(assetPathName))
        {
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPathName);
            if (script != null)
            {
                ProjectWindowUtil.ShowCreatedAsset(script);
                //AssetDatabase.OpenAsset(script);
                View view = activeGameObject.GetComponent<View>();
                Type type = script.GetClass();
                if (view != null && type != null)
                {
                    DestroyImmediate(view);
                    activeGameObject.AddComponent(type);
                    activeGameObject.name = ObjectNames.NicifyVariableName(type.Name);
                }
                Selection.activeGameObject = activeGameObject;
            }
        }
    }
}
