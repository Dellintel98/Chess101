using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    [SerializeField] public GameObject squarePrefab;

    public Square[,] board = new Square[8, 8];

    // Update is called once per frame
    private void Update()
    {
        
    }

    public void CreateNewBoard()
    {
        for (int row = 7; row >= 0; row--)
        {
            for (int column = 0; column < 8; column++)
            {
                float x = (column * 2) - 7;
                float y = 7 - (row * 2);

                GenerateBoardSquare(row, column, x, y);
            }
        }
    }

    private void GenerateBoardSquare(int row, int column, float x, float y)
    {
        GameObject square = Instantiate(squarePrefab, new Vector3(x, y, 0f), Quaternion.identity, gameObject.transform);
        board[row, column] = square.GetComponent<Square>();

        board[row, column].Setup(row, column);
    }

    public Square GetSquareByVector3(Vector3 position)
    {
        foreach(Square square in board)
        {
            if(square.transform.position == position)
            {
                return square;
            }
        }

        return null;
    }
}
