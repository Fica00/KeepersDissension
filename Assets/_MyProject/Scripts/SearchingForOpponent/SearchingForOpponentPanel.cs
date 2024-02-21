using System;
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
        // PhotonManager.OnILeftRoom += HideSearching;
        // PhotonManager.OnOpponentJoinedRoom += StartGameplay;
    }

    private void OnDisable()
    {
        cancelSearch.onClick.RemoveListener(CancelSearching);
        // PhotonManager.OnILeftRoom -= HideSearching;
        // PhotonManager.OnOpponentJoinedRoom -= StartGameplay;
    }

    private void ShowSearching()
    {        
        // cancelSearch.interactable = PhotonManager.IsMasterClient;
        cancelSearch.interactable = true;
        gameObject.SetActive(true);
    }

    private void CancelSearching()
    {
        cancelSearch.interactable = false;
        // PhotonManager.OnILeftRoom += LeftRoom;
        // PhotonManager.Instance.LeaveRoom();
    }

    private void LeftRoom()
    {
        // PhotonManager.OnILeftRoom -= LeftRoom;
        HideSearching();
    }

    private void HideSearching()
    {
        OnCanceledSearch?.Invoke();
        gameObject.SetActive(false);
    }

    private void StartGameplay()
    {
        cancelSearch.interactable = false;
        SceneManager.LoadGameplay();
    }
}
