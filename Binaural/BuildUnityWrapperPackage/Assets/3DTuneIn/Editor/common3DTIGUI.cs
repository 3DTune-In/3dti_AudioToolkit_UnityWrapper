using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using API_3DTI_Common;
using System.Linq;

public static class Common3DTIGUI
{
    static int logoheight = 59;
    static int earsize = 40;
    static int mainTitleSize = 14;
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
    static GUIStyle bigTitleStyle;

	//public static void ListHRTFFiles()
	//{
	//	UnityEditor.
	//}

    /// <summary>
    /// Init all styles
    /// </summary>
    public static void InitStyles()
    {
        titleBoxStyle = new GUIStyle(GUI.skin.box);
        titleBoxStyle.normal.textColor = EditorStyles.label.normal.textColor;
        titleBoxStyle.fontStyle = FontStyle.Bold;

        bigTitleStyle = new GUIStyle(GUI.skin.box);
        bigTitleStyle.normal.textColor = EditorStyles.label.normal.textColor;
        bigTitleStyle.fontStyle = FontStyle.Bold;
        bigTitleStyle.fontSize = mainTitleSize;

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

    public static void ShowGUITitle(string title)
    {        
        GUILayout.Label(title, bigTitleStyle, GUILayout.ExpandWidth(true));
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
        string valueString = GUILayout.TextField(variable.ToString(decimalDigits, System.Globalization.CultureInfo.InvariantCulture), GUILayout.ExpandWidth(false));
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


	public static void CreatePopupStringSelector(string titleText, string tooltip, string[] items, ref string target, string prefix = "", string suffix = "")
	{
		EditorGUILayout.BeginHorizontal();
		//EditorGUILayout.PrefixLabel(new GUIContent(titleText, tooltip), parameterLabelStyle, GUILayout.Width(GetParameterLabelWidth()));
		int selectedIndex = -1;
		if (target.Length > prefix.Length + suffix.Length && target.StartsWith(prefix) && target.EndsWith(suffix))
		{
			string trimmedTarget = target.Remove(target.Length - suffix.Length).Remove(0, prefix.Length);
			selectedIndex = new List<string>(items).IndexOf(trimmedTarget);
		}
		else if (target != "")
		{
			Debug.LogWarning("Unable to find previously selection: " + target);
		}
		int newSelectedIndex = EditorGUILayout.Popup(new GUIContent(titleText, tooltip), selectedIndex, items);
		target = newSelectedIndex < 0? "" : (prefix + items[newSelectedIndex] + suffix);
		EditorGUILayout.EndHorizontal();
	}

    public static T PluginEnumSelector<T>(IAudioEffectPlugin plugin, string parameterName, string title, string tooltip) where T : Enum
    {
        int value = plugin.GetIntParameter(parameterName);
        Debug.Assert(Enum.GetUnderlyingType(typeof(T)) == typeof(int));
        int[] values = (int[])Enum.GetValues(typeof(T));
        if (!values.Contains(value))
        {
            Debug.LogWarning($"Plugin returned invalid value for {parameterName}: {value}");
        }

        int defaultValue = (int)Enum.GetValues(typeof(T)).GetValue(0);


        int newValue = (int)(object) EditorGUILayout.Popup(new GUIContent(title, tooltip), values.Contains(value)? value : defaultValue, Enum.GetNames(typeof(T)));
        if (!Enum.IsDefined(typeof(T), newValue))
        {
            Debug.LogError($"Invalid value for {typeof(T)} received from popup: {newValue}.");
        }

        if (value != newValue)
        {
            plugin.SetFloatParameter(parameterName, newValue);
        }

        return (T)(object)newValue;
    }


    public static void CreatePopupStringSelector<T>(string titleText, string tooltip, ref T target) where T : System.Enum
    {
        EditorGUILayout.BeginHorizontal();
        Array valueObjects = Enum.GetValues(typeof(T));
        var values = new T[valueObjects.Length];
        var labels = new string[valueObjects.Length];
        int currentIndex = 0;
        for (int i=0; i<valueObjects.Length; i++)
        {
            values[i] = (T)(object)valueObjects.GetValue(i);
            labels[i] = values[i].ToString();
            if (EqualityComparer<T>.Default.Equals(values[i], target))
            {
                currentIndex = i;
            }
        }

        int newIndex = EditorGUILayout.Popup(new GUIContent(titleText, tooltip), currentIndex, labels);
        target = values[newIndex];
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
    /// Draw one button 
    /// </summary>
    /// <param name="titleText"></param>
    /// <param name="tooltip"></param>
    /// <returns>true when clicked</returns>
    public static bool CreateButton(string titleText, string tooltip)
    {
        return GUILayout.Button(new GUIContent(titleText, tooltip), GUI.skin.button, GUILayout.ExpandWidth(false));            
    }

    /// <summary>
    ///  Auxiliary function for creating toogle input
    /// </summary>    
    public static bool CreateToggle(ref bool boolvar, string toggleText, string tooltip, bool forceChange)
    {
        bool oldvar = boolvar;
        boolvar = GUILayout.Toggle(boolvar, new GUIContent(toggleText, tooltip), GUILayout.ExpandWidth(false));
        return (oldvar != boolvar) || forceChange;
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


    // Draw two columns, one for each ear, and use the provided function to populate with controls.
    // If any of enablerParameters is false then the contents will be disabled. enablerParameters
    // should all have bool as their underlying type.
    // If toggleParameter is non-null then it should be a bool parameter. A toggle will be drawn and when unchecked
    // the callback will be called in a disabled group
    // perEarCallback is drawn for each ear in its respective column
    // bothEarsCallback is drawn after everything at full width
    // returns true if perEarCallback ever returns true
    public static bool DrawColumnForEachEar<ParameterT>(IAudioEffectPlugin plugin, string title, ParameterT[] enablerParameters, ParameterT? groupEnabledToggleParameter, Func<T_ear, bool> perEarCallback, Func<bool> bothEarsCallback = null) 
        where ParameterT : struct, Enum
    {
        bool returnValue = false;

        if (title != null)
        {
            Common3DTIGUI.BeginSection(title);
        }
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));                     // Begin section (ear pair)
        {
            foreach (T_ear ear in new T_ear[] { T_ear.LEFT, T_ear.RIGHT })
            {
                bool isEnabled = true;
                foreach (ParameterT p in enablerParameters)
                {
                    isEnabled = isEnabled && plugin.GetParameter<ParameterT, bool>(p, ear);
                }
                EditorGUI.BeginDisabledGroup(!isEnabled);
                {
                    //-
                    ResetParameterGroup();
                    if (ear == T_ear.LEFT)
                    {
                        GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(false));  // Begin column (left ear)
                    }
                    else
                    {
                        ResetParameterGroup();
                        GUILayout.BeginVertical(rightColumnStyle, GUILayout.ExpandWidth(false));  // Begin column (right ear)
                    }
                    //-
                    float isGroupToggleEnabled = 1.0f;
                    if (groupEnabledToggleParameter.HasValue)
                    {
                        Convert.ToBoolean(CreateControl(plugin, groupEnabledToggleParameter.Value, out isGroupToggleEnabled, ear));
                    }
                    EditorGUI.BeginDisabledGroup(isGroupToggleEnabled == 0.0f); // Begin DisabledGroup
                    {
                        returnValue = perEarCallback(ear) || returnValue;
                    }
                    EditorGUI.EndDisabledGroup();
                    //-
                    GUILayout.EndVertical();            // End column
                    GUILayout.Space(spaceBetweenColumns);   // Space between columns 
                    //-
                }
                EditorGUI.EndDisabledGroup();
            }
        }
        GUILayout.EndHorizontal();                      // End section (ear pair)        

        if (bothEarsCallback != null)
        {
            returnValue = bothEarsCallback() || returnValue;
        }

        Common3DTIGUI.EndSection();

        return returnValue;
    }


    /// <summary>
    /// Begin column for left ear
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="enable"></param>
    /// <param name="title"></param>
    /// <param name="switchParameters"></param>
    public static bool BeginLeftColumn(IAudioEffectPlugin plugin, ref bool enable, string title, string tooltip, List<string> switchParameters, bool forceChange, bool doExpandWidth = false)
    {
        ResetParameterGroup();
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(doExpandWidth));                     // Begin section (ear pair)             
            GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(doExpandWidth));  // Begin column (left ear)                               
                bool changed = CreateToggle(ref enable, title, tooltip, forceChange);
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

    // Start a column with a toggle control controlling the given parameter.
    public static void BeginColumn<ParameterT>(IAudioEffectPlugin plugin, T_ear ear, ParameterT toggleBoxParameter) where ParameterT : struct, Enum
    {
        ResetParameterGroup();
        if (ear == T_ear.LEFT)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));                     // Begin section (ear pair)
            GUILayout.BeginVertical(leftColumnStyle, GUILayout.ExpandWidth(false));  // Begin column (left ear)                               

        }
        else
        {
            ResetParameterGroup();
            GUILayout.BeginVertical(rightColumnStyle, GUILayout.ExpandWidth(false));  // Begin column (right ear)
        }

        Convert.ToBoolean(CreateControl(plugin, toggleBoxParameter, out float isEnabled, ear));
        EditorGUI.BeginDisabledGroup(isEnabled == 0.0f); // Begin DisabledGroup
    }

    public static void EndLeftColumn()
    {
                EditorGUI.EndDisabledGroup();   // End DisabledGroup 
            GUILayout.EndVertical();            // End column
            GUILayout.Space(spaceBetweenColumns);   // Space between columns        
    }

    public static void EndColumn(T_ear ear)
    {
        EditorGUI.EndDisabledGroup();   // End DisabledGroup 
        GUILayout.EndVertical();            // End column
        if (ear == T_ear.RIGHT)
        {
            GUILayout.EndHorizontal();                      // End section (ear pair)        
        }
        GUILayout.Space(spaceBetweenColumns);   // Space between columns  


    }

    /// <summary>
    /// Begin column for right ear
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="enable"></param>
    /// <param name="title"></param>
    /// <param name="switchParameters"></param>
    public static bool BeginRightColumn(IAudioEffectPlugin plugin, ref bool enable, string title, string tooltip, List<string> switchParameters, bool forceChange, bool doExpandWidth=false)
    {
        ResetParameterGroup();
            GUILayout.BeginVertical(rightColumnStyle, GUILayout.ExpandWidth(doExpandWidth)); // Begin column (right ear)               
                bool changed = CreateToggle(ref enable, title, tooltip, forceChange);
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
    /// Draw an image from 3DTuneIn/Resouces folder
    /// </summary>
    /// <param name="imageFileName"></param>
    public static void DrawImage(string imageFileName, float width, float height)
    {
        Texture imageTexture;
        GUIStyle imageStyle = new GUIStyle(EditorStyles.label);            
        imageTexture = Resources.Load(imageFileName) as Texture;              
        GUILayout.Box(imageTexture, imageStyle, GUILayout.Width(width), GUILayout.Height(height), GUILayout.ExpandWidth(false));
    }

    /// <summary>
    /// Draw an blank space with given dimensions
    /// </summary>    
    public static void DrawBlank(float width, float height)
    {
        GUIStyle blankStyle = new GUIStyle(EditorStyles.label);
        GUILayout.Box("", blankStyle, GUILayout.Width(width), GUILayout.Height(height), GUILayout.ExpandWidth(false));
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

    // Create a group of controls with the labels aligned. Returns true if any control changed
    public static bool CreateControls<T>(IAudioEffectPlugin plugin, T_ear ear, bool isCompact, params T[] parameters)
        where T : Enum
    {
        foreach (T parameter in parameters)
        {
            ParameterAttribute p = parameter.GetAttribute<ParameterAttribute>();
            if (p == null)
            {
                throw new Exception($"Failed to find ParameterAttribute for parameter {parameter}");
            }
            AddLabelToParameterGroup(p.label);
        }
        bool didChange = false;
        foreach (T parameter in parameters)
        {
            ParameterAttribute p = parameter.GetAttribute<ParameterAttribute>();
            Debug.Assert(p != null);
            didChange = CreateControl(plugin, parameter, ear, isCompact) || didChange;
        }
        return didChange;
    }

    // Create a control for a parameter. Returns true if the value has changed
    public static bool CreateControl<T>(IAudioEffectPlugin plugin, T parameter, T_ear ear, bool isCompact = false) where T : Enum
    {
        return CreateControl(plugin, parameter, out float _, ear, isCompact);
    }


    // Create a control for a parameter. Returns true if the value has changed. Use this version if you need to grab a copy of the updated value. The updated value is always represented as a float as received directly from the plugin.
    public static bool CreateControl<ParameterEnum>(IAudioEffectPlugin plugin, ParameterEnum parameter, out float value, T_ear ear, bool isCompact=false) 
        where ParameterEnum : Enum 
    {
        ParameterAttribute p = parameter.GetAttribute<ParameterAttribute>();
        if (p == null)
        {
            throw new Exception($"Failed to find ParameterAttribute for parameter {parameter}");
        }

        SingleSpace();

        if (p.type == typeof(float) || p.type==typeof(int))
        {
            // Get parameter info
            plugin.GetFloatParameterInfo(p.pluginName(ear), out float minValue, out float maxValue, out float _);

            //float oldValue = plugin.GetFloatParameter(p.pluginName(ear));
            float oldValue = plugin.GetParameter<ParameterEnum, float>(parameter, ear);
            float newValue;
            string valueString;
            if (isCompact)
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                GUILayout.Label(new GUIContent(p.label, p.description));
                valueString = GUILayout.TextField(oldValue.ToString(p.type==typeof(float) ? "F2" : "F0", System.Globalization.CultureInfo.InvariantCulture), GUILayout.ExpandWidth(false));
                GUILayout.Label(p.units, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                // TODO: I Think this will have a bug where newValue gets overwritten by oldvalue in the parse below
                newValue = GUILayout.HorizontalSlider(oldValue, minValue, maxValue);
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                AddLabelToParameterGroup(p.label);
                GUILayout.Label(new GUIContent(p.label, p.description), parameterLabelStyle, GUILayout.Width(GetParameterLabelWidth()));
                newValue = GUILayout.HorizontalSlider(oldValue, minValue, maxValue, GUILayout.ExpandWidth(true));
                valueString = GUILayout.TextField(newValue.ToString(p.type==typeof(float) ? "F2" : "F0", System.Globalization.CultureInfo.InvariantCulture), GUILayout.ExpandWidth(false));
                GUILayout.Label(p.units, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
            }

            bool parseOk = float.TryParse(valueString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float parsedValueString);
            if (parseOk)
            {
                newValue = parsedValueString;
            }
            if (p.validValues != null && p.validValues.Length > 0)
            {
                // Lock to nearest valid value
                newValue = p.validValues.OrderBy(x => Math.Abs(x - newValue)).First();
            }
            value = Convert.ToSingle(newValue);

            if (newValue != oldValue)
            {
                plugin.SetFloatParameter(p.pluginName(ear), newValue);
                return true;
            }
            return false;
        }
        else if (p.type == typeof(bool))
        {
            bool oldValue = plugin.GetParameter<ParameterEnum, bool>(parameter, ear);
            bool newValue = GUILayout.Toggle(oldValue, new GUIContent(p.label, p.description), GUILayout.ExpandWidth(false));
            value = Convert.ToSingle(newValue);
            if (newValue != oldValue)
            {
                bool setOK = plugin.SetFloatParameter(p.pluginName(ear), Convert.ToSingle(newValue));
                Debug.Assert(setOK);
                return true;
            }
            return false;
        }
        else if (p.type.IsEnum)
        {
            int oldValue = plugin.GetParameter<ParameterEnum, int>(parameter, ear);
            Debug.Assert(Enum.GetUnderlyingType(p.type) == typeof(int));
            int[] values = (int[])Enum.GetValues(p.type);
            if (!values.Contains(oldValue))
            {
                Debug.LogWarning($"Plugin returned invalid value for {p.label}: {oldValue}");
            }

            int defaultValue = (int)Enum.GetValues(p.type).GetValue(0);

            int newValue = (int)(object)EditorGUILayout.Popup(new GUIContent(p.label, p.description), values.Contains(oldValue) ? oldValue : defaultValue, Enum.GetNames(p.type));
            Debug.Assert(Enum.IsDefined(p.type, newValue));
            value = Convert.ToSingle(newValue);

            if (newValue != oldValue)
            {
                bool setOK = plugin.SetFloatParameter(p.pluginName(ear), newValue);
                Debug.Assert(setOK);
                return true;
            }
            return false;
        }
        else
        {
            throw new Exception($"Cannot create GUI control for Parameter of type {p.type}.");
        }
    }


   

    /// <summary>
    /// Auxiliary function for creating sliders for float variables with specific format
    /// </summary>
    /// <returns></returns>
    public static bool CreateCompactFloatSlider(ref float variable, string name, string decimalDigits, string units, string tooltip, float minValue, float maxValue)
    {
        string valueString;
        float previousVar = variable;

        GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(new GUIContent(name, tooltip));
                valueString = GUILayout.TextField(variable.ToString(decimalDigits, System.Globalization.CultureInfo.InvariantCulture), GUILayout.ExpandWidth(false));
                GUILayout.Label(units, GUILayout.ExpandWidth(false));

                float newValue;
                bool valid = float.TryParse(valueString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out newValue);
                if (valid)
                    variable = newValue;
            }
            GUILayout.EndHorizontal();
            
            variable = GUILayout.HorizontalSlider(variable, minValue, maxValue);
            GUILayout.Space(singleSpace * 3);
        }

        GUILayout.EndVertical();

        return (variable != previousVar);
    }

    /// <summary>
    ///  Auxiliary function for creating toggle input
    /// </summary>    
    public static void CreatePluginToggle(IAudioEffectPlugin plugin, ref bool boolvar, string toggleText, string switchParameter, string tooltip, bool forceChange)
    {        
        if (CreateToggle(ref boolvar, toggleText, tooltip, forceChange))        
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