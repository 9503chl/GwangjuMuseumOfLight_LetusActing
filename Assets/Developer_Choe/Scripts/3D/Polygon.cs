using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Mesh))]

[RequireComponent(typeof(SortingGroup))]
public class Polygon : MonoBehaviour
{
    public Vector3[] Edges;

    [Header("트라이앵글 그리기라고 생각하자. 3의 배수 필요.")]
    public int[] Orders;

    [SerializeField] private Material _Material;

    private MeshFilter _MeshFilter;
    private MeshRenderer _MeshRenderer;
    private Mesh _Mesh;

    public SortingGroup sortingGroup;

    private void Awake()
    {
        _MeshFilter = GetComponent<MeshFilter>();
        _MeshRenderer = GetComponent<MeshRenderer>();
        _Mesh = new Mesh();
        sortingGroup = GetComponent<SortingGroup>();

        _MeshRenderer.material = _Material;

        CreatePolygon();
    }
    public void CreatePolygon()
    {
        _MeshFilter.mesh = MeshComponent.CreatePoylgon(_Mesh, Edges, Orders);
    }
}
