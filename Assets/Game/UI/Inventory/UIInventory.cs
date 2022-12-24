using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    private static UIInventory instance;
    public static UIInventory Instance => instance;
    private static ItemManager ItemManager => ItemManager.Instance;


    public readonly Dictionary<Item, int> Inventory = new();




    private void Awake()
    {
        instance = this;
    }



    public void AddToInventory(Item item, int amt = 1)
    {
        if (Inventory.TryGetValue(item, out int count))
        {
            Inventory[item] = count + amt;
            return;
        }
        Inventory.Add(item, amt);
    }



}
