using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SIDGIN.Patcher.Client
{
    public class SGDispatcher : MonoBehaviour
    {
        static Queue<Action> actions = new Queue<Action>();
        static SGDispatcher _instance;
        protected static SGDispatcher instance
        {
            get
            {

                if (_instance == null)
                {
                    var newObject = new GameObject("SGDispatcher").AddComponent<SGDispatcher>();
                    _instance = newObject;

                }

                return _instance;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var newObject = new GameObject("SGDispatcher").AddComponent<SGDispatcher>();
            _instance = newObject;
        }
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        public void Update()
        {
            if (actions.Count != 0)
            {
                while (actions.Count != 0)
                {
                    var action = actions.Dequeue();
                    if (action != null)
                    {
                        action.Invoke();
                    }

                }
            }
        }

        public static void Register(Action action)
        {
            if (action != null)
            {
                actions.Enqueue(action);
            }
        }
        public static void RunCorotune(IEnumerator enumerator)
        {
            instance.StartCoroutine(enumerator);

        }
    }

}