using System;

[Serializable]
public class CardStats
{
    public int MaxHealth = -1;
    private int health;

    public int Range;
    public int Damage;
    public int Speed;

    public int Health
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
        }
    }
}
