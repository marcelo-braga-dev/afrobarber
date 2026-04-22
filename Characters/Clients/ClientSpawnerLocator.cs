using UnityEngine;

public static class ClientSpawnerLocator
{
    private static ClientSpawner cachedSpawner;

    public static bool TryGet(out ClientSpawner spawner)
    {
        if (cachedSpawner == null)
            cachedSpawner = Object.FindFirstObjectByType<ClientSpawner>();

        spawner = cachedSpawner;
        return spawner != null;
    }
}
