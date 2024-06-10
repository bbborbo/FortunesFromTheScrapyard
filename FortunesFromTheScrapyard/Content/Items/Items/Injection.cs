using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static FortunesFromTheScrapyard.Modules.Language.Styling;
using static FortunesFromTheScrapyard.Modules.HitHooks;
using FortunesFromTheScrapyard.Modules;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Items
{
    class Injection : ItemBase<Injection>
    {
        #region config
        [AutoConfig("Toxin Duration Base", 10)]
        public static int toxinDurationBase = 10;
        [AutoConfig("Toxin Duration Stack", 5)]
        public static int toxinDurationStack = 5;
        [AutoConfig("Toxin Interval", 0.5f)]
        public static float toxinInterval = 0.5f;
        public override string ConfigName => "Item : " + ItemName;
        #endregion

        public override string ItemName => "Lethal Injection";

        public override string ItemLangTokenName => "INJECTION";

        public override string ItemPickupDesc => "High damage hits also inflict a deadly toxin.";

        public override string ItemFullDescription => $"Hits that deal {DamageColor("more than 400% damage")} also inflict a toxin " +
            $"that {HealthColor("permanently reduces maximum health")} by 1% every {toxinInterval} seconds. " +
            $"Toxin lasts for {UtilityColor((toxinDurationBase * toxinInterval).ToString())} seconds " +
            $"{StackText("+" + (toxinDurationStack * toxinInterval).ToString())}.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public override GameObject ItemModel => LoadDropPrefab();

        public override Sprite ItemIcon => LoadItemIcon();


        public override AssetBundle assetBundle => FortunesPlugin.mainAssetBundle;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            CreateItem();

            GetHitBehavior += InjectionOnHit;
        }

        private void InjectionOnHit(CharacterBody attackerBody, DamageInfo damageInfo, GameObject victim)
        {
            if(damageInfo.damage / attackerBody.damage >= 4)
            {
                int injectionCount = GetCount(attackerBody);
                if (injectionCount > 0 && NetworkServer.active)
                {
                    int injectionStacks = (int)(GetStackValue(toxinDurationBase, toxinDurationStack, injectionCount) * damageInfo.procCoefficient);
                    InjectionBehavior injection = victim.GetComponent<InjectionBehavior>();
                    if (injection == null)
                        injection = victim.AddComponent<InjectionBehavior>();
                    injection.AddStacks(injectionStacks);
                }
            }
        }

        public override void Init(ConfigFile config)
        {
            base.Init();
        }
    }

    public class InjectionBehavior : MonoBehaviour
    {
        CharacterBody hostBody;
        float interval => Injection.toxinInterval;
        float stopwatch = 0;
        int stacksRemaining = 0;
        public void Start()
        {
            hostBody = GetComponent<CharacterBody>();
        }
        public void AddStacks(int stacksToAdd)
        {
            stacksRemaining += stacksToAdd;
            stopwatch = 0;
        }
        public void FixedUpdate()
        {
            if(hostBody != null && NetworkServer.active)
            {
                if (stacksRemaining >= 0)
                {
                    stopwatch += Time.fixedDeltaTime;
                    while (stopwatch > interval)
                    {
                        //create effect
                        hostBody.AddBuff(RoR2Content.Buffs.PermanentCurse);
                        stacksRemaining--;
                        stopwatch -= interval;
                    }
                }
                else
                {
                    stopwatch = 0;
                }
            }
        }
    }
}
