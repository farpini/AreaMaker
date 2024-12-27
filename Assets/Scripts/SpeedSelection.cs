using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedSelection : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private int startSpeed;
    [SerializeField] private int speedMinValue;
    [SerializeField] private int speedMaxValue;
    [SerializeField] private TextMeshProUGUI speedTextValue;

    private AreaRenderer areaRenderer;
    private int currentSpeed;

    private void Awake ()
    {
        currentSpeed = startSpeed;
        slider.value = currentSpeed;
        slider.minValue = speedMinValue;
        slider.maxValue = speedMaxValue;
        OnSliderChanged(currentSpeed);
    }

    private void OnDestroy ()
    {
    }

    public void Initialize (AreaRenderer _areaRenderer)
    {
        areaRenderer = _areaRenderer;
        UpdateSpeed();
    }

    public void OnSliderChanged (int sliderValue)
    {
        currentSpeed = (int)slider.value;
        UpdateSpeed();
    }

    private void UpdateSpeed ()
    {
        speedTextValue.text = currentSpeed.ToString();

        if (areaRenderer != null)
        {
            areaRenderer.SetSpeed(currentSpeed);
        }
    }
}