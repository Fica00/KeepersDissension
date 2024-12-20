using System.Collections;
using FirebaseMultiplayer.Room;
using UnityEngine;

public class RoomUpdater : MonoBehaviour
{
    private RoomData roomData = new();
    
    private void Start()
    {
        StartCoroutine(UpdateRoomData());
    }

    private IEnumerator UpdateRoomData()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(5);
            
            if (!GameplayManager.Instance.MyTurn)
            {
                Debug.Log("Not my turn");
                continue;
            }

            if (FirebaseManager.Instance.RoomHandler.RoomData == roomData)
            {
                Debug.Log("Data is the same");
                continue;
            }
            
            Debug.Log("Updating room data");
            roomData = FirebaseManager.Instance.RoomHandler.RoomData;
            FirebaseManager.Instance.RoomHandler.UpdateRoomData();
        }
    }
}
