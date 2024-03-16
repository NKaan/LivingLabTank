
public class SGResources :
    #if UNITY_EDITOR
    SIDGIN.Patcher.Editors.InternalSGResourcesEditor
    #else
    SIDGIN.Patcher.Client.InternalSGResources
    #endif
{
}

