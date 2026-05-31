using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BrutalCompanyMinus;
using Dawn;
using HarmonyLib;
using UnityEngine;

namespace BCMERDice
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(DawnLib.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("SoftDiamond.BrutalCompanyMinusExtraReborn", BepInDependency.DependencyFlags.HardDependency)]
    //BepInDependency("Theronguard.EmergencyDice", BepInDependency.DependencyFlags.HardDependency)]


    public class BCMERDiceBase : BaseUnityPlugin
    {

        internal static BCMERDiceBase Instance { get; private set; }

        public ManualLogSource mls;

        private const string GUID = "SoftDiamond.BCMERDice";
        private const string NAME = "BCMERDice";
        private const string VERSION = "0.0.1";

        private readonly Harmony harmony = new Harmony(GUID);

        private void Awake()
        {
            if (Instance == null) Instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(GUID);

            //Asset.Load();

            ConfigSetup();

            harmony.PatchAll();

            mls.LogInfo($"BCMERDice has initialized!");
        }

        private void ConfigSetup()
        {
        }
    }
}