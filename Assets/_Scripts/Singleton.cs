// Authors: Linus Ziesel
// Disclaimer: Written with support of AI tools.
// Created: 2026-06-09 by Linus Ziesel
// Last Modified: 2026-06-09 by Linus Ziesel

using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance;

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}