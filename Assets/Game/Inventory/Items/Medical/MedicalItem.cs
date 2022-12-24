using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Medical", menuName = "Item/Medical")]
public class MedicalItem : Item
{

    protected override string IdentifierStarter => "medical_";

    public float HealthFill;

}
