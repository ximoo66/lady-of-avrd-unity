// Authors: Linus Ziesel, Ekaterina Siling
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-09 by Linus Ziesel
// Last Modified: 2026-07-07 by Linus Ziesel

﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.UI;
using WebSocket = NativeWebSocket.WebSocket;
using WebSocketState = NativeWebSocket.WebSocketState;
using System.Linq;

public class NetworkConnection : Singleton<NetworkConnection>
{

    private string serverUrl = LocalConfig.serverUrl;
    public WebSocket websocket;
    private UTF8Encoding encoding = new();

    public event Action<string> OnImgReceived;
    // public event Action<string> OnTTSResultReceived;

    protected override void Awake()
    {
        base.Awake();
        SetupSocket();
    }

    private async void SetupSocket()
    {
#if UNITY_EDITOR
        serverUrl = LocalConfig.editorServerUrl; 
#endif
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () => Debug.Log("NetworkConnection: WS Connected");
        websocket.OnError += (e) => Debug.LogError("NetworkConnection: WS Error: " + e);
        websocket.OnClose += (e) => Debug.Log("NetworkConnection: WS Closed");
        // websocket.OnMessage += DistributeOnMsgType; gcc
       
        await websocket.Connect();
    }

    private void Update()
    {
        // if you make a WebGL build this needs to be removed or smth
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
    }

    /*
    private void DistributeOnMsgType(byte[] bytes)
    {
        if (bytes.Length == 0) return;
        if (bytes[0] == Config.HEADER_AUDIO)
        {
            // Debug.Log($"{this} does not handle audio yet");
            return;
        }
        
        // Skip the first byte (header)
        string jsonString = encoding.GetString(bytes, 1, bytes.Length - 1);
        ServerMessage msg = JsonConvert.DeserializeObject<ServerMessage>(jsonString);
        if (msg == null)
        {
            Debug.LogError($"{this}: msg was null");
            return;
        }

        switch (msg.type)
        {
            case Config.imgType:
                Debug.Log($"{this} received: {msg.type}");
                OnImgReceived.Invoke(msg.content);
                break;
            default:
                Debug.Log($"{this} received {msg.type}");
                break;
        }
           
    }
    private void OnDestroy()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.Close();
        }
    }
    */
}
