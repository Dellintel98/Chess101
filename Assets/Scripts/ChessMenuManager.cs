using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;

public class ChessMenuManager : MonoBehaviour
{
    [SerializeField] public Dropdown pieceColorPicker;
    [SerializeField] public Dropdown boardThemeColorPicker;
    [SerializeField] public Dropdown pieceSetPicker;
    [SerializeField] public Sprite[] pieceSets;
    [SerializeField] public GameObject boardPreview;
    [SerializeField] public Text player2Color;

    private ColorList pieceColorList = new ColorList();
    private ColorList boardColorList = new ColorList();
    private SpritePieceSets myPieceSets = new SpritePieceSets();
    private GameData myGameData = new GameData();
    private Player myPlayers = new Player();

    public bool jsonFilesHaveLoaded { get; set; }
    public bool pieceSpritesHaveBeenSet { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(Time.realtimeSinceStartup);

        if(!jsonFilesHaveLoaded)
            LoadColorListFromJson();

        if(!pieceSpritesHaveBeenSet)
            SetMyPieceSets();

        SetMyGameData();
        SetBoardPreview();

        CreatePlayers();

        SetPickerDropdownOptions(pieceColorList.colorList.Length, pieceColorPicker);
        SetPickerDropdownOptions(boardColorList.colorList.Length, boardThemeColorPicker);
        SetPickerDropdownOptions(myPieceSets.spritePieceList.Count, pieceSetPicker);
    }

    public void LoadColorListFromJson()
    {
        string json = File.ReadAllText(Application.dataPath + "/Resources/piece-colors.json");
        pieceColorList = JsonUtility.FromJson<ColorList>(json);

        json = File.ReadAllText(Application.dataPath + "/Resources/board-colors.json");
        boardColorList = JsonUtility.FromJson<ColorList>(json);

        jsonFilesHaveLoaded = true;
    }

    private void SetPickerDropdownOptions(int itemListCount, Dropdown pickerDropdown)
    {
        List<Dropdown.OptionData> listOptions = new List<Dropdown.OptionData>();

        for (int counter = 0; counter < itemListCount; ++counter)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = $"{counter}";

            listOptions.Add(option);
        }

        pickerDropdown.AddOptions(listOptions);
    }

    public ColorList GetColorList(string listName)
    {
        if (listName == "pieceColorList")
            return pieceColorList;
        else if (listName == "boardColorList")
            return boardColorList;
        else
            return null;
    }

    public void SetMyPieceSets()
    {
        for (int i = 0; i < pieceSets.Length; i += 6)
        {
            SpritePieceSet tempSet = new SpritePieceSet();
            tempSet.rook = pieceSets[i + 0];
            tempSet.knight = pieceSets[i + 1];
            tempSet.bishop = pieceSets[i + 2];
            tempSet.queen = pieceSets[i + 3];
            tempSet.king = pieceSets[i + 4];
            tempSet.pawn = pieceSets[i + 5];

            myPieceSets.spritePieceList.Add(tempSet);
        }

        pieceSpritesHaveBeenSet = true;
    }

    public SpritePieceSets GetPieceSets()
    {
        return myPieceSets;
    }

    private void SetBoardPreview()
    {
        for (int row = 0; row < boardPreview.transform.childCount; ++row)
        {
            Transform previewRow = boardPreview.transform.GetChild(row);

            for (int column = 0; column < previewRow.childCount; column++)
            {
                Transform previewSquare = previewRow.GetChild(column);

                if((row + column)%2 == 0)
                {
                    previewSquare.GetComponent<Image>().color = HexToColor32(myGameData.boardThemeColors[0]);
                }
                else
                {
                    previewSquare.GetComponent<Image>().color = HexToColor32(myGameData.boardThemeColors[1]);
                }

                if (previewRow.name == "DarkPieceSetPreviewRow")
                {
                    previewSquare.GetChild(0).GetComponent<Image>().color = HexToColor32(myGameData.pieceSetColors[1]);
                }
                else if(previewRow.name == "LightPieceSetPreviewRow")
                {
                    previewSquare.GetChild(0).GetComponent<Image>().color = HexToColor32(myGameData.pieceSetColors[0]);
                }

                if(previewRow.name != "EmptyPreviewRow")
                {
                    switch(column)
                    {
                        case 0:
                            previewSquare.GetChild(0).GetComponent<Image>().sprite = myGameData.pieceSpriteSet.king;
                            break;
                        case 1:
                            previewSquare.GetChild(0).GetComponent<Image>().sprite = myGameData.pieceSpriteSet.queen;
                            break;
                        case 2:
                            previewSquare.GetChild(0).GetComponent<Image>().sprite = myGameData.pieceSpriteSet.rook;
                            break;
                        case 3:
                            previewSquare.GetChild(0).GetComponent<Image>().sprite = myGameData.pieceSpriteSet.bishop;
                            break;
                        case 4:
                            previewSquare.GetChild(0).GetComponent<Image>().sprite = myGameData.pieceSpriteSet.knight;
                            break;
                    }
                }
            }
        }
    }

    public void SetMyGameData(
        string player1ColorTag = "Light", string player2ColorTag = "Dark", int pieceSetColorIndex = 0, int boardThemeColorIndex = 0,
        string typeOfGame = "Bullet", float gameTime = 30.0f, float increment = 0.0f, int pieceSetSpriteIndex = 0, string partOfDataToSetOrUpdate = "All")
    {
        if(partOfDataToSetOrUpdate == "All" || partOfDataToSetOrUpdate == "ColorTags")
        {
            myGameData.playerColorTags[0] = player1ColorTag;
            myGameData.playerColorTags[1] = player2ColorTag;

            if(player1ColorTag == "Light")
                player2Color.text = "Dark";
            else if(player1ColorTag == "Dark")
                player2Color.text = "Light";
        }

        if (partOfDataToSetOrUpdate == "All" || partOfDataToSetOrUpdate == "PieceColors")
        {
            myGameData.pieceSetColors[0] = pieceColorList.colorList[pieceSetColorIndex].hexCodes[0];
            myGameData.pieceSetColors[1] = pieceColorList.colorList[pieceSetColorIndex].hexCodes[1];
        }

        if (partOfDataToSetOrUpdate == "All" || partOfDataToSetOrUpdate == "BoardColors")
        {
            myGameData.boardThemeColors[0] = boardColorList.colorList[boardThemeColorIndex].hexCodes[0];
            myGameData.boardThemeColors[1] = boardColorList.colorList[boardThemeColorIndex].hexCodes[1];
        }

        if (partOfDataToSetOrUpdate == "All" || partOfDataToSetOrUpdate == "TimeControl")
        {
            myGameData.gameType = typeOfGame;
            myGameData.timeControl = gameTime;
            myGameData.incrementValue = increment;
        }

        if (partOfDataToSetOrUpdate == "All" || partOfDataToSetOrUpdate == "PieceSprites")
            myGameData.pieceSpriteSet = myPieceSets.spritePieceList[pieceSetSpriteIndex];
    }

    private Color32 HexToColor32(string colorInHex)
    {
        string hex = colorInHex.Replace("#", "");

        byte a = 255;
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    //Debug.Log($"<color=yellow></color>");
    public void HandleDropdownData(Dropdown dropdown, Sprite newMask = null)
    {
        if(dropdown.name == "PieceColorPickerDropdown")
        {
            SetMyGameData(pieceSetColorIndex: dropdown.value, partOfDataToSetOrUpdate: "PieceColors");
            SetBoardPreview();
        }
        else if(dropdown.name == "BoardColorPickerDropdown")
        {
            SetMyGameData(boardThemeColorIndex: dropdown.value, partOfDataToSetOrUpdate: "BoardColors");
            SetBoardPreview();
        }
        else if(dropdown.name == "PieceSetPickerDropdown")
        {
            SetMyGameData(pieceSetSpriteIndex: dropdown.value, partOfDataToSetOrUpdate: "PieceSprites");
            SetBoardPreview();
            pieceColorPicker.GetComponent<DropdownPicker>().UpdatePickerDropdownMask(newMask);
        }
    }

    public void HandleUserInput(InputField input)
    {
        if(input.name == "Player1NameInputField" && input.text != "")
        {
            if(!LoadExistingPlayer(input.text, 0))
                UpdatePlayers(0, "Name", nameUpdate: input.text);
        }
        else if(input.name == "Player2NameInputField" && input.text != "")
        {
            if(!LoadExistingPlayer(input.text, 1))
                UpdatePlayers(1, "Name", nameUpdate: input.text);
        }
    }

    public void RandomPlayerColorChoice(ToggleGroup toggleGroup)
    {
        float randomValue = Random.value;
        bool light = (randomValue > 0.5f) ? true : false;

        for (int counter = 0; counter < 2; ++counter)
        {
            Toggle toggle = toggleGroup.transform.GetChild(counter).GetComponent<Toggle>();

            if(toggle.name == "ToggleLight" && light)
            {
                toggle.isOn = true;
            }
            else if(toggle.name == "ToggleDark" && !light)
            {
                toggle.isOn = true;
            }
        }
    }

    public void HandleToggle(Toggle toggle)
    {
        if (toggle.name == "ToggleLight" && toggle.isOn)
        {
            SetMyGameData(player1ColorTag: "Light", player2ColorTag: "Dark", partOfDataToSetOrUpdate: "ColorTags");
        }
        else if(toggle.name == "ToggleDark" && toggle.isOn)
        {
            SetMyGameData(player1ColorTag: "Dark", player2ColorTag: "Light", partOfDataToSetOrUpdate: "ColorTags");
        }
    }

    private void CreatePlayers()
    {
        for(int i = 0; i < 2; ++i)
        {
            PlayerData tempPlayer = new PlayerData();
            tempPlayer.playerName = "Anonymous" + (i+1).ToString();

            for (int j = 0; j < 4; j++)
            {
                tempPlayer.unlimited[i] = 0;
                tempPlayer.bullet[i] = 0;
                tempPlayer.blitz[i] = 0;
                tempPlayer.rapid[i] = 0;
                tempPlayer.custom[i] = 0;
                tempPlayer.light[i] = 0;
                tempPlayer.dark[i] = 0;
            }

            tempPlayer.playerId = GetHashString(tempPlayer.playerName + Random.Range(0.0f,1000.0f).ToString());

            myPlayers.players.Add(tempPlayer);
        }
    }

    private void UpdatePlayers(int playersIndex, string typeOfUpdate = "All", float[] bulletUpdate = null, float[] blitzUpdate = null, float[] rapidUpdate = null,
        float[] unlimitedUpdate = null, float[] customUpdate = null, float[] lightUpdate = null, float[] darkUpdate = null, string nameUpdate = "", string idUpdate = "")
    {
        if(typeOfUpdate == "All" || typeOfUpdate == "Bullet")
        {
            bulletUpdate.CopyTo(myPlayers.players[playersIndex].bullet, 0);
        }
        if(typeOfUpdate == "All" || typeOfUpdate == "Blitz")
        {
            blitzUpdate.CopyTo(myPlayers.players[playersIndex].blitz, 0);
        }
        if (typeOfUpdate == "All" || typeOfUpdate == "Rapid")
        {
            rapidUpdate.CopyTo(myPlayers.players[playersIndex].bullet, 0);
        }
        if (typeOfUpdate == "All" || typeOfUpdate == "Unlimited")
        {
            unlimitedUpdate.CopyTo(myPlayers.players[playersIndex].bullet, 0);
        }
        if (typeOfUpdate == "All" || typeOfUpdate == "Custom")
        {
            customUpdate.CopyTo(myPlayers.players[playersIndex].bullet, 0);
        }
        if (typeOfUpdate == "All" || typeOfUpdate == "Light")
        {
            lightUpdate.CopyTo(myPlayers.players[playersIndex].bullet, 0);
        }
        if (typeOfUpdate == "All" || typeOfUpdate == "Dark")
        {
            darkUpdate.CopyTo(myPlayers.players[playersIndex].bullet, 0);
        }
        if (typeOfUpdate == "All" || typeOfUpdate == "Name")
        {
            myPlayers.players[playersIndex].playerName = nameUpdate;
        }
        if (typeOfUpdate == "All" || typeOfUpdate == "Id")
        {
            myPlayers.players[playersIndex].playerId = idUpdate;
        }
    }

    public void SaveCurrentPlayersGlobally()
    {
        ChessGlobalControl.Instance.savedCurrentPlayers.players.Clear();
        ChessGlobalControl.Instance.savedCurrentPlayers.players.AddRange(myPlayers.players);
    }

    private bool LoadExistingPlayer(string playerName, int playersIndex)
    {
        Player allPlayers = ChessGlobalControl.Instance.allPlayers;

        if (allPlayers == null)
            return false;

        foreach (PlayerData player in allPlayers.players)
        {
            if (player.playerName == playerName)
            {
                myPlayers.players[playersIndex] = player;
                return true;
            }
        }

        return false;
    }

    public void SaveGameDataGlobally()
    {
        ChessGlobalControl.Instance.savedGameData = myGameData;
    }

    public void SetLastSceneIndex()
    {
        ChessGlobalControl.Instance.lastSceneIndex = 1;
    }

    public static byte[] GetHash(string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    public static string GetHashString(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }
}
