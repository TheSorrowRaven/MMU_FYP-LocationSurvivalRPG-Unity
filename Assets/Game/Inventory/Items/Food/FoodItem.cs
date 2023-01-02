using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "Item/Food")]
public class FoodItem : Item
{

    protected override string IdentifierStarter => "food_";

    public int HungerFill;
    public int StaminaRestore;

    public override void Use()
    {
        Player.Instance.FoodEaten(this);
    }

}
