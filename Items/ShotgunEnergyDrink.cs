using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
namespace BleakMod
{
    class ShotgunEnergyDrink : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Shotgun Energy Drink";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/shotgun_energy_drink";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<ShotgunEnergyDrink>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Hyper Active";
            string longDesc = "Boosts most stats for a short time, but at a cost afterwards.\n\n" +
                "Our finest blend of Shotgun Coffee and Shotga Cola. Drink Responsibly.\n\nNote: dropping this item and picking it back up will get rid of all the stats you have built up!";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 15f);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.A;
            CustomSynergies.Add("Sugar Breath", new List<string>
            {
                "bb:shotgun_energy_drink"
            }, new List<string>
            {
                "shotga_cola",
                "shotgun_coffee"
            }, true);
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
            this.TimesUsed = 0;
        }
        protected override void OnPreDrop(PlayerController user)
        {
            if (this.m_isCurrentlyActive)
            {
                this.EndEffect(user);
            }
            else
            {
                float curDamage = user.stats.GetBaseStatValue(PlayerStats.StatType.Damage);
                float newDamage = curDamage += 0.05f * TimesUsed;
                user.stats.SetBaseStatValue(PlayerStats.StatType.Damage, newDamage, user);
                float curRateOfFire = user.stats.GetBaseStatValue(PlayerStats.StatType.RateOfFire);
                float newRateOfFire = curRateOfFire += 0.05f * TimesUsed;
                user.stats.SetBaseStatValue(PlayerStats.StatType.RateOfFire, newRateOfFire, user);
                float curReloadSpeed = user.stats.GetBaseStatValue(PlayerStats.StatType.ReloadSpeed);
                float newReloadSpeed = curReloadSpeed -= 0.05f * TimesUsed;
                user.stats.SetBaseStatValue(PlayerStats.StatType.ReloadSpeed, newReloadSpeed, user);
                float curMovementSpeed = user.stats.GetBaseStatValue(PlayerStats.StatType.MovementSpeed);
                float newMovementSpeed = curMovementSpeed += 0.1f * TimesUsed;
                user.stats.SetBaseStatValue(PlayerStats.StatType.MovementSpeed, newMovementSpeed, user);
                float curDodgeRollSpeed = user.stats.GetBaseStatValue(PlayerStats.StatType.DodgeRollSpeedMultiplier);
                float newDodgeRollSpeed = curDodgeRollSpeed += 0.025f * TimesUsed;
                user.stats.SetBaseStatValue(PlayerStats.StatType.DodgeRollSpeedMultiplier, newDodgeRollSpeed, user);
            }
        }
        protected override void DoEffect(PlayerController user)
        {
            AkSoundEngine.PostEvent("Play_OBJ_power_up_01", base.gameObject);
            this.TimesUsed += 1;
            this.StartEffect(user);
            base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.EndEffect)));
            if(!(this.LastOwner.HasMTGConsoleID("shotga_cola") || this.LastOwner.HasMTGConsoleID("shotgun_coffee")))
            {
                user.baseFlatColorOverride = new Color(0, 0, 0, 0);
            }
            float curDamage = user.stats.GetBaseStatValue(PlayerStats.StatType.Damage);
            float newDamage = curDamage -= 0.05f;
            user.stats.SetBaseStatValue(PlayerStats.StatType.Damage, newDamage, user);
            float curRateOfFire = user.stats.GetBaseStatValue(PlayerStats.StatType.RateOfFire);
            float newRateOfFire = curRateOfFire -= 0.05f;
            user.stats.SetBaseStatValue(PlayerStats.StatType.RateOfFire, newRateOfFire, user);
            float curReloadSpeed = user.stats.GetBaseStatValue(PlayerStats.StatType.ReloadSpeed);
            float newReloadSpeed = curReloadSpeed += 0.05f;
            user.stats.SetBaseStatValue(PlayerStats.StatType.ReloadSpeed, newReloadSpeed, user);
            float curMovementSpeed = user.stats.GetBaseStatValue(PlayerStats.StatType.MovementSpeed);
            float newMovementSpeed = curMovementSpeed -= 0.1f;
            user.stats.SetBaseStatValue(PlayerStats.StatType.MovementSpeed, newMovementSpeed, user);
            float curDodgeRollSpeed = user.stats.GetBaseStatValue(PlayerStats.StatType.DodgeRollSpeedMultiplier);
            float newDodgeRollSpeed = curDodgeRollSpeed -= 0.025f;
            user.stats.SetBaseStatValue(PlayerStats.StatType.DodgeRollSpeedMultiplier, newDodgeRollSpeed, user);
            if (this.LastOwner.HasMTGConsoleID("shotga_cola") || this.LastOwner.HasMTGConsoleID("shotgun_coffee"))
            {
                curDamage = user.stats.GetBaseStatValue(PlayerStats.StatType.Damage);
                newDamage = curDamage += 0.05f;
                user.stats.SetBaseStatValue(PlayerStats.StatType.Damage, newDamage, user);
                curRateOfFire = user.stats.GetBaseStatValue(PlayerStats.StatType.RateOfFire);
                newRateOfFire = curRateOfFire += 0.05f;
                user.stats.SetBaseStatValue(PlayerStats.StatType.RateOfFire, newRateOfFire, user);
                curReloadSpeed = user.stats.GetBaseStatValue(PlayerStats.StatType.ReloadSpeed);
                newReloadSpeed = curReloadSpeed -= 0.05f;
                user.stats.SetBaseStatValue(PlayerStats.StatType.ReloadSpeed, newReloadSpeed, user);
                curMovementSpeed = user.stats.GetBaseStatValue(PlayerStats.StatType.MovementSpeed);
                newMovementSpeed = curMovementSpeed += 0.1f;
                user.stats.SetBaseStatValue(PlayerStats.StatType.MovementSpeed, newMovementSpeed, user);
                curDodgeRollSpeed = user.stats.GetBaseStatValue(PlayerStats.StatType.DodgeRollSpeedMultiplier);
                newDodgeRollSpeed = curDodgeRollSpeed += 0.025f;
                user.stats.SetBaseStatValue(PlayerStats.StatType.DodgeRollSpeedMultiplier, newDodgeRollSpeed, user);
            }
        }
        private void StartEffect(PlayerController user)
        {
            float amount = 0.15f * this.TimesUsed;
            this.damageMod = this.AddPassiveStatModifier(PlayerStats.StatType.Damage, amount, StatModifier.ModifyMethod.ADDITIVE);
            this.rateOfFireMod = this.AddPassiveStatModifier(PlayerStats.StatType.RateOfFire, amount, StatModifier.ModifyMethod.ADDITIVE);
            this.reloadSpeedMod = this.AddPassiveStatModifier(PlayerStats.StatType.ReloadSpeed, -amount, StatModifier.ModifyMethod.ADDITIVE);
            this.movementSpeedMod = this.AddPassiveStatModifier(PlayerStats.StatType.MovementSpeed, amount * 2, StatModifier.ModifyMethod.ADDITIVE);
            this.dodgeRollSpeedMod = this.AddPassiveStatModifier(PlayerStats.StatType.DodgeRollSpeedMultiplier, amount / 4, StatModifier.ModifyMethod.ADDITIVE);
            user.stats.RecalculateStats(user, true, true);
        }
        private void EndEffect(PlayerController user)
        {
            user.baseFlatColorOverride = new Color(0.4f, 0.4f, 0.33f, Mathf.Clamp01(1.0f));
            bool flag = this.damageMod == null;
            if (!flag)
            {
                this.RemovePassiveStatModifier(this.damageMod);
                this.RemovePassiveStatModifier(this.rateOfFireMod);
                this.RemovePassiveStatModifier(this.reloadSpeedMod);
                this.RemovePassiveStatModifier(this.movementSpeedMod);
                this.RemovePassiveStatModifier(this.dodgeRollSpeedMod);
                user.stats.RecalculateStats(user, true, true);
            }
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return base.CanBeUsed(user);
        }

        private StatModifier damageMod;
        private StatModifier rateOfFireMod;
        private StatModifier reloadSpeedMod;
        private StatModifier movementSpeedMod;
        private StatModifier dodgeRollSpeedMod;

        private float duration = 15f;
        private int TimesUsed;
    }
}