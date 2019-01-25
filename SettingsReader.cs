using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsReader : MonoBehaviour
{
    private static SettingsReader instance;
    public static SettingsReader Instance { get { return instance; } }

    private void Awake()
    {
        instance = this;
    }

    [SerializeField]
    private Settings settings;
    public Settings Settings { get { return settings; } } 
}
