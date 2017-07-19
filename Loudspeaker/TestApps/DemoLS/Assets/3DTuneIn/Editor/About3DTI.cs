using UnityEditor;
using UnityEngine;
 
public class About3DTI : EditorWindow
{
    Vector2 scrollPos;
    const float windowWidth = 600;    
    static GUIStyle aboutTextStyle;
    static GUIStyle aboutSectionStyle;
    static GUIStyle aboutTitleStyle;
    static GUIStyle urlStyle;

    public static void ShowAboutWindow()
    {
        // Setup styles
        aboutTextStyle = new GUIStyle(EditorStyles.textArea);
        aboutTextStyle.wordWrap = true;
        aboutTextStyle.richText = true;
        aboutSectionStyle = new GUIStyle(GUI.skin.box);
        aboutTitleStyle = new GUIStyle(GUI.skin.box);
        aboutTitleStyle.normal.textColor = EditorStyles.label.normal.textColor;
        aboutTitleStyle.fontStyle = FontStyle.Bold;
        urlStyle = new GUIStyle(GUI.skin.label);
        urlStyle.normal.textColor = Color.cyan;

        // Show window
        GetWindow<About3DTI>(false, "About", true);
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(windowWidth));
        
        BeginAboutSection("3D-TUNE-IN TOOLKIT UNITY WRAPPER");

            GUILayout.Label("Version 1.9 D2017xxxx Tk-xxxxx", EditorStyles.boldLabel);

            ShowParagraph("This software was developed by a team coordinated by Arcadio Reyes Lecuona (University of Malaga) and Lorenzo Picinali (Imperial College London). " +
            "The members of the team are: Maria Cuevas Rodriguez, Daniel Gonzalez Toledo, Carlos Garre, Luis Molina Tanco and Ernesto de la Rubia, all affiliated to the University of Malaga. " + 
            "BRIR and near field simulation filters provided by David Poirier-Quinot (Imperial College London)."); // High performance?

            ShowParagraph("Copyright (c) University of Malaga and Imperial College London - 2017. Email : grupodiana@uma.es");

            ShowParagraph("The 3D Tune-In Toolkit is a standard C++ library for audio spatialisation and simulation of hearing loss and hearing aids", "http://3d-tune-in.eu/toolkit-developers");
            ShowParagraph("The 3D Tune-In Toolkit together with the 3D Tune-In Resource Management Package will be released as open source under GPLv3 license for non-commercial use. Contact developers for commercial use.");

            ShowParagraph("The Unity Wrapper of the 3DTi Toolkit (3DTi Unity Wrapper) allows integration of the different components of the Toolkit in any Unity Scene.These components are packed in the form of a Unity Package requiring Unity 5.2 or above. The current version of the package is built to support the following platforms:");
            BeginBulletList();
                ShowBulletListItem("As Host: Microsoft Windows, Mac OS X.");
                ShowBulletListItem("As Target: Microsoft Windows x64, Mac OS X.");
            EndBulletList();
            

            ShowParagraph("You may use this package to generate 3D sounds without additional restrictions to those imposed by the license of the original audio." +
            "You are not compelled to make any mention to the 3D - Tune - In project or to the app when you use or distribute these audio files," +
            "but we would highly appreciate if you might kindly acknowledge the use of the 3D - Tune - In Toolkit.");
            
            BeginAboutSection("Libraries used in this package");
                BeginBulletList();
                    ShowBulletListItem("3D Tune-In Toolkit (Copyright © University of Malaga and Imperial College London - 2017), which uses:");
                    BeginBulletList();                      
                        ShowBulletListItem("Takuya OOURA General purpose FFT URL: http://www.kurims.kyoto-u.ac.jp/~ooura/fft.html");
                    EndBulletList();
                    ShowBulletListItem("3D Tune-In Resource Management Package (Copyright (c) University of Malaga and Imperial College London - 2017), which uses:");
                    BeginBulletList();
                        ShowBulletListItem("Cereal (Grant, W.Shane and Voorhies, Randolph(2017).cereal - A C++11 library for serialization URL: http://uscilab.github.io/cereal).");
                    EndBulletList();
                EndBulletList();
            EndAboutSection();

            ShowParagraph("This project has received funding from the European Union's Horizon 2020 research and innovation programme under grant agreement No 644051");

        EndAboutSection();

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Show a paragraph of text ending with an optional hyperlink
    /// </summary>
    /// <param name="text"></param>
    /// <param name="urlText"></param>
    public static void ShowParagraph(string text, string urlText="")
    {                
        GUILayout.Label(text, aboutTextStyle, GUILayout.Width(windowWidth));
        if (urlText != "")
        {
            if (GUILayout.Button(urlText, urlStyle))
            {
                Application.OpenURL(urlText);
            }
        }
    }

    /// <summary>
    /// Start a new level for bullet lists
    /// </summary>
    public void BeginBulletList()
    {        
        EditorGUI.indentLevel++;
    }

    /// <summary>
    /// End level for bullet lists
    /// </summary>
    public void EndBulletList()
    {     
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// Show one item in the current bullet list level
    /// </summary>
    public void ShowBulletListItem(string itemText, string urlText="")
    {
        string bulletChar = "";
        if (EditorGUI.indentLevel == 1)
            bulletChar = " • ";
        if (EditorGUI.indentLevel == 2)
            bulletChar = " · ";

        EditorGUILayout.LabelField(bulletChar + itemText, aboutTextStyle);
    }

    /// <summary>
    /// Begin a section containing many text paragraphs
    /// </summary>
    /// <param name="titleText"></param>
    public static void BeginAboutSection(string titleText)
    {
        GUILayout.BeginVertical(aboutSectionStyle);
        GUILayout.Box(titleText, aboutTitleStyle, GUILayout.Width(windowWidth));
    }

    /// <summary>
    /// End section
    /// </summary>
    public static void EndAboutSection()
    {
        GUILayout.EndVertical();        
        //GUILayout.Space(spaceBetweenSections);          // Line spacing    
    }
}