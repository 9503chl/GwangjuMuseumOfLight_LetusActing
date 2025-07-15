using Kamgam.UGUIWorldImage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance;

    [NonSerialized]
    public ObjectGroup[] groups;

    #region ÄÁÅÙÃ÷ Àü¿ë
    [SerializeField]
    private WorldImage[] worldImages;
    #endregion


    #region Ä¸ÃÄ Àü¿ë
    [SerializeField]
    private WorldImage PlayerImage;

    [SerializeField]
    private WorldImage AnimationImage;
    #endregion
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
            WebServerUtility.E3Data.characterType = "Girl_1";
            IntializeObject();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            index = 1;
            WebServerUtility.E3Data.characterType = "Boy_2";
            IntializeObject();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            index = 2;
            WebServerUtility.E3Data.characterType = "Girl_3";
            IntializeObject();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            index = 3;
            WebServerUtility.E3Data.characterType = "Boy_4";
            IntializeObject();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            index = 4;
            WebServerUtility.E3Data.characterType = "Girl_5";
            IntializeObject();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            index = 5;
            WebServerUtility.E3Data.characterType = "Boy_6";
            IntializeObject();
        }
    }


    public void IntializeObject()
    {
        switch (WebServerUtility.E3Data.characterType)
        {
            case "Girl_1": index = 0; break;
            case "Boy_2": index = 1; break;
            case "Girl_3": index = 2; break;
            case "Boy_4": index = 3; break;
            case "Girl_5": index = 4; break;
            case "Boy_6": index = 5; break;
        }

        Animator[] characterAnimators = groups[index]._Animators.ToArray();

        for (int i = 0; i < characterAnimators.Length; i++)
        {
            characterAnimators[i].gameObject.SetActive(false);
        }

        Transform tf = characterAnimators[WebServerUtility.captureIndex].transform;
        tf.gameObject.SetActive(true);

        PlayerImage.Clear();
        PlayerImage.AddWorldObject(groups[index].Saver.transform);

        AnimationImage.Clear();
        AnimationImage.AddWorldObject(tf);

        for (int i = 0; i < worldImages.Length; i++)
        {
            worldImages[i].Clear();
            worldImages[i].AddWorldObject(groups[index].Loaders[i].transform);
        }
    }

    public void TextureInitialize()
    {
        groups[index].TextureInitialize();
    }


    public void AnimationInvoke()
    {
        groups[index].AnimationInvoke();
    }

    public void Clear()
    {
        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].Clear();
        }
    } 
}
