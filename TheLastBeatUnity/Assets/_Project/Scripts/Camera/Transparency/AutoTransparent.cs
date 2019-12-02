using UnityEngine;

public class AutoTransparent : MonoBehaviour
{
    public Material TransparentMaterial { get; set; }
    public float TargetTransparency { get; set; }
    public float FadeInTimeout { get; set; }
    public float FadeOutTimeout { get; set; }
    
    Material[] oldMaterials = null;
    float transparency = 1.0f;
    bool shouldBeTransparent = true;

    private void Start()
    {
        if (oldMaterials == null)
        {
            oldMaterials = GetComponent<Renderer>().materials;

            Material[] materialsList = new Material[oldMaterials.Length];

            for (int i = 0; i < materialsList.Length; i++)
            {
                // replace material with transparent
                materialsList[i] = Object.Instantiate(TransparentMaterial);
                materialsList[i].SetColor("_Color", oldMaterials[i].GetColor("_Color"));
            }

            // make transparent
            GetComponent<Renderer>().materials = materialsList;
        }
    }

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

        Material[] materialsList = GetComponent<Renderer>().materials;
        for (int i = 0; i < materialsList.Length; i++)
        {
            Color C = oldMaterials[i].GetColor("_Color");

            C.a = transparency;
            materialsList[i].color = C;
        }

        shouldBeTransparent = false;
    }

    private void OnDestroy()
    {
        GetComponent<Renderer>().materials = oldMaterials;
    }
}