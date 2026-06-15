using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{

    #region Singleton

    public static AudioManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of playercontroller present in scene");
            return;
        }

        instance = this;
    }

    #endregion

    private float min = -80f;

    [SerializeField] private AudioMixer generalMixer;
    [SerializeField] private float lerpSpeed;


    [Header("Background Fade")]
    [SerializeField] private bool isBackgroundFadingIn = false;
    [SerializeField] private bool isBackgroundFadingOut = false;
    
    // Update is called once per frame
    void Update()
    {
        if (isBackgroundFadingIn)
        {
            BackgroundFadeUpdate(true);
        }
        else if (isBackgroundFadingOut) {
            BackgroundFadeUpdate(false);
        }
    }

    private void BackgroundFadeUpdate(bool fadingIn)
    {
        float volume;
        generalMixer.GetFloat("backgroundVol", out volume);
        
        if (fadingIn)
        {
            volume += lerpSpeed * Time.deltaTime;
            if (volume >= 0.0f)
            {
                isBackgroundFadingIn = false;
                volume = 0.0f;
            }
        }
        else
        {
            volume -= lerpSpeed * Time.deltaTime;
            if(volume <= min){ 
                isBackgroundFadingOut = false;
                volume = min;
            }
        }

        generalMixer.SetFloat("backgroundVol", volume);
    }

    public void SetBackgroundQuiet()
    {
        generalMixer.SetFloat("backgroundVol", -80.0f);
    }

    public void FadeBackgroundFade(bool fadingIn)
    {
        isBackgroundFadingIn = fadingIn;
        isBackgroundFadingOut = !fadingIn;
    }
}
