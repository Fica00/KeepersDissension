using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShowAbilityCardsToBuyHand : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(ShowAbilities);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(ShowAbilities);
    }

    private void ShowAbilities()
    {
        if (GameplayManager.Instance.GameState == GameplayState.UsingSpecialAbility)
        {
            return;
        }
        
        if (GameplayManager.Instance.IsSettingUpTable)
        {
            return;
        }
        GameplayPlayer _player = FindObjectsOfType<GameplayPlayer>().ToList().Find(_player => _player.IsMy);
        _player.ShowCards(CardType.Ability);
    }

}
