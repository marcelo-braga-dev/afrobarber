using UnityEngine;

public static class PersistentGameObject
{
    public static void MakePersistent(GameObject target)
    {
        if (target == null)
            return;

        Transform targetTransform = target.transform;
        if (targetTransform.parent != null)
            targetTransform.SetParent(null, true);

        Object.DontDestroyOnLoad(target);
    }
}
