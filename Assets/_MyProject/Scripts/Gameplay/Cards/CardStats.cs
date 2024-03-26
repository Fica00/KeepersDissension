using System;
using UnityEngine;

[Serializable]
public class CardStats
{
    [HideInInspector] public int MaxHealth = -1;
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

            if (health>MaxHealth && MaxHealth!=-1)
            {
                health = MaxHealth;
            }
            UpdatedHealth?.Invoke();
        }
    }
}
