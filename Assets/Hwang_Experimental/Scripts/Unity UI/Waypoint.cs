using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Waypoint", menuName = "Preset/Waypoint", order = 1002)]
public class Waypoint : ScriptableObject
{
#if UNITY_2022_2_OR_NEWER
    public LayerMask ExcludeLayers;
#endif
    public Space CoordinateSpace;
    public Vector3[] Positions;
    public Vector3 InitialAngles;

    private void Awake()
    {
        if (Positions == null)
        {
            Positions = new Vector3[0];
        }
    }
}
