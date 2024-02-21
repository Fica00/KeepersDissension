using System;
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
                PhotonManager.Instance.JoinRandomRoom();
                PhotonManager.OnIJoinedRoom += JoinedRandomRoom;
                break;
            case MatchMode.Private:
                friendlyMatchUI.Activate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void JoinedRandomRoom()
    {
        PhotonManager.OnIJoinedRoom -= JoinedRandomRoom;
        FinishedSettingUpFriendlyMatch();
    }

    public void FinishedSettingUpFriendlyMatch()
    {
        searchingForOpponentPanel.Activate();
    }
}
