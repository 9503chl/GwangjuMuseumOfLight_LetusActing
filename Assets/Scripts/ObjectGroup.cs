using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectGroup : MonoBehaviour
{
    [NonSerialized]
    public BodyDataSaver Saver;

    [NonSerialized]
    public BodyDataLoader[] Loaders;

    [NonSerialized]
    public List<Animator> _Animators;

    private SkinnedMeshRenderer _Cloth;
    private SkinnedMeshRenderer _Face;
    private SkinnedMeshRenderer _Item;

    private Transform firstTransform;

    private Material cloned_Cloth;
    private Material cloned_Face;
    private Material cloned_Item;

    private List<SkinnedMeshRenderer> allSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();

    private void Awake()
    {
        Saver = GetComponentInChildren<BodyDataSaver>(true);

        Loaders = GetComponentsInChildren<BodyDataLoader>(true);

        _Animators = GetComponentsInChildren<Animator>(true).ToList();
        _Animators.RemoveAt(0);

        firstTransform = transform.GetChild(0);

        SkinnedMeshRenderer[] skinnedMeshRenderers = firstTransform.GetComponentsInChildren<SkinnedMeshRenderer>();

        allSkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>().ToList();

        if (skinnedMeshRenderers.Length == 3)
        {
            _Cloth = skinnedMeshRenderers[0];
            cloned_Cloth = new Material(_Cloth.material);

            _Face = skinnedMeshRenderers[1];
            cloned_Face = new Material(_Face.materials[1]);

            _Item = skinnedMeshRenderers[2];
            cloned_Item = new Material(_Item.material);
        }
    }

    public void AnimationInvoke()
    {
        for(int i = 0; i< _Animators.Count; i++)
        {
            _Animators[i].gameObject.SetActive(false);
        }
        _Animators[WebServerUtility.captureIndex].gameObject.SetActive(true);
    }

    public void TextureInitialize()
    {
        for (int i = 0; i < allSkinnedMeshRenderers.Count; i++)
        {
            if (WebServerUtility.E3Data.materialTexture1 != null && i % 3 == 0)
            {
                cloned_Cloth.SetTexture("_BaseMap", WebServerUtility.E3Data.materialTexture1);
                allSkinnedMeshRenderers[i].material = cloned_Cloth;
            }
            if (WebServerUtility.E3Data.facialExpression1 != null && i % 3 == 1)
            {
                Material[] materials = allSkinnedMeshRenderers[i].materials;
                cloned_Face.SetTexture("_BaseMap", WebServerUtility.E3Data.facialExpression1);
                materials[1] = cloned_Face;

                allSkinnedMeshRenderers[i].materials = materials;
            }

            if (WebServerUtility.E3Data.materialTexture2 != null && i % 3 == 2)
            {
                cloned_Item.SetTexture("_BaseMap", WebServerUtility.E3Data.materialTexture2);
                allSkinnedMeshRenderers[i].material = cloned_Item;
            }
        }
    }

    public void Clear()
    {
        Saver.ClearData();
    }
}
