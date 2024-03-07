using UnityEngine;

public class BlockaderCard : CardSpecialAbility
{
    [HideInInspector] public bool CanBlock;

    private void Start()
    {
        Player.OnStartedTurn += ResetAbilities;
        CanBlock = true;
    }

    private void OnDisable()
    {
        Player.OnStartedTurn -= ResetAbilities;
    }

    private void ResetAbilities()
    {
        GameplayManager.Instance.ManageBlockaderAbility(true);
    }
}
