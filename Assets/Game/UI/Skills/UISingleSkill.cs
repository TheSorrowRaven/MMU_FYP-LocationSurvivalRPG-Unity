using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISingleSkill : MonoBehaviour
{

    [SerializeField] private Button IncreaseButton;
    [SerializeField] private TextMeshProUGUI ValueText;


    [System.NonSerialized] public int index;


    private void Awake()
    {
        IncreaseButton.onClick.AddListener(IncreaseStatClicked);   
    }

    public void SetButtonActive(bool active)
    {
        IncreaseButton.interactable = active;
    }

    public void SetValue(int value)
    {
        ValueText.SetText(value.ToString());
    }

    private void IncreaseStatClicked()
    {
        UISkills.Instance.SkillIncreased(index);
    }

}
