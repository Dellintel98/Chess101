using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromotionPieceElement : MonoBehaviour
{
    private Color32 mySpecialColor = Color32.Lerp(new Color32(0x1F, 0x7A, 0x8C, 0xFF), new Color32(0xFF, 0xFF, 0xFF, 0xFF), 0.4f);
    private Color32 myPieceColor;

    public void Setup(Sprite pieceSprite, Color32 pieceColor)
    {
        transform.GetComponent<SpriteRenderer>().sortingLayerName = "PiecesLayer";
        transform.GetComponent<SpriteRenderer>().sprite = pieceSprite;

        myPieceColor = pieceColor;
        transform.GetComponent<SpriteRenderer>().color = myPieceColor;
    }

    public void Highlight()
    {
        transform.GetComponent<SpriteRenderer>().color = mySpecialColor;
    }

    public void RemoveHighlight()
    {
        transform.GetComponent<SpriteRenderer>().color = myPieceColor;
    }
}
