using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HokuyoTouchManager : MonoBehaviour
{
    public UrgSensorDetector SensorDetector;
    public bool SensorRequired = false;
    public GameObject CalibrationView;
    public Text CalibrationText;
    public bool EnableCalibration = true;
    public GameObject TestModeView;
    public GameObject TouchPointer;
    public GameObject MousePointer;
    public Transform PointerContainer;
    public bool EnableTestMode = true;

    [Range(0f, 60f)]
    public float HideCursorTime = 5f;

    [NonSerialized]
    private bool showTestMode = false;

    [NonSerialized]
    private float inputlessTime = 0f;

    [NonSerialized]
    private Vector2 lastMousePosition = Vector2.zero;

    [NonSerialized]
    private Dictionary<int, GameObject> pointerObjects = new Dictionary<int, GameObject>();

    [NonSerialized]
    private Dictionary<int, TouchableObject> touchableObjects = new Dictionary<int, TouchableObject>();

    public static HokuyoTouchManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (!HokuyoSensorSettings.LoadFromXml())
        {
            HokuyoSensorSettings.SaveToXml();
        }

        if (SensorDetector != null && HokuyoSensorSettings.HokuyoSensors.Length > 0)
        {
            if (HokuyoSensorSettings.HokuyoSensors[0].HasIPAddress)
            {
                SensorDetector.IPAddress = HokuyoSensorSettings.HokuyoSensors[0].IPAddress;
                SensorDetector.PortNumber = HokuyoSensorSettings.HokuyoSensors[0].PortNumber;
            }
            if (HokuyoSensorSettings.HokuyoSensors[0].HasRectSize)
            {
                SensorDetector.RectSize = HokuyoSensorSettings.HokuyoSensors[0].RectSize;
                SensorDetector.RectOffset = HokuyoSensorSettings.HokuyoSensors[0].RectOffset;
            }
            if (HokuyoSensorSettings.HokuyoSensors[0].HasScreenSize)
            {
                SensorDetector.ScreenSize = HokuyoSensorSettings.HokuyoSensors[0].ScreenSize;
                SensorDetector.ScreenOffset = HokuyoSensorSettings.HokuyoSensors[0].ScreenOffset;
            }
            if (!EnableCalibration)
            {
                SensorDetector.DenoiseTime = 0f;
            }
            SensorDetector.ObjectPressed += SensorDetector_ObjectPressed;
            SensorDetector.ObjectReleased += SensorDetector_ObjectReleased;

#if !UNITY_EDITOR
            // 실수로 꺼놓고 빌드하는 경우가 있어서, 빌드할 때는 후쿠요 센서를 항상 켜준다.
            SensorDetector.gameObject.SetActive(true);
#endif
        }

        CalibrationView.SetActive(false);
        TestModeView.SetActive(EnableTestMode && showTestMode);
        if (TouchPointer != null)
        {
            TouchPointer.SetActive(false);
        }
        if (MousePointer != null)
        {
            MousePointer.SetActive(false);
        }
    }

    private void Start()
    {
        if (EnableCalibration && UrgSensorDetector.Instance != null)
        {
            StartCoroutine(Calibrate());
        }

        if (Input.touchSupported)
        {
            Input.simulateMouseWithTouches = false;
            Input.multiTouchEnabled = true;
        }
    }

    private IEnumerator Calibrate()
    {
        CalibrationView.SetActive(true);
        CalibrationText.text = "레이져 센서에 연결중입니다.            잠시만 기다려 주십시오.";
        CalibrationText.color = Color.yellow;
        yield return new WaitForSeconds(2f);

        while (SensorRequired && !UrgSensorDetector.Instance.Connected)
        {
            CalibrationText.text = "레이져 센서에 연결하지 못했습니다.            재시도 중입니다.";
            CalibrationText.color = Color.red;
            yield return new WaitForSeconds(3f);

            CalibrationText.text = "레이져 센서에 연결중입니다.            잠시만 기다려 주십시오.";
            CalibrationText.color = Color.yellow;
            yield return new WaitForSeconds(2f);
        }

        if (UrgSensorDetector.Instance.Connected)
        {
            CalibrationText.text = "노이즈를 제거하는 중입니다.            잠시만 기다려 주십시오.";
            CalibrationText.color = Color.green;
            yield return new WaitForSeconds(UrgSensorDetector.Instance.DenoiseTime);
        }
        else
        {
            CalibrationText.text = "레이져 센서 연결에 실패하여,   터치 기능을 사용할 수 없습니다.";
            CalibrationText.color = Color.red;
            yield return new WaitForSeconds(5f);
        }
        CalibrationView.SetActive(false);
    }

    private void SensorDetector_ObjectPressed(UrgObjectHit hitObj)
    {
        Vector2 screenPosition = hitObj.ScreenPosition;
        ShowPointer(hitObj.PointerId, screenPosition);
        TouchObject(hitObj.PointerId, screenPosition);
    }

    private void SensorDetector_ObjectReleased(UrgObjectHit hitObj)
    {
        RemovePointer(hitObj.PointerId);
        ReleaseObject(hitObj.PointerId);
    }

    private void Update()
    {
        // 터치 스크린 또는 마우스 왼쪽 버튼 사용
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                Vector2 touchPosition = touch.position;
                if (touch.phase == TouchPhase.Began)
                {
                    ShowPointer(touch.fingerId, touchPosition);
                    TouchObject(touch.fingerId, touchPosition);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    RemovePointer(touch.fingerId);
                    ReleaseObject(touch.fingerId);
                }
            }
        }
        else
        {
            Vector2 mousePosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                ShowPointer(-1, mousePosition);
                TouchObject(-1, mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                RemovePointer(-1);
                ReleaseObject(-1);
            }
        }

        // 테스트 모드 사용시, 스페이스바를 누르면 뷰의 상태를 토글
        if (EnableTestMode && Input.GetKeyDown(KeyCode.Space))
        {
            showTestMode = !showTestMode;
            TestModeView.SetActive(showTestMode);
            if (!showTestMode)
            {
                ClearPointers();
            }
            if (!Cursor.visible)
            {
                Cursor.visible = true;
            }
        }

        // 일정시간동안 입력이 없으면 마우스 커서를 숨김
        if (HideCursorTime > 0f && !showTestMode)
        {
            if (Input.anyKey || DidMouseMoveOrWheelScroll())
            {
                inputlessTime = 0f;
                if (!Cursor.visible)
                {
                    Cursor.visible = true;
                }
            }
            else if (Cursor.visible)
            {
                inputlessTime += Time.unscaledDeltaTime;
                if (inputlessTime > HideCursorTime)
                {
                    Cursor.visible = false;
                }
            }
        }
    }

    private bool DidMouseMoveOrWheelScroll()
    {
        Vector2 mouseMovement = (Vector2)Input.mousePosition - lastMousePosition;
        lastMousePosition = Input.mousePosition;
        return Mathf.Abs(mouseMovement.x) >= 2f || Mathf.Abs(mouseMovement.y) >= 2f || Input.mouseScrollDelta != Vector2.zero;
    }

    private float scaleFactor
    {
        get
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                return canvas.scaleFactor;
            }
            return 1f;
        }
    }

    private void ShowPointer(int pointerId, Vector2 screenPosition)
    {
        if (showTestMode && PointerContainer.gameObject.activeInHierarchy)
        {
            GameObject go;
            if (pointerObjects.TryGetValue(pointerId, out go) && go == null)
            {
                pointerObjects.Remove(pointerId);
            }
            GameObject template = pointerId < 0 ? MousePointer : TouchPointer;
            if (template == null) return;
            if (go == null)
            {
                go = Instantiate(template, PointerContainer);
                go.AddComponent<DelayedDestroyObject>();
                go.SetActive(true);
                pointerObjects.Add(pointerId, go);
            }
            else
            {
                DelayedDestroyObject ddo = go.GetComponent<DelayedDestroyObject>();
                if (ddo != null)
                {
                    ddo.Alive();
                }
            }
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                go.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z + template.transform.position.z));
                go.transform.rotation = Camera.main.transform.rotation;
            }
            else
            {
                rectTransform.anchoredPosition = screenPosition / scaleFactor;
            }
        }
        else
        {
            pointerObjects[pointerId] = null;
        }
    }

    private void RemovePointer(int pointerId)
    {
        if (pointerObjects.ContainsKey(pointerId))
        {
            pointerObjects.Remove(pointerId);
        }
    }

    private void ClearPointers()
    {
        DelayedDestroyObject[] objects = TestModeView.GetComponentsInChildren<DelayedDestroyObject>();
        foreach (DelayedDestroyObject obj in objects)
        {
            Destroy(obj.gameObject);
        }
        pointerObjects.Clear();
    }

    public void TouchObject(int pointerId, Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            TouchableObject touchableObject = hit.collider.GetComponent<TouchableObject>();
            if (touchableObject != null && !touchableObjects.ContainsKey(pointerId))
            {
                if (touchableObjects.ContainsValue(touchableObject))
                {
                    touchableObjects.Add(pointerId, null);
                }
                else
                {
                    touchableObject.Touch();
                    touchableObjects.Add(pointerId, touchableObject);
                }
            }
        }
    }

    public void ReleaseObject(int pointerId)
    {
        if (touchableObjects.ContainsKey(pointerId))
        {
            touchableObjects.Remove(pointerId);
        }
    }
}
