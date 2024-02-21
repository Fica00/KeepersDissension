using System;
using UnityEngine;

[Serializable]
public class CardStats
{
    [SerializeField] private float health;

    public Action UpdatedHealth;
    
    public int Range;
    public float Damage;

    public float Health
    {
        get => health;
        set
        {
            health = value;
            if (health<0)
            {
                health = 0;
            }
            UpdatedHealth?.Invoke();
        }
    }
}
