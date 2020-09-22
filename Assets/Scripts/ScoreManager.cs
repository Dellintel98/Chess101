using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] public GameObject entryPrefab;
    [SerializeField] public GameObject linePrefab;

    private Player allPlayers = new Player();
    private List<GameObject> myPlayers = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        LoadAllPlayers();
        SetUpScore();
    }

    void Update()
    {
        //while(transform.childCount > 0)
        //{
        //    Transform entry = transform.GetChild(0);
        //    entry.parent = null;
        //    Destroy(entry.gameObject);
        //}

        //SetUpScore();
    }
    
    private void LoadAllPlayers()
    {
        allPlayers.players.AddRange(ChessGlobalControl.Instance.allPlayers.players);
    }

    private void SetUpScore()
    {
        foreach (PlayerData player in allPlayers.players)
        {
            GameObject playerEntry = Instantiate(entryPrefab, transform);

            float wins = player.unlimited[0] + player.bullet[0] + player.blitz[0] + player.rapid[0] + player.custom[0];
            float draws = player.unlimited[1] + player.bullet[1] + player.blitz[1] + player.rapid[1] + player.custom[1];
            float losses = player.unlimited[2] + player.bullet[2] + player.blitz[2] + player.rapid[2] + player.custom[2];
            float points = player.unlimited[3] + player.bullet[3] + player.blitz[3] + player.rapid[3] + player.custom[3];

            float pointsLight = player.light[3];
            float pointsDark = player.dark[3];

            playerEntry.transform.GetChild(0).GetComponent<Text>().text = player.playerName;
            playerEntry.transform.GetChild(1).GetComponent<Text>().text = wins.ToString();
            playerEntry.transform.GetChild(2).GetComponent<Text>().text = draws.ToString();
            playerEntry.transform.GetChild(3).GetComponent<Text>().text = losses.ToString();
            playerEntry.transform.GetChild(4).GetComponent<Text>().text = points.ToString();
            playerEntry.transform.GetChild(5).GetComponent<Text>().text = (points != 0) ? ((pointsLight > pointsDark) ? "Light" : "Dark") : "None";

            myPlayers.Add(playerEntry);
        }

        foreach (GameObject entry in myPlayers)
        {
            entry.SetActive(true);
        }
    }
}
