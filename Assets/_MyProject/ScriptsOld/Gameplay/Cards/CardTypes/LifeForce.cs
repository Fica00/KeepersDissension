using System.Globalization;
using UnityEngine;
using TMPro;

public class LifeForce : Card
{
    [SerializeField] private TextMeshProUGUI lifeForceDisplay;

    private GameplayPlayer player;
    private Guardian guardian;

    protected override void Setup()
    {
        player = My ? GameplayManager.Instance.MyPlayer : GameplayManager.Instance.OpponentPlayer;
        UpdatedHealth += ShowHealth;
        ShowHealth();
    }

    private void OnDisable()
    {
        UpdatedHealth += ShowHealth;
    }

    private void ShowHealth()
    {
        lifeForceDisplay.text = Health.ToString(CultureInfo.InvariantCulture);
        if (Health==0 && !GameplayManager.Instance.IsAbilityActive<Risk>())
        {
            GameplayManager.Instance.EndGame(!player.IsMy);
        }
    }


}