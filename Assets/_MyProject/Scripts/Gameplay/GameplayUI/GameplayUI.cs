using System;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI Instance;
    [SerializeField] private Button resignButton;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button unchainGuardianButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Image topBackground;
    [SerializeField] private Image botBackground;
    [SerializeField] private ActionAndTurnDisplay actionAndTurnDisplay;
    [SerializeField] private StrangeMatterTracker strangeMatterTracker;
    [SerializeField] private SettingsPanel settingsPanel;

    private void Awake()
    {
        Instance = this;
    }

    protected virtual void OnEnable()
    {
        resignButton.onClick.AddListener(Resign);
        endTurnButton.onClick.AddListener(EndTurn);
        unchainGuardianButton.onClick.AddListener(UnchainGuardian);
        settingsButton.onClick.AddListener(ShowSettings);
        GameplayManager.OnUnchainedGuardian += DisableUnchainGuardianButton;
    }

    protected virtual void OnDisable()
    {
        resignButton.onClick.RemoveListener(Resign);
        endTurnButton.onClick.RemoveListener(EndTurn);
        settingsButton.onClick.RemoveListener(ShowSettings);
        unchainGuardianButton.onClick.RemoveListener(UnchainGuardian);
        GameplayManager.OnUnchainedGuardian -= DisableUnchainGuardianButton;
    }

    private void ShowSettings()
    {
        settingsPanel.Setup();
    }
    
    private void Resign()
    {
        DialogsManager.Instance.ShowYesNoDialog("Are you sure that you want to resign?",YesResign);
    }

    private void YesResign()
    {
        GameplayManager.Instance.Resign();
    }

    private void EndTurn()
    {
        if (!GameplayManager.Instance.CanPlayerDoActions())
        {
            return;
        }
        
        GameplayManager.Instance.EndTurn();
    }

    private void UnchainGuardian()
    {
        GameplayManager.Instance.TryUnchainGuardian();
    }

    public void DisableUnchainGuardianButton()
    {
        unchainGuardianButton.interactable = false;
    }

    public void Setup()
    {
        SetupTableBackground();
        SetupActionAndTurnDisplay();
    }

    private void SetupTableBackground()
    {
        topBackground.sprite = GameplayManager.Instance.OpponentPlayer.FactionSo.Board;
        botBackground.sprite = GameplayManager.Instance.MyPlayer.FactionSo.Board;
    }

    private void SetupActionAndTurnDisplay()
    {
        actionAndTurnDisplay.Setup();
        strangeMatterTracker.Setup();
    }

    public virtual void ShowResult(bool _didIWin)
    {
        throw new Exception("Show result must be implemented");
    }

    private void Update()
    {
        if (GameplayManager.Instance.GetGameplayState() < GameplayState.Gameplay)
        {
            return;
        }

        resignButton.interactable = false;
        if (GameplayManager.Instance.IsResponseAction())
        {
            if (GameplayManager.Instance.IsMyResponseAction())
            {
                resignButton.interactable = true;
            }
        }
        else if (GameplayManager.Instance.IsMyTurn())
        {
            resignButton.interactable = true;
        }

        if (GameplayManager.Instance.IsRoomOwner())
        {
            if (GameplayManager.Instance.GetGameplaySubState() == GameplaySubState.Player1UseKeeperReposition)
            {
                resignButton.interactable = true;
            }
        }
        else
        {
            if (GameplayManager.Instance.GetGameplaySubState() == GameplaySubState.Player2UseKeeperReposition)
            {
                resignButton.interactable = true;
            }
        }
    }
}
