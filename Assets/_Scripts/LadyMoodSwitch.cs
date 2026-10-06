// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-11 by Noah Wendt
// Last Modified: 2026-07-18 by Noah Wendt

using System;
using System.Collections.Generic;
using System.Globalization;
using Haon.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class LadyMoodSwitch : MonoSingleton<LadyMoodSwitch>
{
    
    public Material darkMat, lightMat, hairMat, faceMat;
    private Dictionary<string, Color> _darkHighlightColors,  _lightHighlightColors, _faceFadeColors;
    private Dictionary<string, Texture> _eyeTextures;
    public Texture purpleTex, pinkTex, redTex, blueTex, cyanTex, greenTex, yellowTex, orangeTex;
    private string eyesID = "_Eyes";
    void Start()
    {
        _darkHighlightColors = new Dictionary<string, Color>();
        _lightHighlightColors = new Dictionary<string, Color>();
        _faceFadeColors = new Dictionary<string, Color>();
        _eyeTextures = new Dictionary<string, Texture>();

        // Purple
        _darkHighlightColors["purple"] = ColorFromHex("#2B0024");
        _lightHighlightColors["purple"] = ColorFromHex("#C183B7");
        _faceFadeColors["purple"] = ColorFromHex("#C183B7");
        _eyeTextures["purple"] = purpleTex;        

        // Pink
        _darkHighlightColors["pink"] = ColorFromHex("#FF78CA");
        _lightHighlightColors["pink"] = ColorFromHex("#DDA0C5");
        _faceFadeColors["pink"] = ColorFromHex("#FF78CA");
        _eyeTextures["pink"] = pinkTex;
        // Red
        _darkHighlightColors["red"] = ColorFromHex("#8A1A00");
        _lightHighlightColors["red"] = ColorFromHex("#E0694D");
        _faceFadeColors["red"] = ColorFromHex("#8A1A00");
        _eyeTextures["red"] = redTex;

        // Blue
        _darkHighlightColors["blue"] = ColorFromHex("#447EA6");
        _lightHighlightColors["blue"] = ColorFromHex("#8FBBE3");
        _faceFadeColors["blue"] = ColorFromHex("#447EA6");
        _eyeTextures["blue"] = blueTex;

        // Cyan
        _darkHighlightColors["cyan"] = ColorFromHex("#33A89C");
        _lightHighlightColors["cyan"] = ColorFromHex("#7AE9DE");
        _faceFadeColors["cyan"] = ColorFromHex("#33A89C");
        _eyeTextures["cyan"] = cyanTex;

        // Green
        _darkHighlightColors["green"] = ColorFromHex("#559A5D");
        _lightHighlightColors["green"] = ColorFromHex("#96CC9C");
        _faceFadeColors["green"] = ColorFromHex("#559A5D");
        _eyeTextures["green"] = greenTex;
        // Yellow
        _darkHighlightColors["yellow"] = ColorFromHex("#FFD256");
        _lightHighlightColors["yellow"] = ColorFromHex("#F3CC88");
        _faceFadeColors["yellow"] = ColorFromHex("#FFD256");
        _eyeTextures["yellow"] = yellowTex;
        // Orange
        _darkHighlightColors["orange"] = ColorFromHex("#CD7E2C");
        _lightHighlightColors["orange"] = ColorFromHex("#FDB166");
        _faceFadeColors["orange"] = ColorFromHex("#CD7E2C");
        _eyeTextures["orange"] = orangeTex;
        // Set Default
        darkMat.SetColor("_HighlightsDarkColor", _darkHighlightColors["purple"]);
        lightMat.SetColor("_HighlightsDarkColor", _lightHighlightColors["purple"]);
        faceMat.SetColor("_FadeColor", _faceFadeColors["purple"]);
        hairMat.SetColor("_HairHightlightColor", _darkHighlightColors["purple"]);
        faceMat.SetTexture(eyesID, _eyeTextures["green"]);

    }

    [Button]
    public void ChangeColor(string key)
    {
        darkMat.SetColor("_HighlightsDarkColor", _darkHighlightColors[key]);
        lightMat.SetColor("_HighlightsDarkColor", _lightHighlightColors[key]);
        faceMat.SetColor("_FadeColor", _faceFadeColors[key]);
        hairMat.SetColor("_HairHightlightColor", _darkHighlightColors[key]);
        
        faceMat.SetTexture(eyesID, _eyeTextures[key]);
        
    }
    
    public static Color ColorFromHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var colFromHex))
            return colFromHex;
        Debug.LogError($"Error tyring to parse hex: {hex}");
        return Color.gray1;
        
    }
}
