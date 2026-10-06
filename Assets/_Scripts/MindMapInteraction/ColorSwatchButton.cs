//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script is used for changing the color of the node. it sits on the UI element

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorSwatchButton : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Image _image;
    private Color _baseColor;
    [SerializeField] private float brightnessBoost = 0.1f;

    private void Awake()
    {
        _image     = GetComponent<Image>();
        _baseColor = _image.color;
    }

    public void SetColor(Color color)
    {
        _baseColor   = color;
        _image.color = color;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        _image.color = _baseColor + new Color(brightnessBoost, brightnessBoost, brightnessBoost);
    }

    public void OnPointerExit(PointerEventData e)
    {
        _image.color = _baseColor;
    }

    public void OnPointerClick(PointerEventData e)
    {
        _image.color = _baseColor + new Color(brightnessBoost*2, brightnessBoost*2, brightnessBoost*2);
    }
}