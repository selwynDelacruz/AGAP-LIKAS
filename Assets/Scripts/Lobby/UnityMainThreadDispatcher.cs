using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lobby
{
    /// <summary>
    /// Dispatcher to execute actions on Unity's main thread
    /// Required because network callbacks happen on background threads
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                // Try to find existing instance
                _instance = FindObjectOfType<UnityMainThreadDispatcher>();
                
                if (_instance == null)
                {
                    // Create new instance
                    GameObject dispatcherObject = new GameObject("UnityMainThreadDispatcher");
                    _instance = dispatcherObject.AddComponent<UnityMainThreadDispatcher>();
                    DontDestroyOnLoad(dispatcherObject);
                    Debug.Log("[UnityMainThreadDispatcher] Created new instance");
                }
            }
            return _instance;
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[UnityMainThreadDispatcher] Instance initialized");
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[UnityMainThreadDispatcher] Duplicate instance destroyed");
                Destroy(gameObject);
            }
        }

        void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        public void Enqueue(Action action)
        {
            if (action == null)
            {
                Debug.LogWarning("[UnityMainThreadDispatcher] Attempted to enqueue null action");
                return;
            }

            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
