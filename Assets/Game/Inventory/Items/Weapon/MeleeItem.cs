using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeItem : WeaponItem
{
    protected override string IdentifierStarter => "melee_";

    public override bool Consumable => false;

    public override void Use()
    {
        Player.Instance.EquipWeapon(this);
    }
}
