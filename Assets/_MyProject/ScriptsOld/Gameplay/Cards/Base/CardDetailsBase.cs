using System;
using UnityEngine;

[Serializable]
public class CardDetailsBase
{
    [field: SerializeField] public int Id { get; set; }
    [field: SerializeField] public CardType Type{ get; private set; }
    [field: SerializeField] public Sprite Foreground { get; private set; }
    [field: SerializeField] public Sprite Background { get; private set; }
}
