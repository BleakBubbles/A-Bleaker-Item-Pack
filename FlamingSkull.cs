using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using System.Runtime.CompilerServices;

namespace BleakMod
{
    public class FlamingSkull : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Flaming Skull";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/flaming_skull";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<FlamingSkull>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Why? 'Curse why not?";
            string longDesc = "If above a certain curse, doubles your curse but prevents it from being raised any further.\n\n" +
                "You can tell that this scorching artifact is extremely cursed, but you sense that it might be able to help you in some way...\n\n" +
                "Note: If the Lord of the Jammed spawns while you are holding this item, he will go away once you reach the next floor.";
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            item.quality = PickupObject.ItemQuality.D;
            item.AddToSubShop(ItemBuilder.ShopType.Cursula, 1f);
            CustomSynergies.Add("Great Skulls of Fire", new List<string>
            {
                "bb:flaming_skull"
            }, new List<string>
            {
                "pitchfork",
                "demon_head",
                "flame_hand",
                "gunner",
                "skull_spitter"
            }, true);
        }
        public override void Pickup(PlayerController player)
        {
            this.m_fireImmunity = new DamageTypeModifier();
            this.m_fireImmunity.damageMultiplier = 0f;
            this.m_fireImmunity.damageType = CoreDamageTypes.Fire;
            player.healthHaver.damageTypeModifiers.Add(this.m_fireImmunity);
            base.Pickup(player);
            if (player.stats.GetStatValue(PlayerStats.StatType.Curse) != 0)
            {
                this.newCurse = player.stats.GetStatValue(PlayerStats.StatType.Curse) * 2f;
                this.RemoveStat(PlayerStats.StatType.Curse);
                this.AddStat(PlayerStats.StatType.Curse, newCurse);
                player.stats.RecalculateStats(this.m_owner, true, false);
            }
            else
            {
                this.newCurse = player.stats.GetStatValue(PlayerStats.StatType.Curse) + 5f;
                this.RemoveStat(PlayerStats.StatType.Curse);
                this.AddStat(PlayerStats.StatType.Curse, newCurse);
                player.stats.RecalculateStats(this.m_owner, true, false);
            }
            this.synergizedCurse = (int)(this.newCurse / 2);
            this.origCurse = this.newCurse;
            this.curseToAdd = 2 * this.newCurse;
            this.initiatedCurse = true;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            player.healthHaver.damageTypeModifiers.Remove(this.m_fireImmunity);
            DebrisObject debrisObject = base.Drop(player);
            return debrisObject;
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
        protected override void Update()
        {
            base.Update();
            if (this.m_pickedUp == true && this.m_owner != null)
            {
                base.Update();
                this.curse = this.m_owner.stats.GetStatValue(PlayerStats.StatType.Curse);
                if((this.m_owner.HasMTGConsoleID("pitchfork") || this.m_owner.HasMTGConsoleID("demon_head") || this.m_owner.HasMTGConsoleID("flame_hand") || this.m_owner.HasMTGConsoleID("gunner") || this.m_owner.HasMTGConsoleID("skull_spitter")) && this.newCurse != this.synergizedCurse)
                {
                    this.newCurse = this.synergizedCurse;
                }
                else if(!(this.m_owner.HasMTGConsoleID("pitchfork") || this.m_owner.HasMTGConsoleID("demon_head") || this.m_owner.HasMTGConsoleID("flame_hand") || this.m_owner.HasMTGConsoleID("gunner") || this.m_owner.HasMTGConsoleID("skull_spitter")) && this.newCurse != this.origCurse)
                {
                    this.newCurse = this.origCurse;
                }
                if (this.curse != this.newCurse && this.initiatedCurse)
                {
                    this.curseToAdd = 2 * this.newCurse;
                    this.curseToAdd -= this.curse;
                    this.curseToAdd -= this.curseToChange;
                    this.RemoveStat(PlayerStats.StatType.Curse);
                    this.m_owner.stats.SetBaseStatValue(PlayerStats.StatType.Curse, curseToAdd, this.m_owner);
                    this.curseToChange += this.curse - this.newCurse;
                    ETGModConsole.Log("The player has " + this.m_owner.stats.GetStatValue(PlayerStats.StatType.Curse) + " curse.");
                }
            }
        }
        private bool initiatedCurse = false;
        private float curseToChange = 0;
        private float curse;
        private float newCurse;
        private float origCurse;
        private float synergizedCurse;
        private float curseToAdd;
        private DamageTypeModifier m_fireImmunity;
    }
}
