using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropdownPicker : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public Text timeControlText;
    [SerializeField] public Button unlimitedButton;
    [SerializeField] public GameObject bulletObject;
    [SerializeField] public GameObject blitzObject;
    [SerializeField] public GameObject rapidObject;
    [SerializeField] public GameObject customObject;
    private bool listOfColorsHasToBeSet { get; set; }
    private bool pieceSetListHasToBeSet { get; set; }
    private ColorList listOfColors = new ColorList();
    private SpritePieceSets pieceSets = new SpritePieceSets();
    private Sprite pieceColorPickerMask;

    private Button selectedTime;
    private float[] customTime = new float[2];
    private GameObject currentlyActiveTimeControlObject;
    
    private string gameType { get; set; }
    private float timeIncrement { get; set; }
    private float timeControlInSeconds { get; set; }

    private string noSelectedTimeColor = "#BE2121";
    private string selectedTimeColor = "#008080";

    // Start is called before the first frame update
    void Start()
    {
        listOfColorsHasToBeSet = false;
        pieceSetListHasToBeSet = false;
        //Debug.Log("DropdownPicker" + Time.realtimeSinceStartup);
        ChessMenuManager chessMenu = FindObjectOfType<ChessMenuManager>();

        if (!chessMenu.jsonFilesHaveLoaded)
            chessMenu.LoadColorListFromJson();

        if (!chessMenu.pieceSpritesHaveBeenSet)
            chessMenu.SetMyPieceSets();

        listOfColors = (transform.name == "PieceColorPickerDropdown") ? chessMenu.GetColorList("pieceColorList") : chessMenu.GetColorList("boardColorList");
        pieceSets = chessMenu.GetPieceSets();

        if (transform.name == "PieceColorPickerDropdown" || transform.name == "BoardColorPickerDropdown" || transform.name == "PieceSetPickerDropdown")
        {
            transform.GetComponent<Dropdown>().captionText.text = "0";
        }

        if(pieceSets != null && transform.name == "PieceColorPickerDropdown")
        {
            pieceColorPickerMask = pieceSets.spritePieceList[0].pawn;
        }


        SetCaptionImageOnSelectionChange();

        transform.GetComponent<Dropdown>().onValueChanged.AddListener(delegate {
            SetCaptionImageOnSelectionChange();
            chessMenu.HandleDropdownData(transform.GetComponent<Dropdown>(), pieceColorPickerMask);
            SetVisibleTimeControlObject();
        });

        selectedTime = unlimitedButton;
        customTime[0] = 0.0f;
        customTime[1] = 0.0f;

        SetVisibleTimeControlObject();
        if (transform.name == "TimeControlDropdown")
        {
            SetTimeControlData($"{selectedTime.transform.GetChild(0).GetComponent<Text>().text.Split(' ')[0]}", 0, 0);
        }
    }

    private void SetItemImageSpriteColors()
    {
        if (transform.name != "PieceColorPickerDropdown" && transform.name != "BoardColorPickerDropdown")
            return;

        if (transform.childCount < 5 || !listOfColorsHasToBeSet)
            return;

        Transform dropdownContentTransform = transform.GetChild(4).GetChild(0).GetChild(0);

        int index;
        string numberStr;

        for (int i = 0; i < dropdownContentTransform.childCount; ++i)
        {
            numberStr = dropdownContentTransform.transform.GetChild(i).GetChild(2).GetComponent<Text>().text;

            bool isParsable = int.TryParse(numberStr, out index);

            if (!isParsable)
                continue;

            Transform[] itemImages = new Transform[2];
            if(transform.name == "PieceColorPickerDropdown")
            {
                itemImages[0] = dropdownContentTransform.transform.GetChild(i).GetChild(3).GetChild(0).GetChild(0);
                itemImages[1] = dropdownContentTransform.transform.GetChild(i).GetChild(3).GetChild(1).GetChild(0);

                dropdownContentTransform.transform.GetChild(i).GetChild(3).GetChild(0).GetComponent<Image>().sprite = pieceColorPickerMask;
                dropdownContentTransform.transform.GetChild(i).GetChild(3).GetChild(1).GetComponent<Image>().sprite = pieceColorPickerMask;
            }
            else
            {
                itemImages[0] = dropdownContentTransform.transform.GetChild(i).GetChild(3).GetChild(0);
                itemImages[1] = dropdownContentTransform.transform.GetChild(i).GetChild(3).GetChild(1);
            }

            itemImages[0].GetComponent<Image>().color = HexToColor32(listOfColors.colorList[index].hexCodes[0]);
            itemImages[1].GetComponent<Image>().color = HexToColor32(listOfColors.colorList[index].hexCodes[1]);
        }

        listOfColorsHasToBeSet = false;
    }
    //Debug.Log($"<color=yellow></color>");

    private void SetPieceSprites()
    {
        if (transform.name != "PieceSetPickerDropdown" || pieceSets == null)
            return;

        if (transform.childCount < 5 || !pieceSetListHasToBeSet)
            return;

        Transform dropdownContentTransform = transform.GetChild(4).GetChild(0).GetChild(0);

        int index;
        string numberStr;

        for (int i = 0; i < dropdownContentTransform.childCount; ++i)
        {
            numberStr = dropdownContentTransform.transform.GetChild(i).GetChild(2).GetComponent<Text>().text;

            bool isParsable = int.TryParse(numberStr, out index);

            if (!isParsable)
                continue;

            Transform[] itemImage = new Transform[1];
            itemImage[0] = dropdownContentTransform.transform.GetChild(i).GetChild(3).GetChild(0);

            itemImage[0].transform.GetComponent<Image>().sprite = pieceSets.spritePieceList[index].knight;
        }

        pieceSetListHasToBeSet = false;
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

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        int dropdownCount = transform.childCount;

        if (dropdownCount < 5)
            return;

        listOfColorsHasToBeSet = true;
        pieceSetListHasToBeSet = true;
    }

    private void SetCaptionImageOnSelectionChange()
    {
        if (transform.name != "PieceColorPickerDropdown" && transform.name != "BoardColorPickerDropdown" && transform.name != "PieceSetPickerDropdown")
            return;

        int index;
        string numberStr = transform.GetComponent<Dropdown>().captionText.text;

        bool isParsable = int.TryParse(numberStr, out index);

        if (!isParsable)
            return;

        if (transform.name == "PieceColorPickerDropdown")
        {
            transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>().color = HexToColor32(listOfColors.colorList[index].hexCodes[0]);
            transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().color = HexToColor32(listOfColors.colorList[index].hexCodes[1]);
        }
        else if (transform.name == "BoardColorPickerDropdown")
        {
            transform.GetChild(1).GetChild(0).GetComponent<Image>().color = HexToColor32(listOfColors.colorList[index].hexCodes[0]);
            transform.GetChild(1).GetChild(1).GetComponent<Image>().color = HexToColor32(listOfColors.colorList[index].hexCodes[1]);
        }
        else if(transform.name == "PieceSetPickerDropdown")
        {
            transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = pieceSets.spritePieceList[index].knight;
            pieceColorPickerMask = pieceSets.spritePieceList[index].pawn;
        }
    }

    public void UpdatePickerDropdownMask(Sprite newMask)
    {
        if (transform.name != "PieceColorPickerDropdown" || newMask == null)
            return;

        pieceColorPickerMask = newMask;
        transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = pieceColorPickerMask;
        transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = pieceColorPickerMask;
    }

    private void SetVisibleTimeControlObject()
    {
        if (transform.name != "TimeControlDropdown")
            return;

        if (currentlyActiveTimeControlObject)
            currentlyActiveTimeControlObject.SetActive(false);

        switch (transform.GetComponent<Dropdown>().value)
        {
            case 0:
                bulletObject.SetActive(true);
                currentlyActiveTimeControlObject = bulletObject;
                break;
            case 1:
                blitzObject.SetActive(true);
                currentlyActiveTimeControlObject = blitzObject;
                break;
            case 2:
                rapidObject.SetActive(true);
                currentlyActiveTimeControlObject = rapidObject;
                break;
            case 3:
                customObject.SetActive(true);
                currentlyActiveTimeControlObject = customObject;
                SetTimeControlData("Custom", 15, 0, true, isDecimal: true, isMinuteSlider: true, sliderString: "1/4");
                ChessMenuManager chessMenu = FindObjectOfType<ChessMenuManager>();
                chessMenu.SetMyGameData(typeOfGame: gameType, gameTime: timeControlInSeconds, increment: timeIncrement, partOfDataToSetOrUpdate: "TimeControl");
                break;
        }
    }

    public void OnTimeControlButtonClick(Button button)
    {
        string buttonValue = button.transform.GetChild(0).GetComponent<Text>().text;

        switch (buttonValue)
        {
            case "Unlimited time":
                SetTimeControlData("Unlimited", 0, 0);
                break;
            case "30 s":
                SetTimeControlData("Bullet", 30.0f, 0, sliderString: "1/2", isDecimal: true);
                break;
            case "1 min":
                SetTimeControlData("Bullet", 60.0f, 0);
                break;
            case "1 | 1":
                SetTimeControlData("Bullet", 60.0f, 1.0f);
                break;
            case "2 | 1":
                SetTimeControlData("Bullet", 2 * 60.0f, 1.0f);
                break;
            case "3 min":
                SetTimeControlData("Blitz", 3 * 60.0f, 0);
                break;
            case "3 | 2":
                SetTimeControlData("Blitz", 3 * 60.0f, 2.0f);
                break;
            case "5 min":
                SetTimeControlData("Blitz", 5 * 60.0f, 0);
                break;
            case "5 | 5":
                SetTimeControlData("Blitz", 5 * 60.0f, 5.0f);
                break;
            case "10 min":
                SetTimeControlData("Blitz", 10 * 60.0f, 0);
                break;
            case "15 | 10":
                SetTimeControlData("Rapid", 15 * 60.0f, 10.0f);
                break;
            case "20 min":
                SetTimeControlData("Rapid", 20 * 60.0f, 0);
                break;
            case "30 min":
                SetTimeControlData("Rapid", 30 * 60.0f, 0);
                break;
            case "45 | 45":
                SetTimeControlData("Rapid", 45 * 60.0f, 45.0f);
                break;
            case "60 min":
                SetTimeControlData("Rapid", 60 * 60.0f, 0);
                break;
        }

        ChessMenuManager chessMenu = FindObjectOfType<ChessMenuManager>();
        chessMenu.SetMyGameData(typeOfGame: gameType, gameTime: timeControlInSeconds, increment: timeIncrement, partOfDataToSetOrUpdate: "TimeControl");
    }

    private void SetTimeControlData(
        string typeOfAGame, float time, float increment, bool isTimeSelected = true, bool isDecimal = false,
        bool isMinuteSlider = false, bool isIncrementSlider = false, string sliderString = "", string siblingSliderString = "")
    {
        float sliderTime;
        if (isMinuteSlider)
        {
            sliderTime = (isDecimal) ? time : time * 60.0f;

            timeControlInSeconds = sliderTime;
            timeIncrement = increment;

            timeControlText.text = (isDecimal) ? $"{sliderString} | {increment}" : $"{sliderTime / 60} | {increment}";
        }
        else if(isIncrementSlider)
        {
            sliderTime = (isDecimal) ? increment : increment * 60.0f;

            timeControlInSeconds = sliderTime;
            timeIncrement = time;

            timeControlText.text = (isDecimal) ? $"{siblingSliderString} | {time}" : $"{sliderTime / 60} | {time}";
        }
        else
        {
            timeControlInSeconds = time;
            timeIncrement = increment;

            string minutes = (isDecimal) ? sliderString : $"{time / 60}";

            timeControlText.text = (typeOfAGame != "Unlimited") ? $"{minutes} | {increment}" : typeOfAGame;
        }

        gameType = typeOfAGame;
        timeControlText.color = HexToColor32(isTimeSelected ? selectedTimeColor : noSelectedTimeColor);
    }

    public void HandleSliderData(Slider slider)
    {
        Transform sliderParent = slider.transform.parent;
        int siblingIndex = (sliderParent.name == "MinuteSlider") ? sliderParent.GetSiblingIndex() + 1 : sliderParent.GetSiblingIndex() - 1;
        Slider siblingSlider = sliderParent.parent.GetChild(siblingIndex).GetChild(2).GetComponent<Slider>();

        bool isDecimal = false;

        float sliderValue = slider.value;
        string sliderValueString = sliderValue.ToString();
        float siblingSliderValue = siblingSlider.value;
        string siblingSliderValueString = siblingSliderValue.ToString();

        if (sliderParent.name == "MinuteSlider")
        {
            if(sliderValue >= 4 && sliderValue < 16)
            {
                if (sliderValue % 4 != 0)
                    sliderValue = 4 * ((int)sliderValue / 4);

                sliderValueString = (sliderValue / 4 == 2) ? $"{sliderValue / 8}/2" : $"{sliderValue / 4}/4";
                sliderValue = (sliderValue / 4) * 15.0f;
                isDecimal = true;
            }
            else if(sliderValue >= 16)
            {
                sliderValue -= 15;
                sliderValueString = $"{sliderValue}";
            }
        }
        else if (sliderParent.name == "IncrementSlider")
        {
            if (siblingSliderValue >= 4 && siblingSliderValue < 16)
            {
                if (siblingSliderValue % 4 != 0)
                    siblingSliderValue = 4 * ((int)siblingSliderValue / 4);

                siblingSliderValueString = (siblingSliderValue / 4 == 2) ? $"{siblingSliderValue / 8}/2" : $"{siblingSliderValue / 4}/4";
                siblingSliderValue = (siblingSliderValue / 4) * 15.0f;
                isDecimal = true;
            }
            else if (siblingSliderValue >= 16)
            {
                siblingSliderValue -= 15;
                siblingSliderValueString = $"{siblingSliderValue}";
            }
        }

        sliderParent.GetChild(1).GetComponent<Text>().text = sliderValueString;
        sliderParent.GetChild(1).GetComponent<Text>().color = HexToColor32((sliderValue != 0) ? selectedTimeColor : noSelectedTimeColor);

        bool isTimeSelected = (sliderValue == 0 && siblingSliderValue == 0) ? false : true;

        if (sliderParent.name == "MinuteSlider")
        {
            SetTimeControlData("Custom", sliderValue, siblingSliderValue, isTimeSelected, isDecimal, isMinuteSlider: true, sliderString: sliderValueString);
        }
        else if(sliderParent.name == "IncrementSlider")
        {
            SetTimeControlData("Custom", sliderValue, siblingSliderValue, isTimeSelected, isDecimal, isIncrementSlider: true, siblingSliderString: siblingSliderValueString);
        }

        ChessMenuManager chessMenu = FindObjectOfType<ChessMenuManager>();
        chessMenu.SetMyGameData(typeOfGame: gameType, gameTime: timeControlInSeconds, increment: timeIncrement, partOfDataToSetOrUpdate: "TimeControl");
    }

    // Update is called once per frame
    void Update()
    {
        SetItemImageSpriteColors();
        SetPieceSprites();
    }
}
