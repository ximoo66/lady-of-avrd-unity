// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-12 by Linus Ziesel
// Last Modified: 2026-07-20 by Linus Ziesel

﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GenNodeMan : MonoBehaviour
{
    private ModelHandler modelHandler;
    private ImageHandler imgHandler;
    private void Start()
    {
        AIManagerMain.OnServerStartedGen += SpawnNode;
        modelHandler = FindFirstObjectByType<ModelHandler>();
        imgHandler = FindFirstObjectByType<ImageHandler>();
    }

    private void SpawnNode(Dictionary<string, string> genMsgDict)
    {
        Debug.Log($"gen_type: {genMsgDict["gen_type"]}, prompt: {genMsgDict["prompt"]}");

        string prompt = genMsgDict["prompt"];
        string genType = genMsgDict["gen_type"];
        switch (genType)
        {
            case "mesh":
                if (modelHandler != null)
                {
                    modelHandler.SpawnModelNodeLoading(prompt);
                }
                else
                {
                    Debug.LogError("ModelHandler not found in scene.");
                }
                break;
            case "img":
                if (imgHandler != null)
                {
                    imgHandler.SpawnImgNodeLoading(prompt);
                }
                else
                {
                    Debug.LogError("ImageHandler not found in scene.");
                }
                break;
        }
    }
    
}
