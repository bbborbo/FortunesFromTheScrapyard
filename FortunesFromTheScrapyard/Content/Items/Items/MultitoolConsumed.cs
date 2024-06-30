using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Items
{
    class MultitoolConsumed : ItemBase<MultitoolConsumed>
    {
        #region config
        public override string ConfigName => "Broken Multitool";
        #endregion
        public override string ItemName => "Broken Multitool";

        public override string ItemLangTokenName => "BROKENMULTITOOL";

        public override string ItemPickupDesc => "It is no longer useful.";

        public override string ItemFullDescription => "It is no longer useful.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.NoTier;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.OnStageBeginEffect };

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
            On.RoR2.CharacterMaster.OnServerStageBegin += TryRegenerateMultitool;
        }

        private void TryRegenerateMultitool(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, RoR2.CharacterMaster self, RoR2.Stage stage)
        {
            orig(self, stage);
            if (NetworkServer.active)
            {
                int count = GetCount(self);
                if (count > 0)
                {
                    TransformMultitools(count, self);
                }
            }
        }
        private void TransformMultitools(int count, CharacterMaster master)
        {
            Inventory inv = master.inventory;
            inv.RemoveItem(instance.ItemsDef, count);
            inv.GiveItem(Multitool.instance.ItemsDef, count);

            CharacterMasterNotificationQueue.SendTransformNotification(
                master, instance.ItemsDef.itemIndex,
                Multitool.instance.ItemsDef.itemIndex,
                CharacterMasterNotificationQueue.TransformationType.RegeneratingScrapRegen);
        }

        public override void Init(ConfigFile config)
        {
            base.Init();
        }
    }
}
