using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessManager : MonoBehaviour
{
    [SerializeField] public GameObject chessSetPrefab;
    [SerializeField] public GameObject chessPlayerPrefab;
    [SerializeField] public GameObject[] chessClockAndPlayerPrefab = new GameObject[2];

    ChessBoard chessBoard;
    ChessGameplayManager game;
    private ChessSet[] chessSets = new ChessSet[2];
    private ChessPlayer[] chessPlayers = new ChessPlayer[2];
    private Color32[] pieceColors = new Color32[2];
    private Color32[] squareColors = new Color32[2];
    private Player currentPlayers = new Player();
    private GameData gameData = new GameData();

    // Start is called before the first frame update
    void Start()
    {
        LoadGlobalData();

        string[] playerNames = new string[2] { currentPlayers.players[0].playerName, currentPlayers.players[1].playerName };

        SetSquareColorSet(gameData.boardThemeColors[0], gameData.boardThemeColors[1]);
        chessBoard = FindObjectOfType<ChessBoard>();
        chessBoard.CreateNewBoard(squareColors);
        
        CreateChessPlayers(gameData.playerColorTags, playerNames, gameData.timeControl, gameData.incrementValue, gameData.gameType);
        CreateChessSets(gameData.pieceSetColors, gameData.pieceSpriteSet);

        game = FindObjectOfType<ChessGameplayManager>();
        game.InitializeGame(chessSets, chessBoard, chessPlayers);
    }

    private void CreateChessPlayers(string[] playerColorTag, string[] playerNames, float clockTimeValue, float increment, string gameType)
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject newPlayer = Instantiate(chessPlayerPrefab, gameObject.transform);
            chessPlayers[i] = newPlayer.GetComponent<ChessPlayer>();

            GameObject clockObject;
            if (playerColorTag[i] == "Dark")
                clockObject = chessClockAndPlayerPrefab[1];
            else
                clockObject = chessClockAndPlayerPrefab[0];

            chessPlayers[i].InitializePlayer(playerColorTag[i], i, playerNames[i], clockObject, clockTimeValue, increment, gameType);
        }

        chessPlayers[0].SetMyEnemy(chessPlayers[1]);
        chessPlayers[1].SetMyEnemy(chessPlayers[0]);
    }

    private void CreateChessSets(string[] hexSetColors, SpritePieceSet spriteSet)
    {
        for(int i = 0; i < 2; i++)
        {
            GameObject newSet = Instantiate(chessSetPrefab, chessPlayers[i].gameObject.transform);
            chessSets[i] = newSet.GetComponent<ChessSet>();
        }

        SetPieceColorSet(hexSetColors[0], hexSetColors[1]);

        for (int i = 0; i < 2; i++)
        {
            chessSets[i].CreatePieceSet(chessBoard, chessPlayers[i], i, chessSets[1 - i], pieceColors, spriteSet);
        }
    }

    private void SetPieceColorSet(string lightPlayerColor, string darkPlayerColor)
    {
        pieceColors[0] = HexToColor32(lightPlayerColor);
        pieceColors[1] = HexToColor32(darkPlayerColor);
    }

    private void SetSquareColorSet(string lightSquareColor, string darkSquareColor)
    {
        squareColors[0] = HexToColor32(lightSquareColor);
        squareColors[1] = HexToColor32(darkSquareColor);
    }

    private Color32 HexToColor32(string colorInHex)
    {
        string hex = colorInHex.Replace("#", "");

        byte a = 255;
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    private void LoadGlobalData()
    {
        currentPlayers.players.AddRange(ChessGlobalControl.Instance.savedCurrentPlayers.players);
        gameData = ChessGlobalControl.Instance.savedGameData;
    }

    private void SetSceneIndex()
    {
        ChessGlobalControl.Instance.lastSceneIndex = 2;
    }

    public void SaveGlobalData()
    {
        UpdatePlayerData();
        SetSceneIndex();
        ChessGlobalControl.Instance.savedCurrentPlayers.players.Clear();
        ChessGlobalControl.Instance.savedCurrentPlayers.players.AddRange(currentPlayers.players);
        ChessGlobalControl.Instance.SaveCurrentPlayers();
    }

    public string[] ExportGameInPGNFormat()
    {
        string chessEvent = "Chess 101 Game";
        string site = "Local PC";

        DateTime utcDate = DateTime.UtcNow;
        string date = utcDate.Date.ToString("yyyy.MM.dd");

        string round = "?";
        string result = "";
        if (chessPlayers[0].GetMyState() == "Won" && chessPlayers[0].GetMyChosenColor() == "Light" || chessPlayers[1].GetMyState() == "Won" && chessPlayers[1].GetMyChosenColor() == "Light")
            result = "1-0";
        else if (chessPlayers[0].GetMyState() == "Lost" && chessPlayers[0].GetMyChosenColor() == "Light" || chessPlayers[1].GetMyState() == "Lost" && chessPlayers[1].GetMyChosenColor() == "Light")
            result = "0-1";
        else if (chessPlayers[0].GetMyState() == "Remi")
            result = "1/2-1/2";
        else
            return null;

        string white = (chessPlayers[0].GetMyChosenColor() == "Light") ? currentPlayers.players[0].playerName : currentPlayers.players[1].playerName;
        string black = (chessPlayers[0].GetMyChosenColor() == "Dark") ? currentPlayers.players[0].playerName : currentPlayers.players[1].playerName;

        return new string[7] { chessEvent, site, date, round, result, white, black };
    }

    private void UpdatePlayerData()
    {
        for (int i = 0; i < 2; ++i)
        {
            if (chessPlayers[i].tag == "Active" || chessPlayers[i].tag == "Waiting")
                continue;

            float[] unlimited = new float[4];
            float[] bullet = new float[4];
            float[] blitz = new float[4];
            float[] rapid = new float[4];
            float[] custom = new float[4];
            float[] light = new float[4];
            float[] dark = new float[4];

            float wins = (chessPlayers[i].tag == "Won") ? 1 : 0;
            float draws = (chessPlayers[i].tag == "Remi") ? 1 : 0;
            float losses = (chessPlayers[i].tag == "Lost") ? 1 : 0;
            float points = wins + draws * 0.5f;

            float[] temp = { wins, draws, losses, points };

            for (int j = 0; j < 4; ++j)
            {

                switch(gameData.gameType)
                {
                    case "Unlimited":
                        currentPlayers.players[i].unlimited[j] = unlimited[j] + temp[j];
                        break;
                    case "Bullet":
                        currentPlayers.players[i].bullet[j] = bullet[j] + temp[j];
                        break;
                    case "Blitz":
                        currentPlayers.players[i].blitz[j] = blitz[j] + temp[j];
                        break;
                    case "Rapid":
                        currentPlayers.players[i].rapid[j] = rapid[j] + temp[j];
                        break;
                    case "Custom":
                        currentPlayers.players[i].custom[j] = custom[j] + temp[j];
                        break;
                }

                switch(gameData.playerColorTags[i])
                {
                    case "Light":
                        currentPlayers.players[i].light[j] = light[j] + temp[j];
                        break;
                    case "Dark":
                        currentPlayers.players[i].dark[j] = dark[j] + temp[j];
                        break;
                }
            }
        }
    }
}
