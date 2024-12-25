using System;
using System.Collections.Generic;

[Serializable]
public class WarriorAbilityData
{
    public bool CanBlock;
    public List<string> EffectedCards = new();
    public bool CanUseAbility;
}
