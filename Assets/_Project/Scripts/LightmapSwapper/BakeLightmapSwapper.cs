﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThePatient
{
    public class BakeLightmapSwapper : SingletonBehaviour<BakeLightmapSwapper>, ISaveable
    {
        [SerializeField] Material lampSharedBaked;
        [SerializeField] SwitchObject[] switchObjects;

        [SerializeField] LightmapData[] brightLMData;
        [SerializeField] LightmapData[] darkLMData;

        [SerializeField] List<BakeLightmapSwapperSO> lightmapSO;
        [SerializeField] EEventData _lightOn , _lightOff;

        bool isDarkness;
        private void Start()
        {
            TriggerManager.Instance._deactivateSendSignal+= LightOn;


            List<LightmapData> brightLMlist = new List<LightmapData>();

            for (int i = 0; i < lightmapSO[0].lightMapDir.Length; i++)
            {
                LightmapData lmData = new LightmapData();

                lmData.lightmapDir = lightmapSO[0].lightMapDir[i];
                lmData.lightmapColor = lightmapSO[0].lightMapColor[i];
                lmData.shadowMask = lightmapSO[0].shadowMaskColor[i];
                brightLMlist.Add(lmData);
            }

            brightLMData = brightLMlist.ToArray();

            List<LightmapData> darkLMlist = new List<LightmapData>();

            for (int i = 0; i < lightmapSO[1].lightMapDir.Length; i++)
            {
                LightmapData lmData = new LightmapData();

                lmData.lightmapDir = lightmapSO[1].lightMapDir[i];
                lmData.lightmapColor = lightmapSO[1].lightMapColor[i];
                lmData.shadowMask = lightmapSO[0].shadowMaskColor[i];

                darkLMlist.Add(lmData);
            }

            darkLMData = darkLMlist.ToArray();

            switchObjects = FindObjectsOfType<SwitchObject>();
            SetBrightLM();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                isDarkness = SetBrightLM();
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                isDarkness = SetDarkLM();
            }
        }
         
        void LightOn(EEventData data)
        {
            if (_lightOn == data)
            isDarkness = SetBrightLM();

            if (_lightOff == data)
                isDarkness = SetDarkLM();
        }
    


        bool SetBrightLM()
        {
            LightmapSettings.lightmaps = brightLMData;
            LightmapSettings.lightProbes.bakedProbes = lightmapSO[0].lightProbesData;
            lampSharedBaked.EnableKeyword("_EMISSION");
            foreach(var swtichobject in switchObjects)
            {
                swtichobject.ToggleLight(false);
            }
            return false;
        }

        bool SetDarkLM()
        {
            LightmapSettings.lightmaps = darkLMData;
            LightmapSettings.lightProbes.bakedProbes = lightmapSO[1].lightProbesData;
            lampSharedBaked.DisableKeyword("_EMISSION");
            foreach (var swtichobject in switchObjects)
            {
                swtichobject.ToggleLight(true);
            }
            return true;
        }


        public object CaptureState()
        {
            return isDarkness;
        }

        public void RestoreState(object state)
        {
            bool isDark = (bool)state;
            if (isDark)
            {
                SetDarkLM();
            }
            else
            {
                SetBrightLM();
            }
        }
    }
}
