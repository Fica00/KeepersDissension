using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendlyMatchUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_InputField roomNameInput;

    public void Activate()
    {
        ManageInteractables(true);
        roomNameInput.text = string.Empty;
        gameObject.SetActive(true);
    }
    
    private void OnEnable()
    {
        continueButton.onClick.AddListener(PlayFriendlyGame);
    }

    private void OnDisable()
    {
        continueButton.onClick.RemoveListener(PlayFriendlyGame);
    }

    private void PlayFriendlyGame()
    {
        string _roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(_roomName))
        {
            UIManager.Instance.ShowOkDialog("Please enter room name");
            return;
        }
        PhotonManager.OnIJoinedRoom += ShowRoomName;
        PhotonManager.Instance.JoinFriendlyRoom(_roomName);
        ManageInteractables(false);
    }

    private void ShowRoomName()
    {
        PhotonManager.OnIJoinedRoom -= ShowRoomName;
        UIManager.Instance.ShowOkDialog("Room successfully created, room name: "+PhotonManager.Instance.CurrentRoomName);
        MatchMakingHandler.Instance.FinishedSettingUpFriendlyMatch();
        gameObject.SetActive(false);
    }

    private void ManageInteractables(bool _status)
    {
        continueButton.interactable = _status;
        roomNameInput.interactable = _status;
    }
}
