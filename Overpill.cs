using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;

namespace BleakMod
{
    public class Overpill: PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Init()
        {
            //The name of the item
            string itemName = "Overpill";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/overkill_bullets";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Overpill>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Activate Instant Kill";
            string longDesc = "5 uses. On each use, compresses a whole clip's worth of bullets into a single shot in your current gun.\nThe term overkill is not in your dictionary. Overkill? What overkill?\n\nNote: Using overpill will make your current gun undroppable.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 1f);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = true;
            item.numberOfUses = 5;
            item.quality = ItemQuality.S;
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            if (this.affectedGuns.Contains(base.LastOwner.CurrentGun) && !proj.TreatedAsNonProjectileForChallenge)
            {
                proj.ignoreDamageCaps = true;
            }
            if (this.affectedGuns.Contains(base.LastOwner.CurrentGun) && this.stupidGuns.Contains(base.LastOwner.CurrentGun.PickupObjectId))
            {
                proj.baseData.damage *= this.origCapacities[this.stupidGuns.IndexOf(base.LastOwner.CurrentGun.PickupObjectId)];
            }
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
            user.PostProcessProjectile += this.PostProcessProjectile;
        }
        protected override void OnPreDrop(PlayerController user)
        {
            user.PostProcessProjectile -= this.PostProcessProjectile;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            base.LastOwner.PostProcessProjectile -= this.PostProcessProjectile;
        }
        protected override void DoEffect(PlayerController user) 
        {
            Gun gun = user.CurrentGun;
            if (!this.stupidGuns.Contains(gun.PickupObjectId))
            {
                gun.damageModifier += (int)(gun.DefaultModule.projectiles[0].baseData.damage * (gun.ClipCapacity - 1));
            }
            gun.SetBaseMaxAmmo(gun.GetBaseMaxAmmo() / gun.ClipCapacity);
            gun.CurrentAmmo /= gun.ClipCapacity;
            gun.AdditionalClipCapacity = -gun.ClipCapacity + 1;
            this.affectedGuns.Add(gun);
            user.stats.RecalculateStats(user, true, false);
        }
        public override void Update()
        {
            base.Update();
            if (this.LastOwner)
            {
                foreach(Gun gun in this.affectedGuns)
                {
                    if(gun.PickupObjectId == base.LastOwner.CurrentGun.PickupObjectId && base.LastOwner.CurrentGun.ClipCapacity != 1)
                    {
                        base.LastOwner.CurrentGun.ClipShotsRemaining = 1;
                    }
                }
            }
            
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)   
        {
            return user.CurrentGun.ClipCapacity > 1 && !this.affectedGuns.Contains(user.CurrentGun) && base.CanBeUsed(user);
        }
        public List<Gun> affectedGuns = new List<Gun>();
        public List<int> stupidGuns = new List<int>
        {
            329,
            507,
            122
        };
        public List<int> origCapacities = new List<int>
        {
            4,
            100,
            4
        };
    }
}