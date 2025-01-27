using System.Collections;
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
        StartCoroutine(ShowHealthRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator ShowHealthRoutine()
    {
        while (gameObject.activeSelf)
        {
            myGuardianHealth.text = myGuardian.Health.ToString();
            opponentGuardianHealth.text = opponentGuardian.Health.ToString();
            myKeeperHealth.text = myKeeper.Health.ToString();
            opponentKeeperHealth.text = opponentKeeper.Health.ToString();
            myLifeForceHealth.text = myLifeForce.Health.ToString();
            opponentLifeForceHealth.text = opponentLifeForce.Health.ToString();
            if (myKeeper!=null)
            {
                myUltimateIndicator.SetActive(myKeeper.SpecialAbilities[0].CanUseAbility);
            }

            if (opponentKeeper!=null)
            {
                opponentUltimateIndicator.SetActive(opponentKeeper.SpecialAbilities[0].CanUseAbility);
            }
            yield return new WaitForSeconds(1);
        }
    }
}