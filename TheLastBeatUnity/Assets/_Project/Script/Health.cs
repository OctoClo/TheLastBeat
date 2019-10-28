using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Health : MonoBehaviour
{
    [SerializeField]
    float timeBetweenBeats;

    [SerializeField]
    float durationBeat;

    Vector2 anchorMin;
    Vector2 anchorMax;

    [SerializeField]
    Vector2 newAnchorMin;

    [SerializeField]
    Vector2 newAnchorMax;

    int numberBeat = 0;

    [SerializeField]
    Image img;

    [SerializeField]
    Text txt;

    public void Start()
    {
        anchorMin = img.GetComponent<RectTransform>().anchorMin;
        anchorMax = img.GetComponent<RectTransform>().anchorMax;
        //Just for proto
        DOTween.Init();
        StartCoroutine(Beat());
    }

    IEnumerator Beat()
    {
        while (true)
        {
            Sequence seqMin = DOTween.Sequence();
            Sequence seqMax = DOTween.Sequence();

            seqMin.Append(img.GetComponent<RectTransform>().DOAnchorMin(newAnchorMin, durationBeat / 2));
            seqMin.Append(img.GetComponent<RectTransform>().DOAnchorMin(anchorMin, durationBeat / 2));
            seqMin.Play();

            seqMax.Append(img.GetComponent<RectTransform>().DOAnchorMax(newAnchorMax, durationBeat / 2));
            seqMax.Append(img.GetComponent<RectTransform>().DOAnchorMax(anchorMax, durationBeat / 2));
            seqMax.Play();

            numberBeat++;
            txt.text = numberBeat.ToString();

            yield return new WaitForSeconds(timeBetweenBeats);
        }
    }
}
