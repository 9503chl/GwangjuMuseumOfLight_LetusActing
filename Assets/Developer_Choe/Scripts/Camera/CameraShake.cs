using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    
    [SerializeField] private float ShakeAmount;

    private float shakeTime;

    private Vector3 initalPosition;

    public void Vibration(float time)
    {
        shakeTime = time;
        StartCoroutine(Updater());
    }

    private void Awake()
    {
        Instance = this;
        initalPosition = transform.position;
    }
    private IEnumerator Updater()
    {
        while (shakeTime > 0)
        {
            transform.position = Random.insideUnitSphere * ShakeAmount + initalPosition;
            yield return new WaitForSeconds(Time.deltaTime);
            shakeTime -= Time.deltaTime;
        }
        transform.position = initalPosition;
    }
}
