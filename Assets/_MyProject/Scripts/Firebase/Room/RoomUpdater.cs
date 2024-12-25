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
        FirebaseManager.Instance.RoomHandler.UpdateRoomData();
    }
}