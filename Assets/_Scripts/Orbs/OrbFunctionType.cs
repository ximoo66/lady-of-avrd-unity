// Authors: Omid Ameri
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-09 by Omid Ameri
// Last Modified: 2026-06-09 by Omid Ameri

// Course: P6

public enum OrbFunctionType
{
    None,

    // UI / information
    OpenScreen,
    ShowData,
    ShowExplanation,

    // AI generation
    GenerateAsset,
    GenerateText,
    BrainstormIdeas,

    // Mind map / structure
    CreateMindMap,
    AddMindMapNode,
    EditMindMap,

    // Environment / mixed reality
    GenerateEnvironment,
    EnterGeneratedEnvironment,
    ExitGeneratedEnvironment,
    SwitchToMixedReality,
    SwitchToVirtualReality,

    // Character behavior
    CharacterAnimation,
    CharacterExpression,
    CharacterVoiceLine,

    // World / scene interaction
    SpawnObject,
    HighlightObject,
    MoveToTarget,

    // System / testing
    DebugMessage,
    PlaySound,

    // Future flexible type
    Custom
}