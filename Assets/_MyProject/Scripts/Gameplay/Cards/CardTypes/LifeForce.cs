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
        player = IsMy ? GameplayManager.Instance.MyPlayer : GameplayManager.Instance.OpponentPlayer;
        Stats.UpdatedHealth += ShowHealth;
        ShowHealth();
    }

    private void OnDisable()
    {
        Stats.UpdatedHealth += ShowHealth;
    }

    private void ShowHealth()
    {
        lifeForceDisplay.text = Stats.Health.ToString(CultureInfo.InvariantCulture);
        if (Stats.Health==0 && !Risk.IsActive)
        {
            GameplayManager.Instance.StopGame(!player.IsMy);
        }
    }


}