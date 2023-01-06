using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UINav : MonoBehaviour
{

    

    public void InventoryButtonClicked()
    {
        UIInventory.Instance.InventoryButtonClicked();

        UISkills.Instance.SkillsForceDisable();
    }
    public void SkillsButtonClicked()
    {
        UISkills.Instance.SkillsButtonClicked();

        UIInventory.Instance.InventoryForceDisable();
    }

}
