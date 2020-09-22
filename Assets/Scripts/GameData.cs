using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public string[] playerColorTags = new string[2];  // 0-player1 -- 1-player2
    public string[] pieceSetColors = new string[2];  // 0-light -- 1-dark
    public string[] boardThemeColors = new string[2];  // 0-light -- 1-dark
    public float timeControl;
    public float incrementValue;
    public SpritePieceSet pieceSpriteSet = new SpritePieceSet();
    public string gameType;
}
