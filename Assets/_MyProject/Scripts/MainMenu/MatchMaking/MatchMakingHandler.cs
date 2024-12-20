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

    private Action friendlyMatchCreateCallBack;

    private MatchMode mode;

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
        if (_mode == MatchMode.None)
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
        if (DataManager.Instance.PlayerData.FactionId == -1)
        {
            ShowMainMenu();
            return;
        }

        switch (mode)
        {
            case MatchMode.Private:
                friendlyMatchUI.Activate();
                return;
        }

        JoinRoom();
    }

    public void JoinRoom(Action _callBack = null, string _name = "")
    {
        friendlyMatchCreateCallBack = _callBack;
        RoomPlayer _playerData = new RoomPlayer
        {
            Id = FirebaseManager.Instance.Authentication.UserId,
            FactionId = DataManager.Instance.PlayerData.FactionId,
            DateCrated = DataManager.Instance.PlayerData.DateCreated,
            MatchesPlayed = DataManager.Instance.PlayerData.MatchesPlayed,
            Name = DataManager.Instance.PlayerData.Name
        };

        FirebaseManager.Instance.RoomHandler.JoinRoom(_playerData, MatchModeToRoomType(mode), HandleJoinRoom, _name: _name);
    }

    RoomType MatchModeToRoomType(MatchMode _mode)
    {
        switch (_mode)
        {
            case MatchMode.Normal:
                return RoomType.Normal;
            case MatchMode.Private:
                return RoomType.Friendly;
            case MatchMode.Debug:
                return RoomType.Debug;
            default:
                throw new ArgumentOutOfRangeException(nameof(_mode), _mode, null);
        }
    }

    private void HandleJoinRoom(NewJoinRoom _response)
    {
        if (_response.Success)
        {
            FinishSettingUpMatch();
        }
        else
        {
            RoomData _roomData = new RoomData
            {
                Name = _response.Name,
                Id = Guid.NewGuid().ToString(),
                Type = _response.Type,
                Status = RoomStatus.SearchingForOpponent,
                Owner = FirebaseManager.Instance.Authentication.UserId,
                RoomPlayers = new List<RoomPlayer>
                {
                    new()
                    {
                        Id = FirebaseManager.Instance.Authentication.UserId,
                        FactionId = DataManager.Instance.PlayerData.FactionId,
                        DateCrated = DataManager.Instance.PlayerData.DateCreated,
                        MatchesPlayed = DataManager.Instance.PlayerData.MatchesPlayed
                    }
                }
            };

            FirebaseManager.Instance.RoomHandler.CreateRoom(_roomData, HandeCreateRoom);
            DataManager.Instance.PlayerData.CurrentRoomId = _roomData.Id;
        }
    }

    private void HandeCreateRoom(NewCreateRoom _response)
    {
        if (_response.Success)
        {
            FinishSettingUpMatch();
        }
        else
        {
            Debug.Log("Failed to create room!");
        }
    }

    private void FinishSettingUpMatch()
    {
        searchingForOpponentPanel.Activate();
        if (FirebaseManager.Instance.RoomHandler.RoomData.RoomPlayers.Count == 2)
        {
            StartGameplay();
        }
        else
        {
            friendlyMatchCreateCallBack?.Invoke();
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