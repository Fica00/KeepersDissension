using UnityEngine;

public class RoomUpdater : MonoBehaviour
{
    public static RoomUpdater Instance;
    
    private void Awake()
    {
        Instance = this;
    }

    public void ForceUpdate()
    {
        Debug.Log("Amount of abilities: "+ FirebaseManager.Instance.RoomHandler.BoardData.Abilities.Count);
        FirebaseManager.Instance.RoomHandler.UpdateRoomData();
    }
}