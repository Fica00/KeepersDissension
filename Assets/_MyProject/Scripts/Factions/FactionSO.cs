using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFaction", menuName = "ScriptableObject/Faction")]
public class FactionSO : ScriptableObject
{
    public int Id;
    public string Name;
    public Sprite NameSprite;
    public Sprite Board;
    public Sprite CardBackground;
    public Sprite BombSprite;

    public bool IsSnow => Id == 0;
    public bool IsCyber => Id == 1;
    public bool IsDragon => Id == 2;
    public bool IsForest => Id == 3;

    private static List<FactionSO> allFactions = new();
    
    public static void Init()
    {
        allFactions = Resources.LoadAll<FactionSO>("Factions").ToList();
    }

    public static FactionSO Get(int _id)
    {
        FactionSO _faction = allFactions.Find(_faction => _faction.Id == _id);
        if (_faction==null)
        {
            throw new Exception("Cant find faction with id: " + _id);
        }

        return _faction;
    }
}
