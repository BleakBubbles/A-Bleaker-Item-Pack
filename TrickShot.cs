using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;

namespace BleakMod
{
    public class TrickShot: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Trick Shot";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/trick_shot";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<TrickShot>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Total Mayhem";
            string longDesc = "Adds 5 bounces and a bit of projectile speed to the player's shots. Decreases base damage slightly, but each bounce of a projectile increases its damage.\n\nThe elder brother of Bouncy Bullets. The story goes that Trick Shot left his hometown, put on some bling, and went to make a name for himself in the big city.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item
            item.AddPassiveStatModifier(PlayerStats.StatType.Damage, 0.8f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.ProjectileSpeed, 1.33f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.RangeMultiplier, 5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.AdditionalShotBounces, 5f);
            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
            CustomSynergies.Add("Heart-Piercingly Cool", new List<string>
            {
                "bb:trick_shot"
            }, new List<string>
            {
                "rad_gun",
                "ice_cube",
                "ballot",
                "heart_of_ice"
            }, true);
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            BounceProjModifier boing = proj.gameObject.GetOrAddComponent<BounceProjModifier>();
            boing.OnBounceContext += this.OnBounce;
        }
        private void OnBounce(BounceProjModifier boing, SpeculativeRigidbody body) {
            if (boing.projectile)
            {
                boing.projectile.baseData.damage *= 1.25f;
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
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.Owner != null)
            {
                base.Owner.PostProcessProjectile -= this.PostProcessProjectile;
            }
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
            this.currentItems = base.m_owner.passiveItems.Count + base.m_owner.inventory.AllGuns.Count;
            if (this.currentItems != this.lastItems)
            {
                this.RemoveStat(PlayerStats.StatType.AdditionalShotPiercing);
                if (base.m_owner.PlayerHasActiveSynergy("Heart-Piercingly Cool"))
                {
                    this.AddStat(PlayerStats.StatType.AdditionalShotPiercing, 5f);
                }
                else
                {
                    this.AddStat(PlayerStats.StatType.AdditionalShotPiercing, 0f);
                }
                base.m_owner.stats.RecalculateStats(base.m_owner, true, false);
                this.lastItems = this.currentItems;
            }
        }
        private int currentItems;
        private int lastItems;
    }
}