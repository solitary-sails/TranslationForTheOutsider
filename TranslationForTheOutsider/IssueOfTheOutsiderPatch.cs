﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TranslationForTheOutsider {
    [HarmonyPatch]
    public static class IssueOfTheOutsiderPatch {
        static bool _fixShipLogCardPosition = false;
        static int _fixShipLogCardPositionCount = 0;

        public static void Initialize() {
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) => {
                if (loadScene == OWScene.TitleScreen) {
                    _fixShipLogCardPosition = true; // It cannot be true when the title is loaded at first because Initialized() has not run yet, I think
                }
            };

            TranslationForTheOutsider.Instance.Log($"{nameof(IssueOfTheOutsiderPatch)} is initialized.");
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.UpdateTimeFreeze))]
        public static Exception NomaiTranslatorProp_UpdateTimeFreeze_Finalizer(Exception __exception) {
            // Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/7

            if(!TranslationForTheOutsider.Instance.IsFixIssuesOfTheOutsider) {
                return __exception;
            }
            return null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.Awake))]
        [HarmonyAfter(new string[] {"SBtT.TheOutsider"})]
        public static void ShipLogManager_Awake_Prefix(ShipLogManager __instance) {
            // Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/8

            if(!TranslationForTheOutsider.Instance.IsFixIssuesOfTheOutsider) {
                return;
            }

            if(!_fixShipLogCardPosition) {
                return;
            }
            _fixShipLogCardPosition = false; // Fixing is only done after reloading from the title screen.
            ++_fixShipLogCardPositionCount; // Shifts are accumulated, so it should be multiplied.

            var offset = new Vector2(-250f, 0);
            bool isInOutsiderShipLog = false;
            for(int i = 0; i < __instance._shipLogLibrary.entryData.Length; ++i) {
                if (__instance._shipLogLibrary.entryData[i].id == "DB_NORTHERN_OBSERVATORY") {
                    isInOutsiderShipLog = true;
                }
                if(!isInOutsiderShipLog) {
                    continue;
                }

                __instance._shipLogLibrary.entryData[i].cardPosition -= offset * _fixShipLogCardPositionCount;
                //TranslationForTheOutsider.Instance.Log($"{__instance._shipLogLibrary.entryData[i].id}'s cardPosition is fixed! ({__instance._shipLogLibrary.entryData[i].cardPosition})");
            }

            TranslationForTheOutsider.Instance.Log("ShipLog's card positions are fixed.");
        }
    }
}