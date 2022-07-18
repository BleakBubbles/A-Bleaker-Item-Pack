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
    class GunShredder: PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Gun Shredder";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/gun_shredder";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<GunShredder>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Inshredible";
            string longDesc = "Eats your current gun in return for a damage boost depending on the rarity of the eaten gun.\n\nThey say that you should always make the most out of what you have. Well now, with the Gun Shredder®, you can turn unwanted guns into damage you'll actually use! ";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 1.0f);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AdditionalItemCapacity, 1, StatModifier.ModifyMethod.ADDITIVE);
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.B;
            item.AddToSubShop(ItemBuilder.ShopType.Trorc, 1f);
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }
		protected override void DoEffect(PlayerController user)
		{
			AkSoundEngine.PostEvent("Play_NPC_BabyDragun_Munch_01", base.gameObject);
            if (user.CurrentGun.quality == ItemQuality.S) 
            {
                dmgBoost = 0.25f;
            }
            else if (user.CurrentGun.quality == ItemQuality.A)
            {
                dmgBoost = 0.2f;
            }
            else if (user.CurrentGun.quality == ItemQuality.B)
            {
                dmgBoost = 0.15f;
            }
            else if (user.CurrentGun.quality == ItemQuality.C)
            {
                dmgBoost = 0.10f;
            }
            else
            {
                dmgBoost = 0.05f;
            }
            user.inventory.DestroyCurrentGun();
			StatModifier item = new StatModifier
			{
				statToBoost = PlayerStats.StatType.Damage,
				amount = dmgBoost,
				modifyType = StatModifier.ModifyMethod.ADDITIVE
			};
			user.ownerlessStatModifiers.Add(item);
			user.stats.RecalculateStats(user, true, false);
		}
		//Disable or enable the active whenever you need!
		public override bool CanBeUsed(PlayerController user)
        {
            return user.CurrentGun != null && user.CurrentGun.CanActuallyBeDropped(user) && !user.CurrentGun.InfiniteAmmo && base.CanBeUsed(user);
        }
        public float dmgBoost;
    }
}
    