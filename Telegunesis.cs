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
    class Telegunesis : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Telegunesis";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/telegunesis";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<Telegunesis>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Center Of Attraction";
            string longDesc = "Pulls enemies towards you and slows down enemy bullets while active.\n\n" +
                "Those who enter the Gungeon must have a brain so big that it generates tangible gravity.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 250);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.C;
        }
        protected override void DoEffect(PlayerController user)
        {
            this.StartEffect(user);
            base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.EndEffect)));
            base.StartCoroutine(this.reverseKnockback());
        }
        private void StartEffect(PlayerController user)
        {
            this.AddStat(PlayerStats.StatType.EnemyProjectileSpeedMultiplier, 0.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            user.stats.RecalculateStats(user, true, false);
        }
        private void EndEffect(PlayerController user)
        {                       
            this.RemoveStat(PlayerStats.StatType.EnemyProjectileSpeedMultiplier);
            user.stats.RecalculateStats(user, true, false);
        }
        private IEnumerator reverseKnockback()
        {
            for(int i = 0; i < 6; i++)
            {
                AkSoundEngine.PostEvent("Play_WPN_moonscraperLaser_shot_01", base.gameObject);
                Exploder.DoRadialKnockback(base.LastOwner.CenterPosition, -25f, 25f);
                yield return new WaitForSeconds(1);
            }
            AkSoundEngine.PostEvent("Stop_WPN_All", base.gameObject);
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
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
        }
        protected override void OnPreDrop(PlayerController user)
        {
            if (this.m_isCurrentlyActive)
            {
                this.EndEffect(user);
            }
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return base.CanBeUsed(user);
        }
        public float duration = 6f;
    }
}