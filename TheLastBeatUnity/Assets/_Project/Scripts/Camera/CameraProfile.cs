using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraProfile : ScriptableObject
{
    public float DistanceToViewer { get; set; }
    public float Angle { get; set; }
    public float FOV { get; set; }
}
