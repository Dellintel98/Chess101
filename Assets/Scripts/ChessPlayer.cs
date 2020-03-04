using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPlayer : MonoBehaviour
{
    private int myIndex;
    private string mySetColorTag;
    private string myPlayerName;
    private ChessBoard myChessBoard;

    public void InitializePlayer(ChessBoard chessBoard, string chosenColor, int playerIndex, string playerName)
    {
        myIndex = playerIndex;
        mySetColorTag = chosenColor;
        myPlayerName = playerName;
        myChessBoard = chessBoard;

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
        if(state == "Active" || state == "Waiting" || state == "Won" || state == "Lost" || state == "Remi" || state == "Disqualified")
        {
            tag = state;
        }
        else
        {
            //staviti bacanje greške
        }
    }
}
