using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUIModel : MonoBehaviour
{
    public Image cardImage;
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI cardWoodCost;
    public TextMeshProUGUI cardMeatCost;
    public Button selectBtn;

    public void SetCardInfo(string name, int woodCost, int meatCost)
    {
        if (cardName != null)
            cardName.text = name;
        
        if (cardWoodCost != null)
            cardWoodCost.text = woodCost.ToString();

        if (cardMeatCost != null)
            cardMeatCost.text = meatCost.ToString();
    }
}
