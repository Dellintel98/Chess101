﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessManager : MonoBehaviour
{
    [SerializeField] public ChessBoard board;

    // Start is called before the first frame update
    void Start()
    {
        board.GenerateBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
