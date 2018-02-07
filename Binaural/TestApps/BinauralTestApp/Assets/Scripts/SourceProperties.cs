using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SourceProperties : MonoBehaviour {

    public AudioMixer masterMixer;    
    private string volumeName = "MasterVolume"; // Name of exposed parameter from the mixer    

    // Preset Sounds
    public AudioClip ClipAnechoicFemale;
    public AudioClip ClipAnechoicMale;
    public AudioClip ClipNoiseBurst;
    public AudioClip ClipClassical;
    public AudioClip ClipElectronic;
    public AudioClip ClipBeat;
    //

    // Use this for initialization
    void Start () {
        Slider slVolume = GameObject.Find("Slider_Volume").GetComponent<Slider>();

        // Using audio source volume
        //slVolume.value = this.GetComponent<AudioSource>().volume;

        // Using mixer
        float mixerVolume;
        masterMixer.GetFloat(volumeName, out mixerVolume);
        slVolume.value = mixerVolume;

        UpdateGUIText();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void UpdateGUIText()
    {
        Text textVolume = GameObject.Find("Text_Volume").GetComponent<Text>();

        // Using audio source volume
        //textVolume.text = "Volume:\t" + GainToDecibel(this.GetComponent<AudioSource>().volume);

        // Using mixer
        float mixerVolume;
        bool foundVolume = masterMixer.GetFloat(volumeName, out mixerVolume);
        if (!foundVolume)
            textVolume.text = "Error accessing audio mixer";
        else            
            textVolume.text = "Volume:\n" + mixerVolume.ToString("F2") + "dB";
    }

    private float GainToDecibel(float gain)
    {
        float dB;

        if (gain != 0)
            dB = 20.0f * Mathf.Log10(gain);
        else
            dB = -144.0f;

        return dB;
    }

    private float DecibelToGain(float dB)
    {
        float gain = Mathf.Pow(10.0f, dB / 20.0f);

        return gain;
    }

    public float SliderVolume
    {
        get
        {
            // Using audio source volume
            //return this.GetComponent<AudioSource>().volume;

            // Using mixer
            float mixerVolume;
            masterMixer.GetFloat(volumeName, out mixerVolume);
            return mixerVolume;
        }
        set
        {
            // Using audio source volume 
            //this.GetComponent<AudioSource>().volume = value;

            // Using mixer
            masterMixer.SetFloat(volumeName, value);

            UpdateGUIText();
        }
    }

// SOUND SELECTION

    public void SelectSoundAnechoicFemale()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Stop();
        audio.clip = ClipAnechoicFemale;
        audio.Play();
    }

    public void SelectSoundAnechoicMale()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Stop();
        audio.clip = ClipAnechoicMale;
        audio.Play();
    }

    public void SelectSoundNoiseBurst()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Stop();
        audio.clip = ClipNoiseBurst;
        audio.Play();
    }

    public void SelectSoundClassical()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Stop();
        audio.clip = ClipClassical;
        audio.Play();
    }

    public void SelectSoundElectronic()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Stop();
        audio.clip = ClipElectronic;
        audio.Play();
    }

    public void SelectSoundBeat()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Stop();
        audio.clip = ClipBeat;
        audio.Play();
    }
}
