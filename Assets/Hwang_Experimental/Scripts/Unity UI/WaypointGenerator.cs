using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WaypointGenerator : MonoBehaviour
{
    [SerializeField]
    private Waypoint waypoint = null;

#if UNITY_2022_2_OR_NEWER
    [SerializeField]
    private LayerMask excludeLayers;
#endif

    [SerializeField]
    private Space coordinateSpace;

    public bool HasWaypoint
    {
        get { return waypoint != null; }
    }

#if UNITY_EDITOR
    private void Reset()
    {
        if (!Application.isPlaying)
        {
            tag = "EditorOnly";
#if UNITY_2022_2_OR_NEWER
            excludeLayers = 0;
#endif
            RemoveAllPoints();
            AddPoint();
            AddPoint();
        }
    }

    public GameObject[] GetAllPoints()
    {
        List<GameObject> result = new List<GameObject>();
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i].transform != transform && IsPoint(meshRenderers[i].gameObject))
            {
                result.Add(meshRenderers[i].gameObject);
            }
        }
        return result.ToArray();
    }

    private bool IsPoint(GameObject sphere)
    {
        MeshFilter meshFilter = sphere.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null && string.Compare(meshFilter.sharedMesh.name, "Sphere") == 0)
        {
            return sphere.GetComponent<SphereCollider>() != null && sphere.GetComponent<Rigidbody>() != null;
        }
        return false;
    }

    public void AddPoint()
    {
        GameObject[] spheres = GetAllPoints();
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Point";
        sphere.transform.parent = transform;
        sphere.transform.SetAsLastSibling();
#if UNITY_2022_2_OR_NEWER
        SphereCollider collider = sphere.GetComponent<SphereCollider>();
        collider.excludeLayers = excludeLayers;
#endif
        Rigidbody rigidBody = sphere.AddComponent<Rigidbody>();
        rigidBody.mass = 0.01f;
        rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
#if UNITY_2022_2_OR_NEWER
        rigidBody.excludeLayers = excludeLayers;
#else
        sphere.layer = LayerMask.NameToLayer("Ignore Raycast");
#endif
        if (spheres.Length > 0)
        {
            if (coordinateSpace == Space.World)
            {
                sphere.transform.position = spheres[spheres.Length - 1].transform.position;
            }
            else
            {
                sphere.transform.localPosition = spheres[spheres.Length - 1].transform.localPosition;
            }
        }
    }

    public void RemoveLastPoint()
    {
        GameObject[] spheres = GetAllPoints();
        if (spheres.Length > 0)
        {
            DestroyImmediate(spheres[spheres.Length - 1].gameObject);
        }
    }

    public void RemoveAllPoints()
    {
        GameObject[] spheres = GetAllPoints();
        foreach (GameObject sphere in spheres)
        {
            DestroyImmediate(sphere.gameObject);
        }
    }

    private Vector3 DeltaAngles(Vector3 current, Vector3 target)
    {
        return new Vector3(Mathf.DeltaAngle(current.x, target.x), Mathf.DeltaAngle(current.y, target.y), Mathf.DeltaAngle(current.z, target.z));
    }

    public void SaveWaypoint()
    {
        GameObject[] spheres = GetAllPoints();
        if (spheres.Length > 1)
        {
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < spheres.Length; i++)
            {
                if (coordinateSpace == Space.World)
                {
                    points.Add(spheres[i].transform.position);
                }
                else
                {
                    points.Add(spheres[i].transform.localPosition);
                }
            }
            Vector3 initialAngles;
            if (coordinateSpace == Space.World)
            {
                initialAngles = DeltaAngles(Vector3.zero, spheres[0].transform.eulerAngles);
            }
            else
            {
                initialAngles = DeltaAngles(Vector3.zero, spheres[0].transform.localEulerAngles);
            }
            if (waypoint != null)
            {
#if UNITY_2022_2_OR_NEWER
                waypoint.ExcludeLayers = excludeLayers;
#endif
                waypoint.CoordinateSpace = coordinateSpace;
                waypoint.Positions = points.ToArray();
                waypoint.InitialAngles = initialAngles;
                EditorUtility.SetDirty(waypoint);
#if UNITY_2020_3_OR_NEWER
                AssetDatabase.SaveAssetIfDirty(waypoint);
#else
                AssetDatabase.SaveAssets();
#endif
            }
            else
            {
                waypoint = ScriptableObject.CreateInstance<Waypoint>();
#if UNITY_2022_2_OR_NEWER
                waypoint.ExcludeLayers = excludeLayers;
#endif
                waypoint.CoordinateSpace = coordinateSpace;
                waypoint.Positions = points.ToArray();
                waypoint.InitialAngles = initialAngles;
                if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets", "Settings");
                }
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("Assets/Settings/{0}.asset", name));
                AssetDatabase.CreateAsset(waypoint, assetPath);
                ProjectWindowUtil.ShowCreatedAsset(waypoint);
            }
            Debug.Log(string.Format("Waypoint has been saved : {0}", Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(waypoint))));
        }
        else
        {
            Debug.LogError("At least two points are required to save");
        }
    }

    public void RestoreWaypoint()
    {
        if (waypoint == null)
        {
            waypoint = AssetDatabase.LoadAssetAtPath<Waypoint>(string.Format("Assets/Settings/{0}.asset", name));
        }
        if (waypoint != null)
        {
#if UNITY_2022_2_OR_NEWER
            excludeLayers = waypoint.ExcludeLayers;
#endif
            coordinateSpace = waypoint.CoordinateSpace;
            GameObject[] spheres = GetAllPoints();
            int addCount = waypoint.Positions.Length - spheres.Length;
            if (addCount > 0)
            {
                for (int i = 0; i < addCount;i++)
                {
                    AddPoint();
                }
            }
            else if (addCount < 0)
            {
                for (int i = 0; i < Mathf.Abs(addCount); i++)
                {
                    RemoveLastPoint();
                }
            }
            spheres = GetAllPoints();
            if (spheres.Length > 0)
            {
                if (coordinateSpace == Space.World)
                {
                    spheres[0].transform.eulerAngles = waypoint.InitialAngles;
                }
                else
                {
                    spheres[0].transform.localEulerAngles = waypoint.InitialAngles;
                }
            }
            for (int i = 0; i < waypoint.Positions.Length; i++)
            {
                if (coordinateSpace == Space.World)
                {
                    spheres[i].transform.position = waypoint.Positions[i];
                }
                else
                {
                    spheres[i].transform.localPosition = waypoint.Positions[i];
                }
            }
            Debug.Log(string.Format("Waypoint has been restored : {0}", Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(waypoint))));
        }
        else
        {
            Debug.LogError("No waypoint assigned to restore");
        }
    }
#endif
        }
