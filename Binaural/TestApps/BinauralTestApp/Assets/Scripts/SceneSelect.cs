using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneSelect : MonoBehaviour {

    private GameObject jazzquartetGO;
    private GameObject dancemusicGO;
    private GameObject speechGO;
    private GameObject speechmaskGO;
    private Button buttonSpeech;
    private Button buttonMaskedSpeech;
    private Button buttonJazzQuartet;
    private Button buttonDanceMusic;
    private Button buttonEverything;

    // Speech rotation
    private bool speechRotation = false;
    private float SPEECH_ROTATION_SPEED = 30.0f;

    // Use this for initialization
    void Start ()
    {
        jazzquartetGO = GameObject.Find("JazzQuartet");
        dancemusicGO = GameObject.Find("DanceMusic");
        speechGO = GameObject.Find("Speech");
        speechmaskGO = GameObject.Find("Masks");

        buttonSpeech = GameObject.Find("Button_SceneSpeech").GetComponent<Button>();
        buttonMaskedSpeech = GameObject.Find("Button_SceneSpeechMask").GetComponent<Button>();
        buttonJazzQuartet = GameObject.Find("Button_SceneJazzQuartet").GetComponent<Button>();
        buttonDanceMusic = GameObject.Find("Button_SceneDanceMusic").GetComponent<Button>();
        buttonEverything = GameObject.Find("Button_SceneEverything").GetComponent<Button>();

        SelectSceneSpeech();        
    }

    // Update is called once per frame
    void Update ()
    {
        RotateSource();
    }

    /// <summary>
    /// Scene 1: single speech
    /// </summary>
    public void SelectSceneSpeech()
    {
        SetSpeechRotation(false);
        SetActiveGO(jazzquartetGO, false);
        SetActiveGO(dancemusicGO, false);
        SetActiveGO(speechmaskGO, false);
        SetActiveGO(speechGO, true);
        SelectButton(buttonSpeech);
    }

    /// <summary>
    /// Scene 2: masked speech
    /// </summary>
    public void SelectSceneMaskedSpeech()
    {
        SetSpeechRotation(true);
        SetActiveGO(jazzquartetGO, false);
        SetActiveGO(dancemusicGO, false);
        SetActiveGO(speechmaskGO, true);
        SetActiveGO(speechGO, true);
        SelectButton(buttonMaskedSpeech);
    }

    /// <summary>
    /// Scene 3: jazz quartet
    /// </summary>
    public void SelectSceneJazzQuartet()
    {
        SetSpeechRotation(false);
        SetActiveGO(jazzquartetGO, true);
        SetActiveGO(dancemusicGO, false);
        SetActiveGO(speechmaskGO, false);
        SetActiveGO(speechGO, false);
        SelectButton(buttonJazzQuartet);
    }

    /// <summary>
    /// Scene 4: dance music
    /// </summary>
    public void SelectSceneDanceMusic()
    {
        SetSpeechRotation(false);
        SetActiveGO(jazzquartetGO, false);
        SetActiveGO(dancemusicGO, true);
        SetActiveGO(speechmaskGO, false);
        SetActiveGO(speechGO, false);
        SelectButton(buttonDanceMusic);
    }

    /// <summary>
    /// Scene 5: everything
    /// </summary>
    public void SelectSceneEverything()
    {
        SetSpeechRotation(true);
        SetActiveGO(jazzquartetGO, true);
        SetActiveGO(dancemusicGO, true);
        SetActiveGO(speechmaskGO, true);
        SetActiveGO(speechGO, true);
        SelectButton(buttonEverything);
    }

    /// <summary>
    /// Highlight only selected button
    /// </summary>
    public void SelectButton (Button button)
    {
        buttonSpeech.interactable = true;
        buttonMaskedSpeech.interactable = true;
        buttonJazzQuartet.interactable = true;
        buttonDanceMusic.interactable = true;
        buttonEverything.interactable = true;

        button.Select();
        button.interactable = false;        
    }

    /// <summary>
    /// Set if the speech audio source should rotate around listener
    /// </summary>    
    public void SetSpeechRotation(bool setrotation)
    {
        speechRotation = setrotation;
        if (!setrotation)
            speechGO.transform.position = new Vector3(0.0f, 0.0f, 1.95f);            
    }

    /// <summary>
    ///  Source rotation
    /// </summary>
    public void RotateSource()
    {
        if (speechRotation)
            speechGO.transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * SPEECH_ROTATION_SPEED);
    }

    /// <summary>
    /// Enable/disable one game object containing audiosource/s
    /// </summary>
    public void SetActiveGO(GameObject go, bool setactive)
    {        
        // Enable/disable audio 
        if (setactive)
            SetVolumeRecursive(go, 1.0f);
        else
            SetVolumeRecursive(go, 0.0f);

        // Enable/disable render of speaker and text
        SetRenderRecursive(go, setactive);
    }

    /// <summary>
    /// Recursively set render of all speaker models and text in this gameobject xor its children
    /// </summary>
    public void SetRenderRecursive(GameObject go, bool setrender)
    {
        int childCount = go.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (go.transform.GetChild(i).name.Contains("subwoofer"))
                go.transform.GetChild(i).gameObject.SetActive(setrender);
            //if (go.transform.GetChild(i).name.Contains("Text"))
            //    go.transform.GetChild(i).gameObject.SetActive(setrender);
            SetRenderRecursive(go.transform.GetChild(i).gameObject, setrender);
        }
    }

    /// <summary>
    /// Recursively set volume of all audio sources in this gameobject xor its children
    /// </summary>
    public void SetVolumeRecursive(GameObject go, float volume)
    {
        AudioSource source = go.GetComponent<AudioSource>();
        if (source != null)
            source.volume = volume;
        else
        {
            int childCount = go.transform.childCount;
            for (int i=0; i < childCount; i++)            
                SetVolumeRecursive(go.transform.GetChild(i).gameObject, volume);            
        }
    }

    /// <summary>
    /// For debug. remove before release
    /// </summary>        
    public void DebugWrite(string text)
    {
        GameObject textGO = GameObject.Find("DebugText");
        if (textGO == null)
            Debug.Log(text);
        else
        {
            TextMesh debugText = textGO.GetComponent<TextMesh>();
            debugText.text = debugText.text + "\n" + text;
        }
    }
}
