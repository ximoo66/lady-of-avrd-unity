// Authors: Linus Ziesel, Ekaterina Siling
// Disclaimer: Written with support of AI tools.
// Created: 2026-05-07 by Linus Ziesel
// Last Modified: 2026-05-31 by Ekaterina Siling

﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Registers and executes methods marked with the [Tool] attribute based on events received from the AI backend.
/// </summary>
[RequireComponent(typeof(AIManagerMain))]
public class ToolCallManager : MonoBehaviour
{
    private AIManagerMain _aiManagerMain;
    private readonly Dictionary<string, ToolDefinition> toolRegistry = new Dictionary<string, ToolDefinition>();

    private void Start()
    {
        _aiManagerMain = GetComponent<AIManagerMain>();
        if (_aiManagerMain == null)
        {
            Debug.LogError("FATAL: ToolCallManager cannot find the AIManagerMain component.", this);
            this.enabled = false;
            return;
        }

        DiscoverTools();
        AIManagerMain.OnToolCallReceived += HandleToolCall;
        AIManagerMain.OnToolCallWithArgsReceived += HandleToolCallWithArgs;

    }


    private void OnDestroy()
    {
        if (_aiManagerMain != null)
        {
            AIManagerMain.OnToolCallReceived -= HandleToolCall;
            AIManagerMain.OnToolCallWithArgsReceived -= HandleToolCallWithArgs;

        }
    }


    private void DiscoverTools()
    {
        toolRegistry.Clear();

        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var mb in allBehaviours)
        {
            var methods = mb.GetType().GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var toolAttr = method.GetCustomAttribute<ToolAttribute>();
                if (toolAttr == null)
                    continue;

                try
                {
                    var definition = new ToolDefinition(method, mb);

                    if (toolRegistry.ContainsKey(definition.Name))
                    {
                        Debug.LogWarning($"Tool '{definition.Name}' already registered. Skipping duplicate on {mb.name}.");
                        continue;
                    }

                    toolRegistry.Add(definition.Name, definition);
                    Debug.Log($"<color=lightblue>Registered Tool: {definition.Name} on {mb.GetType().Name}</color>");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to register tool '{method.Name}' on {mb.name}: {e.Message}", this);
                }
            }
        }

        Debug.Log($"Tool discovery complete. Registered {toolRegistry.Count} tools.");
    }

    private void HandleToolCall(string toolName)    {

        if (toolRegistry.TryGetValue(toolName, out ToolDefinition toolDef))
        {
            Debug.Log($"<color=yellow>Invoking Tool: {toolName}</color>");

            toolDef.Invoke();
        }
        else
        {
            Debug.LogError($"Tool '{toolName}' was called but is not registered or found.", this);
        }
    }
    private void HandleToolCallWithArgs(string toolName, Dictionary<string, string> args)
    {
        if (toolRegistry.TryGetValue(toolName, out ToolDefinition toolDef))
            toolDef.Invoke(args);
        else
            Debug.LogError($"Tool '{toolName}' not registered.");
    }

}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ToolAttribute : Attribute
{
    public string Description { get; }
    public ToolAttribute(string description) { Description = description; }
}

public class ToolDefinition
{
    public MethodInfo Method { get; }
    public object Instance { get; }
    public string Name => Method.Name;
    private readonly ParameterInfo[] _params;

    public ToolDefinition(MethodInfo method, object instance)
    {
        Method   = method;
        Instance = instance;
        _params  = method.GetParameters();

        // allow 0 or more parameters, but only strings
        foreach (var p in _params)
        {
            if (p.ParameterType != typeof(string))
            {
                throw new ArgumentException(
                    $"Tool method '{method.Name}' parameter '{p.Name}' must be of type string.");
            }
        }
    }

    public void Invoke()
    {
        Method.Invoke(Instance, null);
    }

    public void Invoke(Dictionary<string, string> args)
    {
        Debug.Log("Invoke MINDMAP CALL");
        var invokeArgs = new object[_params.Length];

        for (int i = 0; i < _params.Length; i++)
        {
            var p = _params[i];

            if (args != null && args.TryGetValue(p.Name, out string val))
            {
                invokeArgs[i] = val;
            }
            else if (p.HasDefaultValue)
            {
                invokeArgs[i] = p.DefaultValue;
            }
            else
            {
                Debug.LogWarning($"Tool '{Name}': missing arg '{p.Name}', using empty string.");
                invokeArgs[i] = string.Empty;
            }
        }

        Method.Invoke(Instance, invokeArgs);
    }
}