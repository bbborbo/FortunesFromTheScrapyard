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
using RoR2.CharacterAI;

namespace FortunesFromTheScrapyard.Items
{
    class Headphones : ItemBase<Headphones>
    {
        #region config
        public override string ConfigName => "Headphones";

        [AutoConfig("Proc Chance Base", 17f)]
        public static float chanceBase = 17f;
        [AutoConfig("Proc Chance Stack", 10f)]
        public static float chanceStack = 10f;
        [AutoConfig("Disorient Duration", 5f)]
        public static float disorientDuration = 5f;
        [AutoConfig("Disorient Damage Increase", 0.2f)]
        public static float disorientDamage = 0.2f;
        #endregion

        public static BuffDef DisorientDebuff;

        public override string ItemName => "Broken Headphones";

        public override string ItemLangTokenName => "HEADPHONES";

        public override string ItemPickupDesc => "Chance to disorient enemies.";

        public override string ItemFullDescription => $"{UtilityColor(chanceBase.ToString() + "%")} {StackText(chanceBase.ToString() + "%")} chance on hit to " +
            $"{UtilityColor("disorient")} enemies for {UtilityColor(disorientDuration.ToString() + " seconds")}, " +
            $"scrambling their aim and {DamageColor("increasing damage taken")} by {DamageColor(ConvertDecimal(disorientDamage))}.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };

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
            DisorientDebuff = Content.CreateAndAddBuff("FortunesDisorient", null, Color.gray, false, true);

            GetHitBehavior += HeadphoneOnHit;
            On.EntityStates.AI.BaseAIState.AimAt += DisorientAimAt;
            On.EntityStates.AI.BaseAIState.AimInDirection += DisorientAimDirection;
            On.RoR2.HealthComponent.TakeDamage += DisorientDamage;
        }

        private void DisorientDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if(self.body && self.body.HasBuff(DisorientDebuff))
            {
                damageInfo.damage *= 1 + disorientDamage;
            }
            orig(self, damageInfo);
        }

        private void DisorientAimDirection(On.EntityStates.AI.BaseAIState.orig_AimInDirection orig, EntityStates.AI.BaseAIState self, ref BaseAI.BodyInputs dest, Vector3 aimDirection)
        {
            if (self.body && self.body.HasBuff(DisorientDebuff))
            {
                orig(self, ref dest, UnityEngine.Random.onUnitSphere);
                dest.desiredAimDirection = UnityEngine.Random.onUnitSphere;
            }
            else orig(self, ref dest, aimDirection);
        }

        private void DisorientAimAt(On.EntityStates.AI.BaseAIState.orig_AimAt orig, EntityStates.AI.BaseAIState self, ref BaseAI.BodyInputs dest, BaseAI.Target aimTarget)
        {
            if (self.body && self.body.HasBuff(DisorientDebuff))
            {
                orig(self, ref dest, aimTarget);
                dest.desiredAimDirection = UnityEngine.Random.onUnitSphere;
            }
            else orig(self, ref dest, aimTarget);
        }

        private void HeadphoneOnHit(CharacterBody attackerBody, DamageInfo damageInfo, CharacterBody victimBody)
        {
            int headphoneCount = GetCount(attackerBody);
            if (headphoneCount > 0 && !victimBody.HasBuff(DisorientDebuff))
            {
                float procChance = GetStackValue(chanceBase, chanceStack, headphoneCount) * damageInfo.procCoefficient;
                float adjustedProcChance = Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance);
                if (Util.CheckRoll(adjustedProcChance, attackerBody.master))
                {
                    victimBody.AddTimedBuff(DisorientDebuff, disorientDuration);
                }
            }
        }

        public override void Init(ConfigFile config)
        {
            base.Init();
        }
    }
}
