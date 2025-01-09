using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HokuyoTouchDragManager : MonoBehaviour
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
    private Dictionary<int, DraggableObject> draggableObjects = new Dictionary<int, DraggableObject>();

    public static HokuyoTouchDragManager Instance { get; private set; }

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
            SensorDetector.ObjectUpdated += SensorDetector_ObjectUpdated;
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

    private void SensorDetector_ObjectUpdated(UrgObjectHit hitObj)
    {
        Vector2 screenPosition = hitObj.ScreenPosition;
        ShowPointer(hitObj.PointerId, screenPosition);
        DragObject(hitObj.PointerId, screenPosition);
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
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    ShowPointer(touch.fingerId, touchPosition);
                    DragObject(touch.fingerId, touchPosition);
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
            else if (Input.GetMouseButton(0))
            {
                ShowPointer(-1, mousePosition);
                DragObject(-1, mousePosition);
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
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            DraggableObject draggableObject = hit.collider.GetComponent<DraggableObject>();
            if (draggableObject != null && !draggableObjects.ContainsKey(pointerId))
            {
                if (draggableObjects.ContainsValue(draggableObject))
                {
                    draggableObjects.Add(pointerId, null);
                }
                else
                {
                    draggableObject.BeginDrag(worldPosition);
                    draggableObjects.Add(pointerId, draggableObject);
                }
            }
        }
    }

    public void DragObject(int pointerId, Vector2 screenPosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
        DraggableObject draggableObject;
        if (draggableObjects.TryGetValue(pointerId, out draggableObject) && draggableObject != null)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // 오브젝트 크기 제한
                //draggableObject.SetScalingRange(new Vector3(0.25f, 0.25f, 0.25f), new Vector3(4f, 4f, 4f), !Input.GetKey(KeyCode.A));
                // 드래그시 오브젝트 크기 변경
                draggableObject.Scale(worldPosition, new Vector3(1f, 1f, 1f), !Input.GetKey(KeyCode.D));
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                // 오브젝트 회전 제한
                //draggableObject.SetRotationRange(new Vector3(-180f, -20f, 0f), new Vector3(540f, 40f, 0f), !Input.GetKey(KeyCode.A));
                // 드래그시 오브젝트 회전
                draggableObject.Rotate(worldPosition, new Vector3(0f, 1f, 0f), !Input.GetKey(KeyCode.D));
            }
            else
            {
                // 오브젝트 이동 제한
                //draggableObject.SetTranslationRange(new Vector3(-10f, -10f, 0f), new Vector3(10f, 10f, 0f), !Input.GetKey(KeyCode.A));
                // 드래그시 오브젝트 이동
                draggableObject.Translate(worldPosition, new Vector3(1f, 1f, 0f), !Input.GetKey(KeyCode.D));
            }
        }
    }

    public void ReleaseObject(int pointerId)
    {
        DraggableObject draggableObject;
        if (draggableObjects.TryGetValue(pointerId, out draggableObject) && draggableObject != null)
        {
            draggableObject.EndDrag();
        }
        draggableObjects.Remove(pointerId);
    }
}
