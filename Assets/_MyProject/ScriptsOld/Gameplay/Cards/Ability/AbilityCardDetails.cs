using System;
using System.Collections.Generic;

[Serializable]
public class AbilityCardDetails: CardDetailsBase
{
    public new AbilityCardType Type;
    public AbilityColor Color;
    public bool CanBeGivenToPlayer;
}
