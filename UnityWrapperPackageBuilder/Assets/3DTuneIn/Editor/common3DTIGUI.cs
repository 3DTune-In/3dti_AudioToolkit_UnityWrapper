using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using API_3DTI_Common;

public class Common3DTIGUI
{
    static int logoheight = 59;
    static int earsize = 40;
    static float singleSpace = 5.0f;
    static float spaceBetweenSections = 5.0f;
    static float spaceBetweenColumns = 5.0f;
    static float parameterLabelWidth = 0.0f;    
    
    static GUIStyle titleBoxStyle;
    static GUIStyle subtitleBoxStyle;
    static GUIStyle sectionStyle;
    static GUIStyle subsectionStyle;
    static GUIStyle dragdropStyle;
    static GUIStyle parameterLabelStyle;
    static GUIStyle leftColumnStyle;
    static GUIStyle rightColumnStyle;
    static GUIStyle intFieldStyle;
    static GUIStyle aboutButtonStyle;

    /// <summary>
    /// Init all styles
    /// </summary>
    public static void InitStyles()
    {
        titleBoxStyle = new GUIStyle(GUI.skin.box);
        titleBoxStyle.normal.textColor = EditorStyles.label.normal.textColor;
        titleBoxStyle.fontStyle = FontStyle.Bold;

        subtitleBoxStyle = new GUIStyle(EditorStyles.label);
        subtitleBoxStyle.fontStyle = FontStyle.Bold;
        subtitleBoxStyle.alignment = TextAnchor.MiddleLeft;

        sectionStyle = new GUIStyle(GUI.skin.box);
        subsectionStyle = new GUIStyle(GUI.skin.box);
        leftColumnStyle = new GUIStyle(GUI.skin.box);
        leftColumnStyle.alignment = TextAnchor.MiddleLeft;
        rightColumnStyle = new GUIStyle(GUI.skin.box);
        rightColumnStyle.alignment = TextAnchor.MiddleRight;

        dragdropStyle = new GUIStyle(EditorStyles.textField);
        dragdropStyle.alignment = TextAnchor.MiddleRight; 
        parameterLabelStyle = new GUIStyle(GUI.skin.label);
        parameterLabelStyle.alignment = TextAnchor.MiddleRight;
        intFieldStyle = new GUIStyle(EditorStyles.textField);
        intFieldStyle.alignment = TextAnchor.MiddleLeft;

        aboutButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
        aboutButtonStyle.alignment = TextAnchor.MiddleCenter;
    }

    public static void SetInspectorIcon(GameObject go)
    {
        Texture2D tex = EditorGUIUtility.IconContent("Assets/3DTuneIn/Resources/3D_tuneinNoAlpha").image as Texture2D;
        Type editorGUIUtilityType = typeof(EditorGUIUtility);
        BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
        object[] args = new object[] { go, tex };
        editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
    }

    /// <summary>
    /// Show 3D-Tune-In logo centered 
    /// </summary>
    public static void Show3DTILogo()
    {
        Texture logo3DTI;
        GUIStyle logoStyle = EditorStyles.label;
        logoStyle.alignment = TextAnchor.MiddleCenter;        
        if (EditorGUIUtility.isProSkin)
            logo3DTI = Resources.Load("logo3DTIDarkBackground") as Texture;
        else
            logo3DTI = Resources.Load("logo3DTILightBackground") as Texture;        
        GUILayout.Box(logo3DTI, logoStyle, GUILayout.Height(logoheight), GUILayout.ExpandWidth(true));          
    }

    /// <summary>
    /// Show button for opening About window
    /// </summary>
    public static void ShowAboutButton()
    {
        SingleSpace();
        EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            if (GUILayout.Button("About 3D-Tune-In Toolkit", aboutButtonStyle, GUILayout.ExpandWidth(true)))
                About3DTI.ShowAboutWindow();
        EditorGUILayout.EndHorizontal();  
    }

    /// <summary>
    /// Create a toggle for folding out parameters
    /// </summary>
    /// <param name="boolvar"></param>
    /// <param name="titleText"></param>
    /// <returns></returns>
    public static bool CreateFoldoutToggle(ref bool boolvar, string titleText)
    {
        //return EditorGUILayout.Foldout(boolvar, titleText, true);
        return GUILayout.Toggle(boolvar, titleText, "foldout");
    }

    /// <summary>
    /// Auxiliary function for creating sliders for float variables with specific format
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="name"></param>
    /// <param name="decimalDigits"></param>
    /// <param name="units"></param>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <param name="action"></param>
    public static bool CreateFloatSlider(ref float variable, string name, string decimalDigits, string units, string tooltip, float minValue, float maxValue, System.Action action=null)
    {
        SingleSpace();

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

        // Parameter name        
        GUILayout.Label(new GUIContent(name, tooltip), parameterLabelStyle, GUILayout.Width(GetParameterLabelWidth()));

        // Slider
        float previousVar = variable;
        variable = GUILayout.HorizontalSlider(variable, minValue, maxValue, GUILayout.ExpandWidth(true));
        if (variable != previousVar)
        {            
            if (action != null)
                action.Invoke();
        }

        // Text field with units
        string valueString = GUILayout.TextField(variable.ToString(decimalDigits), GUILayout.ExpandWidth(false));
        GUILayout.Label(units, GUILayout.ExpandWidth(false));
        float newValue;
        bool valid = float.TryParse(valueString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out newValue);
        if (valid)
            variable = newValue;

        GUILayout.EndHorizontal();

        return previousVar != variable;
    }
   
    /// <summary>
    /// Auxiliary function for creating a text input accepting integer variables with specific format
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="name"></param>
    /// <param name="units"></param>
    /// <param name="action"></param>
    public static void CreateIntInput(ref int variable, string name, string units, string tooltip, int minValue, int maxValue, System.Action action)
    {        
        SingleSpace();
        float previousVar = variable;

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));                
        EditorGUIUtility.labelWidth = GetOneLabelWidth(name);
        variable = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent(name, tooltip), variable, intFieldStyle, GUILayout.Width(5 + GetOneLabelWidth(name)+GetRangeWidth(minValue, maxValue))), minValue, maxValue);
        GUILayout.Label(units, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        //int newValue;        
        //string valueString = GUILayout.TextField(variable.ToString(), GUILayout.ExpandWidth(false));                        
        //bool valid = int.TryParse(valueString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out newValue);
        //if (valid)
        //    variable = newValue;

        if ((variable != previousVar) && (variable > minValue) && (variable < maxValue))
        {
            if (action != null)
                action.Invoke();
        }
    }

    /// <summary>
    /// Auxiliary function for creating a new section with title for parameter groups
    /// </summary>    
    public static void BeginSection(string titleText)
    {
        GUILayout.BeginVertical(sectionStyle);
        GUILayout.Box(titleText, titleBoxStyle, GUILayout.ExpandWidth(true));
        ResetParameterGroup();
    }

    /// <summary>
    /// Auxiliary function for ending section of parameter group
    /// </summary>
    public static void EndSection()
    {
        GUILayout.EndVertical();
        //GUILayout.Label("");    // Line spacing        
        GUILayout.Space(spaceBetweenSections);          // Line spacing    
    }

    /// <summary>
    /// Auxiliary function for creating a new subsection with title for parameter groups within one section
    /// </summary>    
    public static void BeginSubsection(string titleText)
    {
        GUILayout.BeginVertical(subsectionStyle);        
        GUILayout.Box(titleText, subtitleBoxStyle, GUILayout.ExpandWidth(true));
        ResetParameterGroup();
    }

    /// <summary>
    /// Auxiliary function for ending subsection for parameter groups within one section
    /// </summary>
    public static void EndSubsection()
    {
        GUILayout.EndVertical();
        GUILayout.Space(singleSpace);          // Line spacing      
    }    

    /// <summary>
    ///  Reset width of parameter labels, to adjust the width independently for each group of parameters
    /// </summary>
    public static void ResetParameterGroup()
    {
        parameterLabelWidth = 0.0f;
    }

    /// <summary>
    /// Add one label to a group of parameters, to adjust the width of all labels to the bigger one
    /// </summary>
    /// <param name="labeltext"></param>
    public static void AddLabelToParameterGroup(string labeltext)
    {
        float labelwidth = GetOneLabelWidth(labeltext);
        if (parameterLabelWidth < labelwidth)
            parameterLabelWidth = labelwidth;
    }

    /// <summary>
    /// Get width of one label 
    /// </summary>
    /// <param name="labeltext"></param>
    /// <returns></returns>
    public static float GetOneLabelWidth(string labeltext)
    {
        //return GUI.skin.label.CalcSize(new GUIContent(labeltext)).x;
        return (parameterLabelStyle.CalcSize(new GUIContent(labeltext))).x;
    }

    /// <summary>
    /// Get maximum width of a string from a range of ints
    /// </summary>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <returns></returns>
    public static float GetRangeWidth(int minValue, int maxValue)
    {
        float minwidth = GetOneLabelWidth(minValue.ToString());
        float maxwidth = GetOneLabelWidth(maxValue.ToString());
        if (minwidth > maxwidth)
            return minwidth;
        else
            return maxwidth;
    }

    /// <summary>
    ///  Get width of bigger parameter
    /// </summary>
    /// <returns></returns>
    public static float GetParameterLabelWidth()
    {
        return parameterLabelWidth;
    }

    /// <summary>
    /// Action to do when a new file has been selected (either for HRTF or ILD, and either from button or drag&drop)
    /// </summary>
    public static void ChangeFileName(ref string whichfilename, string newname)
    {
        // Set new name for toolkit API
        whichfilename = newname;

        // Check that Resources folder exists. Create it otherwise.
        if (!Directory.Exists("Assets/Resources"))
        {
            Directory.CreateDirectory("Assets/Resources");
        }

        // Save it in resources as .byte                
        string newnamewithpath = "Assets/Resources/" + Path.GetFileNameWithoutExtension(newname) + ".bytes";
        if (!File.Exists(newnamewithpath))
            FileUtil.CopyFileOrDirectory(whichfilename, newnamewithpath);
    }

    /// <summary>
    /// Auxiliary function for creating a drag&drop text box
    /// </summary>
    public static void CreateLoadDragDropBox(ref string textvar)
    {
        //Rect drop_area = GUILayoutUtility.GetRect(0.0f, dropAreaHeight, GUILayout.ExpandWidth(true));
        ////drop_area.y += 14.0f;    
        ////drop_area.y += 4.0f;
        //GUI.Box(drop_area, textvar, dragdropStyle);

        textvar = GUILayout.TextField(textvar, GUILayout.MinWidth(GetOneLabelWidth(textvar) + 5.0f), GUILayout.ExpandWidth(true));
        Rect drop_area = GUILayoutUtility.GetLastRect();

        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    ChangeFileName(ref textvar, DragAndDrop.paths[0]);
                }
                break;
        }
    }

    /// <summary>
    /// Create a button and a drag&drop box for loading a file
    /// </summary>
    /// <param name="titleText"></param>
    /// <param name="strvar"></param>
    /// <param name="action"></param>
    public static void CreateLoadButtonAndBox(string titleText, string tooltip, ref string strvar, System.Action action)
    {
        //GUILayout.BeginHorizontal();
        //if (GUILayout.Button(titleText, GUILayout.ExpandWidth(false), GUILayout.Height(buttonHeight)))
        //    action();
        //CreateLoadDragDropBox(ref strvar);
        //GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();        
        GUILayout.Label(new GUIContent(titleText, tooltip), parameterLabelStyle, GUILayout.Width(GetParameterLabelWidth()));
        CreateLoadDragDropBox(ref strvar);
        if (GUILayout.Button("", EditorStyles.radioButton, GUILayout.ExpandWidth(false)))
            action();        
        GUILayout.EndHorizontal();
    }

    /// <summary>
    ///  Auxiliary function for creating toogle input
    /// </summary>    
    public static bool CreateToggle(ref bool boolvar, string toggleText, string tooltip)
    {
        bool oldvar = boolvar;
        boolvar = GUILayout.Toggle(boolvar, new GUIContent(toggleText, tooltip), GUILayout.ExpandWidth(false));
        return (oldvar != boolvar);
    }

    ///// <summary>
    /////  Auxiliary function for creating radio buttons
    ///// </summary>    
    //public static bool CreateTwoChoiceRadioButton(ref bool boolvar, string choiceFalseText, string choiceFalseTooltip, string choiceTrueText, string choiceTrueTooltip, bool vertical=true)
    //{
    //    int choice;
    //    bool oldvar = boolvar;
    //    if (boolvar)
    //        choice = 1;
    //    else
    //        choice = 0;

    //    // xCount tells how many elements fit into one horizontal row
    //    int xCount;
    //    if (vertical)
    //        xCount = 1;
    //    else
    //        xCount = 2;

    //    GUIContent[] contents = { new GUIContent(choiceFalseText, choiceFalseTooltip), new GUIContent(choiceTrueText, choiceTrueTooltip) };
    //    choice = GUILayout.SelectionGrid(choice, contents, xCount, EditorStyles.radioButton);
    //    if (choice == 0)
    //        boolvar = false;
    //    else
    //        boolvar = true;

    //    return (oldvar != boolvar);
    //}

    /// <summary>
    ///  Auxiliary function for creating radio buttons
    /// </summary>    
    public static bool CreateRadioButtons(ref int choice, List<string> choiceTexts, List<string> tooltips, bool vertical = true)
    {        
        int oldvar = choice;     

        // xCount tells how many elements fit into one horizontal row
        int xCount;
        if (vertical)
            xCount = 1;
        else
            xCount = choiceTexts.Count;

        GUIContent[] contents = new GUIContent[choiceTexts.Count];        
        for (int i = 0; i < choiceTexts.Count; i++)
        {
            contents[i] = new GUIContent(choiceTexts[i], tooltips[i]);
        }
        choice = GUILayout.SelectionGrid(choice, contents, xCount, EditorStyles.radioButton);

        return (oldvar != choice);
    }

    /// <summary>
    /// Auxiliary function for creating a led/lamp indicator
    /// </summary>
    /// <param name="lightOn"></param>
    public static void CreateLamp(bool lightOn)
    {
        //GUILayout.Button("", lampStyle, GUILayout.ExpandWidth(false));        
        //GUILayout.Button("", GUILayout.ExpandWidth(false));
    }

    /// <summary>
    /// Put a single horizontal space
    /// </summary>
    public static void SingleSpace()
    {
        GUILayout.Space(singleSpace);
    }

    /// <summary>
    /// Begin column for left ear
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="enable"></param>
    /// <param name="title"></param>
    /// <param name="switchParameters"></param>
    public static bool BeginLeftColumn(IAudioEffectPlugin plugin, ref bool enable, string title, string tooltip, List<string> switchParameters)
    {
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));                     // Begin section (ear pair)             
            GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(false));  // Begin column (left ear)                               
                bool changed = CreateToggle(ref enable, title, tooltip);
                if (changed)
                { 
                    foreach (string switchParameter in switchParameters)
                    {
                        plugin.SetFloatParameter(switchParameter, CommonFunctions.Bool2Float(enable));
                    }
                }
                EditorGUI.BeginDisabledGroup(!enable); // Begin DisabledGroup 
                    // ...
        return changed;
    }

    /// <summary>
    /// End column for left ear
    /// </summary>
    /// <param name="endToogleGroup"></param>
    public static void EndLeftColumn()
    {
                EditorGUI.EndDisabledGroup();   // End DisabledGroup 
            GUILayout.EndVertical();            // End column
            GUILayout.Space(spaceBetweenColumns);   // Space between columns        
    }

    /// <summary>
    /// Begin column for right ear
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="enable"></param>
    /// <param name="title"></param>
    /// <param name="switchParameters"></param>
    public static bool BeginRightColumn(IAudioEffectPlugin plugin, ref bool enable, string title, string tooltip, List<string> switchParameters)
    {
            GUILayout.BeginVertical(rightColumnStyle, GUILayout.ExpandWidth(false)); // Begin column (right ear)               
                bool changed = CreateToggle(ref enable, title, tooltip);
                if (changed)
                {
                    foreach (string switchParameter in switchParameters)
                    {
                        plugin.SetFloatParameter(switchParameter, CommonFunctions.Bool2Float(enable));
                    }
                }
                EditorGUI.BeginDisabledGroup(!enable);      // Begin Disabled if right ear is switched off
                    // ...
        return changed;
    }

    /// <summary>
    /// End column for right ear
    /// </summary>
    /// <param name="endToogleGroup"></param>
    public static void EndRightColumn()
    {
                EditorGUI.EndDisabledGroup();       // End disabled if right ear is switched off 
            GUILayout.EndVertical();                    // End column (right ear)        
        GUILayout.EndHorizontal();                      // End section (ear pair)        
        GUILayout.Space(spaceBetweenSections);              // Space between sections
    }

    /// <summary>
    /// Begin column for left ear
    /// </summary>
    /// <param name="leftEarOn"></param>
    public static void BeginLeftColumn(bool leftEarOn)
    {
        ResetParameterGroup();
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));                     // Begin section (ear pair)
            GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(true));  // Begin column (left ear)
                EditorGUI.BeginDisabledGroup(!leftEarOn);                           // Begin Disabled if left ear is switched off                    
                    GUILayout.Label("LEFT EAR", subtitleBoxStyle);                    
    }

    /// <summary>
    /// Begin column for right ear
    /// </summary>
    /// <param name="rightEarOn"></param>
    public static void BeginRightColumn(bool rightEarOn)
    {
            ResetParameterGroup();
            GUILayout.BeginVertical(rightColumnStyle, GUILayout.ExpandWidth(true)); // Begin column (right ear)
                EditorGUI.BeginDisabledGroup(!rightEarOn);                          // Begin Disabled if right ear is switched off
                    GUILayout.Label("RIGHT EAR", subtitleBoxStyle);        
    }

    // Begin column inside other (left/right) column
    public static void BeginSubColumn(string title)
    {
        ResetParameterGroup();
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));        
            GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label(title, subtitleBoxStyle);
    }

    /// <summary>
    ///  End column inside other (left/right) column
    /// </summary>
    public static void EndSubColumn()
    {
            GUILayout.EndVertical();                                               
        GUILayout.EndHorizontal();
    }

    /// <summary>
    ///  Draw one ear with alpha background
    /// </summary>
    /// <param name="whichear"></param>
    public static void DrawEar(T_ear whichear)
    {        
        Texture earTexture;
        GUIStyle earStyle = new GUIStyle (EditorStyles.label);
        if (whichear == T_ear.LEFT)
        {
            earStyle.alignment = TextAnchor.MiddleLeft;
            if (EditorGUIUtility.isProSkin)
                earTexture = Resources.Load("LeftEarDarkAlpha") as Texture;
            else
                earTexture = Resources.Load("LeftEarLightAlpha") as Texture;            
        }
        else
        {
            earStyle.alignment = TextAnchor.MiddleRight;
            if (EditorGUIUtility.isProSkin)
                earTexture = Resources.Load("RightEarDarkAlpha") as Texture;
            else
                earTexture = Resources.Load("RightEarLightAlpha") as Texture;
        }
        GUILayout.Box(earTexture, earStyle, GUILayout.Width(earsize), GUILayout.Height(earsize), GUILayout.ExpandWidth(false));        
    }

    /// <summary>
    /// Create a slider associated to a parameter of an audio plugin
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="parameterName"></param>
    /// <param name="parameterText"></param>
    /// <param name="isFloat"></param>
    /// <param name="units"></param>
    /// <returns>True if slider value has changed</returns>
    public static bool CreatePluginParameterSlider(IAudioEffectPlugin plugin, ref float APIparam, string parameterName, string parameterText, bool isFloat, string units, string tooltip, bool isCompact=false)
    {
        // Get parameter info
        float newValue;
        float minValue, maxValue;
        plugin.GetFloatParameterInfo(parameterName, out minValue, out maxValue, out newValue);

        // Set float resolution
        string resolution;
        if (isFloat)
            resolution = "F2";
        else
            resolution = "F0";

        // Create slider and set value
        plugin.GetFloatParameter(parameterName, out newValue);
        bool valueChanged;
        if (isCompact)
            valueChanged = CreateCompactFloatSlider(ref newValue, parameterText, resolution, units, tooltip, minValue, maxValue);
        else
            valueChanged = CreateFloatSlider(ref newValue, parameterText, resolution, units, tooltip, minValue, maxValue);

        if (valueChanged)
        {
            plugin.SetFloatParameter(parameterName, newValue);
            APIparam = newValue;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Auxiliary function for creating sliders for float variables with specific format
    /// </summary>
    /// <returns></returns>
    public static bool CreateCompactFloatSlider(ref float variable, string name, string decimalDigits, string units, string tooltip, float minValue, float maxValue)
    {
        string valueString;
        float previousVar;

        GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(new GUIContent(name, tooltip));
                valueString = variable.ToString(decimalDigits);
                valueString = GUILayout.TextField(valueString, GUILayout.ExpandWidth(false));
                GUILayout.Label(units, GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();

            float newValue;
            bool valid = float.TryParse(valueString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out newValue);
            if (valid)
                variable = newValue;

            previousVar = variable;
            variable = GUILayout.HorizontalSlider(variable, minValue, maxValue);
        }
        GUILayout.EndVertical();

        return (variable != previousVar);
    }

    /// <summary>
    ///  Auxiliary function for creating toogle input
    /// </summary>    
    public static void CreatePluginToggle(IAudioEffectPlugin plugin, ref bool boolvar, string toggleText, string switchParameter, string tooltip)
    {        
        if (CreateToggle(ref boolvar, toggleText, tooltip))        
        {
            plugin.SetFloatParameter(switchParameter, CommonFunctions.Bool2Float(boolvar));
        }
    }

    /// <summary>
    /// Show a text box with readonly value of one float variable
    /// </summary>
    /// <param name="titleText"></param>
    /// <param name="decimalDigits"></param>
    /// <param name="units"></param>
    /// <param name="tooltip"></param>
    /// <param name="value"></param>
    public static void CreateReadonlyFloatText(string titleText, string decimalDigits, string units, string tooltip, float value)
    {
        GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(titleText, tooltip), GUILayout.ExpandWidth(false));
            string valueStr = value.ToString(decimalDigits);
            GUILayout.TextArea(valueStr, GUILayout.ExpandWidth(false));
            GUILayout.Label(units, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Get one string with the first letter of one ear ("L" or "R")
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public static string GetEarLetter(T_ear ear)
    {
        if (ear == T_ear.LEFT)
            return "L";
        if (ear == T_ear.RIGHT)
            return "R";
        return "";
    }

    /// <summary>
    /// Get one string with the full of one ear ("left" or "right")
    /// </summary>
    /// <param name="ear"></param>
    /// <returns></returns>
    public static string GetEarName(T_ear ear)
    {
        if (ear == T_ear.LEFT)
            return "left";
        if (ear == T_ear.RIGHT)
            return "right";
        if (ear == T_ear.BOTH)
            return "both";
        return "unknown";
    }
}