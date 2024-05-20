using System.Globalization;
using BepInEx;
using Eremite;
using Eremite.Services;
using Eremite.View.HUD;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ATSUtils
{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        private Harmony harmony;
        
        private static float[] _overwriteTimeScales = new float[]
        {
            0,
            1f,
            1.5f,
            2f,
            3f,
            5f, // digit5
            10f // digit6
        };
        
        private static Vector3[] _timeScaleSlotPositions = new Vector3[]
        {
            new (-83.4292f, 77.0265f, 0f),
            new (-69.7224f, 43.2358f, 0f),
            new (-44.9018f, 19.485f, 0f),
            new (-12.1654f, 7.7813f, 0f),
            new (22.3388f, 10.1524f, 0f),
            new (53.1689f, 26.425f, 0f),
            new (73.1727f, 54.405f, 0f)
        };

        private void Awake()
        {
            Instance = this;
            harmony = Harmony.CreateAndPatchAll(typeof(Plugin));  
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        
        /// <summary>
        /// Adds the two new timescales as children and adjusts the position of all elements.
        /// To change the used timescales, simply adjust their values in the <see cref="_overwriteTimeScales"/>
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof (TimeScalePanel), nameof(TimeScalePanel.SetUp))]
        [HarmonyPostfix]
        private static void HookEveryGameStart(TimeScalePanel __instance)
        {
            var parent = __instance.transform;
            var lastSlot = parent.childCount - 2;
            var slotPrefab = parent.GetChild(parent.childCount-2);
            _ = Instantiate(slotPrefab.gameObject, parent);
            _ = Instantiate(slotPrefab.gameObject, parent);
            
            // PAUSE TEXT child
            parent.GetChild(lastSlot + 1).SetAsLastSibling();

            for (var i = 0; i < _timeScaleSlotPositions.Length; i++)
            {
                var slot = parent.GetChild(i);
                slot.localPosition = _timeScaleSlotPositions[i];

                var tmp = slot.GetComponentInChildren <TextMeshProUGUI>();

                if (tmp)
                {
                    tmp.text = $"x{_overwriteTimeScales[i].ToString(CultureInfo.InvariantCulture)}";
                }

                var slotMB = slot.GetComponent <TimeScaleSlot>();
                slotMB.timeScale = _overwriteTimeScales[i];
            }
        }

        /// <summary>
        /// Check if the shortcut keys were pressed for the newly added time scales.
        /// <see cref="TimeScaleService"/> SelfUpdate()
        /// </summary>
        private void Update()
        {
            if (Keyboard.current.digit5Key.wasPressedThisFrame)
            {
                GameMB.TimeScaleService.Change(_overwriteTimeScales[5], true);
            } 
            else if (Keyboard.current.digit6Key.wasPressedThisFrame)
            {
                GameMB.TimeScaleService.Change(_overwriteTimeScales[6], true);
            }
        }
    }
}
