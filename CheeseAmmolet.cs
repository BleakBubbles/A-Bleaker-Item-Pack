using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using System.Runtime.CompilerServices;
using Dungeonator;

namespace BleakMod
{
    public class CheeseAmmolet : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Cheese Ammolet";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/cheese_ammolet";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<CheeseAmmolet>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Blanks Encheese";
            string longDesc = "Blanks have a chance to encheese enemies.\n\nMade from a hunk of cheese found deep within the Resourceful Rat's lair, this ammolet will make for a quick way to neutralize enemies - as well as a tasty snack.";
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            item.quality = PickupObject.ItemQuality.A;
            item.AddToSubShop(ItemBuilder.ShopType.OldRed, 1f);
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnUsedBlank += this.OnUsedBlank;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            player.OnUsedBlank -= this.OnUsedBlank;
            return base.Drop(player);
        }
        private void OnUsedBlank(PlayerController player, int blanksRemaining)
        {
            bool isInCombat = player.IsInCombat;
            if (isInCombat)
            {
                foreach (AIActor aiactor in player.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
                {
                    bool flag = aiactor != null && UnityEngine.Random.value <= this.cheeseApplyChance;
                    if (flag)
                    {
                        aiactor.ApplyEffect(this.cheeseEffect, 100f, null);
                    }
                }
            }
        }
        public GameActorCheeseEffect cheeseEffect = (PickupObjectDatabase.GetById(626) as Gun).DefaultModule.projectiles[0].cheeseEffect;
        public float cheeseApplyChance = 0.75f;
    }
}