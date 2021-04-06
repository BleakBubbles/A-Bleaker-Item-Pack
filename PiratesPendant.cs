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
    public class PiratesPendant: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Pirate's Pendant";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/pirates_pendant";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<PiratesPendant>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Reckless Necklace";
            string longDesc = "A treasured keepsake of the pirate's that has accompanied him through many journies.\n\nIt seems to glow brightly in the presence of golds and riches.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.SPECIAL;
        }
        protected override void Update()
        {
            base.Update();
            if (base.Owner)
            {
                StatModifier item = new StatModifier
                {
                    statToBoost = PlayerStats.StatType.RateOfFire,
                    amount = base.Owner.carriedConsumables.Currency * 0.01f,
                    modifyType = StatModifier.ModifyMethod.ADDITIVE
                };
                base.Owner.ownerlessStatModifiers.Add(item);
                base.Owner.stats.RecalculateStats(base.Owner, true, false);
                base.Owner.ownerlessStatModifiers.Remove(item);
            }
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            return base.Drop(player);
        }
    }
}