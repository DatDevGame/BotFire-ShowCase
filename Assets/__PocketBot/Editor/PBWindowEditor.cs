using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

public class PBWindowEditor : GameWindowEditor
{
    [MenuItem("PocketBots/OpenWindowEditor")]
    private static void OpenWindowEditor()
    {
        var window = GetWindow<PBWindowEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1000, 700);
    }
}