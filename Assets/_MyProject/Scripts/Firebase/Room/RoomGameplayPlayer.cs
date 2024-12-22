using System;
using Newtonsoft.Json;

[Serializable]
public class RoomGameplayPlayer
{
    public string PlayerId;
    public int LootChange;
    public int StrangeMatter;
    public int AmountOfAbilitiesPlayerCanBuy;

    public int ActionsLeft;

    [JsonIgnore] public bool IsMy => PlayerId == FirebaseManager.Instance.PlayerId;
}
