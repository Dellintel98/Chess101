using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChessGlobalControl : MonoBehaviour
{
    public static ChessGlobalControl Instance;

    public Player savedCurrentPlayers = new Player();
    public Player allPlayers = new Player();
    public GameData savedGameData = new GameData();
    public int lastSceneIndex;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        LoadScoreboardList();
    }

    private void LoadScoreboardList()
    {
        string json = File.ReadAllText(Application.dataPath + "/Resources/players.json");
        allPlayers = JsonUtility.FromJson<Player>(json);
    }

    private void SaveScoreboardList()
    {
        string json = JsonUtility.ToJson(allPlayers, true);
        File.WriteAllText(Application.dataPath + "/Resources/players.json", json);
    }

    public void SaveCurrentPlayers()
    {
        foreach (PlayerData player in savedCurrentPlayers.players)
        {
            if (player.playerName == "Anonymous1" || player.playerName == "Anonymous2")
                continue;

            for (int i = 0; i < allPlayers.players.Count; ++i)
            {
                if(allPlayers.players[i].playerName == player.playerName)
                {
                    allPlayers.players.RemoveAt(i);
                    break;
                }
            }

            allPlayers.players.Add(player);
        }
    }

    public void SaveBeforeExit()
    {
        SaveCurrentPlayers();
        SaveScoreboardList();
    }
}
