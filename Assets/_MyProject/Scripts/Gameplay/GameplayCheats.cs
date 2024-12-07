using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameplayCheats : MonoBehaviour
{
    public static bool UnlimitedActions;
    public static bool CheckForCD;
    public static bool HasUnlimitedGold;
    
    [SerializeField] private Toggle unlimitedActions;
    [SerializeField] private Toggle checkForCD;
    [SerializeField] private Toggle hasUnlimitedGold;
    [SerializeField] private Button healKeeperButton;
    [SerializeField] private Button healGuardianButton;
    [SerializeField] private Button showButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform holder;

    private void Awake()
    {
        UnlimitedActions = false;
        CheckForCD = true;
        HasUnlimitedGold = false;
        gameObject.SetActive(FirebaseManager.Instance.RoomHandler.IsTestingRoom);
    }

    private void OnEnable()
    {
        hasUnlimitedGold.onValueChanged.AddListener(ToggleUnlimitedGold);
        unlimitedActions.onValueChanged.AddListener(ToggleUnlimitedActions);
        checkForCD.onValueChanged.AddListener(ToggleCheckForCD);
        healGuardianButton.onClick.AddListener(HealGuardian);
        healKeeperButton.onClick.AddListener(HealKeeper);
        showButton.onClick.AddListener(Show);
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        hasUnlimitedGold.onValueChanged.RemoveListener(ToggleUnlimitedGold);
        unlimitedActions.onValueChanged.RemoveListener(ToggleUnlimitedActions);
        checkForCD.onValueChanged.RemoveListener(ToggleCheckForCD);
        healGuardianButton.onClick.RemoveListener(HealGuardian);
        healKeeperButton.onClick.RemoveListener(HealKeeper);
        showButton.onClick.RemoveListener(Show);
        closeButton.onClick.RemoveListener(Close);
    }

    private void ToggleUnlimitedGold(bool _status)
    {
        HasUnlimitedGold = _status;
    }

    private void ToggleUnlimitedActions(bool _status)
    {
        UnlimitedActions = _status;
        if (UnlimitedActions)
        {
            GameplayManager.Instance.MyPlayer.Actions = 3;
        }
    }    
    
    private void ToggleCheckForCD(bool _status)
    {
        CheckForCD = _status;
    }

    private void HealGuardian()
    {
        Guardian _guardian = FindObjectsOfType<Guardian>().ToList().Find(_guardian => _guardian.My);
        if (_guardian==null)
        {
            return;
        }

        _guardian.Heal(10);
    }

    private void HealKeeper()
    {
        Keeper _keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        if (_keeper==null)
        {
            return;
        }

        _keeper.Heal(10);
    }

    private void Show()
    {
        holder.DOScale(Vector3.one, 0.3f);
    }

    private void Close()
    {
        holder.DOScale(new Vector3(1,0,1), 0.3f);
    }
}
