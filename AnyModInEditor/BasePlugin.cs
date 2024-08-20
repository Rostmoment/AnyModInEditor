using BepInEx;
using HarmonyLib;
using System;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI;
using System.Collections;
using MTM101BaldAPI.OptionsAPI;
using BepInEx.Logging;
using UnityEngine;
using System.IO;
using PlusLevelFormat;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using BaldiLevelEditor;
using PlusLevelLoader;

namespace AnyModInEditor
{
    [BepInPlugin("rost.moment.baldiplus.editoranymod", "Any mod in editor", "1.0.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.baldiplus.levelloader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.baldiplus.leveleditor", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pixelguy.pixelmodding.baldiplus.bbextracontent", BepInDependency.DependencyFlags.SoftDependency)]
    public class BasePlugin : BaseUnityPlugin
    {
        public static Sprite missingTexture;
        public static Dictionary<string, RandomEvent> events = new Dictionary<string, RandomEvent>() { };
        public static Items[] originalItems = (Items[])Enum.GetValues(typeof(Items));
        public static Character[] originalCharacters = (Character[])Enum.GetValues(typeof(Character));
        public static RoomCategory[] originalRooms = (RoomCategory[])Enum.GetValues(typeof(RoomCategory));
        public static BasePlugin Instance = null;
        private void Awake()
        {
            Harmony harmony = new Harmony("rost.moment.baldiplus.extramod");
            harmony.PatchAllConditionals();
            if (Instance == null)
            {
                Instance = this;
            }
            LoadingEvents.RegisterOnAssetsLoaded(Info, PostLoad, true);
        }
        private void PostLoad()
        {
            Texture2D texture = new Texture2D(32, 32);

            Color magenta = new Color(1f, 0f, 1f);
            Color black = Color.black;

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    if ((x < 16 && y < 16) || (x >= 16 && y >= 16))
                    {
                        texture.SetPixel(x, y, magenta);
                    }
                    else
                    {
                        texture.SetPixel(x, y, black);
                    }
                }
            }

            texture.Apply();
            missingTexture = AssetLoader.SpriteFromTexture2D(texture, 1);
            foreach (ItemMetaData item in ItemMetaStorage.Instance.FindAll(x => !originalItems.Contains(x.value.itemType)))
            {
                try
                {
                    PlusLevelLoaderPlugin.Instance.itemObjects.Add(item.nameKey, item.value);
                    BaldiLevelEditorPlugin.itemObjects.Add(item.nameKey, item.value);
                }
                catch (ArgumentException){ }
            }
            foreach (NPCMetadata npc in NPCMetaStorage.Instance.FindAll(x => !originalCharacters.Contains(x.value.Character)))
            {
                try
                {
                    GameObject gameObject = BaldiLevelEditorPlugin.StripAllScripts(npc.value.gameObject, true);
                    try
                    {
                        gameObject.transform.Find("SpriteBase").GetComponentInChildren<SpriteRenderer>().sprite = npc.value.spriteRenderer[0].sprite;
                    }
                    catch (UnityException) { }
                    BaldiLevelEditorPlugin.characterObjects.Add(npc.value.name, gameObject);
                    PlusLevelLoaderPlugin.Instance.npcAliases.Add(npc.value.name, npc.value);
                }
                catch (ArgumentException) { }
            }
            foreach (EnvironmentObject environmentObject in Resources.FindObjectsOfTypeAll<EnvironmentObject>())
            {
                try
                {
                    BaldiLevelEditorPlugin.editorObjects.Add(EditorObjectType.CreateFromGameObject<EditorPrefab, PrefabLocation>(environmentObject.name, environmentObject.gameObject, Vector3.zero));
                    PlusLevelLoaderPlugin.Instance.prefabAliases.Add(environmentObject.name, BaldiLevelEditorPlugin.StripAllScripts(environmentObject.gameObject, true));
                }
                catch { }
            }
            foreach (RoomAsset room in Resources.FindObjectsOfTypeAll<RoomAsset>().Where(x => !BasePlugin.originalRooms.Contains(x.category)))
            {
                try
                {
                    string name = EnumExtensions.GetExtendedName<RoomCategory>(int.Parse(room.category.ToString()));
                    PlusLevelLoaderPlugin.Instance.textureAliases.Add("Wall" + room.name.ToString(), room.wallTex);
                    PlusLevelLoaderPlugin.Instance.textureAliases.Add("Floor" + room.name.ToString(), room.florTex);
                    PlusLevelLoaderPlugin.Instance.textureAliases.Add("Ceil" + room.name.ToString(), room.ceilTex);
                    PlusLevelLoaderPlugin.Instance.roomSettings.Add(name, new RoomSettings(room.category, room.type, room.color, room.doorMats, room.mapMaterial));
                    PlusLevelLoaderPlugin.Instance.roomSettings[name].container = room.roomFunctionContainer;
                }
                catch { }
            }
            foreach (RandomEventMetadata randomEvent in RandomEventMetaStorage.Instance.All())
            {
                try
                {
                    string name = randomEvent.value.eventType.ToString();
                    try
                    {
                        name = EnumExtensions.GetExtendedName<RandomEventType>(int.Parse(randomEvent.value.Type.ToString()));
                    }
                    catch { }
                    string s = name;
                    if (s.StartsWith("ExtraModCustomEvent_")) s = s.Substring(20);
                    if (s.Length > 9) s = s.Substring(0, 9);
                    GameObject gameObject = randomEvent.value.gameObject;
                    SpriteRenderer sprite = gameObject.AddComponent<SpriteRenderer>();
                    sprite.sprite = AssetLoader.SpriteFromTexture2D(Letters.CreateTexture(s), 1);
                    gameObject.transform.localScale = new Vector2(0.1f, 0.1f);
                    BaldiLevelEditorPlugin.editorObjects.Add(EditorObjectType.CreateFromGameObject<EditorPrefab, PrefabLocation>("RTMTEvent" + name, gameObject, Vector3.zero));
                    PlusLevelLoaderPlugin.Instance.prefabAliases.Add("RTMTEvent" + name, gameObject);
                    events.Add("RTMTEvent"+ name, randomEvent.value);
                }
                catch { }
            }
            Letters.Create();
        }
    }
}