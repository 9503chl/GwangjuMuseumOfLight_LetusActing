using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomPopup : MonoBehaviour
{
    [SerializeField] private Sprite[] cardSprites;

    [SerializeField] private Image cardImage;

    [SerializeField] private GameObject[] soundObjects;

    [SerializeField] private GameObject animationTarget;

    private int random;

    private void Awake()
    {
        animationTarget.SetActive(false);
    }

    private void OnEnable()
    {
        random = Random.Range(0, 3);

        animationTarget.SetActive(true);


        for (int i = 0; i < soundObjects.Length; i++)
        {
            soundObjects[i].SetActive(false);
        }

        if (cardImage != null && cardSprites.Length != 0)
        {
            cardImage.sprite = cardSprites[random];
            StartCoroutine(WaitForSave());
        }
    }

    private IEnumerator WaitForSave()
    {
        WebServerUtility.E3Data.card_index_3 = random + 8;

        yield return StartCoroutine(WebServerUtility.E3Post(WebServerUtility.E3Data.userInfo));

        yield return new WaitForSeconds(3f);

        soundObjects[random].SetActive(true);

        yield return new WaitForSeconds(6.5f);

        BaseManager.Instance.ActiveView = ViewKind.Finish;
    }

    private void OnDisable()
    {
        animationTarget.SetActive(false);
    }
}
