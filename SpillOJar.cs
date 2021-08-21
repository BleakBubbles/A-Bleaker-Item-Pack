using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using System.Reflection;
using MultiplayerBasicExample;
using Dungeonator;

namespace BleakMod
{
    class SpillOJar: PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Spill-O' Jar";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/spill_o'_tar";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<SpillOJar>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "The Underscorer_";
            string longDesc = "Spills goop in a line in front of the user. standing in the goop provides a damage and rate of fire boost.\n\nA rogue blobulon refitted into the blobulonian army by trapping it inside a jar. The result? a goop spiller of mass destruction.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 50f);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.numberOfUses = 3;
            item.UsesNumberOfUsesBeforeCooldown = true;
            item.consumable = false;
            item.quality = ItemQuality.A;

            SpillOJar.goopDef = new GoopDefinition();
            AssetBundle assetBundle = ResourceManager.LoadAssetBundle("shared_auto_001");
            GoopDefinition goopDefinition;
            try
            {
                GameObject gameObject2 = assetBundle.LoadAsset(SpillOJar.goop) as GameObject;
                goopDefinition = gameObject2.GetComponent<GoopDefinition>();
            }
            catch
            {
                goopDefinition = (assetBundle.LoadAsset(SpillOJar.goop) as GoopDefinition);
            }
            goopDefinition.name = SpillOJar.goop.Replace("assets/data/goops/", "").Replace(".asset", "");
            SpillOJar.goopDef = goopDefinition;
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnRoomClearEvent += this.OnRoomClear;
        }
        protected override void OnPreDrop(PlayerController user)
        {
            base.OnPreDrop(user);
            user.OnRoomClearEvent -= this.OnRoomClear;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.LastOwner != null)
            {
                base.LastOwner.OnRoomClearEvent -= this.OnRoomClear;
            }
        }
        private void OnRoomClear(PlayerController user)
        {
            this.position = Vector2.zero;
        }
        protected override void DoEffect(PlayerController user)
        {
            if(this.numberOfUses == 3 || this.position == Vector2.zero)
            {
                this.position = user.sprite.WorldCenter;
            }
            DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(SpillOJar.goopDef).TimedAddGoopLine(this.position, this.position + BraveMathCollege.DegreesToVector(user.FacingDirection, 8f), 2f, 0.75f);
            this.position += BraveMathCollege.DegreesToVector(user.FacingDirection, 6);
        }
        public override void Update()
        {
            base.Update();
            Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(base.LastOwner.sprite);
            if (base.LastOwner.CurrentGoop != null)
            {
                this.currentGoop = base.LastOwner.CurrentGoop == EasyGoopDefinitions.BlobulonGoopDef ? true : false;
            }
            else
            {
                this.currentGoop = false;
            }
            if (currentGoop != lastGoop && outlineMaterial != null)
            {
                this.RemoveStat(PlayerStats.StatType.Damage);
                this.DisableVFX(base.LastOwner);
                if (currentGoop)
                {
                    this.AddStat(PlayerStats.StatType.Damage, 0.5f, StatModifier.ModifyMethod.ADDITIVE);
                    outlineMaterial.SetColor("_OverrideColor", new Color(99f, 99f, 0f));
                }
            }
            base.LastOwner.stats.RecalculateStats(base.LastOwner, true, false);
            this.lastGoop = this.currentGoop;
        }
        private void AddStat(PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            StatModifier statModifier = new StatModifier();
            statModifier.amount = amount;
            statModifier.statToBoost = statType;
            statModifier.modifyType = method;
            foreach (StatModifier statModifier2 in this.passiveStatModifiers)
            {
                bool flag = statModifier2.statToBoost == statType;
                if (flag)
                {
                    return;
                }
            }
            bool flag2 = this.passiveStatModifiers == null;
            if (flag2)
            {
                this.passiveStatModifiers = new StatModifier[]
                {
                    statModifier
                };
                return;
            }
            this.passiveStatModifiers = this.passiveStatModifiers.Concat(new StatModifier[]
            {
                statModifier
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
        private void DisableVFX(PlayerController user)
        {
            if (user)
            {
                Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(user.sprite);
                outlineMaterial.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
            }
        }
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        public static GoopDefinition goopDef;
        private static string goop = "assets/data/goops/blobulongoop.asset";
        bool currentGoop = false;
        bool lastGoop = false;
        Vector2 position = Vector2.zero;
    }
}