using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderPercentText : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI label;

    void Start()
    {
        UpdateText(slider.value);
        slider.onValueChanged.AddListener(UpdateText);
    }

    void UpdateText(float value)
    {
        label.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
