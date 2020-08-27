using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;

namespace BleakMod
{
    public class Carrot: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Carrot";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/carrot";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Carrot>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Mom was right!";
            string longDesc = "Increases range of sight, damage, projectile speed, and projectile range.\n\nThe carrot is a plant whose typically orange roots are eaten as a vegetable.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
   
            //Adds the actual passive effect to the item
            

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.A;
            item.AddPassiveStatModifier(PlayerStats.StatType.ProjectileSpeed, 0.33f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.RangeMultiplier, 1.33f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            CustomSynergies.Add("Eat Your Veggies", new List<string>
            {
                "bb:carrot"
            }, new List<string>
            {
                "broccoli",
                "blasphemy_alt"
            }, true);
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
            this.currentItems = base.m_owner.passiveItems.Count;
            if (this.currentItems != this.lastItems)
            {
                if (base.m_owner.HasMTGConsoleID("broccoli") || base.m_owner.HasMTGConsoleID("blasphemy_alt"))
                {
                    this.handleDamage(true);
                }
                else
                {
                    this.handleDamage(false);
                }
                this.lastItems = this.currentItems;
            }
        }
        private void handleDamage(bool hasSynergy)
        {
            this.RemoveStat(PlayerStats.StatType.Damage);
            if (hasSynergy)
            {
                this.AddStat(PlayerStats.StatType.Damage, 0.33f);
            }
            else
            {
                this.AddStat(PlayerStats.StatType.Damage, 0.1f);
            }
            base.m_owner.stats.RecalculateStats(base.m_owner, true, false);
        }
        public override void Pickup(PlayerController player)
        {
            GameManager.Instance.MainCameraController.OverrideZoomScale = 0.66f;
            base.Pickup(player);
        }
        public override DebrisObject Drop(PlayerController player)
        {
            GameManager.Instance.MainCameraController.OverrideZoomScale = 1f;
            DebrisObject debrisObject = base.Drop(player);
            debrisObject.GetComponent<Carrot>().m_pickedUpThisRun = true;
            return debrisObject;
        }
        public int currentItems;
        public int lastItems;
    }
}
