using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FortunesFromTheScrapyard.Modules
{
    public abstract class SharedBase
    {
        public abstract string ConfigName { get; }
        public virtual bool isEnabled { get; } = true;
        public static ManualLogSource Logger => Log._logSource;
        public abstract AssetBundle assetBundle { get; }

        public abstract void Hooks();
        public abstract void Lang();

        public virtual void Init()
        {
            ConfigManager.HandleConfigAttributes(GetType(), ConfigName, Config.MyConfig);
            Hooks();
            Lang();
        }

        public static float GetHyperbolic(float firstStack, float cap, float chance) // Util.ConvertAmplificationPercentageIntoReductionPercentage but Better :zanysoup:
        {
            if (firstStack >= cap) return cap * (chance / firstStack); // should not happen, but failsafe
            float count = chance / firstStack;
            float coeff = 100 * firstStack / (cap - firstStack); // should be good
            return cap * (1 - (100 / ((count * coeff) + 100)));
        }
    }
}
