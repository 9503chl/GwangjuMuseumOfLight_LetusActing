using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Runtime.CompilerServices;

public class FadeManager : PivotalManager
{
    [SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private float speed;

    private Material materialInstance;

    public List<GameObject> TargetOffs;

    private bool isFadeInOut;
    
    private Coroutine FadeInOutCor;

    public override void OnAwake()
    {
        materialInstance = Instantiate(meshRenderer.material) as Material;

        meshRenderer.material = materialInstance;

        base.OnAwake();
    }

    public bool FadeInOut()
    {
        isFadeInOut = false;
        if(FadeInOutCor == null)
        {
            isFadeInOut = true;
            FadeInOutCor = StartCoroutine(IFadeInOut());
        }
        return isFadeInOut;
    }
    IEnumerator IFadeInOut()
    { 
        for (int i = 0; i < TargetOffs.Count; i++)
        {
            TargetOffs[i].SetActive(false);
        }

        materialInstance.DOColor(Color.black, speed);

        yield return new WaitForSeconds(speed);

        materialInstance.DOFade(0, speed);

        yield return new WaitForSeconds(speed);

        for (int i = 0; i < TargetOffs.Count; i++)
        {
            TargetOffs[i].SetActive(true);
        }
        isFadeInOut = false;

        FadeInOutCor = null;
    }

    public override void Clear(bool @true)
    {
        if (!@true)
        {
            Destroy(materialInstance);
            materialInstance = null;

            for (int i = 0; i < TargetOffs.Count; i++)
            {
                TargetOffs[i].SetActive(true);
            }
        }

        base.Clear(@true);
    }
}
