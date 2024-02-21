using System;
using UnityEngine;

[Serializable]
public class AudioSound
{
    [field: SerializeField] public string Key;
    [field: SerializeField] public AudioClip AudioClip;
    [field: SerializeField] public float Volume;
}