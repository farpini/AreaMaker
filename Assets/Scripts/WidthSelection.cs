using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WidthSelection : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private int startWidth;
    [SerializeField] private int widthMinValue;
    [SerializeField] private int widthMaxValue;
    [SerializeField] private TextMeshProUGUI widthTextValue;

    private AreaRenderer areaRenderer;
    private int currentWidth;

    private void Awake ()
    {
        currentWidth = startWidth;
        slider.value = currentWidth;
        slider.minValue = widthMinValue;
        slider.maxValue = widthMaxValue;
        OnSliderChanged(currentWidth);
    }

    private void OnDestroy ()
    {
    }

    public void Initialize (AreaRenderer _areaRenderer)
    {
        areaRenderer = _areaRenderer;
        UpdateWidth();
    }

    public void OnSliderChanged (int sliderValue)
    {
        currentWidth = (int)slider.value;
        UpdateWidth();
    }

    private void UpdateWidth ()
    {
        widthTextValue.text = currentWidth.ToString();

        if (areaRenderer != null)
        {
            areaRenderer.SetWidth(currentWidth);
        }
    }
}