using System;
using System.Collections.Generic;

[Serializable]
public class GameplayData
{
    public int WhiteMatterInReserves;
    public List<GameplayPlayerData> PlayersData = new ();
}
