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
    public class HungryClips: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Hungry Clips";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/hungry_clips";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<HungryClips>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Snappy Reload";
            string longDesc = "Killing an enemy refills your gun's clip. Slightly reduces reload time.\n\nIn a freak accident, an abandoned gun's clip was granted life as well as an unquenchable thirst. It's now up to you to sustain this hungry being by feeding it the souls of your enemies.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item
            item.AddPassiveStatModifier(PlayerStats.StatType.ReloadSpeed, 0.75f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            proj.OnHitEnemy += this.OnHitEnemy;
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            Gun gun = base.Owner.CurrentGun;
            if (fatal && !gun.IsReloading && gun.ClipShotsRemaining != gun.ClipCapacity)
            {
                gun.MoveBulletsIntoClip(gun.ClipCapacity - gun.ClipShotsRemaining);
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
    }
}