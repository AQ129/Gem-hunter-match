using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static event System.Action<GemType, int> OnGemBonusChanged;
    [SerializeField]
    private GameObject EndGamePanel;
    [SerializeField]
    private GameObject Victory;
    [SerializeField]
    private GameObject Defeat;
    [SerializeField]
    private Button ReplayBtn;
    [SerializeField]
    private Image fadeImage;
    private float fadeDuration = 0.5f;
    [SerializeField]
    private Button menuBtn;
    [SerializeField]
    private Button settingBtn;
    [SerializeField]
    private GameObject settingUI;
    [SerializeField]
    private Button closeSettingBtn;
    [SerializeField]
    private Button menuFromSettingBtn;
    private BoardState lastState;
    [SerializeField]
    private Slider mainSlider;
    [SerializeField]
    private Slider musicSlider;
    [SerializeField]
    private Slider sfxSlider;
    [SerializeField]
    private GameObject shop;
    [SerializeField]
    private Button openShopBtn;
    [SerializeField]
    private Button closeShopBtn;
    [SerializeField]
    private TextMeshProUGUI coinText;
    [SerializeField]
    private Button buyCoinBtn;
    [SerializeField]
    private Button buyVerticalBtn;
    [SerializeField]
    private Button buyHorizontalBtn;
    [SerializeField]
    private Button buyBombBtn;
    [SerializeField]
    private Button buyColorBtn;
    private int coin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Board.Instance.OnGameEnding += OnGameEnding;
        ReplayBtn.onClick.AddListener(() => OnReplay());
        menuBtn.onClick.AddListener(() => OnMenu());
        settingBtn.onClick.AddListener(() => OnSetting());
        closeSettingBtn.onClick.AddListener(() => OffSetting());
        menuFromSettingBtn.onClick.AddListener(() => OnMenu());
        mainSlider.onValueChanged.AddListener(OnMainChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        openShopBtn.onClick.AddListener(OnShopOn);
        closeShopBtn.onClick.AddListener(OnShopOff);
        buyCoinBtn.onClick.AddListener(OnBuyCoin);
        buyVerticalBtn.onClick.AddListener(OnBuyVertical);
        buyHorizontalBtn.onClick.AddListener(OnBuyHorizontal);
        buyBombBtn.onClick.AddListener(OnBuyBomb);
        buyColorBtn.onClick.AddListener(OnBuyColor);
        StartCoroutine(Fade(1, 0));
    }

    private void OnShopOn()
    {
        shop.SetActive(true);
        coin = PlayerPrefs.GetInt(Board.COIN_QUANTITY, 0);
        coinText.text = coin.ToString();
        CheckItemCanBuy();
    }

    private void OnShopOff()
    {
        shop.SetActive(false);
        PlayerPrefs.Save();
    }

    private void OnBuyCoin()
    {
        coin += 100;
        coinText.text = coin.ToString();
        PlayerPrefs.SetInt(Board.COIN_QUANTITY, coin);
        CheckItemCanBuy();
    }

    private void OnBuyVertical()
    {
        coin -= 30;
        coinText.text = coin.ToString();
        PlayerPrefs.SetInt(Board.COIN_QUANTITY, coin);
        int quantity = PlayerPrefs.GetInt(Board.KEY_VERTICAL, 0);
        quantity++;
        PlayerPrefs.SetInt(Board.KEY_VERTICAL, quantity);
        CheckItemCanBuy();
        OnGemBonusChanged?.Invoke(GemType.Vertical, quantity);
    }

    private void OnBuyHorizontal()
    {
        coin -= 30;
        coinText.text = coin.ToString();
        PlayerPrefs.SetInt(Board.COIN_QUANTITY, coin);
        int quantity = PlayerPrefs.GetInt(Board.KEY_HORIZONTAL, 0);
        quantity++;
        PlayerPrefs.SetInt(Board.KEY_HORIZONTAL, quantity);
        CheckItemCanBuy();
        OnGemBonusChanged?.Invoke(GemType.Horizontal, quantity);
    }

    private void OnBuyBomb()
    {
        coin -= 40;
        coinText.text = coin.ToString();
        PlayerPrefs.SetInt(Board.COIN_QUANTITY, coin);
        int quantity = PlayerPrefs.GetInt(Board.KEY_BOMB, 0);
        quantity++;
        PlayerPrefs.SetInt(Board.KEY_BOMB, quantity);
        CheckItemCanBuy();
        OnGemBonusChanged?.Invoke(GemType.SmallBomb, quantity);
    }

    private void OnBuyColor()
    {
        coin -= 50;
        coinText.text = coin.ToString();
        PlayerPrefs.SetInt(Board.COIN_QUANTITY, coin);
        int quantity = PlayerPrefs.GetInt(Board.KEY_COLOR, 0);
        quantity++;
        PlayerPrefs.SetInt(Board.KEY_COLOR, quantity);
        CheckItemCanBuy();
        OnGemBonusChanged?.Invoke(GemType.Color, quantity);
    }

    private void CheckItemCanBuy()
    {
        buyBombBtn.interactable = true;
        buyColorBtn.interactable = true;
        buyVerticalBtn.interactable = true;
        buyHorizontalBtn.interactable = true;
        if (coin < 50)
        {
            buyColorBtn.interactable = false;
        }
        if(coin < 40)
        {
            buyBombBtn.interactable = false;
        }
        if(coin < 30)
        {
            buyVerticalBtn.interactable = false;
            buyHorizontalBtn.interactable = false;
        }
        
    }

    private void OnGameEnding(bool obj)
    {
        EndGamePanel.SetActive(true);
        if (obj)
        {
            Defeat.gameObject.SetActive(false);
            AudioManager.Instance.OnStopMusic();
            AudioManager.Instance.PlayWinSound();
        }
        else
        {
            Victory.gameObject.SetActive(false);
            AudioManager.Instance.OnStopMusic();
            AudioManager.Instance.PlayLoseSound();
        }
    }

    void OnMainChanged(float value)
    {
        PlayerPrefs.SetFloat(Board.MAIN_VOL, value);
        ApplyVolume();
    }

    void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat(Board.MUSIC_VOL, value);
        ApplyVolume();
    }

    void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat(Board.SFX_VOL, value);
        ApplyVolume();
    }

    void ApplyVolume()
    {
        AudioManager.Instance.SetVolume(mainSlider.value, musicSlider.value, sfxSlider.value);
    }
    private void OnReplay()
    {
        AudioManager.Instance.PlayClick();
        StartCoroutine(FadeAndReplay());
    }

    private IEnumerator FadeAndReplay()
    {
        yield return StartCoroutine(Fade(0, 1));
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnMenu()
    {
        PlayerPrefs.Save();
        AudioManager.Instance.PlayClick();
        StartCoroutine(FadeAndMenu());
    }

    IEnumerator FadeAndMenu()
    {
        yield return StartCoroutine(Fade(0, 1));
        SceneManager.LoadScene(0);
    }

    void OnSetting()
    {
        mainSlider.value = PlayerPrefs.GetFloat(Board.MAIN_VOL, 1f);
        musicSlider.value = PlayerPrefs.GetFloat(Board.MUSIC_VOL, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(Board.SFX_VOL, 1f);
        AudioManager.Instance.PlayClick();
        StartCoroutine(FadeAndSetting());
    }

    IEnumerator FadeAndSetting()
    {
        yield return StartCoroutine(Fade(0, 0.7f));
        lastState = Board.Instance.CurrentState;
        AudioManager.Instance.OnPauseMusic();
        Board.Instance.CurrentState = BoardState.Paused;
        settingUI.SetActive(true);
    }

    void OffSetting()
    {
        AudioManager.Instance.PlayClick();
        PlayerPrefs.Save();
        StartCoroutine(FadeAndOffSetting());
    }

    IEnumerator FadeAndOffSetting()
    {
        yield return StartCoroutine(Fade(0.7f, 0));
        AudioManager.Instance.OnUnPauseMusic();
        Board.Instance.CurrentState = lastState;
        settingUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Fade(float start, float end)
    {
        Color color = fadeImage.color;
        float t = 0;
        float smoothT;
        while(t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            smoothT = Mathf.SmoothStep(0, 1, t);
            float alpha = Mathf.Lerp(start, end, smoothT);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
    }
}
