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
    class AmmocondasNest : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Ammoconda's Nest";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/ammocondas_nest";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<AmmocondasNest>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "How Did You Get This?";
            string longDesc = "A curious nest full of brownish, spotted eggs.";
            item.numberOfUses = 3;
            item.UsesNumberOfUsesBeforeCooldown = true;
            item.consumable = false;
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 200);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Curse, 1, StatModifier.ModifyMethod.ADDITIVE);
            //Set some other fields
            item.quality = ItemQuality.C;
        }
        protected override void DoEffect(PlayerController user)
        {
            if (base.LastOwner && base.LastOwner.CurrentRoom != null)
            {
                try
                {
                    AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid("f38686671d524feda75261e469f30e0b");
                    IntVector2? intVector = new IntVector2?(base.LastOwner.CurrentRoom.GetRandomVisibleClearSpot(1, 1));
                    bool flag = intVector != null;
                    if (flag)
                    {
                        AIActor aiactor = AIActor.Spawn(orLoadByGuid.aiActor, intVector.Value, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector.Value), true, AIActor.AwakenAnimationType.Default, true);
                        aiactor.ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect, 1f, null);
                        aiactor.IgnoreForRoomClear = true;
                        aiactor.bulletBank.OnProjectileCreated += this.NotDamagePlayer;
                        aiactor.gameObject.AddComponent<KillOnRoomClear>();
                    }
                }
                catch (Exception ex)
                {
                    ETGModConsole.Log(ex.Message, false);
                }
                
            }
            this.StartEffect(user);
            if (this.numberOfUses > 1)
            {
                base.StartCoroutine(this.Buff());
            }
            else
            {
                base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.EndEffect)));
            }
        }
        private void StartEffect(PlayerController user)
        {
            this.AddStat(PlayerStats.StatType.Damage, 1.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            this.AddStat(PlayerStats.StatType.MovementSpeed, 1f, StatModifier.ModifyMethod.ADDITIVE);
            this.AddStat(PlayerStats.StatType.DodgeRollSpeedMultiplier, 1.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            user.stats.RecalculateStats(user, true, false);
        }
        private void EndEffect(PlayerController user)
        {                       
            this.RemoveStat(PlayerStats.StatType.Damage);
            this.RemoveStat(PlayerStats.StatType.MovementSpeed);
            this.RemoveStat(PlayerStats.StatType.DodgeRollSpeedMultiplier);
            user.stats.RecalculateStats(user, true, false);
        }
        private void AddStat(PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            StatModifier modifier = new StatModifier();
            modifier.amount = amount;
            modifier.statToBoost = statType;
            modifier.modifyType = method;

            foreach (var m in passiveStatModifiers)
            {
                if (m.statToBoost == statType) return; //don't add duplicates
            }

            if (this.passiveStatModifiers == null)
                this.passiveStatModifiers = new StatModifier[] { modifier };
            else
                this.passiveStatModifiers = this.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
        }
        private void RemoveStat(PlayerStats.StatType statType)
        {
            var newModifiers = new List<StatModifier>();
            for (int i = 0; i < passiveStatModifiers.Length; i++)
            {
                if (passiveStatModifiers[i].statToBoost != statType)
                    newModifiers.Add(passiveStatModifiers[i]);
            }
            this.passiveStatModifiers = newModifiers.ToArray();
        }
        private IEnumerator Buff()
        {
            yield return new WaitForSeconds(10);
            this.EndEffect(base.LastOwner);
        }

        public void NotDamagePlayer(Projectile proj)
        {
            proj.collidesWithPlayer = false;
            proj.UpdateCollisionMask();
        }

        private void OnKilledEnemyContext(PlayerController player, HealthHaver enemy)
        {
            if (enemy.aiActor.EnemyGuid == "f38686671d524feda75261e469f30e0b" && UnityEngine.Random.value <= 0.1f)
            {
                float curHealth = player.healthHaver.GetCurrentHealth();
                player.healthHaver.ForceSetCurrentHealth(curHealth + 1);
            }
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
            user.OnKilledEnemyContext += this.OnKilledEnemyContext;
        }
        protected override void OnPreDrop(PlayerController user)
        {
            if (this.m_isCurrentlyActive)
            {
                this.EndEffect(user);
            }
            user.OnKilledEnemyContext -= this.OnKilledEnemyContext;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.m_isCurrentlyActive)
            {
                this.EndEffect(base.LastOwner);
            }
            base.LastOwner.OnKilledEnemyContext -= this.OnKilledEnemyContext;
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        public float duration = 10f;
    }
}