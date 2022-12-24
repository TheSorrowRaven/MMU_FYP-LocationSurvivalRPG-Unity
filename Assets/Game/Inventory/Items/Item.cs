using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic
}

public abstract class Item : ScriptableObject
{

    public string Identifier;
    public string Name;
    public Sprite Icon;
    public Rarity Rarity;

    protected abstract string IdentifierStarter { get; }

    protected virtual void OnValidate()
    {
        if (!Identifier.StartsWith(IdentifierStarter))
        {
            Identifier = IdentifierStarter + Identifier;
        }
    }


}

public struct ItemAmt
{
    public Item item;
    public int amt;

    public ItemAmt(Item item, int amt)
    {
        this.item = item;
        this.amt = amt;
    }
}
