using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class RoomGameplayPlayer
{
    private int actionsLeft;
    public string PlayerId;
    public int LootChange;
    public int StrangeMatter;
    public int AmountOfAbilitiesPlayerCanBuy;

    public int ActionsLeft
    {
        get
        {
            return actionsLeft;
        }
        set
        {
            Debug.Log("Setting actions to: "+value+ " for:"+IsMy);
            actionsLeft = value;
        }
    }

    [JsonIgnore] public bool IsMy => PlayerId == FirebaseManager.Instance.PlayerId;
}
