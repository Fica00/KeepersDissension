using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HealthTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI myGuardianHealth;
    [SerializeField] private TextMeshProUGUI opponentGuardianHealth;
    [SerializeField] private TextMeshProUGUI myKeeperHealth;
    [SerializeField] private TextMeshProUGUI opponentKeeperHealth;
    [SerializeField] private TextMeshProUGUI myLifeForceHealth;
    [SerializeField] private TextMeshProUGUI opponentLifeForceHealth;
    [SerializeField] private GameObject myUltimateIndicator;
    [SerializeField] private GameObject opponentUltimateIndicator;
    
    private Guardian myGuardian;
    private Guardian opponentGuardian;
    private Keeper myKeeper;
    private Keeper opponentKeeper;
    private LifeForce myLifeForce;
    private LifeForce opponentLifeForce;
    
    public void Setup()
    {
        myGuardian = GameplayManager.Instance.GetMyGuardian();
        opponentGuardian = GameplayManager.Instance.GetOpponentGuardian();
        myKeeper = GameplayManager.Instance.GetMyKeeper();
        opponentKeeper = GameplayManager.Instance.GetOpponentKeeper();
        myLifeForce = GameplayManager.Instance.GetMyLifeForce();
        opponentLifeForce = GameplayManager.Instance.GetOpponentsLifeForce();

        myGuardian.UpdatedHealth += ShowMyGuardianHealth;
        opponentGuardian.UpdatedHealth += ShowOpponentGuardianHealth;
        myKeeper.UpdatedHealth += ShowMyKeeperHealth;
        opponentKeeper.UpdatedHealth += ShowOpponentKeeperHealth;
        myLifeForce.UpdatedHealth += ShowMyLifeForceHealth;
        opponentLifeForce.UpdatedHealth += ShowOpponentLifeForceHealth;
    }

    private void OnDisable()
    {
        if (myGuardian==null)
        {
            return;
        }
        myGuardian.UpdatedHealth -= ShowMyGuardianHealth;
        opponentGuardian.UpdatedHealth += ShowOpponentGuardianHealth;
        myKeeper.UpdatedHealth -= ShowMyKeeperHealth;
        opponentKeeper.UpdatedHealth -= ShowOpponentKeeperHealth;
        myLifeForce.UpdatedHealth -= ShowMyLifeForceHealth;
        opponentLifeForce.UpdatedHealth -= ShowOpponentLifeForceHealth;
    }

    private void ShowMyGuardianHealth()
    {
        myGuardianHealth.text = myGuardian.Health.ToString();
    }

    private void ShowOpponentGuardianHealth()
    {
        opponentGuardianHealth.text = opponentGuardian.Health.ToString();
    }

    private void ShowMyKeeperHealth()
    {
        myKeeperHealth.text = myKeeper.Health.ToString();
    }

    private void ShowOpponentKeeperHealth()
    {
        opponentKeeperHealth.text = opponentKeeper.Health.ToString();
    }

    private void ShowMyLifeForceHealth()
    {
        myLifeForceHealth.text = myLifeForce.Health.ToString();
    }

    private void ShowOpponentLifeForceHealth()
    {
        opponentLifeForceHealth.text = opponentLifeForce.Health.ToString();
    }

    private void Update()
    {
        if (myKeeper!=null)
        {
            myUltimateIndicator.SetActive(myKeeper.SpecialAbilities[0].CanUseAbility);
        }

        if (opponentKeeper!=null)
        {
            opponentUltimateIndicator.SetActive(opponentKeeper.SpecialAbilities[0].CanUseAbility);
        }
    }
}
