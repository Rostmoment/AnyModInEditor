using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaldiLevelEditor;
using BaldiLevelEditor.Types;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PlusLevelFormat;
using PlusLevelLoader;
using UnityEngine;

namespace AnyModInEditor
{
    class EventTool : ModdedRotateAndPlacePrefab
    {
        public Sprite Sprite;
        public override Sprite editorSprite => Sprite;
        public EventTool(string obj) : base(obj)
        {
            string s = obj.Substring(9);
            if (s.StartsWith("ExtraModCustomEvent_")) s = s.Substring(20);
            if (s.Length > 9) s = s.Substring(0, 9);
            Sprite = AssetLoader.SpriteFromTexture2D(Letters.CreateTexture(s), 1);
        }
        public override void OnPlace(Direction dir)
        {
            if (!IsOutOfBounds(selectedPosition.Value.ToInt()) && HasArea(selectedPosition.Value.ToInt()))
            {
                EditorObjectType editorObjectType = BaldiLevelEditorPlugin.editorObjects.Find((EditorObjectType x) => x.name == _object);
                EditorPrefab prefab = Singleton<PlusLevelEditor>.Instance.AddPrefab(new PrefabLocation
                {
                    prefab = _object,
                    position = (Singleton<PlusLevelEditor>.Instance.IntVectorToWorld(selectedPosition.Value.ToInt()) + editorObjectType.offset).ToData(),
                    rotation = Direction.East.ToRotation().ToData()
                });
                Singleton<PlusLevelEditor>.Instance.audMan.PlaySingle(BaldiLevelEditorPlugin.Instance.assetMan.Get<SoundObject>("Slap"));
                Singleton<PlusLevelEditor>.Instance.SelectTool(null);
                prefab.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                prefab.gameObject.transform.localPosition += new Vector3(0, 5, 0);
                prefab.GetComponent<SpriteRenderer>().sprite = Sprite;
            }
        }
    }
    class ModdedRotateAndPlacePrefab : RotateAndPlacePrefab
    {
        public override Sprite editorSprite => BasePlugin.missingTexture;
        public ModdedRotateAndPlacePrefab(string obj) : base(obj)
        {
        }
    }
    class ModdedFloorTool : FloorTool
    {
        public Sprite Sprite;
        public RoomAsset room;
        public override Sprite editorSprite => Sprite;
        public ModdedFloorTool(string room, Texture2D spr, RoomAsset roomAsset) : base(room)
        {
            this.room = roomAsset;
            string s = room;
            if (s.Length > 9)  s = s.Substring(0, 9);

            Sprite = Letters.CreateSprite(ResizeSpriteTo32x32(AssetLoader.SpriteFromTexture2D(spr, 1)), s);
        }
        public override void OnDrop(IntVector2 vector)
        {
            PlusLevelEditor instance = Singleton<PlusLevelEditor>.Instance;
            if (!instance.level.defaultTextures.ContainsKey(roomType))
            {
                string name = EnumExtensions.GetExtendedName<RoomCategory>(int.Parse(room.category.ToString()));
                instance.level.defaultTextures.Add(roomType, new PlusLevelFormat.TextureContainer("Floor" + name, "Wall" + name, "Ceil" + name));
            }
            base.OnDrop(vector);
        }
        Sprite ResizeSpriteTo32x32(Sprite sprite)
        {
            try
            {
                Texture2D originalTexture = sprite.texture;
                Texture2D resizedTexture = new Texture2D(32, 32);

                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        float u = x / 32.0f;
                        float v = y / 32.0f;
                        Color color = originalTexture.GetPixelBilinear(u, v);
                        resizedTexture.SetPixel(x, y, color);
                    }
                }
                resizedTexture.Apply();
                return Sprite.Create(resizedTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            }
            catch { return BasePlugin.missingTexture; }
        }
    }
    class ModdedItem : ItemTool
    {
        public Sprite Sprite;
        public override Sprite editorSprite => ResizeSpriteTo32x32(Sprite);
        public ModdedItem(string obj, Sprite sprite) : base(obj)
        {
            Sprite = sprite;
        }
        Sprite ResizeSpriteTo32x32(Sprite sprite)
        { try
            {
            Texture2D originalTexture = sprite.texture;
            Texture2D resizedTexture = new Texture2D(32, 32);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float u = x / 32.0f;
                    float v = y / 32.0f;
                    Color color = originalTexture.GetPixelBilinear(u, v);
                    resizedTexture.SetPixel(x, y, color);
                }
            }
            resizedTexture.Apply();
            return Sprite.Create(resizedTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            }
            catch { return BasePlugin.missingTexture; }
        }
    }
    class ModdedCharacter : NpcTool
    {
        public Sprite Sprite;
        public override Sprite editorSprite => ResizeSpriteTo32x32(Sprite);
        public ModdedCharacter(string prefab, Sprite sprite) : base(prefab)
        {
            Sprite = sprite;
        }
        Sprite ResizeSpriteTo32x32(Sprite sprite)
        {
            try
            {
                Texture2D originalTexture = sprite.texture;
                Texture2D resizedTexture = new Texture2D(32, 32);

                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        float u = x / 32.0f;
                        float v = y / 32.0f;
                        Color color = originalTexture.GetPixelBilinear(u, v);
                        resizedTexture.SetPixel(x, y, color);
                    }
                }
                resizedTexture.Apply();
                return Sprite.Create(resizedTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            }
            catch 
            {
                if (sprite.texture.width == 32 && sprite.texture.height == 32) return sprite;
                return BasePlugin.missingTexture;
            }
        }
    }
    [HarmonyPatch]
    class Patch
    {
        [HarmonyPatch(typeof(EditorLevelManager), "Initialize")]
        [HarmonyPostfix]
        private static void AddEvents(EditorLevelManager __instance)
        {
            foreach (PrefabLocation prefab in Singleton<PlusLevelEditor>.Instance.level.prefabs.Where(x => x.prefab.StartsWith("RTMTEvent")))
            {
                RandomEvent randomEvent = GameObject.Instantiate(BasePlugin.events[prefab.prefab]);
                randomEvent.SetEventTime(new System.Random());
                randomEvent.Initialize(__instance.ec, new System.Random());
                __instance.ec.AddEvent(randomEvent, 10);
            }
            __instance.ec.RandomizeEvents(__instance.ec.EventsCount, 30f, 30f, 180f, new System.Random());
        }
        [HarmonyPatch(typeof(EditorLevel), "InitializeDefaultTextures")]
        [HarmonyPrefix]
        private static void AddTextures(EditorLevel __instance)
        {
            foreach (RoomAsset room in Resources.FindObjectsOfTypeAll<RoomAsset>().Where(x => !BasePlugin.originalRooms.Contains(x.category)))
            {
                try
                {
                    string name = EnumExtensions.GetExtendedName<RoomCategory>(int.Parse(room.category.ToString()));
                    PlusLevelLoaderPlugin.Instance.textureAliases.Add("Wall" + name, room.wallTex);
                    PlusLevelLoaderPlugin.Instance.textureAliases.Add("Floor" + name, room.florTex);
                    PlusLevelLoaderPlugin.Instance.textureAliases.Add("Ceil" + name, room.ceilTex);
                    __instance.defaultTextures.Add(name, new PlusLevelFormat.TextureContainer("Floor" + name, "Wall" + name, "Ceil" + name));
                }
                catch { }
            }
        }
        [HarmonyPatch(typeof(PlusLevelEditor), "Initialize")]
        [HarmonyPostfix]
        private static void AddTools(PlusLevelEditor __instance)
        {
            List<ModdedCharacter> ch = new List<ModdedCharacter>() { };
            foreach (NPCMetadata npc in NPCMetaStorage.Instance.FindAll(x => !BasePlugin.originalCharacters.Contains(x.value.Character)))
            {
                ch.Add(new ModdedCharacter(npc.value.name, npc.value.spriteRenderer[0].sprite));
            }
            List<ModdedItem> itm = new List<ModdedItem> { };
            foreach (ItemMetaData item in ItemMetaStorage.Instance.FindAll(x => !BasePlugin.originalItems.Contains(x.value.itemType)))
            {
                itm.Add(new ModdedItem(item.nameKey, item.value.itemSpriteSmall));
            }
            List<ModdedFloorTool> rm = new List<ModdedFloorTool> { };
            foreach (RoomAsset room in Resources.FindObjectsOfTypeAll<RoomAsset>().Where(x => !BasePlugin.originalRooms.Contains(x.category)))
            {
                string name = EnumExtensions.GetExtendedName<RoomCategory>(int.Parse(room.category.ToString()));
                rm.Add(new ModdedFloorTool(name, room.wallTex, room));
            }
            List<ModdedRotateAndPlacePrefab> obj = new List<ModdedRotateAndPlacePrefab> { };
            foreach (EnvironmentObject environmentObject in Resources.FindObjectsOfTypeAll<EnvironmentObject>())
            {
                obj.Add(new ModdedRotateAndPlacePrefab(environmentObject.name));
            }
            __instance.toolCats.Find(x => x.name == "characters").tools.AddRange(ch);
            __instance.toolCats.Find(x => x.name == "items").tools.AddRange(itm);
            __instance.toolCats.Find(x => x.name == "halls").tools.AddRange(rm);
            __instance.toolCats.Find(x => x.name == "objects").tools.AddRange(obj);
            List<EventTool> ev = new List<EventTool> { };
            foreach (RandomEventMetadata randomEvent in RandomEventMetaStorage.Instance.All())
            {
                string name = randomEvent.value.Type.ToString();
                try
                {
                    name = EnumExtensions.GetExtendedName<RandomEventType>(int.Parse(randomEvent.value.Type.ToString()));
                }
                catch { }
                ev.Add(new EventTool("RTMTEvent" + name));
            }
            __instance.toolCats.Find(x => x.name == "connectables").tools.AddRange(ev);
        }
    }
}
