using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPlayer : MonoBehaviour
{
    private string playerColorTag;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializePlayer(string chosenTag)
    {
        playerColorTag = chosenTag;
    }
}
