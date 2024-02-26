using System;
using System.Collections.Generic;
using FirebaseGameplay.Responses;
using FirebaseMultiplayer.Room;
using UnityEngine;

public class MatchMakingHandler : MonoBehaviour
{
    public static MatchMakingHandler Instance;

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameModeHandler gameModeHandler;
    [SerializeField] private GameObject chooseFaction;
    [SerializeField] private SearchingForOpponentPanel searchingForOpponentPanel;
    [SerializeField] private FriendlyMatchUI friendlyMatchUI;
    
    private MatchMode mode;
    public MatchMode Mode => mode;

    private void OnEnable()
    {
        SearchingForOpponentPanel.OnCanceledSearch += ShowMainMenu;
    }

    private void OnDisable()
    {
        SearchingForOpponentPanel.OnCanceledSearch -= ShowMainMenu;
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
    }

    private void Awake()
    {
        Instance = this;
    }

    public void Setup()
    {
        gameModeHandler.Setup(OnModeSelected);
    }

    private void OnModeSelected(MatchMode _mode)
    {
        gameModeHandler.Close();
        if (_mode==MatchMode.None)
        {
            ShowMainMenu();
            return;
        }
        mode = _mode;
        chooseFaction.SetActive(true);
        ChooseFaction.OnChosenFaction += OnSelectedFaction;
    }

    private void OnSelectedFaction()
    {
        ChooseFaction.OnChosenFaction -= OnSelectedFaction;
        chooseFaction.SetActive(false);
        if (DataManager.Instance.PlayerData.FactionId==-1)
        {
            ShowMainMenu();
            return;
        }
        switch (mode)
        {
            case MatchMode.Debug:
            case MatchMode.Normal:
                RoomPlayer _playerData = new RoomPlayer
                {
                    Id = FirebaseManager.Instance.Authentication.UserId,
                    FactionId = DataManager.Instance.PlayerData.FactionId,
                    DateCrated = DataManager.Instance.PlayerData.DateCreated
                };
                FirebaseManager.Instance.RoomHandler.JoinRandomRoom(_playerData,HandleJoinRandomRoom);
                break;
            case MatchMode.Private:
                friendlyMatchUI.Activate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleJoinRandomRoom(JoinRoom _response)
    {
        if (_response.Success)
        {
            FinishedSettingUpFriendlyMatch();
        }
        else
        {
            string _roomName = "Test room";
            RoomData _roomData = new RoomData
            {
                Name =_roomName,
                Id = Guid.NewGuid().ToString(),
                Type = RoomType.Normal,
                Status = RoomStatus.SearchingForOpponent,
                Owner = FirebaseManager.Instance.Authentication.UserId,
                RoomPlayers = new List<RoomPlayer>
                {
                    new (){ Id = FirebaseManager.Instance.Authentication.UserId, FactionId = DataManager.Instance.PlayerData.FactionId, DateCrated = 
                    DataManager.Instance.PlayerData.DateCreated }
                }
            };
            FirebaseManager.Instance.RoomHandler.CreateRoom(_roomData, HandeCreateRoom);
        }
    }

    private void HandeCreateRoom(CreateRoom _response)
    {
        if (_response.Success)
        {
            FinishedSettingUpFriendlyMatch();
        }
        else
        {
            Debug.Log("Failed to create room!");
        }
    }
    public void FinishedSettingUpFriendlyMatch()
    {
        searchingForOpponentPanel.Activate();
        FirebaseManager.Instance.RoomHandler.SubscribeToRoom();
        if (FirebaseManager.Instance.RoomHandler.RoomData.RoomPlayers.Count==2)
        {
            StartGameplay();
        }
        else
        {
            RoomHandler.OnPlayerJoined += PlayerJoined;
        }
    }

    private void PlayerJoined(RoomPlayer _)
    {
        StartGameplay();
    }


    private void StartGameplay()
    {
        SceneManager.LoadGameplay();
    }
}
