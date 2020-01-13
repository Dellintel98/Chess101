using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessManager : MonoBehaviour
{
    [SerializeField] public GameObject chessSetPrefab;

    ChessBoard chessBoard;
    ChessGameplayManager game;
    private ChessSet[] chessSets = new ChessSet[2];

    // Start is called before the first frame update
    void Start()
    {
        chessBoard = FindObjectOfType<ChessBoard>();
        chessBoard.CreateNewBoard();

        CreateChessSets();

        game = FindObjectOfType<ChessGameplayManager>();
        game.InitializeGame(chessSets, chessBoard);
    }

    private void CreateChessSets()
    {
        string[] playerColorTag = new string[2] { "Dark", "Light" };

        for(int i = 0; i < 2; i++)
        {
            GameObject newSet = Instantiate(chessSetPrefab, gameObject.transform);
            chessSets[i] = newSet.GetComponent<ChessSet>();
            chessSets[i].CreatePieceSet(chessBoard, playerColorTag[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
