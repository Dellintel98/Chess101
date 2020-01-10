using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] GameObject squarePrefab;

    public Square[,] board = new Square[8, 8];

    // Start is called before the first frame update
    void Start()
    {
        GenerateBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateBoard()
    {
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                board[i, j] = GenerateSquare((float)(i*2), (float)(j*2)).GetComponent<Square>();
            }
        }
    }

    private GameObject GenerateSquare(float x, float y) 
    {
        GameObject square = Instantiate(squarePrefab, new Vector3(x-7, 7-y, 0f), Quaternion.identity, gameObject.transform);
        return square;
    }

}
