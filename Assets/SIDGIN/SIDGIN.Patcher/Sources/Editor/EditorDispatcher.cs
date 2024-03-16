using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace SIDGIN.Patcher.Editors
{
    public static class EditorDispatcher
    {
        private static readonly Queue<Action> dispatchQueue = new Queue<Action>();
        private static double timeSliceLimit = 10.0; // in miliseconds
        private static Stopwatch timer;

        static EditorDispatcher()
        {
            EditorApplication.update += Update;
            timer = new Stopwatch();
        }

        private static void Update()
        {
            lock (dispatchQueue)
            {
                int dispatchCount = 0;

                timer.Reset();
                timer.Start();

                while (dispatchQueue.Count > 0 && (timer.Elapsed.TotalMilliseconds <= timeSliceLimit))
                {
                    dispatchQueue.Dequeue().Invoke();

                    dispatchCount++;
                }

                timer.Stop();

            }
        }

        /// <summary>
        /// Send an Action Delegate to be run on the main thread. See EditorDispatchActions for some common usecases.
        /// </summary>
        /// <param name="task">An action delegate to run on the main thread</param>
        /// <returns>An AsyncDispatch that can be used to track if the dispatch has completed.</returns>
        public static AsyncDispatch Dispatch(Action task)
        {
            lock (dispatchQueue)
            {
                AsyncDispatch dispatch = new AsyncDispatch();

                // enqueue a new task that runs the supplied task and completes the dispatcher 
                dispatchQueue.Enqueue(() => { task(); dispatch.FinishedDispatch(); });

                return dispatch;
            }
        }



        private static IEnumerator DispatchCorotine(IEnumerator dispatched, AsyncDispatch tracker)
        {
            yield return dispatched;
            tracker.FinishedDispatch();
        }
    }

    /// <summary>
    /// Represents the progress of the dispatched action. Can be yielded to in a coroutine.
    /// If not using coroutines, look at the IsDone property to find out when its okay to proceed.
    /// </summary>
    public class AsyncDispatch : CustomYieldInstruction
    {
        public bool IsDone { get; private set; }
        public override bool keepWaiting { get { return !IsDone; } }


        /// <summary>
        /// Flags this dispatch as completed.
        /// </summary>
        internal void FinishedDispatch()
        {
            IsDone = true;
        }
    }


    public static class EditorDispatchActions
    {
        #region play mode
        public static void TogglePlayMode()
        {
            EditorApplication.isPlaying = !EditorApplication.isPlaying;
        }
        public static void EnterPlayMode()
        {
            EditorApplication.isPlaying = true;
        }
        public static void ExitPlayMode()
        {
            EditorApplication.isPlaying = false;
        }

        public static void TogglePausePlayMode()
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }
        public static void PausePlayMode()
        {
            EditorApplication.isPaused = true;
        }
        public static void UnpausePlayMode()
        {
            EditorApplication.isPaused = false;
        }

        public static void Step()
        {
            EditorApplication.Step();
        }
        #endregion

        public static void Build()
        {
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget), EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
        }

        public static void Beep()
        {
            EditorApplication.Beep();
        }

        public static void TestMessage()
        {
            UnityEngine.Debug.Log("Message Dispatched.");
        }

        // todo create texture, load asset? Does AssetDatabase need to be dispatched? Expand me!
    }
}