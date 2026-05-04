using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Button playBtn;
    [SerializeField]
    private Button settingBtn;
    [SerializeField]
    private Button quitBtn;
    [SerializeField]
    private Image fadeImage;
    private float fadeDuration;
    [SerializeField]
    private GameObject settingUI;
    [SerializeField]
    private Button closeSettingBtn;
    [SerializeField]
    private Slider mainSlider;
    [SerializeField]
    private Slider musicSlider;
    [SerializeField]
    private Slider sfxSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fadeDuration = 0.5f;
        playBtn.onClick.AddListener(OnPlay);
        settingBtn.onClick.AddListener(OnSetting);
        quitBtn.onClick.AddListener(OnQuit);
        closeSettingBtn.onClick.AddListener(OnCloseSetting);
        StartCoroutine(Fade(1, 0));
        mainSlider.onValueChanged.AddListener(OnMainChanged);
        musicSlider.onValueChanged.AddListener (OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnPlay()
    {
        AudioManager.Instance.PlayClick();
        StartCoroutine(LoadGame());
    }

    void OnSetting()
    {
        AudioManager.Instance.PlayClick();
        settingUI.SetActive(true);
        mainSlider.value = PlayerPrefs.GetFloat(Board.MAIN_VOL, 1f);
        musicSlider.value = PlayerPrefs.GetFloat(Board.MUSIC_VOL, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(Board.SFX_VOL, 1f);
    }

    void OnQuit()
    {
        AudioManager.Instance.PlayClick();
        StartCoroutine(QuitGame());
    }

    void OnCloseSetting()
    {
        AudioManager.Instance.PlayClick();
        PlayerPrefs.Save();
        settingUI.SetActive(false);
    }

    IEnumerator LoadGame()
    {
        yield return StartCoroutine(Fade(0, 1));
        SceneManager.LoadScene(1);
    }

    IEnumerator QuitGame()
    {
        yield return StartCoroutine(Fade(0, 1));
        Application.Quit();
    }

    IEnumerator Fade(float start, float end)
    {
        Color color = fadeImage.color;
        float t = 0;
        float smoothT;
        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            smoothT = Mathf.SmoothStep(0, 1, t);
            float alpha = Mathf.Lerp(start, end, smoothT);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
    }

    void OnMainChanged(float value)
    {
        PlayerPrefs.SetFloat(Board.MAIN_VOL,value);
        ApplyVolume();
    }

    void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat (Board.MUSIC_VOL,value);
        ApplyVolume();
    }

    void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat(Board.SFX_VOL,value);
        ApplyVolume();
    }
    
    void ApplyVolume()
    {
        AudioManager.Instance.SetVolume(mainSlider.value, musicSlider.value, sfxSlider.value);
    }
}
