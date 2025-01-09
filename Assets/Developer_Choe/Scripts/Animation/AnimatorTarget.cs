using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorTarget : MonoBehaviour
{
    private Animator _Animator;

    public List<string> ValueNames_Int = new List<string>();

    public List<string> ValueNames_Float = new List<string>();

    public List<string> ValueNames_Bool = new List<string>();

    public List<string> AnimationStates = new List<string>();

    public List<string> Triggers = new List<string>();

    private void Awake()
    {
        _Animator = GetComponent<Animator>();
    }

    public void AnimationInvoke(string value)
    {
        if (_Animator != null)
        {
            if (AnimationStates.Contains(value))
            {
                Debug.Log(string.Format("{0} : Animtator Play - {1}", name, value));
                _Animator.Play(value);
            }
            if (Triggers.Contains(value))
            {
                Debug.Log(string.Format("{0} : Animtator Trigger - {1}", name, value));
                _Animator.SetTrigger(value);
            }
        }
    }
    public void AnimationInvoke(string valueName , object value)
    {
        if (_Animator != null)
        {
            if(ValueNames_Int.Contains(valueName))
            {
                Debug.Log(string.Format("{0} : Animtator SetInteger - {1} : {2}", name, valueName ,  value));
                _Animator.SetInteger(valueName , (int)value);
            }
            if (ValueNames_Float.Contains(valueName))
            {
                Debug.Log(string.Format("{0} : Animtator SetFloat - {1} : {2}", name, valueName, value));
                _Animator.SetFloat(valueName, (float)value);
            }
            if (ValueNames_Bool.Contains(valueName))
            {
                Debug.Log(string.Format("{0} : Animtator SetBool - {1} : {2}", name, valueName, value));
                _Animator.SetBool(valueName, (bool)value);
            }
        }
    }

    /// <summary>
    /// 이 친구를 쓸 때, 프레임 한 번 기다리고 사용해야 한다.
    /// </summary>
    /// <returns>애니메이션 실제 타임</returns>
    public float GetAnimationTime()
    {
        return _Animator.GetCurrentAnimatorClipInfo(0).Length;
    }
}
