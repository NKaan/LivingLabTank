using System.Collections;
using UnityEngine;

namespace SIDGIN.Patcher.Client
{
    public class SGAsyncOperation : CustomYieldInstruction
    {
        public bool isDone { get; internal set; }
        public float progress { get; set; }
        public int priority { get; set; }
        public bool allowSceneActivation { get; set; } = true;
        public override bool keepWaiting => !isDone;
        IEnumerator enumerator;
        public SGAsyncOperation(System.Func<SGAsyncOperation, IEnumerator> loadEnumerator)
        {
            enumerator = loadEnumerator(this);
            SGDispatcher.RunCorotune(Load());
        }
        IEnumerator Load()
        {
            isDone = false;
            yield return enumerator;
            isDone = true;
        }
    }
}
