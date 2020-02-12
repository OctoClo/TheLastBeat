using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MonolithCheckpoint : MonoBehaviour
{
    [SerializeField]
    GameObject savingVfx = null;
    [SerializeField]
    float vfxOffsetY = 3;

    [SerializeField]
    GameObject savingIcon = null;
    Image savingImage = null;

    [SerializeField] [Range(0, 360)]
    float rotation = 15;

    [SerializeField]
    int savingDurationBeats = 3;

    [SerializeField]
    float appearDuration = 0.5f;
    [SerializeField]
    float fadeDuration = 0.5f;

    bool displaySavingIcon = false;
    Rock[] rocks = null;

    private void Start()
    {
        if (savingIcon != null)
            savingImage = savingIcon.GetComponent<Image>();
        rocks = GetComponentsInChildren<Rock>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (savingIcon != null)
            {
                displaySavingIcon = true;
                savingIcon.SetActive(true);

                foreach (Rock rock in rocks)
                    rock.ChangeState(ERockState.ILLUMINATE);

                GameObject.Instantiate(savingVfx, transform.position + new Vector3(0, vfxOffsetY, 0), Quaternion.identity, transform);

                DOTween.Sequence()
                    .Append(savingImage.DOFade(1, appearDuration))
                    .InsertCallback(savingDurationBeats * SoundManager.Instance.TimePerBeat, () =>
                    {
                        Debug.Log("Saving done.");
                        displaySavingIcon = false;
                        foreach (Rock rock in rocks)
                            rock.ChangeState(ERockState.PULSE_ON_BEAT);
                        DOTween.Sequence().Append(savingImage.DOFade(0, fadeDuration)).AppendCallback(() => savingIcon.SetActive(false));
                    });

                Destroy(GetComponent<Collider>());
            }
        }
    }

    private void Update()
    {
        if (displaySavingIcon)
        {
            savingIcon.transform.Rotate(0, 0, -rotation * Time.deltaTime);
        }
    }
}
