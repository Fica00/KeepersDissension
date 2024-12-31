using System;
using System.Collections.Generic;

[Serializable]
public class WarriorAbilityData
{
    public bool CanBlock;
    public List<string> EffectedCards = new();
    public List<BomberData> BomberData = new();
    public StealthData StealthData = new();
    public bool CanUseAbility;
}
