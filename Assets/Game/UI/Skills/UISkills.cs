using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISkills : MonoBehaviour
{
    private static UISkills instance;
    public static UISkills Instance => instance;

    [SerializeField] private GameObject SkillsObj;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }


    public void SkillsButtonClicked()
    {
        SkillsObj.SetActive(!SkillsObj.activeSelf);
    }
    public void SkillsForceDisable()
    {
        SkillsObj.SetActive(false);
    }

}
