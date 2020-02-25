using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessManager : MonoBehaviour
{
    [SerializeField] public GameObject chessSetPrefab;
    [SerializeField] public GameObject chessPlayerPrefab;

    ChessBoard chessBoard;
    ChessGameplayManager game;
    private ChessSet[] chessSets = new ChessSet[2];
    private ChessPlayer[] chessPlayers = new ChessPlayer[2];

    // Start is called before the first frame update
    void Start()
    {
        string[] playerColorTag = new string[2] { "Light", "Dark" };
        string[] playerNames = new string[2] { "John", "Paul" };

        chessBoard = FindObjectOfType<ChessBoard>();
        chessBoard.CreateNewBoard();

        CreateChessPlayers(playerColorTag, playerNames);
        CreateChessSets();

        game = FindObjectOfType<ChessGameplayManager>();
        game.InitializeGame(chessSets, chessBoard, chessPlayers);
    }

    private void CreateChessPlayers(string[] playerColorTag, string[] playerNames)
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject newPlayer = Instantiate(chessPlayerPrefab, gameObject.transform);
            chessPlayers[i] = newPlayer.GetComponent<ChessPlayer>();
            chessPlayers[i].InitializePlayer(chessBoard, playerColorTag[i], i, playerNames[i]);
        }
    }

    private void CreateChessSets()
    {
        for(int i = 0; i < 2; i++)
        {
            GameObject newSet = Instantiate(chessSetPrefab, chessPlayers[i].gameObject.transform);
            chessSets[i] = newSet.GetComponent<ChessSet>();
            chessSets[i].CreatePieceSet(chessBoard, chessPlayers[i], i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
