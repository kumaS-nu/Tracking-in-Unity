using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HarmonyLib;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

[InitializeOnLoad]
public static class TogglePlaymodeExtention
{
    static TogglePlaymodeExtention()
    {
        var harmony = new Harmony("org.kumaS.patch");
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(EditorApplication), "TogglePlaying")]
public class EditorApplicationExtention {
    public static List<Func<UniTask>> finalizeActions = new List<Func<UniTask>>();

    static bool Prefix()
    {
        if (EditorApplication.isPlaying)
        {
            RunFinalizeActions();
        }
        else
        {
            EditorApplication.isPlaying = true;
        }
        return false;
    }

    static async void RunFinalizeActions()
    {
        var actions = finalizeActions.Select(action => action()).ToList();
        await UniTask.WhenAll(actions);
        await UniTask.SwitchToMainThread();
        EditorApplication.isPlaying = false;
    }
}

