// Authors: Noah Wendt
// Disclaimer: Written with support of AI tools.
// Created: 2026-07-11 by Noah Wendt
// Last Modified: 2026-07-11 by Noah Wendt

using UnityEngine;

namespace Haon.Utils
{
    /// <summary>
    /// A singleton pattern applied to a MonoBehaviour inheritor 
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        protected MonoSingleton(){} // Prevent instancing from outside of the class
        
        /// <summary>
        /// Readonly. The field to access the singleton. 
        /// </summary>
        public static T Instance => _instance; 

        private static T _instance;
        
        /// <summary>
        /// Overridable. Always call base.Awake to not loose singleton functionality in derived class. 
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(this.gameObject);
            }
            else
                Destroy(this.gameObject);
        }
        
        /// <summary>
        /// Destroys the game object the instance is attached to.
        /// Be careful with this. 
        /// </summary>
        public void Destroy()
        {
            Destroy(this.gameObject);
        }
    }
}