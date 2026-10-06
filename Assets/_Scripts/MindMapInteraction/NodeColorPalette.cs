//P6 - Expanded Realities
//Author: Ekaterina Siling
//Purpose: this script sets available colors for the node and connects to MindMapNodeAnimator to set the color

using UnityEngine;
using UnityEngine.UI;

public class NodeColorPalette : MonoBehaviour
{
    [SerializeField] private Color[] colors = new Color[]
    {
        new Color(0.90f, 0.30f, 0.30f), 
        new Color(0.90f, 0.60f, 0.20f), 
        new Color(0.90f, 0.85f, 0.20f), 
        new Color(0.30f, 0.80f, 0.40f),  
        new Color(0.30f, 0.60f, 1.00f), 
        new Color(0.47f, 0.33f, 0.75f), 
    };

    private MindMapNodeAnimator _animator;

    private void Awake()
    {
        _animator = GetComponentInParent<MindMapNodeAnimator>();

        Transform panel = transform.Find("Panel");
        Button[] buttons = panel.GetComponentsInChildren<Button>(includeInactive: true);


        for (int i = 0; i < buttons.Length && i < colors.Length; i++)
        {
            Color captured = colors[i];

            ColorSwatchButton swatch = buttons[i].gameObject.AddComponent<ColorSwatchButton>();
            swatch.SetColor(captured);

            buttons[i].onClick.AddListener(() =>
            {
                _animator.SetNodeColor(captured);
            });
        }
    }

}