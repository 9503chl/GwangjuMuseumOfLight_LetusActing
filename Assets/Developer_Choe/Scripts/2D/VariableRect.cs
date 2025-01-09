using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VariableRect : MonoBehaviour
{
    public CanvasScaler _CanvasScaler;

    private RectTransform rect;

    public bool FixMultiple = false;

    [SerializeField] private Vector2 resolution;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (FixMultiple)
        {
            float fixedAspectRatio = resolution.x/ resolution.y;

            //현재 해상도의 비율
            float currentAspectRatio = (float)Screen.width / (float)Screen.height;

            //현재 해상도 가로 비율이 더 길 경우
            if (currentAspectRatio > fixedAspectRatio) _CanvasScaler.matchWidthOrHeight = 1;
            //현재 해상도의 세로 비율이 더 길 경우
            else if (currentAspectRatio < fixedAspectRatio) _CanvasScaler.matchWidthOrHeight = 0;
        }
        rect.sizeDelta = new Vector2(Screen.width, Screen.height);
    }
}
