using UnityEngine;

public class BlockaderCard : CardSpecialAbility
{
    public static bool IgnoreSending;
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
        if (IgnoreSending)
        {
            IgnoreSending = false;
            return;
        }
        GameplayManager.Instance.ManageBlockaderAbility(true);
    }
}
