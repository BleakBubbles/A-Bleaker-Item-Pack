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
    public class PendantOfTheFirstOrder: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Pendant Of The First Order";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/pendant_of_the_first_order";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<PendantOfTheFirstOrder>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Necklace of the True Gun";
            string longDesc = "Reloading an empty clip fires a shot that agressively homes in on enemies.\n\nForged from a live bullet, this pendant belonged to one of the highest ranking officers in the Cult of the Gundead.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
        }
        private void OnReloadedGun(PlayerController player, Gun gun)
        {
            if(gun.ClipShotsRemaining == 0)
            {
                Projectile projectile = ((Gun)ETGMod.Databases.Items[598]).DefaultModule.projectiles[0];
                GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, player.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (player.CurrentGun == null) ? 0f : player.FacingDirection), true);
                Projectile proj1 = obj1.GetComponent<Projectile>();
                proj1.Owner = base.gameActor;
                proj1.Shooter = player.specRigidbody;
                proj1.baseData.damage = 50f;
                HomingModifier homingModifier = proj1.gameObject.GetOrAddComponent<HomingModifier>();
                homingModifier.HomingRadius = 100f;
                proj1.baseData.speed *= 0.75f;
            }
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnReloadedGun += this.OnReloadedGun;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            player.OnReloadedGun -= this.OnReloadedGun;
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.Owner != null)
            {
                base.Owner.OnReloadedGun -= this.OnReloadedGun;
            }
        }
    }
}