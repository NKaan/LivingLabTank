using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
namespace SIDGIN.Common.Editors
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

        public static AsyncDispatch Dispatch(Action task)
        {
            lock (dispatchQueue)
            {
                AsyncDispatch dispatch = new AsyncDispatch();
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
    public class AsyncDispatch : CustomYieldInstruction
    {
        public bool IsDone { get; private set; }
        public override bool keepWaiting { get { return !IsDone; } }


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

    }
}