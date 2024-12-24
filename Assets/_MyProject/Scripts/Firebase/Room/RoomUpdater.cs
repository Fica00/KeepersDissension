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
        Debug.Log("Updating room data");
        FirebaseManager.Instance.RoomHandler.UpdateRoomData();
    }
}