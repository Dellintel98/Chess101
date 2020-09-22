using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoverableElement : MonoBehaviour
{
    [SerializeField] public string tooltipText;
    private bool didMyTooltipShowUp { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        didMyTooltipShowUp = false;
    }

    void OnMouseEnter()
    {
        if (transform.name == "ChessButton" && !didMyTooltipShowUp)
        {
            Tooltip.ShowTooltip_Static(tooltipText, positionOffsetY: -200f, positionOffsetX: 50f);
            Tooltip.HideTooltip_Static();
        }
        else if (transform.parent.name == "Scoreboard" && !didMyTooltipShowUp)
        {
            Tooltip.ShowTooltip_Static(tooltipText, positionOffsetY: 20f);
            Tooltip.HideTooltip_Static();
        }
        else if (transform.name != "ChessButton" && !didMyTooltipShowUp)
        {
            Tooltip.ShowTooltip_Static(tooltipText);
            Tooltip.HideTooltip_Static();
        }
    }
    void OnMouseExit()
    {
        Tooltip.HideTooltip_Static(fromOnExitMethod: true);
        didMyTooltipShowUp = false;
    }
}
