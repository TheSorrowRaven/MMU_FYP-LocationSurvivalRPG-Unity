using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private static ItemManager instance;
    public static ItemManager Instance => instance;

    public Sprite CommonSprite;
    public Sprite UncommonSprite;
    public Sprite RareSprite;
    public Sprite EpicSprite;

    public List<Item> Items = new();
    public readonly Dictionary<Rarity, Sprite> RarityToSprite = new();
    public readonly Dictionary<string, Item> IdentifierToItem = new();

    private void Awake()
    {
        instance = this;
        Setup();
    }

    private void Setup()
    {
        RarityToSprite.Clear();
        RarityToSprite.Add(Rarity.Common, CommonSprite);
        RarityToSprite.Add(Rarity.Uncommon, UncommonSprite);
        RarityToSprite.Add(Rarity.Rare, RareSprite);
        RarityToSprite.Add(Rarity.Epic, EpicSprite);

        IdentifierToItem.Clear();
        for (int i = 0; i < Items.Count; i++)
        {
            IdentifierToItem.Add(Items[i].Identifier, Items[i]);
        }
    }



}
