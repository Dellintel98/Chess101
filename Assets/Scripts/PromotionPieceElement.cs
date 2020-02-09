using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromotionPieceElement : MonoBehaviour
{
    private Color32 mySpecialColor = Color32.Lerp(new Color32(0x1F, 0x7A, 0x8C, 0xFF), new Color32(0xFF, 0xFF, 0xFF, 0xFF), 0.4f);
    private Color32 myPieceColor;
    private Sprite mySprite;
    private ChessPiece myChessPiece;
    private PromotionModalBox myParentBox;

    public void Setup(Sprite pieceSprite, Color32 pieceColor, ChessPiece chessPiece, PromotionModalBox promotionBox)
    {
        transform.GetComponent<SpriteRenderer>().sortingLayerName = "PiecesLayer";
        mySprite = pieceSprite;
        transform.GetComponent<SpriteRenderer>().sprite = mySprite;

        myPieceColor = pieceColor;
        transform.GetComponent<SpriteRenderer>().color = myPieceColor;

        myChessPiece = chessPiece;
        myParentBox = promotionBox;
    }

    public void Highlight()
    {
        transform.GetComponent<SpriteRenderer>().color = mySpecialColor;
    }

    public void RemoveHighlight()
    {
        transform.GetComponent<SpriteRenderer>().color = myPieceColor;
    }

    private void OnMouseDown()
    {
        string pieceTag = myParentBox.GetTagEqualToPieceSprite(mySprite);
        myChessPiece.ChangePawnToSelectedPiece(mySprite, pieceTag);
    }

    private void OnMouseUp()
    {
        myChessPiece.DestroyPawnPromotionModalBox();
    }

    private void OnMouseOver()
    {
        myParentBox.OnHover();
    }
}
