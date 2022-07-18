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
    class BowlersBriefcase : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Bowler's Briefcase";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/bowler's_briefcase";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<BowlersBriefcase>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Long term satisfaction";
            string longDesc = "Bowler, being a gentleman, carries his briefcase everywhere. However, he loses it sometimes during his travels throughout the Gungeon. It's your job to return it safely to him... but he wouldn't notice if just a few chests were missing, right?\n\n" +
                "Scribbled on the label of the briefcase is the following:\n\"1 key = brown, blue, or green\"\n\"2 keys = red or black\"\nThe writing below that is written in rainbow ink and smudged, but you can make out the number 7.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 5.0f);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AdditionalItemCapacity, 1, StatModifier.ModifyMethod.ADDITIVE);
            //Set some other fields
            item.consumable = true;
            item.numberOfUses = 5;
            item.quality = ItemQuality.A;
            item.AddToSubShop(ItemBuilder.ShopType.Flynt, 1f);

            Material material = new Material(ShaderCache.Acquire("Brave/Internal/RainbowChestShader"));
            material.SetFloat("_AllColorsToggle", 1f);
            var def = item.sprite.GetCurrentSpriteDef();
            def.material = item.sprite.renderer.material;

            GameManager.Instance.RainbowRunForceExcludedIDs.Add(item.PickupObjectId);
        }
        protected override void DoEffect(PlayerController user)
        {
            if(user.carriedConsumables.KeyBullets == 1)
            {
                Vector2 yourPosition = user.sprite.WorldCenter + new Vector2(-0.5f, 0f);
                List<Chest> possibleChests = new List<Chest>
                {
                  GameManager.Instance.RewardManager.D_Chest,
                  GameManager.Instance.RewardManager.C_Chest,
                  GameManager.Instance.RewardManager.B_Chest,
                };
                Chest chest = Chest.Spawn(BraveUtility.RandomElement(possibleChests), yourPosition, yourPosition.GetAbsoluteRoom());
                chest.IsLocked = false;
                user.carriedConsumables.KeyBullets -= 1;
            }
            else if(user.carriedConsumables.KeyBullets > 1 && user.carriedConsumables.KeyBullets < 7)
            {
                Vector2 yourPosition = user.sprite.WorldCenter + new Vector2(-0.5f, 0f);
                List<Chest> possibleChests = new List<Chest>
                {
                  GameManager.Instance.RewardManager.A_Chest,
                  GameManager.Instance.RewardManager.S_Chest,
                };
                Chest chest = Chest.Spawn(BraveUtility.RandomElement(possibleChests), yourPosition, yourPosition.GetAbsoluteRoom());
                chest.IsLocked = false;
                user.carriedConsumables.KeyBullets -= 2;
            }
            else
            {
                Vector2 yourPosition = user.sprite.WorldCenter + new Vector2(-0.5f, 0f);
                Chest chest = Chest.Spawn(GameManager.Instance.RewardManager.Rainbow_Chest, yourPosition, yourPosition.GetAbsoluteRoom());
                chest.IsLocked = false;
                user.carriedConsumables.KeyBullets -= 7;
            }
        }
      
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return user.carriedConsumables.KeyBullets > 0 && base.CanBeUsed(user);
        }
    }
}