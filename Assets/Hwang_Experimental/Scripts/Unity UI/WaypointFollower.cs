using System;
using UnityEngine;
using UnityEngine.Events;

public class WaypointFollower : MonoBehaviour
{
    [SerializeField]
    private Waypoint[] waypoints = null;

    [SerializeField]
    private Waypoint targetWaypoint = null;
    public Waypoint TargetWaypoint
    {
        get { return targetWaypoint; }
        set { targetWaypoint = value; InitializeWaypoint(); }
    }

    [SerializeField]
    private float movingSpeed = 5f;
    public float FollowingSpeed
    {
        get { return movingSpeed; }
        set { movingSpeed = value; }
    }

    [SerializeField]
    private float rotatingSpeed = 100f;
    public float RotatingSpeed
    {
        get { return rotatingSpeed; }
        set { rotatingSpeed = value; }
    }

    [SerializeField]
    private float stoppingDistance = 0.1f;
    public float StoppingDistance
    {
        get { return stoppingDistance; }
        set { stoppingDistance = value; }
    }

    [SerializeField]
    private bool patrolMode = false;
    public bool PatrolMode
    {
        get { return patrolMode; }
        set { patrolMode = value; }
    }

    [SerializeField]
    private bool ignoreHeights = false;
    public bool IgnoreHeights
    {
        get { return ignoreHeights; }
        set { ignoreHeights = value; }
    }

    public UnityEvent onArrived;

    [NonSerialized]
#pragma warning disable CS0108 
    private Rigidbody rigidbody;
#pragma warning restore CS0108 

    [NonSerialized]
    private Vector3 targetPosition;

    [NonSerialized]
    private int pointIndex = 0;

    [NonSerialized]
    private float elapsedTime = 0f;

    [NonSerialized]
    private float currentSpeed = 0f;

    [NonSerialized]
    private bool isFollowing = true;

    [NonSerialized]
    private bool isReturning = false;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

        InitializeWaypoint();

#if UNITY_EDITOR
        oldWaypoint = targetWaypoint;
#endif
    }

#if UNITY_EDITOR
    [NonSerialized]
    private Waypoint oldWaypoint;
#endif

    private void FixedUpdate()
    {
#if UNITY_EDITOR
        if (oldWaypoint != targetWaypoint)
        {
            oldWaypoint = targetWaypoint;
            InitializeWaypoint();
        }
#endif
        if (isFollowing && targetWaypoint != null)
        {
            if (currentSpeed < movingSpeed)
            {
                elapsedTime += Time.deltaTime;
                currentSpeed = Mathf.SmoothStep(currentSpeed, movingSpeed, elapsedTime / Mathf.PI);
            }
            else
            {
                elapsedTime = 0f;
                currentSpeed = movingSpeed;
            }
            if (ignoreHeights)
            {
                if (targetWaypoint.CoordinateSpace == Space.World)
                {
                    targetPosition.y = transform.position.y;
                }
                else
                {
                    targetPosition.y = transform.localPosition.y;
                }
            }
            if (targetWaypoint.CoordinateSpace == Space.World)
            {
                Quaternion targetRotation = Quaternion.LookRotation((targetPosition - transform.position).normalized);
                targetRotation = Quaternion.Euler(transform.eulerAngles.x, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
                float deltaAngle = Mathf.DeltaAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y);
                if (!Mathf.Approximately(deltaAngle, 0f))
                {
                    if (rigidbody != null)
                    {
                        rigidbody.rotation = Quaternion.RotateTowards(rigidbody.rotation, targetRotation, Time.deltaTime * rotatingSpeed);
                    }
                    else
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotatingSpeed);
                    }
                    deltaAngle -= Mathf.DeltaAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y);
                    currentSpeed *= 1f - (Mathf.Min(Mathf.Abs(deltaAngle), 90f) / 90f);
                }
                if (rigidbody != null)
                {
                    rigidbody.position = Vector3.MoveTowards(rigidbody.position, targetPosition, Time.deltaTime * currentSpeed);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * currentSpeed);
                }
                if (Mathf.Abs(Vector3.Distance(transform.position, targetPosition)) <= stoppingDistance)
                {
                    NextTargetPosition();
                }
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation((targetPosition - transform.localPosition).normalized);
                targetRotation = Quaternion.Euler(transform.localEulerAngles.x, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
                float deltaAngle = Mathf.DeltaAngle(transform.localEulerAngles.y, targetRotation.eulerAngles.y);
                if (!Mathf.Approximately(deltaAngle, 0f))
                {
                    if (rigidbody != null)
                    {
                        rigidbody.rotation = Quaternion.RotateTowards(rigidbody.rotation, targetRotation, Time.deltaTime * rotatingSpeed);
                    }
                    else
                    {
                        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, Time.deltaTime * rotatingSpeed);
                    }
                    deltaAngle -= Mathf.DeltaAngle(transform.localEulerAngles.y, targetRotation.eulerAngles.y);
                    currentSpeed *= 1f - (Mathf.Min(Mathf.Abs(deltaAngle), 90f) / 90f);
                }
                if (rigidbody != null)
                {
                    rigidbody.position = Vector3.MoveTowards(rigidbody.position, targetPosition, Time.deltaTime * currentSpeed);
                }
                else
                {
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, Time.deltaTime * currentSpeed);
                }
                if (Mathf.Abs(Vector3.Distance(transform.localPosition, targetPosition)) <= stoppingDistance)
                {
                    NextTargetPosition();
                }
            }
        }
    }

    private void NextTargetPosition()
    {
        if (targetWaypoint != null)
        {
            if (patrolMode)
            {
                if (isReturning)
                {
                    if (pointIndex > 0)
                    {
                        pointIndex--;
                    }
                    else
                    {
                        pointIndex++;
                        isReturning = false;
                    }
                }
                else
                {
                    if (pointIndex < targetWaypoint.Positions.Length - 1)
                    {
                        pointIndex++;
                    }
                    else
                    {
                        pointIndex--;
                        isReturning = true;
                    }
                }
            }
            else
            {
                if (pointIndex < targetWaypoint.Positions.Length - 1)
                {
                    pointIndex++;
                }
                else
                {
                    isFollowing = false;
                    if (onArrived != null)
                    {
                        onArrived.Invoke();
                    }
                }
            }
            if (pointIndex >= 0 && pointIndex < targetWaypoint.Positions.Length)
            {
                targetPosition = targetWaypoint.Positions[pointIndex];
            }
        }
    }

    public void InitializeWaypoint()
    {
        pointIndex = 0;
        isReturning = false;
        isFollowing = false;
        if (targetWaypoint == null && waypoints != null && waypoints.Length > 0)
        {
            targetWaypoint = waypoints[UnityEngine.Random.Range(0, waypoints.Length)];
        }
        if (targetWaypoint != null && targetWaypoint.Positions.Length > 0)
        {
            if (targetWaypoint.CoordinateSpace == Space.World)
            {
                transform.eulerAngles = targetWaypoint.InitialAngles;
                transform.position = targetWaypoint.Positions[0];
            }
            else
            {
                transform.localEulerAngles = targetWaypoint.InitialAngles;
                transform.localPosition = targetWaypoint.Positions[0];
            }
            if (targetWaypoint.Positions.Length > 1)
            {
                targetPosition = targetWaypoint.Positions[1];
                pointIndex = 1;
                elapsedTime = 0f;
                currentSpeed = 0f;
                isFollowing = true;
            }
        }
    }
}
