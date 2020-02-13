using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AbilityContainer : MonoBehaviour
{
    [SerializeField]
    RectTransform iconTransform = null;

    [SerializeField]
    Image backgroundImage;

    [SerializeField]
    Sprite enabledSprite;

    Image imgComponent;

    public enum AbilityBehavior
    {
        BLINK,
        CDA
    }

    [SerializeField]
    AbilityBehavior behavior;

    public delegate void NumberUpdate(float number);
    public NumberUpdate UpdateDelegate = null;

    [SerializeField]
    GameObject prefabAbility = null;
    Color tempColor = Color.white;

    private void Awake()
    {
        imgComponent = iconTransform.GetComponent<Image>();
        if (behavior == AbilityBehavior.BLINK)
        {
            UpdateDelegate = BlinkBehavior;
        }
        else
        {
            UpdateDelegate = CDABehavior;
            UpdateDelegate(0);
        }
    }

    public void UpdateProgression(float number)
    {
        tempColor = imgComponent.color;
        UpdateDelegate(number);
    }

    void CDABehavior(float number)
    {
        if (number < 4)
        {
            imgComponent.DOColor(new Color(tempColor.r, tempColor.g, tempColor.b, 0.6f), 0.15f);
            iconTransform.DOScale(Vector3.one, 0.15f);
            if (number == 0)
                backgroundImage.DOColor(Color.clear, 0.2f);
        }
        else
        {
            imgComponent.DOColor(new Color(tempColor.r, tempColor.g, tempColor.b, 1.0f), 0.15f);
            iconTransform.DOScale(Vector3.one * 1.2f, 0.15f);
            if (number == 4)
                AbilityAvailable();
        }
    }

    void BlinkBehavior(float number)
    {
        if (number < 1)
        {
            iconTransform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.0f, number);
            imgComponent.color = new Color(tempColor.r, tempColor.g, tempColor.b, Mathf.Lerp(0.1f , 0.5f , number));
            if (number == 0)
                backgroundImage.DOColor(Color.clear, 0.2f);
        }
        else
        {
            AbilityAvailable();

            DOTween.Sequence()
                .Append(iconTransform.DOScale(Vector3.one * 1.2f, 0.15f))
                .Append(iconTransform.DOScale(Vector3.one, 0.15f))
                .Insert(0, imgComponent.DOColor(new Color(tempColor.r, tempColor.g, tempColor.b, 1), 0.3f));
        }
    }

    void AbilityAvailable()
    {
        GameObject instantiated = Instantiate(prefabAbility, transform);

        DOTween.Sequence()
            .AppendInterval(0.1f)
            .Append(instantiated.GetComponent<Image>().DOColor(new Color(0.0f, 0.0f, 0.0f, 0.0f), 0.3f))
            .AppendCallback(() => Destroy(instantiated))
            .Insert(0, backgroundImage.DOColor(Color.white, 0.2f));      
    }

    public void AbilityEarned()
    {
        if (enabledSprite != null)
        {
            backgroundImage.gameObject.SetActive(true);
        }
    }
}
