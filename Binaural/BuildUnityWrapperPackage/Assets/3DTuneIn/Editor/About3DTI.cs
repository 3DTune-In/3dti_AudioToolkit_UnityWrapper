using UnityEditor;
using UnityEngine;
 
public class About3DTI : EditorWindow
{
    Vector2 scrollPos;
    const float windowWidth = 600;
    const float windowHeight = 1000;
    const float scrollMargin = 0;
    const float textMargin = scrollMargin + 30;    
    const float sectionMargin = scrollMargin + 30; 
    static GUIStyle aboutTextStyle;
    static GUIStyle aboutSectionStyle;
    static GUIStyle aboutTitleStyle;
    static GUIStyle urlStyle;

    public static void ShowAboutWindow()
    {
        // Setup styles
        aboutTextStyle = new GUIStyle(EditorStyles.label);
        aboutTextStyle.wordWrap = true;
        aboutTextStyle.richText = true;
        aboutTextStyle.alignment = TextAnchor.MiddleLeft;
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
        maxSize = new Vector2(windowWidth, windowHeight);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(windowWidth - scrollMargin));

        BeginAboutSection("3D-TUNE-IN TOOLKIT UNITY WRAPPER");

        GUILayout.Label("Version 3.0", EditorStyles.boldLabel);

        ShowParagraph("This software was developed by a team coordinated by Lorenzo Picinali (Imperial College London) and Arcadio Reyes Lecuona (University of Malaga). The members of the team are: Maria Cuevas Rodriguez, Daniel Gonzalez Toledo, Carlos Garre, Luis Molina Tanco, Ángel Rodríguez Rivero and Ernesto de la Rubia, all affiliated to the University of Malaga, and Tim Murray-Browne (Imperial College London).");

        ShowParagraph("Copyright © University of Malaga - 2018. Email: areyes@uma.es");

        ShowParagraph("The 3D Tune-In Toolkit Unity Wrapper (3DTi Unity Wrapper) is a package which contains a compiled version of the 3D Tune-In Audio Toolkit (Copyright © University of Malaga and Imperial College London - 2018), which is available at https://github.com/3DTune-In/3dti_AudioToolkit.");

        ShowParagraph("It allows integration of the different components of the Toolkit in any Unity Scene. These components are packed in the form of a Unity Package requiring Unity 5.2 or above. The current version of the package is built to support the following platforms:");
        BeginBulletList();
        ShowBulletListItem("As Host: Microsoft Windows, Mac OS X.");
        ShowBulletListItem("As Target: Microsoft Windows, Mac OS X, Android(4.4 or above), iOS.");
        EndBulletList();

        BeginAboutSection("Libraries used in this package");
        BeginBulletList();
        ShowBulletListItem("3D Tune-In Toolkit, which uses:");
        BeginBulletList();
        ShowBulletListItem("Takuya OOURA General purpose FFT URL: http://www.kurims.kyoto-u.ac.jp/~ooura/fft.html");
        ShowBulletListItem("Eigen, a C++ template library for linear algebra.");
        EndBulletList();
        ShowBulletListItem("3D Tune-In Resource Management Package (Copyright (c) University of Malaga and Imperial College London - 2017), which uses:");
        BeginBulletList();
        ShowBulletListItem("Cereal (Grant, W.Shane and Voorhies, Randolph(2017).cereal - A C++11 library for serialization URL: http://uscilab.github.io/cereal).");
        ShowBulletListItem("[S]patially [O]riented [F]ormat for [A]coustics (SOFA) C++ library.");
        EndBulletList();
        EndBulletList();
        EndAboutSection();

        BeginAboutSection("External content distributed together with this software");
        BeginBulletList();
        ShowBulletListItem("HRTF files, corresponding to IRC_1008, IRC_1013, IRC_1022, IRC_1031, IRC_1032, IRC_1048 and IRC_1053, are extracted from the LISTEN database and processed to extract ITD, shortened in different lengths and resampled at different sampling frequencies.");
        EndBulletList();
        EndAboutSection();

        BeginAboutSection("License terms");
        ShowParagraph("You have received this software as a beta-tester. You may not:");
        BeginBulletList();
        ShowBulletListItem("Modify or create any derivative works from this software.");
        ShowBulletListItem("Separate this software, which is licensed as a single product, into its component parts.");
        ShowBulletListItem("Redistribute, encumber, sell, rent, lease, sublicense, or transfer this software under any circumstances.");
        ShowBulletListItem("Remove or alter any trademark, logo, copyright or other proprietary notices, legends, symbols or labels.");
        ShowBulletListItem("Publish or make public any results of benchmark tests run on any Software to a third party.");
        EndBulletList();
        ShowParagraph("If you have any questions or problems please contact Lorenzo Picinali at l.picinali@imperial.ac.uk.");
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
        GUILayout.Label(text, aboutTextStyle, GUILayout.Width(windowWidth - textMargin));
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
    public void BeginAboutSection(string titleText)
    {
        GUILayout.BeginVertical(aboutSectionStyle);
        GUILayout.Box(titleText, aboutTitleStyle, GUILayout.Width(windowWidth - sectionMargin));
    }

    /// <summary>
    /// End section
    /// </summary>
    public void EndAboutSection()
    {
        GUILayout.EndVertical();        
        //GUILayout.Space(spaceBetweenSections);          // Line spacing    
    }
}