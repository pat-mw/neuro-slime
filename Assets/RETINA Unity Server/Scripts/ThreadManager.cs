using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class allows us to schedule code to run on a single specific thread
 * This is simpler than using multiple threads and avoids unforseen errors 
 */


namespace RetinaNetworking.Server
{
    public class ThreadManager : MonoBehaviour
    {
        private static readonly List<Action> executeOnMainThread = new List<Action>();
        private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
        private static bool actionToExecuteOnMainThread = false;

        private void Awake()
        {
            Time.fixedDeltaTime = ServerConstants.S_PER_TICK;
        }

        private void FixedUpdate()
        {
            UpdateMain();
        }
        

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="_action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread(Action _action)
        {
            if (_action == null)
            {
                Console.WriteLine("No action to execute on main thread!");
                return;
            }

            lock (executeOnMainThread)
            {
                executeOnMainThread.Add(_action);
                actionToExecuteOnMainThread = true;
            }
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        public static void UpdateMain()
        {
            if (actionToExecuteOnMainThread)
            {
                executeCopiedOnMainThread.Clear();
                lock (executeOnMainThread)
                {
                    executeCopiedOnMainThread.AddRange(executeOnMainThread);
                    executeOnMainThread.Clear();
                    actionToExecuteOnMainThread = false;
                }

                for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
                {
                    executeCopiedOnMainThread[i]();
                }
            }
        }
    }
}