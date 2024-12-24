using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StrangeMatterTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI myStrangeMatter;
    [SerializeField] private TextMeshProUGUI opponentsStrangeMatter;
    [SerializeField] private TextMeshProUGUI strangeMatterInEconomy;

    [SerializeField] private Button buyStrangeMatter;
    [SerializeField] private ArrowPanel economyPanel;

    public void Setup()
    {
        StartCoroutine(ShowStrangeMatter());
    }

    private IEnumerator ShowStrangeMatter()
    {
        while (gameObject.activeSelf)
        {
            myStrangeMatter.text = GameplayManager.Instance.MyStrangeMatter().ToString();
            opponentsStrangeMatter.text = GameplayManager.Instance.OpponentsStrangeMatter().ToString();
            strangeMatterInEconomy.text = GameplayManager.Instance.StrangeMaterInEconomy().ToString();
            yield return new WaitForSeconds(1);
        }
    }

    private void OnEnable()
    {
        buyStrangeMatter.onClick.AddListener(BuyStrangeMatter);
    }

    private void OnDisable()
    {
        buyStrangeMatter.onClick.RemoveListener(BuyStrangeMatter);
        StopAllCoroutines();
    }

    private void BuyStrangeMatter()
    {
        economyPanel.Show();
    }
}