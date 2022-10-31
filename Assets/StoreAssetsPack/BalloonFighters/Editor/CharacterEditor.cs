using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Character))]
public class CharacterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        Character character = serializedObject.targetObject as Character;
        if (GUILayout.Button(new GUIContent("SettingBalloon", "Balloon Model Auto Setting"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2.5f)))
        {
            if (character != null)
                character.SettingBalloon();
        }
    }
}