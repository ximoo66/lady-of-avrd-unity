// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-07 by Linus Ziesel
// Last Modified: 2026-07-08 by Linus Ziesel

﻿using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Materials; 
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class FileUtils
{
    public static string SaveImageB64(string contentB64)
    {
        byte[] imageBytes = Convert.FromBase64String(contentB64);
        string outDir = Path.Combine(Application.dataPath, "..", "tmp_imgs");
        Directory.CreateDirectory(outDir);
        string filename = Path.Combine(outDir, $"img_{DateTimeOffset.Now.ToUnixTimeSeconds()}.png");
        File.WriteAllBytes(filename, imageBytes);
        return filename;
    }

    public static string SaveModelB64(string contentB64)
    {
        byte[] modelBytes = Convert.FromBase64String(contentB64);
        string outDir = Path.Combine(Application.dataPath, "..", "tmp_models");
        Directory.CreateDirectory(outDir);
        string filename = Path.Combine(outDir, $"model_{DateTimeOffset.Now.ToUnixTimeSeconds()}.glb");
        File.WriteAllBytes(filename, modelBytes);
        return filename;
    }
}