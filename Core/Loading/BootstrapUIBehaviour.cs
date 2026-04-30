using UnityEngine;

public abstract class BootstrapUIBehaviour : MonoBehaviour, IGameBootstrapInitializable
{
    private bool initialized;

    public bool IsInitialized => initialized;

    public void InitializeFromBootstrap()
    {
        if (initialized)
            return;

        initialized = true;
        OnBootstrapInitialize();
    }

    protected abstract void OnBootstrapInitialize();

    protected void ResetBootstrapInitialization()
    {
        initialized = false;
    }
}