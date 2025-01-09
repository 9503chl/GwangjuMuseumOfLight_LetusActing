using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class EnhanceScrollViewExtension : MonoBehaviour
{
    public AnimationCurve scaleCurve;
    public AnimationCurve positionCurve;
    public AnimationCurve depthCurve;

    public int startIndex = -1;

    public float cellWidth = 100f;

    public float cellPositionY = 0f;

    public float tweenDuration = 0.2f;

    public float dragFactor = 0.001f;

    public UnityEvent onItemSelected = new UnityEvent();

    [NonSerialized]
    private EnhanceScrollItemExtension[] scrollItems;

    [NonSerialized]
    private int centeredIndex = -1;

    [NonSerialized]
    private float totalWidth = 500f;

    [NonSerialized]
    private float currentDuration = 0f;

    [NonSerialized]
    private bool enableLerpTween = true;

    [NonSerialized]
    private EnhanceScrollItemExtension previousItem;

    [NonSerialized]
    private EnhanceScrollItemExtension selectedItem;
    public EnhanceScrollItemExtension SelectedItem
    {
        get { return selectedItem; }
    }

    public int SelectedIndex
    {
        get
        {
            if (selectedItem != null)
            {
                return selectedItem.OffsetIndex;
            }
            return -1;
        }
    }

    [NonSerialized]
    private bool canChangeItem = true;

    [NonSerialized]
    private float moveFactor = 0.2f;

    [NonSerialized]
    private float originHorizontalValue = 0.1f;

    [NonSerialized]
    private float currentHorizontalValue = 0.5f;

    [NonSerialized]
    private List<EnhanceScrollItemExtension> sortedItems = new List<EnhanceScrollItemExtension>();

    private void Start()
    {
        scrollItems = GetComponentsInChildren<EnhanceScrollItemExtension>();
        if (scrollItems.Length == 0)
        {
            Debug.LogError("No scroll items in scroll view!");
            return;
        }

        int count = scrollItems.Length;
        moveFactor = (Mathf.RoundToInt((1f / count) * 10000f)) * 0.0001f;
        centeredIndex = count / 2;
        if (count % 2 == 0)
            centeredIndex = count / 2 - 1;

        int index = 0;
        for (int i = count - 1; i >= 0; i--)
        {
            scrollItems[i].OffsetIndex = i;
            scrollItems[i].CenterOffset = moveFactor * (centeredIndex - index);
            scrollItems[i].SelectScrollItem(false);
            scrollItems[i].SetScrollView(this);
            index++;
        }

        bool changed = false;
        if (startIndex < 0 || startIndex >= count)
        {
            startIndex = centeredIndex;
            changed = true;
        }

        sortedItems = new List<EnhanceScrollItemExtension>(scrollItems);
        totalWidth = cellWidth * count;
        selectedItem = scrollItems[startIndex];
        if (changed && onItemSelected != null)
        {
            onItemSelected.Invoke();
        }
        currentHorizontalValue = 0.5f - selectedItem.CenterOffset;
        LerpTweenToTarget(0f, currentHorizontalValue, false);
    }

    private void Update()
    {
        if (enableLerpTween)
        {
            currentDuration += Time.deltaTime;
            if (currentDuration > tweenDuration)
            {
                currentDuration = tweenDuration;
            }

            float percent = currentDuration / tweenDuration;
            float value = Mathf.Lerp(originHorizontalValue, currentHorizontalValue, percent);
            UpdateEnhanceScrollView(value);
            if (currentDuration >= tweenDuration)
            {
                canChangeItem = true;
                enableLerpTween = false;
                OnTweenFinish();
            }
        }
    }

    private void LerpTweenToTarget(float originValue, float targetValue, bool needTween)
    {
        if (needTween)
        {
            originHorizontalValue = originValue;
            currentHorizontalValue = targetValue;
            currentDuration = 0f;
        }
        else
        {
            SortScrollItems();
            originHorizontalValue = targetValue;
            UpdateEnhanceScrollView(targetValue);
            OnTweenFinish();
        }
        enableLerpTween = needTween;
    }

    private void OnTweenFinish()
    {
        if (previousItem != null)
        {
            previousItem.SelectScrollItem(false);
        }
        if (selectedItem != null)
        {
            selectedItem.SelectScrollItem(true);
            selectedItem.transform.localPosition = Vector3.zero;
        }
    }

    public void UpdateEnhanceScrollView(float value)
    {
        int count = scrollItems.Length;
        for (int i = 0; i < count; i++)
        {
            EnhanceScrollItemExtension item = scrollItems[i];
            float positionX = positionCurve.Evaluate(value + item.CenterOffset) * totalWidth - (totalWidth / 2f);
            float scale = scaleCurve.Evaluate(value + item.CenterOffset);
            float depth = depthCurve.Evaluate(value + item.CenterOffset);
            item.UpdateScrollItem(new Vector3(positionX, cellPositionY, 0f), new Vector3(scale, scale, 1f), depth, count);
        }
    }

    private static int SortPosition(EnhanceScrollItemExtension a, EnhanceScrollItemExtension b)
    {
        return a.transform.localPosition.x.CompareTo(b.transform.localPosition.x);
    }

    private void SortScrollItems()
    {
        sortedItems.Sort(SortPosition);
        for (int i = sortedItems.Count - 1; i >= 0; i--)
        {
            sortedItems[i].RealIndex = i;
        }
    }

    private void ChangeCenteredItem(EnhanceScrollItemExtension item)
    {
        if (canChangeItem && selectedItem != item)
        {
            canChangeItem = false;
            previousItem = selectedItem;
            selectedItem = item;
            if (onItemSelected != null)
            {
                onItemSelected.Invoke();
            }

            SortScrollItems();

            float centerXValue = positionCurve.Evaluate(0.5f) * totalWidth - (totalWidth / 2f);
            int moveIndexCount = Mathf.Abs(Mathf.Abs(previousItem.RealIndex) - Mathf.Abs(item.RealIndex));
            float deltaValue;
            if (item.transform.localPosition.x > centerXValue)
            {
                deltaValue = -moveFactor * moveIndexCount;
            }
            else
            {
                deltaValue = moveFactor * moveIndexCount;
            }

            LerpTweenToTarget(currentHorizontalValue, currentHorizontalValue + deltaValue, true);
        }
    }

    public void OnBeginDragScrollItem(EnhanceScrollItemExtension item)
    {
    }

    public void OnDragScrollItem(EnhanceScrollItemExtension item, Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > 0.0f)
        {
            currentHorizontalValue += delta.x * dragFactor;
            LerpTweenToTarget(0.0f, currentHorizontalValue, false);
        }
    }

    public void OnEndDragScrollItem(EnhanceScrollItemExtension item)
    {
        int closestIndex = 0;
        float value = (currentHorizontalValue - (int)currentHorizontalValue);
        float min = float.MaxValue;
        float center = 0.5f * (currentHorizontalValue < 0 ? -1 : 1);
        for (int i = 0; i < scrollItems.Length; i++)
        {
            float distance = Mathf.Abs(Mathf.Abs(value) - Mathf.Abs((center - scrollItems[i].CenterOffset)));
            if (distance < min)
            {
                closestIndex = i;
                min = distance;
            }
        }
        originHorizontalValue = currentHorizontalValue;
        float target = ((int)currentHorizontalValue + (center - scrollItems[closestIndex].CenterOffset));
        previousItem = selectedItem;
        selectedItem = scrollItems[closestIndex];
        if (onItemSelected != null)
        {
            onItemSelected.Invoke();
        }
        LerpTweenToTarget(originHorizontalValue, target, true);
        canChangeItem = false;
    }

    public void OnPointerClickScrollItem(EnhanceScrollItemExtension item)
    {
        ChangeCenteredItem(item);
    }

    public void SelectLeftItem()
    {
        if (canChangeItem)
        {
            int index = selectedItem.OffsetIndex - 1;
            if (index < 0)
            {
                index = scrollItems.Length - 1;
            }
            ChangeCenteredItem(scrollItems[index]);        
        }
    }

    public void SelectRightItem()
    {
        if (canChangeItem)
        {
            int index = selectedItem.OffsetIndex + 1;
            if (index > scrollItems.Length - 1)
            {
                index = 0;
            }
            ChangeCenteredItem(scrollItems[index]);
        }
    }

    public void SelectItem(int index)
    {
        if (canChangeItem)
        {
            if (index >= 0 && index < scrollItems.Length)
            {
                ChangeCenteredItem(scrollItems[index]);
            }
        }
    }

#if UNITY_EDITOR
    private void Reset()
    {
        if (!Application.isPlaying)
        {
            scaleCurve = new AnimationCurve(new Keyframe(0f, 0.3743f, 0f, 1.2678f), new Keyframe(0.5f, 1f, 1.2678f, -1.2399f));
            scaleCurve.preWrapMode = WrapMode.PingPong;
            scaleCurve.postWrapMode = WrapMode.PingPong;

            positionCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));
            positionCurve.preWrapMode = WrapMode.Loop;
            positionCurve.postWrapMode = WrapMode.Loop;

            depthCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 2f), new Keyframe(0.5f, 1f, 2f, -2f), new Keyframe(1f, 0f, -2f, 0f));
            depthCurve.preWrapMode = WrapMode.Loop;
            depthCurve.postWrapMode = WrapMode.Loop;
        }
    }
#endif
}
