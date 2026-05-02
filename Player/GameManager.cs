using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static string info;

    public static void UpdateInfo(string _info)
    {
        info = _info;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(200f, 200f, 200f, 400f));
        GUILayout.BeginVertical();

        GUILayout.Label(info);

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}