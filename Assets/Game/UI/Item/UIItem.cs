using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIItem : MonoBehaviour, IPointerClickHandler
{
    public enum Interaction
    {
        None,
        Lootable,
        Useable,
    }
    private static ItemManager ItemManager => ItemManager.Instance;
    private static GameSettings GameSettings => GameSettings.Instance;
    private static UIPOI UIPOI => UIPOI.Instance;
    private static UIInventory UIInventory => UIInventory.Instance;

    public ItemAmt itemAmt;

    public Interaction CurrentInteraction { get; private set; }

    [SerializeField] private RectTransform RT;
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI AmtText;
    [SerializeField] private Image BackgroundImage;
    [SerializeField] private Image IconImage;

    [System.NonSerialized] private bool animating = false;
    [System.NonSerialized] private float animCount;



    public void SetLootable(ItemAmt itemAmt)
    {
        AssignItemAmt(itemAmt);
        CurrentInteraction = Interaction.Lootable;
    }

    public void AssignItemAmt(ItemAmt itemAmt)
    {
        this.itemAmt = itemAmt;

        Item item = itemAmt.item;

        NameText.SetText(item.Name);
        BackgroundImage.sprite = ItemManager.RarityToBackgroundSprite[item.Rarity];
        IconImage.sprite = item.Icon;
        SetAmtText();

    }

    private void SetAmtText()
    {
        if (itemAmt.amt <= 1)
        {
            AmtText.enabled = false;
            return;
        }
        AmtText.enabled = true;
        AmtText.SetText($"x{itemAmt.amt}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (CurrentInteraction)
        {
            case Interaction.None:
                return;
            case Interaction.Lootable:
                LootableClicked();
                return;

        }
    }

    private void LootableClicked()
    {
        UIInventory.AddToInventory(itemAmt.item, 1);
        itemAmt.amt--;
        if (itemAmt.amt == 0)
        {
            animating = true;
        }
        else
        {
            SetAmtText();
        }
    }

    private void Update()
    {
        if (!animating)
        {
            return;
        }

        animCount += Time.deltaTime;
        switch (CurrentInteraction)
        {
            case Interaction.Lootable:
                if (animCount > GameSettings.LootableHideTime)
                {
                    UIPOI.UILootRemoved(this);
                    return;
                }
                float scale = 1 - animCount / GameSettings.LootableHideTime;
                RT.sizeDelta = new(RT.sizeDelta.x * scale, RT.sizeDelta.y);
                break;
        }

    }



}
