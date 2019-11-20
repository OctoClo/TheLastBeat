using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PulseZone : ScriptableObject { 
    public float Length;
    public AnimationCurve ModifierInZone;
    public Color colorRepr;
    [Range(0, 10)]
    public float ScaleModifier;
}
