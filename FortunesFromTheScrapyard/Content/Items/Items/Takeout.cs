using BepInEx.Configuration;
using FortunesFromTheScrapyard.Modules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static FortunesFromTheScrapyard.Modules.Language.Styling;
using static R2API.RecalculateStatsAPI;

namespace FortunesFromTheScrapyard.Items
{
    class Takeout : ItemBase<Takeout>
    {
        [AutoConfig("Base Damage Bonus", 0.3f)]
        public static float damageBase = 0.3f;
        [AutoConfig("Stacking Damage Bonus", 0.2f)]
        public static float damageStack = 0.2f;

        [AutoConfig("Base Movement Speed Bonus", 0.3f)]
        public static float mspdBase = 0.3f;
        [AutoConfig("Stacking Movement Speed Bonus", 0.2f)]
        public static float mspdStack = 0.2f;

        [AutoConfig("Base Regen Bonus", 2.5f)]
        public static float regenBase = 2.5f;
        [AutoConfig("Stacking Regen Bonus", 1.5f)]
        public static float regenStack = 1.5f;

        public static BuffDef takeoutDamageBuff;
        public static BuffDef takeoutMspdBuff;
        public static BuffDef takeoutRegenBuff;

        public override string ConfigName => "Item : " + ItemName;

        #region abstract
        public override string ItemName => "Left-over Takeout";

        public override string ItemLangTokenName => "TAKEOUT";

        public override string ItemPickupDesc => "Gain a random stat buff for the rest of the stage.";

        public override string ItemFullDescription => $"On the {UtilityColor("beginning of each stage")}, " +
            $"gain a random {DamageColor("damage")}, {UtilityColor("movement speed")}, or {HealingColor("base health regeneration")} buff " +
            $"until the stage ends. " +
            $"Can increase base damage by {DamageColor(ConvertDecimal(damageBase))} {StackText("+" + ConvertDecimal(damageStack))}, " +
            $"movement speed by {UtilityColor(ConvertDecimal(mspdBase))} {StackText("+" + ConvertDecimal(mspdStack))}, " +
            $"or base health regeneration by {HealingColor(regenBase.ToString() + " per second")} {StackText("+" + regenStack.ToString())}";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.Healing, ItemTag.Damage, ItemTag.OnStageBeginEffect };

        public override GameObject ItemModel => LoadDropPrefab();

        public override Sprite ItemIcon => LoadItemIcon();

        public override AssetBundle assetBundle => FortunesPlugin.mainAssetBundle;
        #endregion

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            CreateItem();
            takeoutDamageBuff = Content.CreateAndAddBuff("TakeoutDmg", null, Color.red, false, false);
            takeoutMspdBuff = Content.CreateAndAddBuff("TakeoutMspd", null, Color.cyan, false, false);
            takeoutRegenBuff = Content.CreateAndAddBuff("TakeoutRegen", null, Color.green, false, false);
            Log.Warning("Takeout Initialized");

            On.RoR2.CharacterBody.OnInventoryChanged += AddItemBehavior;
            GetStatCoefficients += TakeoutStats;
        }

        private void AddItemBehavior(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                if (self.master)
                {
                    int itemCount = GetCount(self);

                    self.AddItemBehavior<TakeoutBehavior>(itemCount);
                }
            }
        }

        private void TakeoutStats(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = GetCount(sender);
            if(itemCount > 0)
            {
                if (sender.HasBuff(takeoutDamageBuff))
                {
                    args.damageMultAdd += GetStackValue(damageBase, damageStack, itemCount);
                }
                else if (sender.HasBuff(takeoutMspdBuff))
                {
                    args.moveSpeedMultAdd += GetStackValue(mspdBase, mspdStack, itemCount);
                }
                else if (sender.HasBuff(takeoutRegenBuff))
                {
                    args.baseRegenAdd += GetStackValue(regenBase, regenStack, itemCount) * (1 + 0.2f * sender.level);
                }
            }
        }

        public override void Init(ConfigFile config)
        {
            base.Init();
        }
    }

    public class TakeoutBehavior : CharacterBody.ItemBehavior
    {
        private void Start()
        {
            if (body)
            {
                if (Util.CheckRoll(100 / 3))
                {
                    body.AddBuff(Takeout.takeoutDamageBuff);
                    return;
                }

                if (Util.CheckRoll(100 / 2))
                {
                    body.AddBuff(Takeout.takeoutMspdBuff);
                    return;
                }

                body.AddBuff(Takeout.takeoutRegenBuff);
            }
        }
    }
}
