using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerStatus : MonoBehaviour
{
    [TabGroup("Status")] [SerializeField]
    bool stunned => Stunned;
    public bool Stunned { get; private set; }

    [TabGroup("Status")]
    bool dashing => Dashing;
    public bool Dashing { get; private set; }
    public bool Blinking { get; private set; }

    [TabGroup("Stun")] [SerializeField]
    float stunDuration = 0.5f;
    float stunTimer = 0;
    [TabGroup("Stun")] [SerializeField]
    Color stunColor = Color.blue;
    Color normalColor = Color.white;
    [TabGroup("Stun")] [SerializeField]
    AK.Wwise.Event stunMusicSXF;

    Material material = null;

    private void Start()
    {
        //material = GetComponent<MeshRenderer>().material;
        //normalColor = material.color;
    }

    public void Stun()
    {
        Stunned = true;
        //material.color = stunColor;
        stunMusicSXF.Post(gameObject);
        stunTimer = stunDuration;
    }

    public void StartDashing()
    {
        Dashing = true;
    }

    public void StartBlink()
    {
        Blinking = true;
    }

    public void StopBlink()
    {
        Blinking = false;
    }

    public void StopDashing()
    {
        Dashing = false;
    }

    private void Update()
    {
        if (stunned)
        {
            stunTimer -= Time.deltaTime / Time.timeScale;

            if (stunTimer <= 0)
            {
                Stunned = false;
                //material.color = normalColor;
            }
        }
    }
}
