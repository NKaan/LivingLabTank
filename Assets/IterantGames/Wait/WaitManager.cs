using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MEC;

namespace SL.Wait
{
  public static class WaitManager
  {
    public static readonly Dictionary<string, List<CoroutineHandle>> instances =
      new Dictionary<string, List<CoroutineHandle>>();

    /// <summary>
    /// Pauses all actively running Wait objects - eg Make it stop checking it's condition.
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
    public static void Pause()
    {
      IEnumerable<CoroutineHandle> handles = instances.SelectMany(
        entry => entry.Value
      );
      foreach (CoroutineHandle handle in handles) {
        Timing.PauseCoroutines(handle);
      }
    }

    /// <summary>
    /// Pauses all running Wait objects in the group - eg Make it stop checking it's condition.
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
    /// <param name="group">The group to pause</param>
    public static void PauseGroup(string group)
    {
      List<CoroutineHandle> handles;
      if (instances.TryGetValue(group, out handles)) {
        foreach (CoroutineHandle handle in handles) {
          Timing.PauseCoroutines(handle);
        }
      }
    }

    /// <summary>
    /// Pauses the Wait object with the given tag - eg Make it stop checking it's condition.
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
    /// <param name="tag">The tag to pause</param>
    public static void PauseTag(string tag)
    {
      Timing.PauseCoroutines(tag);
    }

    /// <summary>
    /// Resumes all actively running Wait objects - eg Make it start checking it's condition again.
    /// If the Waits' timing is complete when Resumed it will finish and run it's action immediately.
    /// </summary>
    public static void Resume()
    {
      IEnumerable<CoroutineHandle> handles = instances.SelectMany(
        entry => entry.Value
      );
      foreach (CoroutineHandle handle in handles) {
        Timing.ResumeCoroutines(handle);
      }
    }

    /// <summary>
    /// Resumes all running Wait objects in the group - eg Make it start checking it's condition again.
    /// If the Waits' timing is complete when Resumed it will finish and run it's action immediately.
    /// </summary>
    /// <param name="group">The group to resume</param>
    public static void ResumeGroup(string group)
    {
      List<CoroutineHandle> handles;
      if (instances.TryGetValue(group, out handles)) {
        foreach (CoroutineHandle handle in handles) {
          Timing.ResumeCoroutines(handle);
        }
      }
    }

    /// <summary>
    /// Resumes the Wait object with the given tag - eg Make it start checking it's condition again.
    /// If the Waits' timing is complete when Resumed it will finish and run it's action immediately.
    /// </summary>
    /// <param name="tag">The tag to resume</param>
    public static void ResumeTag(string tag)
    {
      Timing.ResumeCoroutines(tag);
    }

    /// <summary>
    /// Stop all actively running Wait objects. This will terminate it and remove it from the WaitManager.
    /// </summary>
    public static void Stop()
    {
      IEnumerable<CoroutineHandle> handles = instances.SelectMany(
        entry => entry.Value
      );
      foreach (CoroutineHandle handle in handles) {
        Timing.KillCoroutines(handle);
      }
    }

    /// <summary>
    /// Stop all running Wait objects in the group. This will terminate it and remove it from the WaitManager.
    /// </summary>
    /// <param name="group">The group to stop</param>
    public static void StopGroup(string group)
    {
      List<CoroutineHandle> handles;
      if (instances.TryGetValue(group, out handles)) {
        foreach (CoroutineHandle handle in handles) {
          Timing.KillCoroutines(handle);
        }
      }
    }

    /// <summary>
    /// Stop the Wait object with the given tag. This will terminate it and remove it from the WaitManager.
    /// </summary>
    /// <param name="tag">The tag to stop</param>
    public static void StopTag(string tag)
    {
      Timing.KillCoroutines(tag);
    }

  }
}