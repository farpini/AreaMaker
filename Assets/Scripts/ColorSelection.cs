using UnityEngine;
using UnityEngine.UI;

public class ColorSelection : MonoBehaviour
{
    [SerializeField] private Button selectColorButton;
    [SerializeField] private Image selectColorImage;
    [SerializeField] private FlexibleColorPicker colorPicker;
    [SerializeField] private Color startColor;

    private Color currentColor;
    private AreaRenderer areaRenderer;

    private void Awake ()
    {
        selectColorButton.onClick.AddListener(() => OnSelectColorClicked());
        currentColor = startColor;
    }

    private void OnDestroy ()
    {
        selectColorButton.onClick.RemoveAllListeners();
    }

    private void Start ()
    {
        //colorPicker.color = currentColor;
        colorPicker.SetColor(currentColor);
        UpdateColor();
    }

    public void Initialize (AreaRenderer _areaRenderer)
    {
        areaRenderer = _areaRenderer;
        UpdateColor();
    }

    private void OnSelectColorClicked ()
    {
        colorPicker.gameObject.SetActive(!colorPicker.gameObject.activeSelf);
    }

    public void OnColorChanged (Color color)
    {
        currentColor = color;
        UpdateColor();
    }

    private void UpdateColor ()
    {
        selectColorImage.color = currentColor;

        if (areaRenderer != null)
        {
            areaRenderer.SetColor(currentColor);
        }
    }
}