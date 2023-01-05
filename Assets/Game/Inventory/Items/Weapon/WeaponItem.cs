using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : Item
{
    protected override string IdentifierStarter => "weapon_";

    public override bool Consumable => false;

    public float Range;
    public int Damage;

    public override void Use()
    {
        Player.Instance.EquipWeapon(this);
    }
}
