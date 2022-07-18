using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using Dungeonator;

namespace BleakMod
{
    class TargetingSpecs : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Targeting Specs";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/targeting_specs";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<TargetingSpecs>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Locked on and loaded";
            string longDesc = "While active, grants the user a plethora of attack stat boosts such as damage, fire rate, and most significantly, homing.\n\n" +
                "A marvel of combat engineering, this visor locks on agressively to enemies near and far and gives the user a surge of power in the process.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 400);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.S;
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            if (this.m_isCurrentlyActive)
            {
                HomingModifier homingModifier = proj.gameObject.GetComponent<HomingModifier>();
                if (homingModifier == null)
                {
                    homingModifier = proj.gameObject.AddComponent<HomingModifier>();
                    homingModifier.HomingRadius = 100f;
                    homingModifier.AngularVelocity = 1000f;
                }
            }
        }
        protected override void DoEffect(PlayerController user)
        {
            AkSoundEngine.PostEvent("Play_OBJ_power_up_01", base.gameObject);
            this.StartEffect(user);
            base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.EndEffect)));
        }
        private void StartEffect(PlayerController user)
        {
            this.damageMod = this.AddPassiveStatModifier(PlayerStats.StatType.Damage, 0.3f, StatModifier.ModifyMethod.ADDITIVE);
            this.rateOfFireMod = this.AddPassiveStatModifier(PlayerStats.StatType.RateOfFire, 2f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            this.reloadSpeedMod = this.AddPassiveStatModifier(PlayerStats.StatType.ReloadSpeed, -0.7f, StatModifier.ModifyMethod.ADDITIVE);
            this.piercingMod = this.AddPassiveStatModifier(PlayerStats.StatType.AdditionalShotPiercing, 1, StatModifier.ModifyMethod.ADDITIVE);
            this.shotSpeedMod = this.AddPassiveStatModifier(PlayerStats.StatType.ProjectileSpeed, 0.5f, StatModifier.ModifyMethod.ADDITIVE);
            user.stats.RecalculateStats(user, true, true);
        }
        private void EndEffect(PlayerController user)
        {
            bool flag = this.damageMod == null;
            if (!flag)
            {
                this.RemovePassiveStatModifier(this.damageMod);
                this.RemovePassiveStatModifier(this.rateOfFireMod);
                this.RemovePassiveStatModifier(this.reloadSpeedMod);
                this.RemovePassiveStatModifier(this.piercingMod);
                this.RemovePassiveStatModifier(this.shotSpeedMod);
                user.stats.RecalculateStats(user, true, true);
            }
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
            user.PostProcessProjectile += this.PostProcessProjectile;
        }
        protected override void OnPreDrop(PlayerController user)
        {
            user.PostProcessProjectile -= this.PostProcessProjectile;
            if (this.m_isCurrentlyActive)
            {
                this.EndEffect(user);
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.LastOwner != null)
            {
                base.LastOwner.PostProcessProjectile -= this.PostProcessProjectile;
            }
        }
        public override void Update()
        {
            base.Update();
            if (m_isCurrentlyActive && this.LastOwner != null)
            {
                bool flag = base.LastOwner.CurrentRoom != null && base.LastOwner && base.LastOwner.IsInCombat;
                if (flag)
                {
                    List<AIActor> activeEnemies = base.LastOwner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
                    foreach (AIActor aiactor in activeEnemies)
                    {
                        bool flag2 = aiactor != null;
                        if (flag2)
                        {
                            bool isBoss = aiactor.healthHaver.IsBoss;
                            bool flag4 = !isBoss;
                            if (flag4)
                            {
                                aiactor.PlayEffectOnActor((GameObject)BraveResources.Load("Global VFX/VFX_LockOn_Predator", ".prefab"), Vector3.zero, true, true, true);
                            }
                        }
                    }
                }
            }
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }

        public float duration = 15f;
        private StatModifier damageMod;
        private StatModifier rateOfFireMod;
        private StatModifier reloadSpeedMod;
        private StatModifier shotSpeedMod;
        private StatModifier piercingMod;
    }
}