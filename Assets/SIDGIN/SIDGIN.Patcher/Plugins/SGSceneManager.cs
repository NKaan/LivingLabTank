namespace SIDGIN.Patcher.SceneManagment
{
    public class SGSceneManager :
#if UNITY_EDITOR
        SIDGIN.Patcher.Editors.SGSceneManagerEditor
#else
        SIDGIN.Patcher.SceneManagment.InternalSGSceneManager
#endif
    {
        public static void Initialize()
        {
#if !UNITY_EDITOR
            SIDGIN.Patcher.SceneManagment.InternalSGSceneManager.Initialize();
#endif
        }
    }
}
