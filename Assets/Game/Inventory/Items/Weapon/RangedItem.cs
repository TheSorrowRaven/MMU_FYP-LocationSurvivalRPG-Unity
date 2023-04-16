using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ranged")]
public class RangedItem : WeaponItem
{
    protected override string IdentifierStarter => "ranged_";

    public override bool Consumable => false;

    public override void Use()
    {
        Player.Instance.EquipWeapon(this);
    }
}
