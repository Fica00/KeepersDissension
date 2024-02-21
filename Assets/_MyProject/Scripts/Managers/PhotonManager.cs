using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;
using UnityEngine;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;
    public static Action OnFinishedInit;
    public static Action OnIJoinedRoom;
    public static Action OnOpponentJoinedRoom;
    public static Action OnILeftRoom;
    public static Action OnOpponentLeftRoom;

    public const string FACTION_ID = "ClubName";
    public const string MATCHES_PLAYED = "MatchesPlayed";
    public const string DATE_CREATED = "DateCreated";
    public const string DEBUG_ROOM = "DebugRoom";

    private const byte MAX_PLAYERS_PER_ROOM = 2;

    public static bool IsMasterClient => PhotonNetwork.IsMasterClient;
    private int GetDebuggingValue => MatchMakingHandler.Instance.Mode == MatchMode.Debug ? 1 : -1;
    public bool IsTestingRoom => (int)PhotonNetwork.CurrentRoom.CustomProperties[DEBUG_ROOM]==1;
    private string roomName;

    public string CurrentRoomName
    {
        get
        {
            string _roomName = string.Empty;
            if (PhotonNetwork.CurrentRoom!=null)
            {
                _roomName = PhotonNetwork.CurrentRoom.Name;
            }

            return _roomName;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            PhotonNetwork.AutomaticallySyncScene = true;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Init()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        OnFinishedInit?.Invoke();
    }

    public void JoinFriendlyRoom(string _roomName)
    {
        roomName = _roomName;
        SetPhotonPlayerProperties();
        PhotonNetwork.JoinRoom(_roomName);
    }

    public void JoinRandomRoom()
    {
        roomName = string.Empty;
        SetPhotonPlayerProperties();
        Hashtable _expectedCustomRoomProperties = new Hashtable { { DEBUG_ROOM, GetDebuggingValue } };
        PhotonNetwork.JoinRandomRoom(_expectedCustomRoomProperties, MAX_PLAYERS_PER_ROOM);
    }

    private void SetPhotonPlayerProperties()
    {
        Hashtable _myProperties = new Hashtable
        {
            [FACTION_ID] = DataManager.Instance.PlayerData.FactionId,
            [MATCHES_PLAYED] = DataManager.Instance.PlayerData.MatchesPlayed,
            [DATE_CREATED] = DateUtilities.Convert(DataManager.Instance.PlayerData.DateCreated)
        };
        PhotonNetwork.LocalPlayer.CustomProperties = _myProperties;
    }

    public static string GetProperty(string _key, bool _isMy)
    {
        Hashtable _properties = new Hashtable();
        foreach (var _player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (_isMy&&Equals(_player, PhotonNetwork.LocalPlayer))
            {
                _properties = _player.CustomProperties;
                break;
            }
            
            if (!_isMy&&!Equals(_player, PhotonNetwork.LocalPlayer))
            {
                _properties = _player.CustomProperties;
                break;
            }
        }

        return _properties[_key].ToString();
    }

    public override void OnJoinRandomFailed(short _returnCode, string _message)
    {
        CreateRoom();
    }

    public override void OnJoinRoomFailed(short _returnCode, string _message)
    {
        CreateRoom();
    }

    private void CreateRoom()
    {
        string[] _lobbyOptions = new string[1];
        _lobbyOptions[0] = DEBUG_ROOM;
        RoomOptions _roomOptions = new RoomOptions
        {
            IsOpen = true,
            MaxPlayers = MAX_PLAYERS_PER_ROOM,
            IsVisible = string.IsNullOrEmpty(roomName),
            CustomRoomPropertiesForLobby = _lobbyOptions,
            CustomRoomProperties = new Hashtable { { DEBUG_ROOM,  GetDebuggingValue}}
        };

        PhotonNetwork.CreateRoom(string.IsNullOrEmpty(roomName)?null: roomName, _roomOptions, TypedLobby.Default);
    }


    public override void OnCreateRoomFailed(short _returnCode, string _message)
    {
        Debug.Log(6);
        if (!string.IsNullOrEmpty(roomName))
        {
            roomName += "" + UnityEngine.Random.Range(0, 1000);
        }
        CreateRoom();
    }

    public override void OnJoinedRoom()
    {
        OnIJoinedRoom?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player _newPlayer)
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        OnOpponentJoinedRoom?.Invoke();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        OnILeftRoom?.Invoke();
    }

    public override void OnPlayerLeftRoom(Player _otherPlayer)
    {
        OnOpponentLeftRoom?.Invoke();
    }
}
