using BepInEx.Configuration;
using FortunesFromTheScrapyard.Modules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static FortunesFromTheScrapyard.Modules.Language.Styling;

namespace FortunesFromTheScrapyard.Items
{
    class CounterfeitCurrency : ItemBase<CounterfeitCurrency>
    {
        #region config
        [AutoConfig("Free Money Base", 40)]
        public static int freeMoneyBase = 40;
        [AutoConfig("Free Money Stack", 40)]
        public static int freeMoneyStack = 40;
        [AutoConfig("Income Penalty Base", 0.2f)]
        public static float incomePenaltyBase = 0.2f;
        [AutoConfig("Income Penalty Stack", 0.2f)]
        public static float incomePenaltyStack = 0.2f;

        public override string ConfigName => "Item : " + ItemName;
        #endregion
        public override string ItemName => "Counterfeit Currency";

        public override string ItemLangTokenName => "LUNARMONEY";

        public override string ItemPickupDesc => "Begin each stage with extra gold... " + HealthColor("BUT gain less gold.");

        public override string ItemFullDescription => $"Begin each stage with {DamageColor("$" + freeMoneyBase.ToString() + " gold")} " +
            $"{StackText("+$" + freeMoneyStack.ToString())}. {UtilityColor("Scales over time.")} " +
            $"{HealthColor("ALL other sources of income")} are reduced by {HealthColor("-" + ConvertDecimal(incomePenaltyBase))} " +
            $"{StackText("-" + ConvertDecimal(incomePenaltyStack))}.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Lunar;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

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
            On.RoR2.CharacterMaster.GiveMoney += CounterfeitPenalty;
            Inventory.onServerItemGiven += CounterfeitPickupReward;
            On.RoR2.CharacterMaster.OnBodyStart += CounterfeitStartReward;
        }

        private void CounterfeitStartReward(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            int itemCount = GetCount(body);
            if(itemCount > 0)
            {
                float freeMoney = GetStackValue(freeMoneyBase, freeMoneyStack, itemCount);
                float freeMoneyCompensated = freeMoney / CalculateIncomeModifier(itemCount);
                self.GiveMoney((uint)freeMoneyCompensated);
            }
            orig(self, body);
        }

        private void CounterfeitPickupReward(Inventory inv, ItemIndex itemIndex, int count)
        {
            if(itemIndex == ItemsDef.itemIndex)
            {
                CharacterMaster master = inv.gameObject.GetComponent<CharacterMaster>();
                if (master)
                {
                    float freeMoney = GetStackValue(freeMoneyBase, freeMoneyStack, count);
                    float freeMoneyCompensated = freeMoney / CalculateIncomeModifier(GetCount(master) + count);
                    master.GiveMoney((uint)freeMoneyCompensated);
                }
            }
        }

        private void CounterfeitPenalty(On.RoR2.CharacterMaster.orig_GiveMoney orig, RoR2.CharacterMaster self, uint amount)
        {
            int itemCount = GetCount(self);
            amount = (uint)Mathf.Min(amount * CalculateIncomeModifier(itemCount), amount);

            orig(self, amount);
        }

        public override void Init(ConfigFile config)
        {
            base.Init();
        }

        public static float CalculateIncomeModifier(int itemCount)
        {
            float incomeModifier = 1;
            if(itemCount > 0)
            {
                incomeModifier *= (1 - incomePenaltyBase);
                incomeModifier *= Mathf.Pow(1 - incomePenaltyStack, itemCount - 1);
            }
            return incomeModifier;
        }
    }
}
