using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotalManager : Singleton<PivotalManager>
{
    private void Awake()
    {
        OnAwake();
    }

    private void Start()
    {
        OnStart();
    }

    private void OnEnable()
    {
        Enable();
    }

    private void OnDisable()
    {
        Disable();
    }

    private void Update()
    {
        OnUpdate();
    }

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    public virtual void OnAwake() { }
    public virtual void OnStart() { }
    public virtual void Enable() { }
    public virtual void Disable() { }
    public virtual void OnUpdate() { }
    public virtual void OnFixedUpdate() { }

    public virtual void Clear(bool @true)
    {
        
    }
}
    