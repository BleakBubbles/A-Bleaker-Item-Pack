using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;

namespace BleakMod
{
    public class ShowoffBullets: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Showoff Bullets";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/showoff_bullets";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<ShowoffBullets>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Look at me!";
            string longDesc = "Adds multiple shot modifiers to your bullets, at the cost of a small amount of damage.\n\nThey say he's got it all. The teeth, the hair, the looks... but deep down he knows that he just isn't all that exciting.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item
            item.AddPassiveStatModifier(PlayerStats.StatType.AdditionalShotPiercing, 1f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.AdditionalShotBounces, 2f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.ProjectileSpeed, 0.5f, StatModifier.ModifyMethod.ADDITIVE);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.A;
            CustomSynergies.Add("Radical!", new List<string>
            {
                "bb:showoff_bullets"
            }, new List<string>
            {
                "rad_gun",
                "sunglasses"
            }, true);
            CustomSynergies.Add("Flashy", new List<string>
            {
                "bb:showoff_bullets"
            }, new List<string>
            {
                "camera"
            }, true);
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            HomingModifier homingModifier = proj.gameObject.GetComponent<HomingModifier>();
            if (homingModifier == null)
            {
                homingModifier = proj.gameObject.AddComponent<HomingModifier>();
                homingModifier.HomingRadius = 25;
                homingModifier.AngularVelocity = 250;
            }
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
            user.PostProcessProjectile += this.PostProcessProjectile;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            player.PostProcessProjectile -= this.PostProcessProjectile;
            return base.Drop(player);
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
        protected override void Update()
        {
            base.Update();
            this.currentItems = base.m_owner.passiveItems.Count;
            if (this.currentItems != this.lastItems)
            {
                this.RemoveStat(PlayerStats.StatType.Coolness);
                if (base.m_owner.HasMTGConsoleID("rad_gun") || base.m_owner.HasMTGConsoleID("sunglasses"))
                {
                    this.AddStat(PlayerStats.StatType.Coolness, 3f);
                }
                else
                {
                    this.AddStat(PlayerStats.StatType.Coolness, 0f);
                }
                if (base.m_owner.HasMTGConsoleID("camera"))
                {
                    this.handleDamage(true);
                }
                else
                {
                    this.handleDamage(false);
                }
                this.lastItems = this.currentItems;
            }
        }
        private void handleDamage(bool hasSynergy)
        {
            this.RemoveStat(PlayerStats.StatType.Damage);
            if (hasSynergy)
            {
                this.AddStat(PlayerStats.StatType.Damage, 0.1f);
            }
            else
            {
                this.AddStat(PlayerStats.StatType.Damage, -0.1f);
            }
            base.m_owner.stats.RecalculateStats(base.m_owner, true, false);
        }
        public int currentItems;
        public int lastItems;
    }
}
