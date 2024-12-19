using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EconomyPanelHandler : MonoBehaviour
{
    public static EconomyPanelHandler Instance;
    [SerializeField] private ArrowPanel arrowPanel;
    [SerializeField] private GameObject holder;
    [SerializeField] private EconomyStrangeMatterDisplay matterPrefab;
    [SerializeField] private Transform matterHolder;
    [SerializeField] private Transform myEconomyTarget;
    [SerializeField] private Transform opponentEconomyTarget;

    private List<GameObject> shownObjects = new ();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        arrowPanel.OnOpened += Show;
    }

    private void OnDisable()
    {
        arrowPanel.OnOpened -= Show;
    }

    private void Show()
    {
        ClearShowObjects();
        
        for (int _i = 0; _i < GameplayManager.Instance.StrangeMaterInEconomy(); _i++)
        {
            EconomyStrangeMatterDisplay _matter = Instantiate(matterPrefab, matterHolder);
            _matter.Setup();
            shownObjects.Add(_matter.gameObject);
        }
        
        holder.SetActive(true);
    }

    private void ClearShowObjects()
    {
        foreach (var _shownObject in shownObjects)
        {
            Destroy(_shownObject);
        }
        
        shownObjects.Clear();
    }

    public void ShowBoughtMatter(bool _didIBuy, Vector3 _position = default)
    {
        Show();
        Transform _target = _didIBuy ? myEconomyTarget : opponentEconomyTarget;
        EconomyStrangeMatterDisplay _economyDisplay = Instantiate(matterPrefab, transform);
        _economyDisplay.Setup(false);
        _economyDisplay.transform.localPosition = new Vector3(-Screen.width, 0, 0);
        if (_position!=default)
        {
            _economyDisplay.transform.position = _position;
        }
        _economyDisplay.transform.DOMove(_target.position, 1f).OnComplete(() =>
        {
            Destroy(_economyDisplay.gameObject);
            if (!_didIBuy)
            {
                return;
            }
            
            GameplayManager.Instance.ChangeMyStrangeMatter(1);
        });
    }
}
