using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraProfile : ScriptableObject
{
    public float DistanceToViewer;
    public float Angle;
    public float FOV;
    public float TimeToIn;
    public AnimationCurve curve;
}
