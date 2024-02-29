using System;
using FirebaseMultiplayer.Room;
using UnityEngine;
using UnityEngine.UI;

public class SearchingForOpponentPanel : MonoBehaviour
{
    public static Action OnCanceledSearch;
    [SerializeField] private Button cancelSearch;

    public void Activate()
    {
        ShowSearching();
    }
    
    private void OnEnable()
    {
        cancelSearch.onClick.AddListener(CancelSearching);
        RoomHandler.OnILeftRoom += HideSearching;
    }

    private void OnDisable()
    {
        cancelSearch.onClick.RemoveListener(CancelSearching);
        RoomHandler.OnILeftRoom -= HideSearching;
    }

    private void ShowSearching()
    {        
        cancelSearch.interactable = FirebaseManager.Instance.RoomHandler.IsOwner;
        cancelSearch.interactable = true;
        gameObject.SetActive(true);
    }

    private void CancelSearching()
    {
        cancelSearch.interactable = false;
        FirebaseManager.Instance.RoomHandler.LeaveRoom();
    }

    private void HideSearching()
    {
        OnCanceledSearch?.Invoke();
        gameObject.SetActive(false);
    }
}
