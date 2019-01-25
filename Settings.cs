using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameSettings")]
public class Settings : ScriptableObject
{
    [SerializeField]
    private float mashGrowSpeed = 0.01f;
    [SerializeField]
    private float mashScaleY = 0.25f;

    public float MashScaleY { get { return mashScaleY; } }
    public float MashGrowSpeed { get { return mashGrowSpeed; } }
}
