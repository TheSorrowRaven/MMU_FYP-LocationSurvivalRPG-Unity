using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class SkillSet
{
    public int Health;
    public int Hunger;
    public int Energy;
    public int Zombification;
    public int MeleeDamage;
    public int RangedDamage;

    public SkillSet()
    {

    }
    public SkillSet(SkillSet copy)
    {
        Health = copy.Health;
        Hunger = copy.Hunger;
        Energy = copy.Energy;
        Zombification = copy.Zombification;
        MeleeDamage = copy.MeleeDamage;
        RangedDamage = copy.RangedDamage;
    }

    public IEnumerable<int> EnumerateSkills()
    {
        yield return Health;
        yield return Hunger;
        yield return Energy;
        yield return Zombification;
        yield return MeleeDamage;
        yield return RangedDamage;
    }
    public void SetSkillValue(int index, int value)
    {
        switch (index)
        {
            case 0: Health = value; break;
            case 1: Hunger = value; break;
            case 2: Energy = value; break;
            case 3: Zombification = value; break;
            case 4: MeleeDamage = value; break;
            case 5: RangedDamage = value; break;
        }
    }
    public int GetSkillValue(int index)
    {
        return index switch
        {
            0 => Health,
            1 => Hunger,
            2 => Energy,
            3 => Zombification,
            4 => MeleeDamage,
            5 => RangedDamage,
            _ => 0,
        };
    }

}

public class UISkills : MonoBehaviour
{
    private static UISkills instance;
    public static UISkills Instance => instance;

    [SerializeField] private GameObject SkillsObj;
    [SerializeField] private TextMeshProUGUI LevelText;

    [SerializeField] private TextMeshProUGUI ExpCurrentText;
    [SerializeField] private TextMeshProUGUI ExpRequiredText;

    [SerializeField] private TextMeshProUGUI SkillPointsText;
    [SerializeField] private List<UISingleSkill> Skills;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < Skills.Count; i++)
        {
            Skills[i].index = i;
        }
        SkillsForceDisable();
    }


    public void SkillsButtonClicked()
    {
        SkillsObj.SetActive(!SkillsObj.activeSelf);
    }
    public void SkillsForceDisable()
    {
        SkillsObj.SetActive(false);
    }

    public void SetLevel(int level)
    {
        LevelText.SetText("Level " + level);
    }


    public void SetExperience(int exp, int? expReq)
    {
        ExpCurrentText.SetText(exp.ToString());
        if (expReq == null)
        {
            ExpRequiredText.SetText("MAX");
        }
        else
        {
            ExpRequiredText.SetText(expReq.ToString());
        }
    }

    public void SetSkillPoints(int skillPoints)
    {
        SkillPointsText.SetText(skillPoints.ToString());
        if (skillPoints == 0)
        {
            for (int i = 0; i < Skills.Count; i++)
            {
                Skills[i].SetButtonActive(false);
            }
        }
        else
        {
            for (int i = 0; i < Skills.Count; i++)
            {
                Skills[i].SetButtonActive(true);
            }
        }
    }

    public void SetSkillSet(SkillSet skillSet)
    {
        int index = 0;
        foreach (int value in skillSet.EnumerateSkills())
        {
            Skills[index].SetValue(value);
            index++;
        }
    }

    public void SkillIncreased(int index)
    {
        Player.Instance.SkillIncreased(index);
    }

}
