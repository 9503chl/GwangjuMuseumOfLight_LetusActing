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
    private SkinnedMeshRenderer _Item;
    private SkinnedMeshRenderer _Face;

    private Transform firstTransform;


    private void Awake()
    {
        Saver = GetComponentInChildren<BodyDataSaver>(true);

        Loaders = GetComponentsInChildren<BodyDataLoader>(true);

        _Animators = GetComponentsInChildren<Animator>(true).ToList();
        _Animators.RemoveAt(0);

        firstTransform = transform.GetChild(0);

        SkinnedMeshRenderer[] skinnedMeshRenderers = firstTransform.GetComponentsInChildren<SkinnedMeshRenderer>();

        if(skinnedMeshRenderers.Length == 3)
        {
            _Cloth = skinnedMeshRenderers[0];
            _Item = skinnedMeshRenderers[1];
            _Face = skinnedMeshRenderers[2];
        }
    }

    public void AnimationInvoke()
    {
        for(int i = 0; i< _Animators.Count; i++)
        {
            _Animators[i].gameObject.SetActive(false);
        }
        _Animators[WebServerData.captureIndex].gameObject.SetActive(true);
    }

    public void TextureInitialize()
    {
        if (WebServerData.materialTexture1 != null)
        {
            Material material = new Material(_Cloth.material);
            material.SetTexture("_MainTex", WebServerData.materialTexture1);
            _Cloth.material = material;
        }
        if (WebServerData.materialTexture2 != null)
        {
            Material[] materials = _Item.materials; 

            Material material = new Material(materials[1]);
            material.SetTexture("_MainTex", WebServerData.materialTexture2);
            _Item.material = material;
        }

        if (WebServerData.facialExpression1 != null)
        {
            Material material = new Material(_Face.material);
            material.SetTexture("_MainTex", WebServerData.facialExpression1);
            _Face.material = material;
        }
    }
}
