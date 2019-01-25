using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameSettings")]
public class Settings : ScriptableObject
{
    [Header("Mash settings")]
    [SerializeField]
    private float mashGrowSpeed = 0.01f;
    [SerializeField]
    private float mashScaleY = 0.25f;
    [SerializeField]
    private float minMashScale = 0.1f;
    [SerializeField]
    private float maxMashScale = 1.0f;

    [Header("Perfect Move settings")]
    [SerializeField]
    private float maxDeviation = 0.05f;

    [Header("Wave settings")]
    [SerializeField]
    private float currentMashScaleToAdd = 0.4f;
    [SerializeField]
    private float currentMashScaleToSubstract = 0.2f;
    [SerializeField]
    private float otherMashesScaleToAdd = 0.3f;
    [SerializeField]
    private float otherMashesScaleToMultiplyBy = 0.8f;

    #region MashSettings
    public float MashScaleY { get { return mashScaleY; } }
    public float MashGrowSpeed { get { return mashGrowSpeed; } }
    public float MinMashScale { get { return minMashScale; } }
    public float MaxMashScale { get { return maxMashScale; } }
    #endregion
    #region PerfectMoveSettings
    public float MaxDeviation { get { return maxDeviation; } }
    #endregion
    #region WaveSettings
    public float CurrentMashScaleToAdd { get { return currentMashScaleToAdd; } }
    public float CurrentMashScaleToSubstract { get { return currentMashScaleToSubstract; } }
    public float OtherMashesScaleToAdd { get { return otherMashesScaleToAdd; } }
    public float OtherMashesScaleToMultiplyBy { get { return otherMashesScaleToMultiplyBy; } }
    #endregion

}
