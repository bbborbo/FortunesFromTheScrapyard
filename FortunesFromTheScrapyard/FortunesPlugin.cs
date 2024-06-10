using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using FortunesFromTheScrapyard.Items;
using FortunesFromTheScrapyard.Modules;
using FortunesFromTheScrapyard.Skills;
using FortunesFromTheScrapyard.Survivors;
using R2API;
using R2API.Utils;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]
#pragma warning disable 
namespace FortunesFromTheScrapyard
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(LanguageAPI),nameof(PrefabAPI), nameof(RecalculateStatsAPI), nameof(DotAPI))]
    [BepInPlugin(guid, modName, version)]
    public class FortunesPlugin : BaseUnityPlugin
    {
        public const string guid = "com." + teamName + "." + modName;
        public const string teamName = "GodRayProd";
        public const string modName = "FortunesFromTheScrapyard";
        public const string version = "0.4.0";

        public const string DEVELOPER_PREFIX = "GRP";

        public static FortunesPlugin instance;
        public static AssetBundle mainAssetBundle;

        #region asset paths
        public const string iconsPath = "";
        #endregion

        #region mods loaded
        public static bool ModLoaded(string modGuid) { return modGuid != "" && BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(modGuid); }
        public static bool autoSprintLoaded => ModLoaded("com.johnedwa.RTAutoSprintEx");
        public static bool iabMissilesLoaded => ModLoaded("com.HouseOfFruits.IAmBecomeMissiles");
        #endregion

        [AutoConfig("Enable Debugging", "Enable debug outputs to the log for troubleshooting purposes. Enabling this will slow down the game.", false)]
        public static bool enableDebugging;

        void Awake()
        {
            instance = this;

            Log.Init(Logger);

            //mainAssetBundle = Modules.Assets.LoadAssetBundle("ralseibundle");

            Modules.Config.Init();
            Modules.Language.Init();
            Modules.Hooks.Init();

            ConfigManager.HandleConfigAttributes(GetType(), "Fortunes", Modules.Config.MyConfig);

            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

            //new RalseiSurvivor().Init();
            /*BeginInitializing<SurvivorBase>(allTypes);
            Modules.Language.TryPrintOutput("FortunesSurvivors.txt");

            BeginInitializing<SkillBase>(allTypes);
            Modules.Language.TryPrintOutput("FortunesSkills.txt");*/

            BeginInitializing<ItemBase>(allTypes);
            Modules.Language.TryPrintOutput("FortunesItems.txt");

            //RalseiSurvivor.instance.InitializeCharacterMaster();

            // this has to be last
            new Modules.ContentPacks().Initialize();

            ////refer to guide on how to build and distribute your mod with the proper folders
        }
        private void BeginInitializing<T>(Type[] allTypes) where T : SharedBase
        {
            Type baseType = typeof(T);
            //base types must be a base and not abstract
            if (!baseType.IsAbstract)
            {
                Log.Error(Log.Combine() + "Incorrect BaseType: " + baseType.Name);
                return;
            }

            Log.Debug(Log.Combine(baseType.Name) + "Initializing");

            IEnumerable<Type> objTypesOfBaseType = allTypes.Where(type => !type.IsAbstract && type.IsSubclassOf(baseType));

            foreach (var objType in objTypesOfBaseType)
            {
                string s = Log.Combine(baseType.Name, objType.Name);
                Log.Debug(s);
                T obj = (T)System.Activator.CreateInstance(objType);
                if (ValidateBaseType(obj as SharedBase))
                {
                    Log.Debug(s + "Validated");
                    InitializeBaseType(obj as SharedBase);
                    Log.Debug(s + "Initialized");
                }
            }
        }

        bool ValidateBaseType(SharedBase obj)
        {
            return obj.isEnabled;
            /*TypeInfo typeInfo = obj.GetType().GetTypeInfo();
            PropertyInfo isEnabled = typeof(baseType).GetProperties().Where(x => x.Name == nameof(SharedBase.isEnabled)).First();
            if (isEnabled != null && isEnabled.PropertyType == typeof(bool))
            {
                return (bool)isEnabled.GetValue(obj);
            }

            return false;*/
        }
        void InitializeBaseType(SharedBase obj)
        {
            obj.Init();
            /*MethodInfo method = typeof(baseType).GetMethods().Where(x => x.Name == nameof(SharedBase.Init)).First();
            method.Invoke(obj, new object[] { });*/
        }
    }
}
