    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;

namespace BleakMod
{
    public class FriendshipBracelet: PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Friendship Bracelet";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/friendship_bracelet";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<FriendshipBracelet>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Teamwork makes the dream work";
            string longDesc = "Gives you strength for every companion that fights alongside you.\n\n" +
                "So this is that \"power of friendship\" you see all the time in movies.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
            CustomSynergies.Add("Power Of Friendship", new List<string>
            {
                "bb:friendship_bracelet"
            }, new List<string>
            {
                "potion_of_gun_friendship",
                "ring_of_chest_friendship",
                "ring_of_mimic_friendship",
                "space_friend"
            }, true);
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            this.EvaluateStats(player, true);
        }
        public override DebrisObject Drop(PlayerController player)
        {
            this.EvaluateStats(player, true);
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.Owner != null)
            {
                this.EvaluateStats(base.Owner, true);
            }
        }
        protected override void Update()
        {
            bool flag = base.Owner;
            if (flag)
            {
                this.EvaluateStats(base.Owner, false);
            }
        }
        private void EvaluateStats(PlayerController player, bool force = false)
        {
            this.currentCompanions = player.companions.Count;
            if (player.HasMTGConsoleID("potion_of_gun_friendship") || player.HasMTGConsoleID("ring_of_chest_friendship") || player.HasMTGConsoleID("ring_of_mimic_friendship") || player.HasMTGConsoleID("space_friend"))
            {
                damageBoost = 0.4f * player.companions.Count;
            }
            else
            {
                damageBoost = 0.2f * player.companions.Count;
            }
            this.shouldRestat = this.currentCompanions != this.lastCompanions;
            bool flag = !this.shouldRestat && !force;
            bool flag2 = !flag;
            if (flag2)
            {
                this.RemoveStat(PlayerStats.StatType.Damage);
                this.AddStat(PlayerStats.StatType.Damage, damageBoost, StatModifier.ModifyMethod.ADDITIVE);
                this.lastCompanions = this.currentCompanions;
                player.stats.RecalculateStats(player, true, false);
            }
        }
        private void AddStat(PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            foreach (StatModifier statModifier in this.passiveStatModifiers)
            {
                bool flag = statModifier.statToBoost == statType;
                if (flag)
                {
                    return;
                }
            }
            StatModifier statModifier2 = new StatModifier
            {
                amount = amount,
                statToBoost = statType,
                modifyType = method
            };
            bool flag2 = this.passiveStatModifiers == null;
            if (flag2)
            {
                this.passiveStatModifiers = new StatModifier[]
                {
                    statModifier2
                };
                return;
            }
            this.passiveStatModifiers = this.passiveStatModifiers.Concat(new StatModifier[]
            {
                statModifier2
            }).ToArray<StatModifier>();
        }
        private void RemoveStat(PlayerStats.StatType statType)
        {
            List<StatModifier> list = new List<StatModifier>();
            for (int i = 0; i < this.passiveStatModifiers.Length; i++)
            {
                bool flag = this.passiveStatModifiers[i].statToBoost != statType;
                if (flag)
                {
                    list.Add(this.passiveStatModifiers[i]);
                }
            }
            this.passiveStatModifiers = list.ToArray();
        }
        public List<int> ListOfCompanions = new List<int>
        {
            818,
            249,
            442,
            461,
            451,
            491,
            318,
            580,
            664,
            572,
            607,
            232,
            301,
            492,
            632,
            645,
            300
        };
        public int currentCompanions;
        public int lastCompanions;
        public float damageBoost;
        public bool shouldRestat;
    }
}