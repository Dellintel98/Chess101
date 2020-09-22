using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChessPlayer : MonoBehaviour
{
    private int myIndex;
    private string mySetColorTag;
    private string myPlayerName;
    private ChessGameplayManager myGame;
    private ChessPlayer myEnemy;
    private ChessPiece myLastMovedPiece;
    private GameObject myChessClockAndPlayerTextObject;
    private bool hasGameStarted;
    private float remainingTime;
    private float incrementTime;
    private string myGameType;
    private Color32 flagedColor = new Color32(0xF1, 0x50, 0x25, 0xFF); //#fe7b72  #F15025

    public void InitializePlayer(string chosenColor, int playerIndex, string playerName, GameObject clockObject, float clockTimeValue, float increment, string gameType)
    {
        myIndex = playerIndex;
        mySetColorTag = chosenColor;
        myPlayerName = playerName;
        myLastMovedPiece = null;
        hasGameStarted = false;
        myGameType = gameType;
        remainingTime = clockTimeValue;
        incrementTime = increment;
        myChessClockAndPlayerTextObject = clockObject;

        TimeSpan currentTime = TimeSpan.FromSeconds(remainingTime);
        if (gameType == "Unlimited")
            myChessClockAndPlayerTextObject.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = "--:--";
        else
            myChessClockAndPlayerTextObject.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = currentTime.ToString("mm':'ss");

        myChessClockAndPlayerTextObject.transform.GetChild(1).gameObject.GetComponent<Text>().text = playerName;


        transform.name = $"{mySetColorTag} Colored Player - {myPlayerName}";

        if(mySetColorTag == "Light")
        {
            tag = "Active";
        }
        else
        {
            tag = "Waiting";
        }
    }

    public void SetMyEnemy(ChessPlayer enemyPlayer)
    {
        myEnemy = enemyPlayer;
    }

    public void SetMyGame(ChessGameplayManager theGame)
    {
        myGame = theGame;
    }

    public string GetMyChosenColor()
    {
        return mySetColorTag;
    }

    public int GetPlayerIndex()
    {
        return myIndex;
    }

    public string GetMyState()
    {
        return tag;
    }

    public void SetMyState(string state)
    {
        if(state == "Active" || state == "Waiting" || state == "Won" || state == "Lost" || state == "Remi")
        {
            tag = state;
        }
    }

    public ChessPiece GetLastMovedPiece()
    {
        return myLastMovedPiece;
    }

    public void SetLastMovedPiece(ChessPiece piece)
    {
        myLastMovedPiece = piece;
    }

    public bool HasGameStarted()
    {
        return hasGameStarted;
    }

    public void GameHasStarted()
    {
        hasGameStarted = true;
    }

    public string GetMyEnemyColorTag()
    {
        return myEnemy.GetMyChosenColor();
    }

    private bool IsItADrawByTimeoutVsInsufficientMaterial()
    {
        ChessSet mySet = myLastMovedPiece.GetMyChessSet();
        ChessSet myEnemySet = mySet.GetMyEnemyChessSet();

        return myGame.IsItADeadPosition(mySet, myEnemySet, true);
    }

    public void AddTimeIncrement()
    {
        if (myGame.HasGameEnded()) return;
        if (incrementTime == 0)
            return;

        remainingTime += incrementTime;

        TimeSpan currentTime = TimeSpan.FromSeconds(remainingTime);

        if (remainingTime <= 5.0f)
            myChessClockAndPlayerTextObject.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = currentTime.ToString("mm':'ss'.'fff");
        else if (remainingTime <= 20.0f)
            myChessClockAndPlayerTextObject.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = currentTime.ToString("mm':'ss'.'ff");
        else
            myChessClockAndPlayerTextObject.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = currentTime.ToString("mm':'ss");
    }

    // Update is called once per frame
    void Update()
    {
        if (myGame.HasGameEnded()) return;
        if (mySetColorTag == "Light" && !hasGameStarted) return;
        if (tag != "Active") return;
        if (myGameType == "Unlimited")
            return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0.0f && myEnemy.tag == "Waiting")
        {
            hasGameStarted = false;
            remainingTime = 0.0f;

            bool isADraw = IsItADrawByTimeoutVsInsufficientMaterial();

            if(isADraw)
            {
                tag = "Remi";
                myEnemy.tag = "Remi";
                Debug.Log($"Game over: {mySetColorTag} colored player flagged! But it is a draw by timeout vs insufficient material!");
            }
            else
            {
                tag = "Lost";
                myEnemy.tag = "Won";

                ChessGameplayManager manager = FindObjectOfType<ChessGameplayManager>();
                manager.terminationString = "won on time";
                Debug.Log($"Game over: {mySetColorTag} colored player flagged! {myEnemy.GetMyChosenColor()} colored player won!");
            }
            
            myChessClockAndPlayerTextObject.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().color = flagedColor;

            string winner = (isADraw) ? "None" : myEnemy.GetMyChosenColor();
            myGame.SetFlaggingEndScore(winner, isADraw);
        }

        TimeSpan currentTime = TimeSpan.FromSeconds(remainingTime);

        if(remainingTime <= 5.0f)
            myChessClockAndPlayerTextObject.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = currentTime.ToString("mm':'ss'.'fff");
        else if(remainingTime <= 20.0f)
            myChessClockAndPlayerTextObject.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = currentTime.ToString("mm':'ss'.'ff");
        else
            myChessClockAndPlayerTextObject.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>().text = currentTime.ToString("mm':'ss");

    }
}
