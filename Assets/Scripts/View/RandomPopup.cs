using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomPopup : MonoBehaviour
{
    [SerializeField] private Sprite[] cardSprites;

    [SerializeField] private Image cardImage;

    private int random;

    private void OnEnable()
    {
        random = Random.Range(0, 3);
        
        if(cardImage != null && cardSprites.Length != 0)
        {
            cardImage.sprite = cardSprites[random];
            StartCoroutine(WaitForSave());
        }
    }

    private IEnumerator WaitForSave()
    {
        //yield return new WaitForSeconds(3);

        WebServerUtility.E3Data.card_index_3 = random + 8;

        yield return StartCoroutine(WebServerUtility.E3Post(WebServerUtility.E3Data.userInfo));

        BaseManager.Instance.ActiveView = ViewKind.Finish;
    }
}
