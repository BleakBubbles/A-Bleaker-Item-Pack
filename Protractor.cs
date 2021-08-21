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
    public class Protractor: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Protractor";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/protractor";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Protractor>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Choose The Right Angle";
            string longDesc = "Changes each of the player's shots into two shots that fire in opposite directions, perpendicular to the player's cursor and increases damage by 50%.\n\nYour elementary school teachers always told you that you wouldn't keep a protractor in your pocket everywhere you go. Prove them wrong.";
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, 1.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }
        private void PostProcessProjectile(Projectile proj, float f)
        {
            proj.StartCoroutine(this.SwitchDirection(proj));
        }
        private IEnumerator SwitchDirection(Projectile proj)
        {
            yield return 0;
            Vector2 newDirection = new Vector2(proj.Direction.y, proj.Direction.x * -1);
            proj.SendInDirection(newDirection, true);
            Vector2 spawnPoint = base.Owner.CurrentGun.barrelOffset.position;
            GameObject obj1 = SpawnManager.SpawnProjectile(proj.gameObject, spawnPoint, Quaternion.Euler(0f, 0f, (base.Owner.CurrentGun == null) ? 0f : newDirection.normalized.ToAngle() + 180), true);
            Projectile proj2 = obj1.GetComponent<Projectile>();
            proj2.Owner = base.Owner;
            proj2.Shooter = base.Owner.specRigidbody;
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