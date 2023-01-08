using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventory : MonoBehaviour, Save.ISaver
{
    private class ItemComparer : IComparer<ItemAmt>
    {
        public static ItemComparer Shared = new();

        public int Compare(ItemAmt x, ItemAmt y)
        {
            int val = x.item.GetType().FullName.CompareTo(y.item.GetType().FullName);
            if (val != 0)
            {
                return val;
            }
            val = x.item.Rarity.CompareTo(y.item.Rarity);
            if (val != 0)
            {
                return val;
            }
            return y.amt.CompareTo(x.amt);
        }
    }


    private static UIInventory instance;
    public static UIInventory Instance => instance;
    private static ItemManager ItemManager => ItemManager.Instance;


    [SerializeField] private GameObject UIInventoryObject;
    [SerializeField] private Transform UIItemContainer;
    [SerializeField] private GameObject UIItemPrefab;

    private Dictionary<Item, int> Inventory = new();
    private readonly List<ItemAmt> ItemAmtList = new();
    private readonly List<UIItem> UIItemList = new();

    private bool IsActive => UIInventoryObject.activeSelf;



    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        StartInit();
    }




    public WeaponItem GetFirstWeaponItem()
    {
        foreach (var pair in Inventory)
        {
            if (pair.Key is WeaponItem weapon)
            {
                return weapon;
            }
        }
        return null;
    }


    public void AddToInventory(Item item, int amt = 1)
    {
        if (Inventory.TryGetValue(item, out int count))
        {
            int total = count + amt;
            Inventory[item] = total;
            UpdateUIItems();
            return;
        }
        Inventory.Add(item, amt);
        UpdateUIItems();
    }

    public void RemoveFromInventory(Item item, int amt = 1)
    {
        if (!Inventory.TryGetValue(item, out int count))
        {
            Debug.LogError("Can't find item from inventory");
            return;
        }
        int remaining = count - amt;
        if (remaining <= 0)
        {
            if (remaining < 0)
            {
                Debug.LogError("Removing more than what the inventory has");
            }
            Inventory.Remove(item);
            UpdateUIItems();
            return;
        }
        Inventory[item] = remaining;
        UpdateUIItems();
    }

    private void UpdateUIItems()
    {
        ItemAmtList.Clear();
        foreach (var pair in Inventory)
        {
            ItemAmtList.Add(new(pair.Key, pair.Value));
        }
        ItemAmtList.Sort(ItemComparer.Shared);
        int i = 0;
        for (; i < ItemAmtList.Count; i++)
        {
            UIItem uiItem;
            if (i == UIItemList.Count)
            {
                uiItem = Instantiate(UIItemPrefab, UIItemContainer).GetComponent<UIItem>();
                UIItemList.Add(uiItem);
            }
            else
            {
                uiItem = UIItemList[i];
            }
            uiItem.SetUseable(ItemAmtList[i]);
        }
        for (; i < UIItemList.Count;)
        {
            Destroy(UIItemList[i].gameObject);
            UIItemList.RemoveAt(i);
        }
        Save.Instance.SaveRequest();
    }

    public void UIItemRemoved(UIItem uiItem)
    {
        UIItemList.Remove(uiItem);
        Destroy(uiItem.gameObject);
    }



    public void ButtonClickExitUIInventory()
    {
        UIInventoryObject.SetActive(false);
    }

    public void InventoryButtonClicked()
    {
        UIInventoryObject.SetActive(!UIInventoryObject.activeSelf);
    }
    public void InventoryForceDisable()
    {
        UIInventoryObject.SetActive(false);
    }

    public void StartInit()
    {
        Save.Instance.InitSaver(this);
    }

    public void SaveData(Save.Data data)
    {
        data.Inventory ??= new();
        data.Inventory.Clear();
        foreach (var pair in Inventory)
        {
            data.Inventory.Add(pair.Key.Identifier, pair.Value);
        }
    }

    public void LoadData(Save.Data data)
    {
        if (data.Inventory == null)
        {
            Inventory = new();
        }
        else
        {
            foreach (var pair in data.Inventory)
            {
                Inventory.Add(ItemManager.IdentifierToItem[pair.Key], pair.Value);
            }
        }
        UpdateUIItems();
    }
}
