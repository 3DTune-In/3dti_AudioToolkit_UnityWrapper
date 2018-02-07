using System.Collections;
using UnityEngine;

public class UserInterface : MonoBehaviour {

    enum TPanelState { iddle, audiometry, HL, HA};

    GameObject rightButtonsPanel;
    GameObject audiometryPanel;
    GameObject HLPanel;
    GameObject HAPanel;

    float panelsWidth;
    float panelsTransitionDuration;

    Vector3 openPanelVector;
    Vector3 closePanelVector;

    TPanelState rightPanelState;

    // Use this for initialization
    void Start () {        
        rightPanelState = TPanelState.iddle;

        panelsWidth = 150.0f;
        openPanelVector = new Vector3(-panelsWidth, 0f, 0f);
        closePanelVector = new Vector3(panelsWidth, 0f, 0f);

        panelsTransitionDuration = 1.0f;

        rightButtonsPanel = GameObject.Find("RightButtonsPanel");
        audiometryPanel = GameObject.Find("AudiometryPanel");        
        HLPanel = GameObject.Find("HLPanel");
        HAPanel = GameObject.Find("HAPanel");
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    /////////////       
    //Left Panel
    /////////////       

    public void StartStopDirectionalityLeft(bool value)
    {
        this.GetComponent<API_3DTI_Spatializer>().SwitchOnOffHADirectionality(API_3DTI_Common.T_ear.LEFT, value);
    }
    public void StartStopDirectionalityRight(bool value)
    {
        this.GetComponent<API_3DTI_Spatializer>().SwitchOnOffHADirectionality(API_3DTI_Common.T_ear.RIGHT, value);
    }

    public void ChangeDirectionalityLeft(float newValue)
    {
        this.GetComponent<API_3DTI_Spatializer>().SetHADirectionalityExtend(API_3DTI_Common.T_ear.LEFT, newValue);              
    }
    public void ChangeDirectionalityRight(float newValue)
    {
        this.GetComponent<API_3DTI_Spatializer>().SetHADirectionalityExtend(API_3DTI_Common.T_ear.RIGHT, newValue);
    }

    ////////////////       
    //Right Panel
    ///////////// ///      
    public void OpenCloseAudiometryPanel()
    {
        if (rightPanelState == TPanelState.iddle)
        {
            //Open panel
            StartCoroutine(CoroutineMoveTwoPanelsGradually(audiometryPanel, rightButtonsPanel, 0.0f, panelsTransitionDuration, openPanelVector));
            rightPanelState = TPanelState.audiometry;
        }
        else if (rightPanelState == TPanelState.audiometry)
        {
            //Close panel
            StartCoroutine(CoroutineMoveTwoPanelsGradually(audiometryPanel, rightButtonsPanel, 0.0f, panelsTransitionDuration, closePanelVector));
            rightPanelState = TPanelState.iddle;
        }
        else
        {
            //Close other panels and open this one
            CloseOpenPanelInstantly();
            MoveOnePanelInstantly(audiometryPanel, new Vector3(-panelsWidth, 0f, 0f));
            rightPanelState = TPanelState.audiometry;
        }        
    }
    public void OpenCloseHLPanel()
    {
        if (rightPanelState == TPanelState.iddle)
        {
            //Open panel
            StartCoroutine(CoroutineMoveTwoPanelsGradually(HLPanel, rightButtonsPanel, 0.0f, panelsTransitionDuration, openPanelVector));
            rightPanelState = TPanelState.HL;
        }
        else if (rightPanelState == TPanelState.HL)
        {
            //Close panel
            StartCoroutine(CoroutineMoveTwoPanelsGradually(HLPanel, rightButtonsPanel, 0.0f, panelsTransitionDuration, closePanelVector));
            rightPanelState = TPanelState.iddle;
        }
        else
        {
            //Close other panels and open this one
            CloseOpenPanelInstantly();
            MoveOnePanelInstantly(HLPanel, new Vector3(-panelsWidth, 0f, 0f));
            rightPanelState = TPanelState.HL;
        }      
    }
    public void OpenCloseHAPanel()
    {
        if (rightPanelState == TPanelState.iddle)
        {
            //Open panel
            StartCoroutine(CoroutineMoveTwoPanelsGradually(HAPanel, rightButtonsPanel, 0.0f, panelsTransitionDuration, openPanelVector));
            rightPanelState = TPanelState.HA;
        }
        else if (rightPanelState == TPanelState.HA)
        {
            //Close panel
            StartCoroutine(CoroutineMoveTwoPanelsGradually(HAPanel, rightButtonsPanel, 0.0f, panelsTransitionDuration, closePanelVector));
            rightPanelState = TPanelState.iddle;
        }
        else
        {
            //Close other panels and open this one
            CloseOpenPanelInstantly();
            MoveOnePanelInstantly(HAPanel, new Vector3(-panelsWidth, 0f, 0f));
            rightPanelState = TPanelState.HA;
        }        
    }
    /// <summary>
    /// HA controls
    /// </summary>
    public void StartStopHA(bool value)
    {
        this.GetComponent<API_3DTI_HA>().SwitchHAOnOff(API_3DTI_Common.T_ear.BOTH, value);
    }
    public void SetLowToneHALeft(float newValue)
    {
        this.GetComponent<API_3DTI_HA>().SetTone(API_3DTI_Common.T_ear.LEFT, API_3DTI_HA.T_HAToneBand.LOW, newValue);
    }
    public void SetMidToneHALeft(float newValue)
    {
        this.GetComponent<API_3DTI_HA>().SetTone(API_3DTI_Common.T_ear.LEFT, API_3DTI_HA.T_HAToneBand.MID, newValue);
    }
    public void SetHighToneHALeft(float newValue)
    {
        this.GetComponent<API_3DTI_HA>().SetTone(API_3DTI_Common.T_ear.LEFT, API_3DTI_HA.T_HAToneBand.HIGH, newValue);
    }
    public void SetLowToneHARight(float newValue)
    {
        this.GetComponent<API_3DTI_HA>().SetTone(API_3DTI_Common.T_ear.RIGHT, API_3DTI_HA.T_HAToneBand.LOW, newValue);
    }
    public void SetMidToneHARight(float newValue)
    {
        this.GetComponent<API_3DTI_HA>().SetTone(API_3DTI_Common.T_ear.RIGHT, API_3DTI_HA.T_HAToneBand.MID, newValue);
    }
    public void SetHighToneHARight(float newValue)
    {
        this.GetComponent<API_3DTI_HA>().SetTone(API_3DTI_Common.T_ear.RIGHT, API_3DTI_HA.T_HAToneBand.HIGH, newValue);
    }

    void CloseOpenPanelInstantly()
    {
        if (rightPanelState == TPanelState.audiometry)
        {
            MoveOnePanelInstantly(audiometryPanel, new Vector3(panelsWidth, 0f, 0f));
        }
        else if (rightPanelState == TPanelState.HL)
        {
            MoveOnePanelInstantly(HLPanel, new Vector3(panelsWidth, 0f, 0f));
        }
        else if (rightPanelState == TPanelState.HA)
        {
            MoveOnePanelInstantly(HAPanel, new Vector3(panelsWidth, 0f, 0f));
        }
    }

    void MoveOnePanelInstantly(GameObject panel1, Vector3 shift)
    {
        Vector3 currentPositionPanel1 = panel1.GetComponent<RectTransform>().anchoredPosition;   //Get initial position
        Vector3 targetPositionPanel1 = currentPositionPanel1 + shift;
        panel1.GetComponent<RectTransform>().anchoredPosition = targetPositionPanel1; //Assing value                
    }

    IEnumerator CoroutineMoveTwoPanelsGradually(GameObject panel1, GameObject panel2, float delayTime, float durationTime, Vector3 shift)
    {
        float t = 0.0f;         //Variable to store the interpolation parameter        

        Vector3 currentPositionPanel1 = panel1.GetComponent<RectTransform>().anchoredPosition;   //Get initial position
        Vector3 targetPositionPanel1 = currentPositionPanel1 + shift;
        Vector3 currentPositionPanel2 = panel2.GetComponent<RectTransform>().anchoredPosition;   //Get initial position
        Vector3 targetPositionPanel2 = currentPositionPanel2 + shift;

        yield return new WaitForSeconds(delayTime);         //Start delay

        //Lerp interpolate the value of alpha between current and target en funciton of t.
        //t=0 --> curret, t=1 --> target
        while (t <= 1)
        {
            //Calculate new value in function of deltaTime
            Vector3 newPosition1 = Vector3.Lerp(currentPositionPanel1, targetPositionPanel1, t);
            Vector3 newPosition2 = Vector3.Lerp(currentPositionPanel2, targetPositionPanel2, t);
            t += Time.deltaTime / durationTime;

            panel1.GetComponent<RectTransform>().anchoredPosition = newPosition1; //Assing value            
            panel2.GetComponent<RectTransform>().anchoredPosition = newPosition2; //Assing value            
            yield return true;  //Wait until the next frame to continue
        }
    }

    IEnumerator CoroutineMoveOnePanelGradually(GameObject panel1, float delayTime, float durationTime, Vector3 shift)
    {
        float t = 0.0f;         //Variable to store the interpolation parameter        

        Vector3 currentPositionPanel1 = panel1.GetComponent<RectTransform>().anchoredPosition;   //Get initial position
        Vector3 targetPositionPanel1 = currentPositionPanel1 + shift;        

        yield return new WaitForSeconds(delayTime);         //Start delay

        //Lerp interpolate the value of alpha between current and target en funciton of t.
        //t=0 --> curret, t=1 --> target
        while (t <= 1)
        {
            //Calculate new value in function of deltaTime
            Vector3 newPosition1 = Vector3.Lerp(currentPositionPanel1, targetPositionPanel1, t);            
            t += Time.deltaTime / durationTime;

            panel1.GetComponent<RectTransform>().anchoredPosition = newPosition1; //Assing value            
            
            yield return true;  //Wait until the next frame to continue
        }
    }

    


}
