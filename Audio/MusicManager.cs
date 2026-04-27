using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private AudioSource audioSource;

    [Header("Playlist")]
    [SerializeField] private AudioClip[] musics;
    [SerializeField] private bool shuffle = true;
    [SerializeField] private bool playOnStart = true;

    [Header("Configuração")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.45f;

    private int currentIndex = -1;
    private Coroutine playlistRoutine;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;
    }

    private void Start()
    {
        if (playOnStart)
            PlayPlaylist();
    }

    public void PlayPlaylist()
    {
        if (playlistRoutine != null)
            StopCoroutine(playlistRoutine);

        playlistRoutine = StartCoroutine(PlaylistCoroutine());
    }

    private IEnumerator PlaylistCoroutine()
    {
        while (true)
        {
            PlayNextMusic();

            while (audioSource.isPlaying)
                yield return null;

            yield return new WaitForSeconds(1f);
        }
    }

    public void PlayNextMusic()
    {
        if (musics == null || musics.Length == 0)
        {
            Debug.LogWarning("Nenhuma música cadastrada no MusicManager.");
            return;
        }

        if (shuffle)
        {
            int newIndex = currentIndex;

            if (musics.Length > 1)
            {
                while (newIndex == currentIndex)
                    newIndex = Random.Range(0, musics.Length);
            }
            else
            {
                newIndex = 0;
            }

            currentIndex = newIndex;
        }
        else
        {
            currentIndex++;

            if (currentIndex >= musics.Length)
                currentIndex = 0;
        }

        audioSource.clip = musics[currentIndex];
        audioSource.volume = volume;
        audioSource.Play();
    }

    public void StopMusic()
    {
        if (playlistRoutine != null)
            StopCoroutine(playlistRoutine);

        audioSource.Stop();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
}