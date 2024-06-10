using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FortunesFromTheScrapyard.Modules
{
    public static class Hooks
    {
        public static void Init()
        {
            HitHooks.Init();
        }
    }
    public static class HitHooks
    {
        public static void Init()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        public delegate void HitHookEventHandler(CharacterBody attackerBody, DamageInfo damageInfo, GameObject victim);
        public static event HitHookEventHandler GetHitBehavior;
        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker && damageInfo.procCoefficient > 0f)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody != null)
                {
                    CharacterMaster attackerMaster = attackerBody.master;
                    if (attackerMaster != null)
                    {
                        GetHitBehavior?.Invoke(attackerBody, damageInfo, victim);
                    }
                }
            }
            orig(self, damageInfo, victim);
        }
    }
}
