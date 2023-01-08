using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour
{
    private static UIStats instance;
    public static UIStats Instance => instance;

    [SerializeField] private Slider[] StatSliders;
    [SerializeField] private TextMeshProUGUI LevelText;

    [SerializeField] private TextMeshProUGUI ExpCurrentText;
    [SerializeField] private TextMeshProUGUI ExpRequiredText;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void SetSliderValue(int index, int value)
    {
        StatSliders[index].value = value;
    }

    public void SetSliderMax(int index, int max)
    {
        StatSliders[index].maxValue = max;
        RectTransform rt = StatSliders[index].GetComponent<RectTransform>();
        rt.sizeDelta = new(rt.sizeDelta.x, max);
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



}
