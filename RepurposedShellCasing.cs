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
    public class RepurposedShellCasing: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Repurposed Shell Casing";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/repurposed_shell_casing";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<RepurposedShellCasing>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Reduce, Reload, Recycle";
            string longDesc = "When the player overkills an enemy, the extra damage is added to the player's next shot. Can be triggered multiple times in a row.\n\nReusable and repurposed objects are all the rage these days. With this trendy shell casing, you can both save the gunvironment and show it off to your friends!";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            try
            {
                proj.specRigidbody.OnPreRigidbodyCollision += this.HandlePreCollision;
            }
            catch (Exception ex)
            {
                ETGModConsole.Log(ex.Message, false);
            }
            if (this.hasExtraDamage)
            {
                proj.baseData.damage += this.extraDamage;
                this.hasExtraDamage = false;
                this.DisableVFX(base.Owner);
            }
        }
        private void DisableVFX(PlayerController user)
        {
            Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(user.sprite);
            outlineMaterial.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
        }
        private void HandlePreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
        {
            bool flag = otherRigidbody && otherRigidbody.healthHaver;
            if (flag)
            {
                if(myRigidbody.projectile.baseData.damage > otherRigidbody.healthHaver.GetCurrentHealth())
                {
                    this.extraDamage = myRigidbody.projectile.baseData.damage - otherRigidbody.healthHaver.GetCurrentHealth();
                    this.hasExtraDamage = true;
                    Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(base.Owner.sprite);
                    outlineMaterial.SetColor("_OverrideColor", new Color32(120, 190, 85, 50));
                }
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
            this.DisableVFX(player);
            return base.Drop(player);
        }
        public float extraDamage;
        public bool hasExtraDamage;
    }
}