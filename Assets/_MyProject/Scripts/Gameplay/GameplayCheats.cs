using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameplayCheats : MonoBehaviour
{
    public static bool UnlimitedActions;
    public static bool CheckForCd;
    
    [SerializeField] private Toggle unlimitedActions;
    [SerializeField] private Toggle checkForCD;
    [SerializeField] private Button healKeeperButton;
    [SerializeField] private Button healGuardianButton;
    [SerializeField] private Button showButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button addStrangeMatter;
    [SerializeField] private Transform holder;

    private void Awake()
    {
        UnlimitedActions = false;
        CheckForCd = true;
        gameObject.SetActive(FirebaseManager.Instance.RoomHandler.IsTestingRoom);
    }

    private void OnEnable()
    {
        unlimitedActions.onValueChanged.AddListener(ToggleUnlimitedActions);
        checkForCD.onValueChanged.AddListener(ToggleCheckForCd);
        healGuardianButton.onClick.AddListener(HealGuardian);
        healKeeperButton.onClick.AddListener(HealKeeper);
        showButton.onClick.AddListener(Show);
        closeButton.onClick.AddListener(Close);
        addStrangeMatter.onClick.AddListener(AddStrangeMatter);
    }

    private void OnDisable()
    {
        unlimitedActions.onValueChanged.RemoveListener(ToggleUnlimitedActions);
        checkForCD.onValueChanged.RemoveListener(ToggleCheckForCd);
        healGuardianButton.onClick.RemoveListener(HealGuardian);
        healKeeperButton.onClick.RemoveListener(HealKeeper);
        showButton.onClick.RemoveListener(Show);
        closeButton.onClick.RemoveListener(Close);
        addStrangeMatter.onClick.RemoveListener(AddStrangeMatter);
    }

    private void AddStrangeMatter()
    {
        GameplayManager.Instance.ChangeMyStrangeMatter(20);
    }

    private void ToggleUnlimitedActions(bool _status)
    {
        UnlimitedActions = _status;
        if (UnlimitedActions)
        {
            GameplayManager.Instance.MyPlayer.Actions = 3;
        }
    }    
    
    private void ToggleCheckForCd(bool _status)
    {
        CheckForCd = _status;
    }

    private void HealGuardian()
    {
        Guardian _guardian = FindObjectsOfType<Guardian>().ToList().Find(_guardian => _guardian.My);
        if (_guardian==null)
        {
            return;
        }

        _guardian.ChangeHealth(10);
    }

    private void HealKeeper()
    {
        Keeper _keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        if (_keeper==null)
        {
            return;
        }

        _keeper.ChangeHealth(10);
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
