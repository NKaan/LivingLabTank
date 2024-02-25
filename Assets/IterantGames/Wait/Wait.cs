using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MasterServerToolkit.Networking;
using MEC;

namespace SL.Wait
{
    public class Wait
    {
        private CoroutineHandle _handle;
        private WaitUnit _unit = WaitUnit.Frames;
        private Func<bool> _timingFunction;
        public Action<int> _runFunction = _ => { };
        private Action _onStart;

        private long _startedAt;
        internal float _timing = 1;
        public float _waitedFor;
        public float _numberOfTimes = 1;
        public int _repeated;
        private readonly List<Action<int>> _chainedFunctions = new List<Action<int>>();
        private readonly List<Wait> _chainedWaits = new List<Wait>();
        private readonly List<Chains> _chains = new List<Chains>();

        public List<Wait> chainedWaits => _chainedWaits;
        public List<Action<int>> chainedFunctions => _chainedFunctions;
        public List<Chains> chains => _chains;

        public string tag;
        public string group = "primary";

        public bool isComplete
        {
            get;
            private set;
        } = false;

        /// <summary>
        /// Create a new Wait object with a guid tag
        /// </summary>
        public Wait()
        {
            tag = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Create a new Wait object with the given tag
        /// </summary>
        /// <param name="tagName"></param>
        public Wait(string tagName)
        {
            tag = tagName;
        }

        /// <summary>
        /// Static Constructor
        /// Wait the specified number of frames before performing the given action.
        /// Start() needs to be called before anything happens, this only sets it up.
        /// </summary>
        /// <param name="timing">The number of frames to wait</param>
        /// <param name="action">The action to run after waiting</param>
        /// <returns>The created Wait object</returns>
        public static Wait Frames(float timing, Action action = null)
        {
            Wait wait = new Wait();
            wait.ForFrames(timing);

            if (action != null)
            {
                wait.Then(action);
            }

            return wait;
        }

        /// <summary>
        /// Static Constructor
        /// Wait the specified number of seconds before performing the given action.
        /// Uses Unity Time so will pause execution when the game is paused.
        /// Start() needs to be called before anything happens, this only sets it up.
        /// </summary>
        /// <param name="timing">The number of seconds to wait</param>
        /// <param name="action">The action to run after waiting</param>
        /// <returns>The created Wait object</returns>
        public static Wait Seconds(float timing, Action action = null)
        {
            Wait wait = new Wait();
            wait.ForSeconds(timing);

            if (action != null)
            {
                wait.Then(action);
            }

            return wait;
        }

        public Wait AgainSecondsReleace(float timing)
        {
            ForSeconds(timing);
            return this;
        }

        /// <summary>
        /// Static Constructor
        /// Wait the specified number of seconds before performing the given action.
        /// Checks the time elapsed using system time every frame, will continue to
        /// run even if Unity Time is paused.
        /// Start() needs to be called before anything happens, this only sets it up.
        /// </summary>
        /// <param name="timing">The number of seconds to wait</param>
        /// <param name="action">The action to run after waiting</param>
        /// <returns>The created Wait object</returns>
        public static Wait SecondsWhilePaused(float timing, Action action = null)
        {
            Wait wait = new Wait();
            wait.ForSecondsByFrame(timing);

            if (action != null)
            {
                wait.Then(action);
            }

            return wait;
        }

        /// <summary>
        /// Static Constructor
        /// Wait until the timing function returns true before performing the given action.
        /// Start() needs to be called before anything happens, this only sets it up.
        /// </summary>
        /// <param name="timingFunction">The function it will wait for</param>
        /// <param name="action">The action to run after waiting</param>
        /// <returns>The created Wait object</returns>
        public static Wait For(Func<bool> timingFunction, Action action = null)
        {
            Wait wait = new Wait();
            wait.Until(timingFunction);

            if (action != null)
            {
                wait.Then(action);
            }

            return wait;
        }

        /// <summary>
        /// Wait the specified number of frames before performing the configured action.
        /// Start() needs to be called before anything happens, this only sets it up.
        /// This will overwrite the existing timing
        /// </summary>
        /// <param name="timing">The number of frames to wait</param>
        /// <returns>The Wait object it was called on</returns>
        public Wait ForFrames(float timing)
        {
            _timing = timing;
            _unit = WaitUnit.Frames;
            return this;
        }

        /// <summary>
        /// Wait the specified number of seconds before performing the configured action.
        /// Uses Unity Time so will pause execution when the game is paused.
        /// Start() needs to be called before anything happens, this only sets it up.
        /// This will overwrite the existing timing
        /// </summary>
        /// <param name="timing">The number of seconds to wait</param>
        /// <returns>The Wait object it was called on</returns>
        public Wait ForSeconds(float timing)
        {
            _timing = timing;
            _unit = WaitUnit.Seconds;
            return this;
        }


        public Wait ForRepeatSeconds(float timing,Action action)
        {
            _timing = timing;
            _unit = WaitUnit.Seconds;

            Repeat(0);
            Then(action);

            return this;
        }

        /// <summary>
        /// Wait the specified number of seconds before performing the configured action.
        /// Checks the time elapsed using system time every frame, will continue to
        /// run even if Unity Time is paused.
        /// Start() needs to be called before anything happens, this only sets it up.
        /// This will overwrite the existing timing
        /// </summary>
        /// <param name="timing">The number of seconds to wait</param>
        /// <returns>The Wait object it was called on</returns>
        public Wait ForSecondsByFrame(float timing)
        {
            _timing = timing;
            _unit = WaitUnit.SecondsByFrame;
            return this;
        }

        /// <summary>
        /// Wait until the timing function returns true before performing the configured action.
        /// Start() needs to be called before anything happens, this only sets it up.
        /// </summary>
        /// <param name="timingFunction">The function it will wait for</param>
        /// <returns>The Wait object it was called on</returns>
        public Wait Until(Func<bool> timingFunction)
        {
            _timing = 1;
            _timingFunction = timingFunction;
            _unit = WaitUnit.Function;
            return this;
        }

        /// <summary>
        /// Sets the tag of the Wait object. This tag can be used to tell the WaitManager to
        /// stop, pause, or resume the wait object while it is running.
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns>The Wait object it was called on</returns>
        public Wait Tag(string tagName)
        {
            tag = tagName;
            return this;
        }

        /// <summary>
        /// Sets the group of the Wait object. This tag can be used to tell the WaitManager to
        /// stop, pause, or resume all wait objects in the group while they are running.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>The Wait object it was called on</returns>
        public Wait Group(string groupName)
        {
            if (groupName != "")
            {
                group = groupName;
            }

            return this;
        }

        /// <summary>
        /// Sets the wait object to repeat the specified number of times.
        /// Entering 0 will repeat indefinitely / until it is manually stopped.
        /// </summary>
        /// <param name="numberOfTimes">How many times to repeat</param>
        /// <returns>The Wait object it was called on</returns>
        public Wait Repeat(float numberOfTimes)
        {
            _numberOfTimes = numberOfTimes;
            return this;
        }

        bool _RepeatStop = false;
        public Wait RepeatStop()
        {
            _RepeatStop = true;
            return this;
        }

        /// <summary>
        /// Runs the given action after the wait has completed.
        /// This will run last, after the Then() or constructor function.
        /// </summary>
        /// <param name="nextAction">The action to run</param>
        /// <returns>The Wait object it was called on</returns>
        public Wait Chain(Action nextAction)
        {
            _chains.Add(Chains.Action);
            _chainedFunctions.Add(_ => nextAction());
            return this;
        }

        /// <summary>
        /// Sets the given wait object to start immediately after this one has completed.
        /// </summary>
        /// <param name="nextWait">The Wait object to run after this one</param>
        /// <returns>The Wait object it was called on</returns>
        public Wait Chain(Wait nextWait)
        {
            _chains.Add(Chains.Wait);
            _chainedWaits.Add(nextWait);
            return this;
        }

        /// <summary>
        /// The action to run once this Wait object is done waiting.
        /// </summary>
        /// <param name="action">The action to run after waiting</param>
        /// <returns>The Wait object it was called on</returns>
        public Wait Then(Action action)
        {
            _runFunction = _ => action();
            return this;
        }

        /// <summary>
        /// The action to run when Start() is called. This is mostly useful if
        /// you hold the Wait object instance somewhere after creation to run it later.
        /// </summary>
        /// <param name="action">The action to run on Start()</param>
        /// <returns>The Wait object it was called on</returns>
        public Wait OnStart(Action action)
        {
            _onStart = action;
            return this;
        }

        /// <summary>
        /// Runs the configured start action if it exists.
        /// </summary>
        public void RunStartAction()
        {
            _onStart?.Invoke();
        }

        /// <summary>
        /// Starts the Wait object and registers it with the WaitManager.
        /// </summary>
        /// <returns>The Wait object it was called on</returns>
        /// 
        public virtual Wait Start()
        {
            MstTimer.RunInMainThread(() => {
                _waitedFor = 0;
                _startedAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                RunStartAction();

                List<CoroutineHandle> handles;

                if (!WaitManager.instances.TryGetValue(group, out handles))
                {
                    handles = new List<CoroutineHandle>();
                    WaitManager.instances.Add(group, handles);
                }

                // remove the old if its still there.
                if (_handle.IsValid)
                {
                    handles.Remove(_handle);
                }

                isComplete = false;
                _handle = Timing.RunCoroutine(
                  Run(),
                  tag
                );

                handles.Add(_handle);
                WaitManager.instances[group] = handles;
            });

            return this;
        }
        bool isTask = false;
        public virtual Wait StartTask()
        {
            isTask = true;
            return Start();
        }

        /// <summary>
        /// Pauses the Wait object - eg Make it stop checking it's condition.
        /// This will not pause the timing of the wait function, it will just delay
        /// its completion until Resume() is called.
        /// 
        /// eg. You setup the wait Wait for 10 seconds and call Pause after 5 seconds.
        /// Calling Resume() after 3 seconds the Wait will complete 2 seconds later,
        /// once 10 seconds from start elapsed.
        /// Calling Resume() after 8 seconds the Wait will complete immediately since >10 seconds
        /// (13) have now elapsed since the start.
        /// 
        /// To Pause the timing completely please use Unity Time.timeScale = 0 to pause the game.
        /// </summary>
        /// <returns>The Wait object it was called on</returns>
        /// <exception cref="Exception">
        /// If the tag was removed/not set and _handle doesn't exist then we have no way of
        /// stopping the coroutine. _handle will be set when Start() is run and removed when it finishes.
        /// We cannot Pause something that is not running.
        /// </exception>
        public Wait Pause()
        {
            if (!string.IsNullOrEmpty(tag))
            {
                Timing.PauseCoroutines(tag);
            }
            else if (_handle.IsValid)
            {
                Timing.PauseCoroutines(_handle);
            }
            else
            {
                throw new Exception("Cannot pause the co-routine, it is not running");
            }

            return this;
        }

        /// <summary>
        /// Resumes the Paused Wait object - eg Make it start checking it's condition again.
        /// If the Waits' timing is complete when Resumed it will finish and run it's action immediately.
        /// </summary>
        /// <returns>The Wait object it was called on</returns>
        /// <exception cref="Exception">
        /// If the tag was removed/not set and _handle doesn't exist then we have no way of
        /// stopping the coroutine. _handle will be set when Start() is run and removed when it finishes.
        /// We cannot Resume something that is not running.
        /// </exception>
        public Wait Resume()
        {
            if (!string.IsNullOrEmpty(tag))
            {
                Timing.ResumeCoroutines(tag);
            }
            else if (_handle.IsValid)
            {
                Timing.ResumeCoroutines(_handle);
            }
            else
            {
                throw new Exception($"Cannot resume the co-routine, it is not running");
            }

            return this;
        }

        /// <summary>
        /// Stop the Wait object. This will terminate it and remove it from the WaitManager.
        /// </summary>
        /// <returns>The Wait object it was called on</returns>
        /// <exception cref="Exception">
        /// If the tag was removed/not set and _handle doesn't exist then we have no way of
        /// stopping the coroutine. _handle will be set when Start() is run and removed when it finishes.
        /// We cannot Stop something that is not running.
        /// </exception>
        public Wait Stop()
        {
            isComplete = true;
            if (!string.IsNullOrEmpty(tag))
            {
                Timing.KillCoroutines(tag);
            }
            else if (_handle.IsValid)
            {
                Timing.KillCoroutines(_handle);
            }
            else
            {
                throw new Exception("Cannot stop the co-routine, it is not running");
            }
            return this;
        }

        /// <summary>
        /// Internal Method, the saved action must accept an int to know how many times
        /// it has repeated to pass along to the next run. Actions set by the user
        /// shouldn't care about this.
        /// 
        /// Runs the given action after the wait has completed.
        /// This will run last, after the Then() or constructor function.
        /// </summary>
        /// <param name="nextAction">The action to run</param>
        /// <returns>The Wait object it was called on</returns>
        private Wait Chain(Action<int> nextAction)
        {
            _chains.Add(Chains.Action);
            _chainedFunctions.Add(nextAction);
            return this;
        }

        /// <summary>
        /// Internal Method, the saved action must accept an int to know how many times
        /// it has repeated to pass along to the next run. Actions set by the user
        /// shouldn't care about this.
        /// </summary>
        /// <param name="action">The action to run after waiting</param>
        /// <returns>The Wait object it was called on</returns>
        private Wait Then(Action<int> action)
        {
            _runFunction = action;
            return this;
        }

        /// <summary>
        /// This is the actual coroutine that is running. It handles the given timing
        /// type and once it's complete it runs the action and sets up the next chain
        /// or repeat if there is one.
        /// </summary>
        /// <returns>The Wait object it was called on</returns>
        /// <exception cref="Exception">
        ///   If there is no run function provided by either the constructor or the Then() method.
        ///   OR
        ///   If you wait until/for something but no timing function has been set by the
        ///   constructor or Until() method.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   If you somehow give it a timing that doesn't exist
        /// </exception>
        private IEnumerator<float> Run()
        {
            if (_runFunction == null)
            {
                throw new Exception("You must provide a function to run.");
            }

            while (_waitedFor < _timing)
            {
                switch (_unit)
                {
                    case WaitUnit.Function:
                        {
                            yield return Timing.WaitForOneFrame;
                            if (_timingFunction != null)
                            {
                                if (_timingFunction.Invoke())
                                {
                                    _waitedFor = _timing;
                                }
                            }
                            else
                            {
                                throw new Exception("You must provide a timing function that returns a boolean.");
                            }

                            break;
                        }
                    case WaitUnit.Seconds:
                        {
                            yield return Timing.WaitForSeconds(_timing);
                            _waitedFor = _timing;
                            break;
                        }
                    case WaitUnit.SecondsByFrame:
                        {
                            // Used to wait a specific amount of time while the game is paused
                            // Pausing will stop Timing.WaitForSeconds from updating
                            yield return Timing.WaitForOneFrame;
                            long now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                            _waitedFor = (now - _startedAt) / 1000.0f;
                            break;
                        }
                    case WaitUnit.Frames:
                        {
                            yield return Timing.WaitForOneFrame;
                            _waitedFor++;
                            break;
                        }
                    default:
                        {
                            yield return Timing.WaitForOneFrame;
                            _waitedFor++;
                            break;
                        }
                }
            }

            if (isTask)
                Task.Run(() => _runFunction.Invoke(_repeated));
            else
                _runFunction.Invoke(_repeated);

            _repeated++;

            bool removeOldRoutine = true;

            bool shouldRepeat = _numberOfTimes == 0 ||
                                 _repeated < _numberOfTimes;
            if (shouldRepeat && !_RepeatStop)
            {
                _waitedFor = 0;
                removeOldRoutine = false;
                Start();
            }
            else if (_chains.Count > 0)
            {
                Chains nextChain = _chains[0];
                _chains.RemoveAt(0);

                switch (nextChain)
                {
                    case Chains.Action:
                        {
                            _runFunction = _chainedFunctions[0];
                            _chainedFunctions.RemoveAt(0);
                            removeOldRoutine = false;
                            // Will make it run immediately since we're not updating _waitedFor
                            Start();
                            break;
                        }
                    case Chains.Wait:
                        {
                            ChainWait();
                            yield return Timing.WaitUntilDone(
                              _handle
                            );
                            break;
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException(
                              nameof(nextChain),
                              nextChain,
                              null
                            );
                        }
                }
            }

            // Remove finished co-routines
            isComplete = true;
            List<CoroutineHandle> handles;
            if (removeOldRoutine && WaitManager.instances.TryGetValue(group, out handles))
            {
                handles.Remove(_handle);
            }
        }

        /// <summary>
        /// If a chained Wait object has been configured this will set it up to run the next
        /// Wait object such that this object's handle will point to the chain wait instead of this one.
        /// This allows you to run normal operations like Pause and Stop on the base object and have
        /// it effect the chained Wait object.
        /// </summary>
        private void ChainWait()
        {
            Wait nextWait = _chainedWaits[0];
            _chainedWaits.RemoveAt(0);

            List<CoroutineHandle> handles;
            if (!WaitManager.instances.TryGetValue(group, out handles))
            {
                handles = new List<CoroutineHandle>();
                WaitManager.instances.Add(group, handles);
            }

            handles.Remove(_handle);

            nextWait.chains.AddRange(_chains);
            nextWait.chainedWaits.AddRange(_chainedWaits);
            nextWait.chainedFunctions.AddRange(_chainedFunctions);

            nextWait.RunStartAction();
            _handle = Timing.RunCoroutine(
              nextWait.Run(),
              nextWait.tag
            );

            handles.Add(_handle);
        }
    }

}
