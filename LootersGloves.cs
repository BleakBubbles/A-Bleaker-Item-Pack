using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using Dungeonator;

namespace BleakMod
{
    class LootersGloves: PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Looter's Gloves";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/looters_glove";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<LootersGloves>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Bountiful";
            string longDesc = "Eats your current gun in return for a damage boost depending on the rarity of the eaten gun.\n\nThey say that you should always make the most out of what you have. Well now, with the Gun Shredder®, you can turn unwanted guns into damage you'll actually use! ";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 1.0f);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.SPECIAL;
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            PassiveItem.IncrementFlag(player, typeof(LootersGloves));
            if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
            {
                PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
            }
            if (!PassiveItem.ActiveFlagItems[player].ContainsKey(typeof(CorpseExplodeActiveItem)))
            {
                PassiveItem.ActiveFlagItems[player].Add(typeof(CorpseExplodeActiveItem), 1);
            }
            else
            {
                PassiveItem.ActiveFlagItems[player][typeof(CorpseExplodeActiveItem)] = PassiveItem.ActiveFlagItems[player][typeof(CorpseExplodeActiveItem)] + 1;
            }
        }
        protected override void OnPreDrop(PlayerController user)
        {
            PassiveItem.DecrementFlag(user, typeof(LootersGloves));
            if (PassiveItem.ActiveFlagItems.ContainsKey(user) && PassiveItem.ActiveFlagItems[user].ContainsKey(typeof(CorpseExplodeActiveItem)))
            {
                PassiveItem.ActiveFlagItems[user][typeof(CorpseExplodeActiveItem)] = Mathf.Max(0, PassiveItem.ActiveFlagItems[user][typeof(CorpseExplodeActiveItem)] - 1);
                if (PassiveItem.ActiveFlagItems[user][typeof(CorpseExplodeActiveItem)] == 0)
                {
                    PassiveItem.ActiveFlagItems[user].Remove(typeof(CorpseExplodeActiveItem));
                }
            }
        }
        protected override void DoEffect(PlayerController user)
		{
			for(int i = 0; i < StaticReferenceManager.AllCorpses.Count; i++)
            {
                GameObject corpse = StaticReferenceManager.AllCorpses[i];
                if (corpse && corpse.GetComponent<tk2dBaseSprite>() && gameObject.transform.position.GetAbsoluteRoom() == user.CurrentRoom)
                {
                    if (corpse.gameObject.GetComponent<CorpseInteractManager>() == null)
                    {
                        corpse.gameObject.AddComponent<CorpseInteractManager>();
                    }
                }
            }
		}
        public override bool CanBeUsed(PlayerController user)
        {
            return base.CanBeUsed(user);
        }
    }
}
    