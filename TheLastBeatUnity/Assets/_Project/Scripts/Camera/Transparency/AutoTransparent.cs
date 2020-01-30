using UnityEngine;

public class AutoTransparent : MonoBehaviour
{
    public float TargetTransparency { get; set; }
    public float FadeInTimeout { get; set; }
    public float FadeOutTimeout { get; set; }
    
    float transparency = 1.0f;
    bool shouldBeTransparent = true;
    Color currentColor = Color.white;

    public void BeTransparent()
    {
        shouldBeTransparent = true;
    }

    private void Update()
    {
        if (shouldBeTransparent)
        {
            if (transparency > TargetTransparency)
                transparency -= ((1.0f - TargetTransparency) * Time.deltaTime) / FadeOutTimeout;
        }
        else if (transparency < 1.0f)
            transparency += ((1.0f - TargetTransparency) * Time.deltaTime) / FadeInTimeout;

        SetAllMaterialsAlpha(transparency);

        shouldBeTransparent = false;
    }

    private void SetAllMaterialsAlpha(float alpha)
    {
        Material[] materialsList = GetComponent<Renderer>().materials;
        for (int i = 0; i < materialsList.Length; i++)
        {
            currentColor = materialsList[i].GetColor("_Color");
            currentColor.a = alpha;
            materialsList[i].color = currentColor;
        }
    }

    private void OnDestroy()
    {
        SetAllMaterialsAlpha(1);
    }
}