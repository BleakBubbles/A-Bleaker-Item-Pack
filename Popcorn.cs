using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;

namespace BleakMod
{
    public class Popcorn: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Popcorn";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/popcorn";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Popcorn>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Pop! Pop!";
            string longDesc = "Each enemy killed adds a small chance for shots to fire 2 to 4 times the number of projectiles.\n\nThis delightful snack tastes so good, you almost want to thank the gun godz for it.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
            CustomSynergies.Add("Brrrrap!", new List<string>
            {
                "bb:popcorn"
            }, new List<string>
            {
                "melted_rock"
            }, true);
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            proj.OnHitEnemy += this.OnHitEnemy;
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            if (fatal)
            {
                if (base.m_owner.HasMTGConsoleID("melted_rock"))
                {
                    this.chanceIncrease = 1f;
                }
                else
                {
                    this.chanceIncrease = 0.5f;
                }
                StatModifier item = new StatModifier
                {
                    statToBoost = PlayerStats.StatType.ExtremeShadowBulletChance,
                    amount = this.chanceIncrease,
                    modifyType = StatModifier.ModifyMethod.ADDITIVE
                };
                base.Owner.ownerlessStatModifiers.Add(item);
                base.Owner.stats.RecalculateStats(base.Owner, true, false);
            }
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.PostProcessProjectile += this.PostProcessProjectile;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            player.PostProcessProjectile -= this.PostProcessProjectile;
            return base.Drop(player);
        }
        private float chanceIncrease;
    }
}