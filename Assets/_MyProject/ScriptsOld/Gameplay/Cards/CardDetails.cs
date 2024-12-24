using System;
using UnityEngine;

[Serializable]
public class CardDetails: CardDetailsBase
{
    [field: SerializeField] public CardStats Stats{ get; private set; }
    [field: SerializeField] public FactionSO Faction { get; private set; }
}
