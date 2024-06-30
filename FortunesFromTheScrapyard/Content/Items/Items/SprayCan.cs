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
using RoR2.Projectile;

namespace FortunesFromTheScrapyard.Items
{
    class SprayCan : ItemBase<SprayCan>
    {
        #region config
        [AutoConfig("Poison Proc Chance Base", 11)]
        public static int baseChance = 11;
        [AutoConfig("Poison Proc Chance Stacking", 7)]
        public static int stackChance = 7;
        [AutoConfig("Poison Damage Coefficient Total", 2)]
        public static float poisonTotalDamage = 2;
        [AutoConfig("Poison Damage Coefficient Tick", 0.1f)]
        public static float poisonTickDamage = 0.1f;
        [AutoConfig("Poison Tick Rate", 0.1f)]
        public static float poisonTickRate = 0.1f;
        public override string ConfigName => "Item : " + ItemName;
        #endregion

        public static BuffDef poisonBuff;
        public static DotController.DotDef poisonDotDef;
        public static DotController.DotIndex poisonDotIndex;

        public override string ItemName => "Shattered Spray Can";

        public override string ItemLangTokenName => "SPRAYCAN";

        public override string ItemPickupDesc => "Chance to inflict poison on hit. Prioritized when used with 3D Printers.";

        public override string ItemFullDescription => $"{DamageColor(baseChance.ToString() + "%")} {StackText("+" + stackChance.ToString() + "%")} " +
            $"chance to {DamageColor("poison")} an enemy for {DamageColor(ConvertDecimal(poisonTotalDamage))} TOTAL damage. Prioritized when used with 3D Printers.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Scrap, ItemTag.Damage, ItemTag.AIBlacklist };

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

            GetHitBehavior += SprayCanHit;

            poisonBuff = Content.CreateAndAddBuff("SprayPoison", null, Color.green, true, true);
            poisonDotDef = new DotController.DotDef
            {
                associatedBuff = poisonBuff,
                damageCoefficient = poisonTickDamage,
                damageColorIndex = DamageColorIndex.Poison,
                interval = poisonTickRate
            };
            poisonDotIndex = DotAPI.RegisterDotDef(poisonDotDef, (self, dotStack) =>
            {

            });
        }

        private void SprayCanHit(CharacterBody attackerBody, DamageInfo damageInfo, GameObject victim)
        {
            int sprayCount = GetCount(attackerBody);
            if(sprayCount > 0)
            {
                float chance = GetStackValue(baseChance, stackChance, sprayCount);
                float finalChance = Util.ConvertAmplificationPercentageIntoReductionPercentage(chance);
                if(Util.CheckRoll(finalChance, attackerBody.master))
                {
                    uint? maxStacksFromAttacker = null;
                    if ((damageInfo != null) ? damageInfo.inflictor : null)
                    {
                        ProjectileDamage component = damageInfo.inflictor.GetComponent<ProjectileDamage>();
                        if (component && component.useDotMaxStacksFromAttacker)
                        {
                            maxStacksFromAttacker = new uint?(component.dotMaxStacksFromAttacker);
                        }
                    }

                    InflictDotInfo inflictDotInfo = new InflictDotInfo
                    {
                        attackerObject = damageInfo.attacker,
                        victimObject = victim,
                        totalDamage = new float?(damageInfo.damage * poisonTotalDamage),
                        damageMultiplier = 1f,
                        dotIndex = poisonDotIndex,
                        maxStacksFromAttacker = maxStacksFromAttacker
                    };
                    DotController.InflictDot(ref inflictDotInfo);
                }
            }
        }

        public override void Init(ConfigFile config)
        {
            base.Init();
        }
    }
}
