using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class StrangeMatterAnimation
{
    public string Id;
    public int Amount;
    public bool ForMe;
    public float X;
    public float Y;
    public float Z;


    [JsonIgnore]
    public Vector3 PositionOfDefendingCard => new(X,Y,Z);
}
