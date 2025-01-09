using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class TouchableObject : MonoBehaviour
{
    public GameObject SourceObject;
    public GameObject TargetObject;
    public float ActiveTime = 10f;

    private Coroutine untouchRoutine;

    private void Start()
    {
        Untouch();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Untouch();
    }

    public void Touch()
    {
        if (untouchRoutine == null)
        {
            if (SourceObject != null)
            {
                SourceObject.SetActive(false);
            }
            if (TargetObject != null)
            {
                TargetObject.SetActive(true);
            }
            untouchRoutine = StartCoroutine(DelayedUntouch());
        }
    }

    private IEnumerator DelayedUntouch()
    {
        yield return new WaitForSeconds(ActiveTime);
        Untouch();
    }

    public void Untouch()
    {
        if (TargetObject != null)
        {
            TargetObject.SetActive(false);
        }
        if (SourceObject != null)
        {
            SourceObject.SetActive(true);
        }
        untouchRoutine = null;
    }
}
