using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;

namespace BleakMod
{
    class MicroEnhancement : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Micro Enhancement";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/micro_enhancement";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<MicroEnhancement>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Powered Up";
            string longDesc = "Boosts the attack damage of companions significantly, for a short period of time. Gives the user a slight boost in damage as well.\n\n" +
                "This serum uses nano technology to empower the receiver. Strangely, it is much more potent when used upon an ally.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 400);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.B;
        }
        public override void Update()
        {
            if (this.LastOwner)
            {
                if (this.LastOwner.companions.Count > 0 && this.hasCompanions != true)
                {
                    this.hasCompanions = true;
                }
                else if (this.LastOwner.companions.Count == 0 && this.hasCompanions != false)
                {
                    this.hasCompanions = false;
                }
            }
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            if (this.hasCompanions)
            {
                if (m_isCurrentlyActive)
                {
                    if (proj.TreatedAsNonProjectileForChallenge)
                    {
                        proj.baseData.damage *= 5f;
                    }
                    else proj.baseData.damage *= 1.2f;
                }
            }
            else
            {
                if (this.m_isCurrentlyActive)
                {
                    proj.baseData.damage *= 1.5f;
                }
            }
            
        }
        protected override void DoEffect(PlayerController user)
        {
            base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, null));
        }
        public override void Pickup(PlayerController user)
        {
            user.PostProcessProjectile += this.PostProcessProjectile;
            base.Pickup(user);
        }
        protected override void OnPreDrop(PlayerController user)
        {
            user.PostProcessProjectile -= this.PostProcessProjectile;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.LastOwner != null)
            {
                base.LastOwner.PostProcessProjectile -= this.PostProcessProjectile;
            }
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        
        public float duration = 10f;
        public bool hasCompanions;
    }
}