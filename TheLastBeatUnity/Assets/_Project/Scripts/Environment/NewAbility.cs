using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NewAbility : MonoBehaviour
{
    [SerializeField]
    GameObject prefab = null;

    [SerializeField]
    GameObject prompt = null;

    bool done = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().OnOk += GetNewAbility;
            if (!done)
                prompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().OnOk -= GetNewAbility;
            if (!done)
                prompt.SetActive(false);
        }
    }

    void GetNewAbility()
    {
        if (done)
            return;

        InputDelegate.Instance.Inputable = null;
        prompt.SetActive(false);
        done = true;
        DOTween.Sequence()
            .AppendCallback(() => Destroy(Instantiate(prefab, transform), 4))
            .InsertCallback(3, () => InputDelegate.Instance.ObtainAbility());
    }
}
