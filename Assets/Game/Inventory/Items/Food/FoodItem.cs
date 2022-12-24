using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "Item/Food")]
public class FoodItem : Item
{

    protected override string IdentifierStarter => "food_";

    public float HungerFill;

}
