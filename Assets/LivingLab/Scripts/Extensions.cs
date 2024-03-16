using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions
{
    public static Transform FindChildObjectByName(this Transform transform, string childObjectName)
        => transform.GetComponentsInChildren<Transform>().FirstOrDefault(k => k.gameObject.name == childObjectName);
}
