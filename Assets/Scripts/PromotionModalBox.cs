using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromotionModalBox : MonoBehaviour
{
    [SerializeField] public GameObject promotionPieceObjectPrefab;

    private Sprite[] myPieceSprites;
    private GameObject[] myPromotionPieces;
    private ChessPiece myChessPiece;
    private Color32 myColor = Color32.Lerp(new Color32(0x52, 0x51, 0x51, 0xFF), new Color32(0xC6, 0xC6, 0xC6, 0xFF), 0.3f);


    public void SetupPromotionalModalBox(ChessPiece chessPiece, Sprite[] sprites)
    {
        myChessPiece = chessPiece;
        myPromotionPieces = new GameObject[4];
        myPieceSprites = new Sprite[4];

        for(int i = 0; i < 4; i++)
        {
            myPieceSprites[i] = sprites[i];
        }

        transform.GetComponent<SpriteRenderer>().color = myColor;

        SetupPromotionPieces();
    }

    private void SetupPromotionPieces()
    {
        float promotionPieceYCoordinate;

        for (int i = 0; i < 4; i++)
        {
            promotionPieceYCoordinate = (transform.position.y - 3f) + i * 2;

            InstantiatePromotionPieces(i, promotionPieceYCoordinate);
        }
    }

    private void InstantiatePromotionPieces(int index, float pieceYCoordinate)
    {
        int spriteIndex = index;

        if (myChessPiece.GetMyColorTag() == "Dark")
        {
            spriteIndex = 3 - index;
        }

        myPromotionPieces[index] = Instantiate(promotionPieceObjectPrefab, new Vector3(transform.position.x, pieceYCoordinate, -6f), Quaternion.identity, transform) as GameObject;
        myPromotionPieces[index].GetComponent<SpriteRenderer>().sortingLayerName = "PiecesLayer";
        myPromotionPieces[index].GetComponent<SpriteRenderer>().sprite = myPieceSprites[spriteIndex];
        myPromotionPieces[index].GetComponent<SpriteRenderer>().color = myChessPiece.transform.GetComponent<SpriteRenderer>().color;
    }
}
