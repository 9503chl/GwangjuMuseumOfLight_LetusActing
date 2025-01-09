using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ProgressSliderImage : MonoBehaviour
{
    private Slider slider;

    private float fillValue;

    public float FillValue
    {
        get { return fillValue; }
        set { FillAmountChange(value); }
    }


    private void Awake()
    {
        slider = GetComponentInChildren<Slider>(true);
        
        foreach(Image image in GetComponentsInChildren<Image>())
        {
            image.type = Image.Type.Sliced;
        }
    }
    private void FillAmountChange(float value)
    {
        if(slider == null)
        {
            return;
        }
        slider.value = value;
    }
}
