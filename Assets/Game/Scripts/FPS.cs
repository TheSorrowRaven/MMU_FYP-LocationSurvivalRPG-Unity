using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPS : MonoBehaviour
{

    public TextMeshProUGUI text;

    private void Update()
    {
        if (Time.frameCount % 10 == 0)
        text.SetText((1f / Time.deltaTime).ToString());
    }
}
