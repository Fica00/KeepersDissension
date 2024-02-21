using System;
using System.Collections.Generic;

[Serializable]
public class AbilityCardDetails: CardDetailsBase
{
    public new AbilityCardType Type;
    public List<AbilityEffectsType> Effects;
    public AbilityColor Color;
    public bool CanBeGivenToPlayer;
}
