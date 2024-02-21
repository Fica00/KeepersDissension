using System;
using UnityEngine;

[Serializable]
public class StrangeMatter
{
    public Action UpdatedAmountInEconomy;
    
    [SerializeField] private int amountInEconomy;

    public int Value;

    public int AmountInEconomy
    {
        get => amountInEconomy;
        set
        {
            amountInEconomy = value;
            UpdatedAmountInEconomy?.Invoke();
        }
    }

    public void SetAmountInEconomyWithoutNotify(int _amount)
    {
        amountInEconomy = _amount;
    }
}
