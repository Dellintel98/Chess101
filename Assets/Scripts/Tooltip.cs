using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    private static Tooltip instance;

    [SerializeField] public Camera uiCamera;
    [SerializeField] public RectTransform canvasRectTransform;
    private Text tooltipText;
    private RectTransform backgroundRectTransform;
    private float offsetX;
    private float offsetY;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
        backgroundRectTransform = transform.Find("Background").GetComponent<RectTransform>();
        tooltipText = transform.Find("Text").GetComponent<Text>();
     }
 
    IEnumerator LateCall(bool fromOnExitMethod)
    {
        if(!fromOnExitMethod)
            yield return new WaitForSeconds(2.5f);
        if(instance.isActiveAndEnabled)
            HideTooltip();
    }

    //IEnumerator fadeInAndOut(GameObject objectToFade, bool fadeIn, float duration)
    //{
    //    float counter = 0f;

    //    //Set Values depending on if fadeIn or fadeOut
    //    float a, b;
    //    if (fadeIn)
    //    {
    //        a = 0;
    //        b = 1;
    //    }
    //    else
    //    {
    //        a = 1;
    //        b = 0;
    //    }

    //    Color currentBackgroundColor = Color.clear;
    //    Color currentTextColor = Color.clear;

    //    Image tempImage = objectToFade.GetComponent<Image>();
    //    Text tempText = objectToFade.GetComponent<Text>();

    //    currentBackgroundColor = tempImage.color;
    //    currentTextColor = tempText.color;

    //    while (counter < duration)
    //    {
    //        counter += Time.deltaTime;
    //        float alpha = Mathf.Lerp(a, b, counter / duration);

    //        tempImage.color = new Color(currentBackgroundColor.r, currentBackgroundColor.g, currentBackgroundColor.b, alpha);
    //        tempText.color = new Color(currentTextColor.r, currentTextColor.g, currentTextColor.b, alpha);

    //        yield return null;
    //    }
    //}

    private void ShowTooltip(string tooltipString, string tooltipBackgroundColor, string tooltipFontColor, int tooltipFontSize, float positionOffsetX, float positionOffsetY)
    {
        gameObject.SetActive(true);
        offsetX = positionOffsetX;
        offsetY = positionOffsetY;

        tooltipText.text = tooltipString;
        tooltipText.color = HexToColor32(tooltipFontColor);
        tooltipText.fontSize = tooltipFontSize;
        float padding = 10f;
        Vector2 backgroundSize = new Vector2(tooltipText.preferredWidth + padding * 2f, tooltipText.preferredHeight + padding * 2f);
        backgroundRectTransform.sizeDelta = backgroundSize;
        backgroundRectTransform.transform.GetComponent<Image>().color = HexToColor32(tooltipBackgroundColor);
    }

    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    public static void ShowTooltip_Static(string tooltipString, string tooltipBackgroundColor = "#333333D9", string tooltipFontColor = "#FFFFFF",
        int tooltipFontSize = 26, float positionOffsetX = 0f, float positionOffsetY = 0f)
    {
        instance.ShowTooltip(tooltipString, tooltipBackgroundColor, tooltipFontColor, tooltipFontSize, positionOffsetX, positionOffsetY);
    }

    public static void HideTooltip_Static(bool fromOnExitMethod = false)
    {
        if (!instance.isActiveAndEnabled)
            return;
        instance.StartCoroutine(instance.LateCall(fromOnExitMethod));
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

    // Update is called once per frame
    void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
        transform.localPosition = new Vector3(localPoint.x + offsetX, localPoint.y + offsetY);

        Vector2 anchoredPosition = transform.GetComponent<RectTransform>().anchoredPosition;
        if(anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
        {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        }

        if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
        {
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
        }

        transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
    }
}
