using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance;

    [NonSerialized]
    public ObjectGroup[] groups;

    private int index;

    private void Awake()
    {
        Instance = FindObjectOfType<ObjectManager>(true);

        groups = GetComponentsInChildren<ObjectGroup>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            index = 0;
            Attemp();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            index = 1;
            Attemp();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            index = 2;
            Attemp();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            index = 3;
            Attemp();

        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            index = 4;
            Attemp();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            index = 5;
            Attemp();
        }
    }
    private void Attemp()
    {
        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].gameObject.SetActive(false);
        }

        groups[index].gameObject.SetActive(true);

        groups[index].TextureInitialize();

        WebServerData.characterType = ((CharacterType)index).ToString();
    }

    public void IntializeObject()
    {
        switch (WebServerData.characterType)
        {
            case "Girl_1": index = 0; break;
            case "Boy_2": index = 1; break;
            case "Girl_3": index = 2; break;
            case "Boy_4": index = 3; break;
            case "Girl_5": index = 4; break;
            case "Boy_6": index = 5; break;
        }

        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].gameObject.SetActive(false);
        }

        groups[index].gameObject.SetActive(true);

        groups[index].TextureInitialize();
    }

    public void AnimationInvoke()
    {
        groups[index].AnimationInvoke();
    }
}
