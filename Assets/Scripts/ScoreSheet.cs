using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class ScoreSheet : MonoBehaviour
{
    private List<string[]> scoreSheetText = new List<string[]>();
    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = "";
    }

    void UpdateScoreSheetTextComponent()
    {
        string tempString = "";

        foreach(string[] line in scoreSheetText)
        {
            tempString += (line[0] + line[1] + line[2] + line[3] + "\n");
        }

        transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = tempString;
    }

    public void SetNewMove(
        string originalPieceTag, string currentPieceTag, string originalSquare, string newSquare, int currentRound, 
        bool isItCheck, bool isItCheckmate, bool kingSideCastling, bool queenSideCastling, bool capturedAPiece,
        bool isPromoted, string winnerPlayerColorTag, bool isItADraw, bool hasGameEnded, bool isItResignation, bool isItADrawAgreement)
    {
        string playerTurnNotation = GeneratePlayerTurnNotation(originalPieceTag, currentPieceTag, originalSquare, newSquare, isItCheck, isItCheckmate, capturedAPiece, isPromoted);


        if (kingSideCastling)
            playerTurnNotation = "O-O";
        else if (queenSideCastling)
            playerTurnNotation = "O-O-O";


        if (scoreSheetText.Count == currentRound && !isItResignation && !isItADrawAgreement && currentPieceTag != "")
        {
            scoreSheetText[currentRound - 1][3] = playerTurnNotation;
        }
        else if (scoreSheetText.Count < currentRound && !isItResignation && !isItADrawAgreement && currentPieceTag != "")
        {
            string[] temp = { currentRound + ". ", playerTurnNotation, " ", ""};

            scoreSheetText.Add(temp);
        }

        if(hasGameEnded)
        {
            string[] temp1 = { "1-0", "", "", "" };
            string[] temp2 = { "0-1", "", "", "" };
            string[] temp3 = { "1/2-1/2", "", "", "" };

            if (winnerPlayerColorTag == "Light")
                scoreSheetText.Add(temp1);
            else if (winnerPlayerColorTag == "Dark")
                scoreSheetText.Add(temp2);
            else if (isItADraw)
                scoreSheetText.Add(temp3);
        }

        UpdateScoreSheetTextComponent();
    }

    private string GeneratePlayerTurnNotation(
        string originalPieceTag, string currentPieceTag, string originalSquare, string newSquare, bool isItCheck, bool isItCheckmate, bool capturedAPiece, bool isPromoted)
    {
        string playerTurnNotation = "";
        string captureString = (capturedAPiece) ? "x" : "";
        string checkString = (isItCheck) ? "+" : "";
        string endString = (isItCheckmate) ? "#" : checkString;

        string pieceTag = (isPromoted) ? originalPieceTag : currentPieceTag;
        string pieceTagAbbrevation;

        switch (currentPieceTag)
        {
            case "Knight":
                pieceTagAbbrevation = "N";
                break;
            case "Bishop":
                pieceTagAbbrevation = "B";
                break;
            case "Rook":
                pieceTagAbbrevation = "R";
                break;
            case "Queen":
                pieceTagAbbrevation = "Q";
                break;
            default:
                pieceTagAbbrevation = "";
                break;
        }

        switch (pieceTag)
        {
            case "Pawn":
                string startingSquare = (capturedAPiece) ? originalSquare : "";
                string promotedString = (isPromoted) ? "=" + pieceTagAbbrevation : "";
                playerTurnNotation = startingSquare + captureString + newSquare + promotedString + endString;
                break;
            case "Knight":
                playerTurnNotation = pieceTagAbbrevation + originalSquare + captureString + newSquare + endString;
                break;
            case "Bishop":
                playerTurnNotation = pieceTagAbbrevation + captureString + newSquare + endString;
                break;
            case "Rook":
                playerTurnNotation = pieceTagAbbrevation + originalSquare + captureString + newSquare + endString;
                break;
            case "Queen":
                playerTurnNotation = pieceTagAbbrevation + captureString + newSquare + endString;
                break;
            case "King":
                playerTurnNotation = "K" + captureString + newSquare;
                break;
        }

        return playerTurnNotation;
    }

    public bool CheckIsItAThreefoldRepetition(string originalPieceTag, string currentPieceTag, string originalSquare, 
        string newSquare, bool isItCheck, bool isItCheckmate, bool capturedAPiece, bool isPromoted)
    {
        if (scoreSheetText.Count < 5) return false;
        string newMoveNotation = GeneratePlayerTurnNotation(originalPieceTag, currentPieceTag, originalSquare, newSquare, isItCheck, isItCheckmate, capturedAPiece, isPromoted);

        List<string[]> last5Moves = new List<string[]>();

        for(int i = scoreSheetText.Count - 1; i >= scoreSheetText.Count - 5; --i)
        {
            string[] temp = { scoreSheetText[i][1], scoreSheetText[i][3] };
            last5Moves.Add(temp);
        }

        if(string.Join("", last5Moves[0]).Length < string.Join("", last5Moves[2]).Length)
        {
            if(CompareMoves(last5Moves[1], last5Moves[3]) && CompareMoves(last5Moves[2], last5Moves[4]))
            {
                last5Moves[0][1] = newMoveNotation;

                if (CompareMoves(last5Moves[0], last5Moves[2]))
                    return true;
            }
        }
        else if (string.Join("", last5Moves[0]).Length == string.Join("", last5Moves[2]).Length)
        {
            if(CompareTurns(last5Moves[0][0], last5Moves[1][1], last5Moves[2][0], last5Moves[3][1]) 
                && CompareTurns(last5Moves[1][0], last5Moves[2][1], last5Moves[3][0], last5Moves[4][1]))
            {
                if (CompareTurns(newMoveNotation, last5Moves[0][1], last5Moves[1][0], last5Moves[2][1]))
                    return true;
            }
        }

        return false;
    }

    private bool CompareMoves(string[] move1, string[] move2)
    {
        if (move1[0] == move2[0] && move1[1] == move2[1])
            return true;

        return false;
    }

    private bool CompareTurns(string turn1l, string turn1d, string turn2l, string turn2d)
    {
        if (turn1l == turn2l && turn1d == turn2d)
            return true;

        return false;
    }

    public void ExportGameAsPGN(string chessEvent, string gameSite, string gameDate, string gameRound, string gameResult, string light, string dark, string terminationCondition)
    {
        string eventString = chessEvent;
        string site = gameSite;
        string date = gameDate;
        string round = gameRound;
        string result = gameResult;
        string white = light;
        string black = dark;
        string termination = terminationCondition;

        if (result == "1-0")
            termination = white + " " + termination;
        else if (result == "0-1")
            termination = black + " " + termination;

        string pgnHeading = $"[Event \"{eventString}\"]\n[Site \"{site}\"]\n[Date \"{date}\"]\n[Round \"{round}\"]\n[Result \"{result}\"]\n[White \"{white}\"]\n[Black \"{black}\"]\n[Termination \"{termination}\"]\n";
        string pgnBody = "";

        int counter = 0;
        foreach (string[] line in scoreSheetText)
        {
            ++counter;
            pgnBody += (line[0] + line[1] + line[2] + line[3]);

            if (counter == 3)
            {
                pgnBody += "\n";
                counter = 0;
            }
            else
                pgnBody += " ";
        }

        string pgnString = pgnHeading + "\n" + pgnBody;

        string randomFileNameSalt = UnityEngine.Random.Range(0, 20000).ToString();
        string fileName = randomFileNameSalt + "_" + white + "_vs_" + black + "_" + date + ".pgn";

        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        Debug.Log($"path: <color=orange>{path}</color>");

        File.WriteAllText(path + "/" + fileName, pgnString);

        Tooltip.ShowTooltip_Static("Game has been saved to Desktop!", "#008080D9", tooltipFontSize: 32);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
