using BepInEx.Configuration;
using FortunesFromTheScrapyard.Modules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static R2API.RecalculateStatsAPI;
using static FortunesFromTheScrapyard.Modules.Language.Styling;

namespace FortunesFromTheScrapyard.Equipment
{
    class EnergyBar : EquipmentBase<EnergyBar>
    {
        public static BuffDef speedBuff;
        #region config
        [AutoConfig("Cooldown", 12f)]
        public static float cooldown = 12f;
        [AutoConfig("Speed Bonus", 0.3f)]
        public static float speedBonus = 0.3f;
        [AutoConfig("Speed Bonus Duration", 5f)]
        public static float speedBonusDuration = 5f;
        public override string ConfigName => "Equipment : " + EquipmentName;
        #endregion
        public override string EquipmentName => "Energy Bar";

        public override string EquipmentLangTokenName => "ENERGYBAR";

        public override string EquipmentPickupDesc => "Reset all other cooldowns, then gain a brief speed boost.";

        public override string EquipmentFullDescription => $"Reset all {UtilityColor("ability cooldowns")}, then " +
            $"increase {UtilityColor("movement speed")} by {UtilityColor(ConvertDecimal(speedBonus))} " +
            $"for {UtilityColor(speedBonusDuration.ToString())} seconds.";

        public override string EquipmentLore => "";

        public override GameObject EquipmentModel => LoadDropPrefab();

        public override Sprite EquipmentIcon => LoadItemIcon();

        public override AssetBundle assetBundle => FortunesPlugin.mainAssetBundle;

        public override float Cooldown => cooldown;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            CreateEquipment();
            speedBuff = Content.CreateAndAddBuff("EnergyBarSpeed", null, Color.yellow, true, false);

            GetStatCoefficients += EnergyBarSpeedBuff;
        }

        private void EnergyBarSpeedBuff(CharacterBody sender, StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(speedBuff);
            args.moveSpeedMultAdd += speedBonus * buffCount;
        }

        public override void Init(ConfigFile config)
        {
            base.Init();
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;
            SkillLocator skill = body.skillLocator;
            if(skill != null)
            {
                skill.ApplyAmmoPack();
                body.AddTimedBuffAuthority(speedBuff.buffIndex, speedBonusDuration);
                return true;
            }
            return false;
        }
    }
}
