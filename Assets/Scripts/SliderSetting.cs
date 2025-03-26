using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderSetting : MonoBehaviour
{
    Slider slider;
    TMP_InputField input;

    public void Init(float value, UnityAction<float> action)
    {
        slider = GetComponentInChildren<Slider>();
        input = GetComponentInChildren<TMP_InputField>();

        slider.value = value;
        slider.onValueChanged.AddListener(action);

        OnSliderChanged(slider.value);
        slider.onValueChanged.AddListener(OnSliderChanged);
        input.onValueChanged.AddListener(OnInputChanged);
        input.onEndEdit.AddListener(_ => OnEndEdit());
    }

    private void OnInputChanged(string value) 
    {
        if (float.TryParse(value, out var result))
        {
            slider.value = result;
        }
    }

    private void OnEndEdit()
    {
        input.text = slider.value.ToString("n1");
    }

    private void OnSliderChanged(float value)
    {
        if (input.isFocused) return;
        input.text = value.ToString("n1");
    }
}
