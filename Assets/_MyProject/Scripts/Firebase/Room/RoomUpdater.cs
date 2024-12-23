using System.Collections;
using FirebaseMultiplayer.Room;
using UnityEngine;

public class RoomUpdater : MonoBehaviour
{
    public static RoomUpdater Instance;
    
    private RoomData roomData = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(UpdateRoomData());
    }

    private IEnumerator UpdateRoomData()
    {
        yield break;
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(5);
            
            if (!GameplayManager.Instance.IsMyTurn())
            {
                continue;
            }

            if (FirebaseManager.Instance.RoomHandler.RoomData == roomData)
            {
                continue;
            }

            ForceUpdate();
        }
    }

    public void ForceUpdate()
    {
        Debug.Log("Updating data");
        roomData = FirebaseManager.Instance.RoomHandler.RoomData;
        FirebaseManager.Instance.RoomHandler.UpdateRoomData();
    }
}