using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;

namespace BleakMod
{
    public class Underpill: PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Init()
        {
            //The name of the item
            string itemName = "Underpill";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/underpill";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Underpill>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Activate Delayed Kill";
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
            item.quality = ItemQuality.EXCLUDED;
        }
        protected override void DoEffect(PlayerController user)
        {
            Gun gun = user.CurrentGun;
            gun.AdditionalClipCapacity = gun.GetBaseMaxAmmo() - gun.ClipCapacity;
            gun.CurrentAmmo *= gun.GetBaseMaxAmmo() / gun.ClipCapacity;
            gun.SetBaseMaxAmmo(gun.GetBaseMaxAmmo() * (gun.GetBaseMaxAmmo() / gun.ClipCapacity));
            if (gun.Volley != null)
            {
                foreach (ProjectileModule projMod in gun.Volley.projectiles)
                {
                    foreach (Projectile proj in projMod.projectiles)
                    {
                        gun.DefaultModule.cooldownTime /= proj.baseData.damage;
                        proj.baseData.damage = 1f;
                    }
                }
            }
            else
            {
                foreach (Projectile proj in gun.singleModule.projectiles)
                {
                    gun.DefaultModule.cooldownTime /= proj.baseData.damage;
                    proj.baseData.damage = 1f;
                }
            }
            gun.reloadTime /= 2; 
            this.affectedGuns.Add(gun);
            user.stats.RecalculateStats(user, true, false);
        }
        public override void Update()
        {
            base.Update();
            this.currentGun = base.LastOwner.CurrentGun;
            if (this.LastOwner && this.currentGun != this.lastGun)
            {
                this.RemoveStat(PlayerStats.StatType.ChargeAmountMultiplier);
                foreach (Gun gun in this.affectedGuns)
                {
                    if (gun.PickupObjectId == base.LastOwner.CurrentGun.PickupObjectId)
                    {
                        this.AddStat(PlayerStats.StatType.ChargeAmountMultiplier, 0f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                        base.LastOwner.CurrentGun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
                        
                    }
                }
                base.LastOwner.stats.RecalculateStats(base.LastOwner, true, false);
                this.lastGun = this.currentGun;
            }
        }
        private void AddStat(PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            StatModifier modifier = new StatModifier();
            modifier.amount = amount;
            modifier.statToBoost = statType;
            modifier.modifyType = method;

            foreach (var m in passiveStatModifiers)
            {
                if (m.statToBoost == statType) return; //don't add duplicates
            }

            if (this.passiveStatModifiers == null)
                this.passiveStatModifiers = new StatModifier[] { modifier };
            else
                this.passiveStatModifiers = this.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
        }
        private void RemoveStat(PlayerStats.StatType statType)
        {
            var newModifiers = new List<StatModifier>();
            for (int i = 0; i < passiveStatModifiers.Length; i++)
            {
                if (passiveStatModifiers[i].statToBoost != statType)
                    newModifiers.Add(passiveStatModifiers[i]);
            }
            this.passiveStatModifiers = newModifiers.ToArray();
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)   
        {
            return !this.affectedGuns.Contains(user.CurrentGun) && base.CanBeUsed(user);
        }
        public List<Gun> affectedGuns = new List<Gun>();
        public Gun currentGun;
        public Gun lastGun = (PickupObjectDatabase.GetById(183) as Gun);
    }
}