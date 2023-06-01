using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ammo")]
public class AmmoItem : Item
{
    protected override string IdentifierStarter => "ammo_";

    public override bool Consumable => false;

    public override void Use()
    {

    }
}
