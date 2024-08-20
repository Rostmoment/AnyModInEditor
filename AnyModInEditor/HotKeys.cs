using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BaldiLevelEditor;
using HarmonyLib;
using PlusLevelFormat;
using UnityEngine;

namespace AnyModInEditor
{
    [HarmonyPatch]
    class HotKeys
    {
        public static bool CheckForHotKey(KeyCode keyCode)
        {
            return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKeyDown(keyCode);
        }
        [HarmonyPatch(typeof(PlusLevelEditor), "Update")]
        [HarmonyPrefix]
        private static void HotKeyCheck(PlusLevelEditor __instance)
        {
            if (CheckForHotKey(KeyCode.L))
            {
                string path = "";
                try
                {
                    path = FileController.OpenFile();
                    if (!File.Exists(path)) __instance.audMan.PlaySingle(BaldiLevelEditorPlugin.Instance.assetMan.Get<SoundObject>("Elv_Buzz"));
                    else if (path.ToLower().EndsWith(".bld"))
                    {
                        __instance.LoadLevelFromFile(path);
                        if (__instance.tempLevel == null)
                        {
                            __instance.audMan.PlaySingle(BaldiLevelEditorPlugin.Instance.assetMan.Get<SoundObject>("Elv_Buzz"));
                            throw new Exception("Level Loading failed!");
                        }
                        __instance.LoadLevel(__instance.tempLevel);
                        __instance.RefreshLevel();
                        __instance.UpdateLines();
                        __instance.SelectTool(null);
                        __instance.tempLevel = null;
                    }
                    else if (path.ToLower().EndsWith(".cbld"))
                    {
                        using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) __instance.tempPlayLevel = reader.ReadLevel();
                        __instance.LoadTempPlay();
                    }
                    else if (path.ToLower().EndsWith(".rtmt"))
                    {
                        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                        {
                            for (int i = 0; i < 3; i++) reader.ReadInt32();
                            for (int i = 0; i < 5; i++) reader.ReadBoolean();
                            __instance.tempPlayLevel = reader.ReadLevel();
                            __instance.LoadTempPlay(); __instance.tempPlayLevel = reader.ReadLevel();
                        }
                    }
                    else __instance.audMan.PlaySingle(BaldiLevelEditorPlugin.Instance.assetMan.Get<SoundObject>("Elv_Buzz"));
                }
                catch { }
            }
        }
    }
}
