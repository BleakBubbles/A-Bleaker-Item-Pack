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
    public class MimicBullets : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Mimic Bullets";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/door_lord_item";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<MimicBullets>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Shells of the Mimic Lord";
            string longDesc = "The miniature husk of an elusive mimic lord.\n\nYou do 50% more damage, but your shots jam enemy bullets that pass through them.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, 1.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            proj.collidesWithProjectiles = true;
            SpeculativeRigidbody specRigidbody = proj.specRigidbody;

            specRigidbody.OnPreRigidbodyCollision += this.HandlePreCollision;
        }
        private void HandlePreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
        {
            bool flag = otherRigidbody && otherRigidbody.projectile;
            if (flag)
            {
                bool flag2 = otherRigidbody.projectile.Owner is AIActor;
                if (flag2)
                {
                    bool isNormalBullet = !otherRigidbody.projectile.IsBlackBullet;
                    if (isNormalBullet && otherRigidbody.projectile.CanBecomeBlackBullet)
                    {
                        otherRigidbody.projectile.BecomeBlackBullet();
                    }
                }
                PhysicsEngine.SkipCollision = true;
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
    }
}