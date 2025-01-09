using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class UrgSensorDetector : MonoBehaviour
{
    public enum UrgCommunicateType
    {
        Serial, Ethernet
    }

    public enum UrgSensorDirection
    {
        BottomToTop, TopToBottom, LeftToRight, RightToLeft
    }

    public enum AreaCroppingMethod
    {
        Radius, Rect
    }

    [Header("Sensor Settings")]
    public UrgCommunicateType DeviceType = UrgCommunicateType.Ethernet;
    public string PortName = "COM1";
    public int BoudRate = 19200;
    public string IPAddress = "192.168.0.10";
    public int PortNumber = 10940;

    [SerializeField]
    [Range(-135f, 135f)]
    private float startDegree = -90f;
    public float StartDegree
    {
        get { return startDegree; }
        set { if (startDegree != value && value <= endDegree) { startDegree = Mathf.Clamp(value, -135f, 135f); RestartAcquire(); } }
    }

    [SerializeField]
    [Range(-135f, 135f)]
    private float endDegree = 90f;
    public float EndDegree
    {
        get { return endDegree; }
        set { if (endDegree != value && value >= startDegree) { endDegree = Mathf.Clamp(value, -135f, 135f); RestartAcquire(); } }
    }

    [SerializeField]
    private UrgSensorDirection sensorDirection = UrgSensorDirection.TopToBottom;
    public UrgSensorDirection SensorDirection
    {
        get { return sensorDirection; }
        set { if (sensorDirection != value) { sensorDirection = value; ChangeSensorDirection(); } }
    }

    [SerializeField]
    [Range(-360f, 360f)]
    private float rotateDegree = 180f;
    public float RotateDegree
    {
        get { return rotateDegree; }
        set { if (rotateDegree != value) { rotateDegree = Mathf.Clamp(value, -180f, 180f); RecalculateAll(); } }
    }

    [SerializeField]
    private bool isFlipped = true;
    public bool IsFlipped
    {
        get { return isFlipped; }
        set { if (isFlipped != value) { isFlipped = value; RecalculateAll(); } }
    }

    [SerializeField]
    private bool continuousMode = true;
    public bool ContinuousMode
    {
        get { return continuousMode; }
        set { if (continuousMode != value) { continuousMode = value; RestartAcquire(); } }
    }

    [NonSerialized]
    private bool acquireStrength = false;
    public bool AcquireStrength
    {
        get { return acquireStrength; }
        set { if (acquireStrength != value) { acquireStrength = value; RestartAcquire(); } }
    }

    [SerializeField]
    [Range(1, 10)]
    private int groupSize = 1;
    public int GroupSize
    {
        get { return groupSize; }
        set { if (groupSize != value) { groupSize = Mathf.Clamp(value, 1, 10); RestartAcquire(); } }
    }

    [SerializeField]
    [Range(0, 9)]
    private int skipCount = 0;
    public int SkipCount
    {
        get { return skipCount; }
        set { if (skipCount != value) { skipCount = Mathf.Clamp(value, 0, 9); if (continuousMode) RestartAcquire(); } }
    }

    [SerializeField]
    [Header("Detection Area (actual size in mm)")]
    private AreaCroppingMethod cropMethod = AreaCroppingMethod.Rect;
    public AreaCroppingMethod CropMethod
    {
        get { return cropMethod; }
        set { if (cropMethod != value) { cropMethod = value; } }
    }

    [SerializeField]
    [Range(0, 49900)]
    private int radiusMin = 0;
    public int RadiusMin
    {
        get { return radiusMin; }
        set { if (radiusMin != value && value <= radiusMax - 100) { radiusMin = Mathf.Clamp(value, 0, 49900); } }
    }

    [SerializeField]
    [Range(100, 50000)]
    private int radiusMax = 5000;
    public int RadiusMax
    {
        get { return radiusMax; }
        set { if (radiusMax != value && value >= radiusMin + 100) { radiusMax = Mathf.Clamp(value, 100, 50000); } }
    }

    [SerializeField]
    private Vector2Int rectSize = new Vector2Int(5200, 2100);
    public Vector2Int RectSize
    {
        get { return rectSize; }
        set { if (!rectSize.Equals(value)) { rectSize = value; } }
    }

    [SerializeField]
    private Vector2Int rectOffset = new Vector2Int(0, 0);
    public Vector2Int RectOffset
    {
        get { return rectOffset; }
        set { if (!rectOffset.Equals(value)) { rectOffset = value; } }
    }

    [SerializeField]
    [Header("Screen Coordinates")]
    private Vector2Int screenSize = new Vector2Int(3840, 1200);
    public Vector2Int ScreenSize
    {
        get { return screenSize; }
        set { if (!screenSize.Equals(value)) { screenSize = value; } }
    }

    [SerializeField]
    private Vector2Int screenOffset = new Vector2Int(0, 0);
    public Vector2Int ScreenOffset
    {
        get { return screenOffset; }
        set { if (!screenOffset.Equals(value)) { screenOffset = value; } }
    }

    [Header("Initial Noise Removal")]
    [Range(0f, 60f)]
    public float DenoiseTime = 5f;
    [Range(1, 100)]
    public int DenoiseBlockSize = 25;

    [Header("Object Detection")]
    [Range(1, 40)]
    public int DetectionNoise = 5;
    [Range(10, 1000)]
    public int DetectionDelta = 200;

    [Header("Object Tracking")]
    [Range(0, 1000)]
    public int TrackingDistance = 300;
    [Range(0f, 1f)]
    public float TrackingSmoothTime = 0.2f;

    [Header("Event Delays (0.025 sec. per frame)")]
    [Range(0, 40)]
    public int PressObjectDelay = 8;
    [Range(0, 40)]
    public int UpdateObjectDelay = 2;
    [Range(0, 40)]
    public int ReleaseObjectDelay = 2;
    [Range(0, 40)]
    public int RemoveObjectDelay = 8;

#if UNITY_EDITOR
    [SerializeField]
    [Header("Debug Options")]
    private bool drawDistanceRay = true;
    [NonSerialized]
    private bool drawStrengthRay = true;
    [SerializeField]
    private bool drawDetectedRay = true;
    [SerializeField]
    private bool drawRawObjects = true;
    [SerializeField]
    private bool drawHitObjects = true;
    [NonSerialized]
    private Color urgSensorColor = new Color(1f, 0.5f, 0f, 1f);
    [NonSerialized]
    private Color distanceRayColor = new Color(0f, 1f, 0f, 0.5f);
    [NonSerialized]
    private Color strengthRayColor = new Color(1f, 0f, 1f, 0.5f);
    [NonSerialized]
    private Color detectedRayColor = new Color(0f, 1f, 1f, 0.5f);
    [NonSerialized]
    private Color aliveObjectColor = new Color(0f, 0f, 1f, 1f);
    [NonSerialized]
    private Color noiseObjectColor = new Color(1f, 0f, 0f, 1f);
#endif

    [NonSerialized]
    private UrgDevice urg;
    [NonSerialized]
    private int areaTotal = 1440;
    [NonSerialized]
    private int noiseRate = 1;
    [NonSerialized]
    private int startStep = 0;
    [NonSerialized]
    private int endStep = 1080;
    [NonSerialized]
    private int stepCount = 0;
    [NonSerialized]
    private float frameTime = 0f;
    [NonSerialized]
    private float frameCheckTime = 0f;
    [NonSerialized]
    private int frameCount = 0;
    [NonSerialized]
    private int missingCount = 0;
    [NonSerialized]
    private int framesPerSecond = 0;
    public int FramesPerSecond
    {
        get { return framesPerSecond; }
    }
    [NonSerialized]
    private int lastTimeStamp = 0;
    public int LastTimeStamp
    {
        get { return lastTimeStamp; }
    }

    [NonSerialized]
    private Coroutine denoiseRoutine;
    [NonSerialized]
    private Coroutine restartRoutine;

    [NonSerialized]
    private readonly List<int> distances = new List<int>();
    [NonSerialized]
    private readonly List<int> strengths = new List<int>();
    [NonSerialized]
    private Vector2[] directions;
    [NonSerialized]
    private readonly List<UrgObjectRaw> rawObjects = new List<UrgObjectRaw>();
    [NonSerialized]
    private readonly List<UrgObjectHit> hitObjects = new List<UrgObjectHit>();

    public event Action<UrgObjectHit> ObjectAdded;
    public event Action<UrgObjectHit> ObjectPressed;
    public event Action<UrgObjectHit> ObjectUpdated;
    public event Action<UrgObjectHit> ObjectReleased;
    public event Action<UrgObjectHit> ObjectRemoved;

    public bool Connected
    {
        get { return urg != null && urg.Connected; }
    }

    public bool IsLaserOn
    {
        get { return urg != null && urg.Connected && urg.IsLaserOn; }
    }

    private static UrgSensorDetector instance;
    public static UrgSensorDetector Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        RecalculateAll();

        if (DeviceType == UrgCommunicateType.Ethernet)
        {
            UrgDeviceEthernet urgEthernet = gameObject.GetComponent<UrgDeviceEthernet>();
            if (urgEthernet == null)
            {
                urgEthernet = gameObject.AddComponent<UrgDeviceEthernet>();
            }
            urgEthernet.IPAddress = IPAddress;
            urgEthernet.PortNumber = PortNumber;
            urg = urgEthernet;
        }
        else
        {
            UrgDeviceSerial urgSerial = gameObject.GetComponent<UrgDeviceSerial>();
            if (urgSerial == null)
            {
                urgSerial = gameObject.AddComponent<UrgDeviceSerial>();
            }
            urgSerial.PortName = PortName;
            urgSerial.BaudRate = BoudRate;
            urg = urgSerial;
        }
        urg.OnConnect += Urg_OnConnect;
        urg.OnDisconnect += Urg_OnDisconnect;
        urg.OnGetVersion += Urg_OnGetVersion;
        urg.OnGetParameters += Urg_OnGetParameters;
        urg.OnGetStatus += Urg_OnGetStatus;
        urg.OnResetParameters += Urg_OnResetParameters;
        urg.OnReceiveData += Urg_OnReceiveData;
        urg.Connect();
    }

    private void OnEnable()
    {
        if (urg != null)
        {
            if (urg.Connected)
            {
                StartAcquire();
            }
            else
            {
                urg.Connect();
            }
        }
    }

    private void OnDisable()
    {
        StopAcquire();
    }

    private void Update()
    {
        frameTime += Time.deltaTime;
        if (frameTime < 0.02f) return;
        frameCheckTime += frameTime;
        frameTime = 0f;

        if (frameCheckTime >= 1f)
        {
            if (frameCount == 0)
            {
                missingCount++;
                if (missingCount >= 5)
                {
                    missingCount = -5;
                    ResetParameters();
                }
            }
            framesPerSecond = frameCount;
            frameCheckTime = 0f;
            frameCount = 0;
        }

        if (!AcquireData() || !urg.HasMeasurementData) return;

        lock (distances)
        {
            distances.Clear();
            distances.AddRange(urg.Distances);
        }

        lock (strengths)
        {
            strengths.Clear();
            strengths.AddRange(urg.Strengths);
        }

        if (stepCount != distances.Count)
        {
            RecalculateAll();
        }
        else
        {
            UpdateAllObjects();
        }
    }

    private void Urg_OnConnect()
    {
        urg.RequestVersion();
        urg.RequestParameters();

        StartDenoise();
        StartAcquire();
    }

    private void Urg_OnDisconnect()
    {
        StopDenoise();

        distances.Clear();
        strengths.Clear();

        frameTime = 0f;
        frameCheckTime = 0f;
        frameCount = 0;
        missingCount = 0;
        framesPerSecond = 0;
        lastTimeStamp = 0;
    }

    private void Urg_OnGetVersion()
    {
        Debug.Log(string.Format("URG Device : Version information\n{0}", urg.Version));
        if (!string.IsNullOrEmpty(urg.Version.Protocol) && urg.Version.Protocol.StartsWith("SCIP 1."))
        {
            urg.SwitchToSCIP2();
        }
    }

    private void Urg_OnGetParameters()
    {
        Debug.Log(string.Format("URG Device : Parameter information\n{0}", urg.Parameters));
        areaTotal = urg.Parameters.AreaTotal;
        noiseRate = areaTotal > 1440 ? 2 : 1;
    }

    private void Urg_OnGetStatus()
    {
        Debug.Log(string.Format("URG Device : Status information\n{0}", urg.Status));
    }

    private void Urg_OnResetParameters()
    {
        Debug.Log("URG Device : Reset parameters");
        StartAcquire(false);
    }

    private void Urg_OnReceiveData()
    {
        frameCount++;
        lastTimeStamp = urg.TimeStamp;
    }

    private IEnumerator WaitForDenoise()
    {
        yield return new WaitForSeconds(DenoiseTime);
        denoiseRoutine = null;
    }

    public void StartDenoise()
    {
        rawObjects.Clear();
        hitObjects.Clear();
        if (DenoiseTime > 0f)
        {
            if (denoiseRoutine != null)
            {
                StopCoroutine(denoiseRoutine);
            }
            denoiseRoutine = StartCoroutine(WaitForDenoise());
        }
    }

    public void StopDenoise()
    {
        if (denoiseRoutine != null)
        {
            StopCoroutine(denoiseRoutine);
            denoiseRoutine = null;
        }
    }

    private void StartAcquire(bool checkStatus = true)
    {
        if (urg != null && urg.Connected)
        {
            if (continuousMode)
            {
                urg.RequestContinuousMeasurementData(acquireStrength ? 2 : 1, startStep, endStep, groupSize, skipCount);
            }
            else
            {
                urg.BeginMeasurement();
            }
            if (checkStatus)
            {
                urg.RequestStatus();
            }
        }
    }

    private bool AcquireData()
    {
        if (urg != null && urg.Connected)
        {
            if (!continuousMode)
            {
                urg.RequestMeasurementData(acquireStrength ? 2 : 1, startStep, endStep, groupSize);
            }
            return true;
        }
        return false;
    }

    private void StopAcquire(bool checkStatus = true)
    {
        if (urg != null && urg.Connected)
        {
            urg.SuspendMeasurement();
            if (checkStatus)
            {
                urg.RequestStatus();
            }
        }
    }

    private IEnumerator DelayedRestart()
    {
        StopAcquire(false);
        yield return new WaitForSeconds(0.05f);
        RecalculateAll();
        StartAcquire(false);
        restartRoutine = null;
    }

    public void RestartAcquire()
    {
        if (Application.isPlaying)
        {
            if (restartRoutine != null)
            {
                StopCoroutine(restartRoutine);
            }
            restartRoutine = StartCoroutine(DelayedRestart());
        }
    }

    public void ResetParameters()
    {
        if (urg != null && urg.Connected)
        {
            urg.ResetParameters();
        }
    }

    public void RecalculateAll()
    {
        startStep = Mathf.RoundToInt((startDegree + 135f) * (areaTotal / 360));
        endStep = Mathf.RoundToInt((endDegree + 135f) * (areaTotal / 360));
        stepCount = (endStep - startStep) / groupSize + 1;
        directions = new Vector2[stepCount];

        int stepOffset = Mathf.RoundToInt((startDegree - rotateDegree + 270f - (isFlipped ? startDegree + endDegree : 0f)) * (areaTotal / 360));
        if (stepOffset < 0) stepOffset += areaTotal;
        else if (stepOffset >= areaTotal) stepOffset -= areaTotal;

        float stepAngle = Mathf.PI * 2f / areaTotal;
        for (int i = 0; i < directions.Length; i++)
        {
            float angle = stepAngle * (stepOffset + (isFlipped ? stepCount - 1 - i : i) * groupSize);
            directions[i] = new Vector2(-Mathf.Cos(angle), -Mathf.Sin(angle));
        }
    }

    private List<UrgObjectRaw> DetectObjects()
    {
        List<UrgObjectRaw> result = new List<UrgObjectRaw>();
        if (directions.Length == 0) return result;

        Rect detectionArea = GetDetectionArea();
        bool isGrouping = false;
        for (int i = 1; i < distances.Count - 1; i++)
        {
            float deltaA = Mathf.Abs(distances[i] - distances[i - 1]);
            float deltaB = Mathf.Abs(distances[i + 1] - distances[i]);
            int distance = distances[i];
            bool isInside;
            if (cropMethod == AreaCroppingMethod.Rect)
            {
                float angle = -Vector2.SignedAngle(directions[i], Vector2.right) * Mathf.Deg2Rad;
                Vector2 position = new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
                isInside = detectionArea.Contains(position);
            }
            else
            {
                isInside = distance >= radiusMin && distance <= radiusMax;
            }

            if (isInside && deltaA < DetectionDelta && deltaB < DetectionDelta)
            {
                if (isGrouping)
                {
                    UrgObjectRaw rawObj = result[result.Count - 1];
                    rawObj.Add(i, distance);
                }
                else
                {
                    isGrouping = true;
                    UrgObjectRaw rawObj = new UrgObjectRaw(directions, i, distance);
                    result.Add(rawObj);
                }
            }
            else
            {
                isGrouping = false;
            }
        }

        for (int i = result.Count - 1; i >= 0; i--)
        {
            if (result[i].stepList.Count < DetectionNoise * noiseRate)
            {
                result.RemoveAt(i);
            }
            else
            {
                result[i].UpdatePosition();
                result[i].UpdateDegree(rotateDegree, IsFlipped);
            }
        }
        return result;
    }

    private void UpdateAllObjects()
    {
        List<UrgObjectRaw> detectedObjects = DetectObjects();

        lock (rawObjects)
        {
            rawObjects.Clear();
            rawObjects.AddRange(detectedObjects);
        }

        bool isDenoising = denoiseRoutine != null;
        Rect detectionArea = GetDetectionArea();
        Rect screenArea = new Rect(screenOffset, screenSize);

        lock (hitObjects)
        {
            for (int i = 0; i < hitObjects.Count; i++)
            {
                float minDistance = float.MaxValue;
                UrgObjectRaw closestRawObj = null;
                UrgObjectHit hitObj = hitObjects[i];
                for (int n = 0; n < detectedObjects.Count; n++)
                {
                    UrgObjectRaw rawObj = detectedObjects[n];
                    float distance = Vector2.Distance(rawObj.Position, hitObj.Position);
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        closestRawObj = rawObj;
                    }
                }

                bool wasPressed = hitObj.Pressed;
                if (minDistance <= (hitObj.IsNoise ? DenoiseBlockSize : TrackingDistance))
                {
                    bool updated = hitObj.UpdateObject(closestRawObj, TrackingSmoothTime, PressObjectDelay, UpdateObjectDelay);
                    hitObj.UpdateScreenPosition(detectionArea, screenArea);
                    detectedObjects.Remove(closestRawObj);
                    if (!isDenoising && !hitObj.IsNoise)
                    {
                        if (!wasPressed && hitObj.Pressed)
                        {
                            if (ObjectPressed != null)
                            {
                                ObjectPressed(hitObj);
                            }
                        }
                        else if (updated)
                        {
                            if (ObjectUpdated != null)
                            {
                                ObjectUpdated(hitObj);
                            }
                        }
                    }
                }
                else
                {
                    hitObj.UpdateObject(ReleaseObjectDelay, RemoveObjectDelay);
                    if (!isDenoising && !hitObj.IsNoise)
                    {
                        if (wasPressed && !hitObj.Pressed)
                        {
                            if (ObjectReleased != null)
                            {
                                ObjectReleased(hitObj);
                            }
                        }
                    }
                }
            }

            for (int i = hitObjects.Count - 1; i >= 0; i--)
            {
                UrgObjectHit hitObj = hitObjects[i];
                if (hitObj.Expired)
                {
                    if (!hitObj.IsNoise)
                    {
                        hitObjects.Remove(hitObj);
                    }
                    if (!isDenoising && !hitObj.IsNoise)
                    {
                        if (ObjectRemoved != null)
                        {
                            ObjectRemoved(hitObj);
                        }
                    }
                }
            }

            for (int n = 0; n < detectedObjects.Count; n++)
            {
                UrgObjectRaw rawObj = detectedObjects[n];
                UrgObjectHit hitObj = new UrgObjectHit(rawObj, startStep);
                hitObj.UpdateScreenPosition(detectionArea, screenArea);
                if (isDenoising)
                {
                    hitObj.IsNoise = true;
                }
                hitObjects.Add(hitObj);
                if (!isDenoising && !hitObj.IsNoise)
                {
                    if (ObjectAdded != null)
                    {
                        ObjectAdded(hitObj);
                    }
                }
            }
        }
    }

    private void ChangeSensorDirection()
    {
        switch (sensorDirection)
        {
            case UrgSensorDirection.BottomToTop:
                rotateDegree = 0f;
                break;
            case UrgSensorDirection.TopToBottom:
                rotateDegree = 180f;
                break;
            case UrgSensorDirection.LeftToRight:
                rotateDegree = 90f;
                break;
            case UrgSensorDirection.RightToLeft:
                rotateDegree = 270f;
                break;
        }
        RecalculateAll();
    }

    private Rect GetDetectionArea()
    {
        if (cropMethod == AreaCroppingMethod.Rect)
        {
            Vector2 rectPosition = Vector2.zero;
            switch (sensorDirection)
            {
                case UrgSensorDirection.BottomToTop:
                    rectPosition = new Vector2(rectOffset.x - rectSize.x / 2f, rectOffset.y);
                    break;
                case UrgSensorDirection.TopToBottom:
                    rectPosition = new Vector2(rectOffset.x - rectSize.x / 2f, -rectOffset.y - rectSize.y);
                    break;
                case UrgSensorDirection.LeftToRight:
                    rectPosition = new Vector2(rectOffset.x, rectOffset.y - rectSize.y / 2f);
                    break;
                case UrgSensorDirection.RightToLeft:
                    rectPosition = new Vector2(-rectOffset.x - rectSize.x, rectOffset.y - rectSize.y / 2f);
                    break;
            }
            return new Rect(rectPosition, rectSize);
        }
        else
        {
            return new Rect(-radiusMax, -radiusMax, radiusMax * 2f, radiusMax * 2f);
        }
    }

    public UrgObjectRaw[] GetRawObjects(float minSize = 100f)
    {
        List<UrgObjectRaw> result = new List<UrgObjectRaw>();
        for (int i = 0; i < rawObjects.Count; i++)
        {
            if (rawObjects[i].Size >= minSize)
            {
                result.Add(rawObjects[i]);
            }
        }
        return result.ToArray();
    }

    public UrgObjectHit[] GetHitObjects(float minAge = 0.25f)
    {
        List<UrgObjectHit> result = new List<UrgObjectHit>();
        for (int i = 0; i < hitObjects.Count; i++)
        {
            if (hitObjects[i].Age >= minAge && !hitObjects[i].IsNoise)
            {
                result.Add(hitObjects[i]);
            }
        }
        return result.ToArray();
    }

#if UNITY_EDITOR
    [NonSerialized]
    private UrgSensorDirection oldDirection;

    private void Reset()
    {
        oldDirection = sensorDirection;
    }

    private void OnValidate()
    {
        if (endDegree < startDegree) endDegree = startDegree;
        if (radiusMin > radiusMax - 100) radiusMin = radiusMax - 100;
        if (oldDirection != sensorDirection)
        {
            oldDirection = sensorDirection;
            ChangeSensorDirection();
        }
        if (rectSize == Vector2Int.zero)
        {
            if (Camera.main != null)
            {
                rectSize = new Vector2Int(Camera.main.pixelWidth, Camera.main.pixelHeight);
            }
            else
            {
                rectSize = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            }
            EditorGUIUtility.editingTextField = false;
        }
        if (screenSize == Vector2Int.zero)
        {
            if (Camera.main != null)
            {
                screenSize = new Vector2Int(Camera.main.pixelWidth, Camera.main.pixelHeight);
            }
            else
            {
                screenSize = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            }
            EditorGUIUtility.editingTextField = false;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        Rect detectionArea = GetDetectionArea();
        if (cropMethod == AreaCroppingMethod.Rect)
        {
            Handles.DrawWireCube(center + (Vector3)detectionArea.center, (Vector3)detectionArea.size);
        }
        else
        {
            if (radiusMin > 0)
            {
                Handles.DrawWireDisc(center, Vector3.forward, radiusMin);
            }
            Handles.DrawWireDisc(center, Vector3.forward, radiusMax);
        }

        Color handlesColor = Handles.color;
        Handles.color = urgSensorColor;
        Handles.DrawSolidArc(center, Vector3.forward, (Vector3)GetRotatedPosition(rotateDegree + (isFlipped ? -135f : 135f)), isFlipped ? -270f : 270f, 25f);
        Handles.color = handlesColor;
        Handles.DrawLine(center, center + (Vector3)GetRotatedPosition(rotateDegree * (isFlipped ? 1f : -1f) + startDegree, isFlipped, 25f));
        Handles.DrawLine(center, center + (Vector3)GetRotatedPosition(rotateDegree * (isFlipped ? 1f : -1f) + endDegree, isFlipped, 25f));

        if (!Connected || directions.Length < distances.Count) return;

        if (drawDistanceRay)
        {
            for (int i = 0; i < distances.Count; i++)
            {
                Debug.DrawLine(center, center + (Vector3)directions[i] * distances[i], distanceRayColor);
            }
        }

        if (drawStrengthRay)
        {
            for (int i = 0; i < strengths.Count; i++)
            {
                Debug.DrawLine(center, center + (Vector3)directions[i] * strengths[i], strengthRayColor);
            }
        }

        if (drawRawObjects || drawDetectedRay)
        {
            for (int i = 0; i < rawObjects.Count; i++)
            {
                UrgObjectRaw rawObj = rawObjects[i];
                if (drawDetectedRay)
                {
                    for (int j = 0; j < rawObj.distList.Count; j++)
                    {
                        Debug.DrawLine(center, center + (Vector3)directions[rawObj.stepList[j]] * rawObj.distList[j], detectedRayColor);
                    }
                }
                if (drawRawObjects)
                {
                    Debug.DrawLine(center, center + ((Vector3)directions[rawObj.MedianStep] * rawObj.MedianDist), aliveObjectColor);
                    Gizmos.DrawWireCube(center + (Vector3)rawObj.Position, new Vector3(100f, 100f, 0f));
                }
            }
        }

        if (drawHitObjects)
        {
            Color gizmosColor = Gizmos.color;
            for (int i = 0; i < hitObjects.Count; i++)
            {
                UrgObjectHit hitObj = hitObjects[i];
                Gizmos.color = hitObj.IsNoise ? noiseObjectColor : aliveObjectColor;
                Gizmos.DrawCube(center + (Vector3)hitObj.Position, new Vector3(30f, 30f, 0f));
                Handles.Label(center + (Vector3)hitObj.Position + new Vector3(30f, -30f, 0f), hitObj.ToString());
            }
            Gizmos.color = gizmosColor;
        }
    }

    private Vector2 GetRotatedPosition(float degree, bool clockwise = true, float distance = 1f)
    {
        float angle = Mathf.DeltaAngle(clockwise ? -90f : 90f, degree) * Mathf.Deg2Rad * (clockwise ? -1f : 1f);
        return new Vector2(-Mathf.Cos(angle), -Mathf.Sin(angle)) * distance;
    }
#endif
}
