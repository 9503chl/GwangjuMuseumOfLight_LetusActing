using System;
using UnityEngine;

[Serializable]
public class DrawTrailObject : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer = null;

    [SerializeField]
    private ParticleSystem trailParticle = null;

    [SerializeField]
    [Range(0f, 60f)]
    private float timeToAlive = 30f;
    public float TimeToAlive
    {
        get
        {
            return timeToAlive;
        }
        set
        {
            if (trailParticle != null)
            {
                ParticleSystem.MainModule mainModule = trailParticle.main;
                mainModule.startLifetimeMultiplier = value;
            }
            timeToAlive = value;
            elspsedTime = 0f;
        }
    }

    [SerializeField]
    [Range(0f, 60f)]
    private float TimeToDisappear = 3f;

    [NonSerialized]
    private float elspsedTime = 0f;

    private void OnEnable()
    {
        if (trailParticle != null)
        {
            ParticleSystem.MainModule mainModule = trailParticle.main;
            mainModule.startLifetimeMultiplier = timeToAlive;
        }
    }

    private void Update()
    {
        elspsedTime += Time.unscaledDeltaTime;
        if (elspsedTime > timeToAlive + TimeToDisappear)
        {
            Destroy(gameObject);
        }
    }

    public void BeginDraw(Vector3 position)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        elspsedTime = 0f;
        transform.position = position;
        gameObject.SetActive(true);
    }

    public void DrawTo(Vector3 position)
    {
        elspsedTime = 0f;
        transform.position = position;
    }

    public void EndDraw()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }
}
