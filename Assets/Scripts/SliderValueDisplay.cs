using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueDisplay : MonoBehaviour
{
    public Slider slider;
    public TMP_Text valueText;

    void Start()
    {
        slider.onValueChanged.AddListener(UpdateValueText);
        UpdateValueText(slider.value);
    }

    void UpdateValueText(float value)
    {
        valueText.text = value.ToString("0.00");
    }
}