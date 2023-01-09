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
    [SerializeField] private CanvasGroup CanvasGroup;
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

    public void SetUseable(ItemAmt itemAmt)
    {
        AssignItemAmt(itemAmt);
        CurrentInteraction = Interaction.Useable;
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


    public void SetSibling(int sibling)
    {
        RT.SetSiblingIndex(sibling);
    }

    public void SetAlpha(float alpha)
    {
        CanvasGroup.alpha = alpha;
    }
    public void SetInteractable(bool interactable)
    {
        CanvasGroup.interactable = interactable;
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
            case Interaction.Useable:
                UseableClicked();
                return;
        }
    }

    private void LootableClicked()
    {
        LootOne();
    }
    public void LootOne()
    {
        if (itemAmt.amt == 0)
        {
            return;
        }
        UIInventory.AddToInventory(itemAmt.item, 1);
        itemAmt.amt--;
        UIPOI.UILootUpdate();
        if (itemAmt.amt == 0)
        {
            animating = true;
            SetInteractable(false);
        }
        else
        {
            SetAmtText();
        }
    }

    private int useableClickedFrame = -1;
    private void UseableClicked()
    {
        if (useableClickedFrame == Time.frameCount)
        {
            return;
        }
        useableClickedFrame = Time.frameCount;
        if (itemAmt.amt == 0)
        {
            return;
        }

        itemAmt.item.Use();
        if (itemAmt.item.Consumable)
        {
            itemAmt.amt--;
            UIInventory.RemoveFromInventory(itemAmt.item, 1);
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
                SetAlpha(scale);
                break;
            case Interaction.Useable:
                //if (animCount > GameSettings.LootableHideTime)
                //{
                //    //Destroy(gameObject);
                //    UIInventory.UIItemRemoved(this);
                //    return;
                //}
                //float scale1 = 1 - animCount / GameSettings.LootableHideTime;
                //SetAlpha(scale1);
                break;
        }

    }



}
