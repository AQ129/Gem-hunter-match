using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField]
    private AudioSource musicSource;
    [SerializeField]
    private AudioSource sfxSource;
    [SerializeField]
    private AudioClip music;
    [SerializeField]
    private AudioClip btnClick;
    [SerializeField]
    private AudioClip bombExplosion;
    [SerializeField]
    private AudioClip rocketExplosion;
    [SerializeField]
    private AudioClip winSound;
    [SerializeField]
    private AudioClip loseSound;
    [SerializeField]
    private AudioClip crateSound;
    [SerializeField]
    private AudioClip swapSound;
    [SerializeField]
    private AudioClip bubble_effect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnCrateRopBreak()
    {
        sfxSource.PlayOneShot(crateSound);
    }

    public void OnStopMusic()
    {
        musicSource.Stop();
    }

    public void OnPauseMusic()
    {
        musicSource.Pause();
    }

    public void OnUnPauseMusic()
    {
        musicSource.UnPause();
    }

    public void OnRocket()
    {
        sfxSource.PlayOneShot(rocketExplosion);
    }

    public void OnBomb()
    {
        sfxSource.PlayOneShot(bombExplosion);
    }

    public void OnClickGem()
    {
        sfxSource.PlayOneShot(bubble_effect);
    }

    public void OnSwap()
    {
        sfxSource.PlayOneShot(swapSound);
    }

    public void PlayClick()
    {
        sfxSource.PlayOneShot(btnClick);
    }

    public void PlayWinSound()
    {
        sfxSource.PlayOneShot(winSound);
    }

    public void PlayLoseSound()
    {
        sfxSource.PlayOneShot(loseSound);
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayMusicBG()
    {
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void SetVolume(float main, float music, float sfx)
    {
        sfxSource.volume = main * sfx;
        musicSource.volume = main * music;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
