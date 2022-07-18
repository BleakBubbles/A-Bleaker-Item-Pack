using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using HutongGames.PlayMaker.Actions;
using Dungeonator;

namespace BleakMod
{
    class PackOfLostItems : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Pack Of Lost Items";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/pack_of_lost_items";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<PackOfLostItems>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Bot and Sold";
            string longDesc = "A strange satchel dropped by a mysterious figure.\n\n" +
                "It feels powerful and sturdy, but well-worn and abandoned as well...";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 5.0f);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AdditionalItemCapacity, 1, StatModifier.ModifyMethod.ADDITIVE);
            //Set some other fields
            item.consumable = true;
            item.numberOfUses = 3;
            item.quality = ItemQuality.EXCLUDED;
        }
        protected override void DoEffect(PlayerController user)
        {
            foreach(int x in this.stolenItems)
            {
                if (!user.HasPickupID(x))
                {
                    DebrisObject item = LootEngine.SpawnItem(PickupObjectDatabase.GetById(x).gameObject, user.specRigidbody.UnitCenter, Vector2.up, 1f, false, true, false);
                    if (item.GetComponent<PickupObject>())
                    {
                        item.GetComponent<PickupObject>().CanBeDropped = false;
                    }
                }
            }
            this.stolenItems.Clear();
        }
        public override void Update()
        {
            base.Update();
            List<PickupObject> floorItems = new List<PickupObject>();
            foreach (DebrisObject debris in StaticReferenceManager.AllDebris)
            {
                if (debris.GetComponent<PickupObject>() != null)
                {
                    floorItems.Add(debris.GetComponent<PickupObject>());
                }
            }
            foreach(PickupObject thing in floorItems)
            {
                if (!this.stolenItems.Contains(thing.PickupObjectId) && (thing.IsBeingEyedByRat || thing.IsBeingSold))
                {
                    int id = thing.PickupObjectId;
                    this.stolenItems.Add(id);
                }
            }
        }
        private IEnumerator delayedEffect()
        {
            yield return new WaitForSeconds(5f);
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return base.CanBeUsed(user);
        }
        private List<int> stolenItems = new List<int>();
    }
}