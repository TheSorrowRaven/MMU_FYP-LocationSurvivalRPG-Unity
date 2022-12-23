using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILoot : MonoBehaviour
{

    public ItemAmt itemAmt;

    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI AmtText;
    [SerializeField] private Image BackgroundImage;
    [SerializeField] private Image IconImage;


    public void Activate(ItemAmt itemAmt)
    {
        this.itemAmt = itemAmt;

        Item item = itemAmt.item;

        NameText.SetText(item.Name);
        AmtText.SetText($"x{itemAmt.amt}");
        BackgroundImage.sprite = ItemManager.Instance.RarityToSprite[item.Rarity];
        IconImage.sprite = item.Icon;
    }

    


}
