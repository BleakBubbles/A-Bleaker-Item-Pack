using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

namespace BleakMod
{
    public class Bleaker: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Bleaker";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/bleaker";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Bleaker>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Flask Bullets";
            string longDesc = "Grants immunity to the elements, and makes enemies spill a random goop when killed. Standing in a goop now increases a certain stat, depending on the goop.\n\n" +
                "A bleak, bubbling container of liquids unknown to man. These liquids seem to be alive, churning and changing every second... and they seem to like you.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            Bleaker.goopDefs = new List<GoopDefinition>();
            AssetBundle assetBundle = ResourceManager.LoadAssetBundle("shared_auto_001");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.S;
            item.AddToSubShop(ItemBuilder.ShopType.Goopton, 1f);

            foreach (string text in Bleaker.goops)
            {
                GoopDefinition goopDefinition;
                try
                {
                    GameObject gameObject2 = assetBundle.LoadAsset(text) as GameObject;
                    goopDefinition = gameObject2.GetComponent<GoopDefinition>();
                }
                catch
                {
                    goopDefinition = (assetBundle.LoadAsset(text) as GoopDefinition);
                }
                goopDefinition.name = text.Replace("assets/data/goops/", "").Replace(".asset", "");
                Bleaker.goopDefs.Add(goopDefinition);
            }
            Bleaker.goopDefs.Add(Bleaker.cheeseGoop);
        }
        private void OnEnemyKilled(PlayerController player, HealthHaver enemy)
        {
            bool flag = enemy.specRigidbody != null && enemy.aiActor != null && base.Owner != null;
            if (flag)
            {
                PickupObject byId = PickupObjectDatabase.GetById(310);
                bool flag2 = byId == null;
                if (!flag2)
                {
                    WingsItem component = byId.GetComponent<WingsItem>();
                    GoopDefinition goopDefinition = (component != null) ? component.RollGoop : null;
                }
                float duration = 0.75f;
                DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(Bleaker.goopDefs[UnityEngine.Random.Range(0, 3)]).TimedAddGoopCircle(enemy.specRigidbody.UnitCenter, 5f, duration, false);
            }
        }
        protected override void Update()
        {
            base.Update();
            if (this.m_owner)
            {
                this.goopBoost();
                for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
                {
                    StaticReferenceManager.AllGoops[i].ElectrifyGoopCircle(this.m_owner.specRigidbody.UnitBottomCenter, 5f);
                }
            }
        }
        private void goopBoost()
        {
            Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(base.Owner.sprite);
            goop = base.m_owner.CurrentGoop;
            bool flag1 = this.goop == this.goopLast;
            if (!flag1)
            {
                this.RemoveStat(PlayerStats.StatType.Damage);
                this.RemoveStat(PlayerStats.StatType.RateOfFire);
                this.RemoveStat(PlayerStats.StatType.ReloadSpeed);
                this.RemoveStat(PlayerStats.StatType.AdditionalShotPiercing);
                this.RemoveStat(PlayerStats.StatType.EnemyProjectileSpeedMultiplier);
                this.DisableVFX(base.Owner);
                bool flag2 = this.goop == goopDefs[0];
                if (flag2)
                {
                    this.AddStat(PlayerStats.StatType.Damage, 1.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    outlineMaterial.SetColor("_OverrideColor", new Color(255f, 0f, 0f, 50f));
                }
                bool flag3 = this.goop == goopDefs[1];
                if (flag3)
                {
                    this.AddStat(PlayerStats.StatType.ReloadSpeed, 0.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    outlineMaterial.SetColor("_OverrideColor", new Color(0f, 128f, 0f, 50f));
                }
                bool flag4 = this.goop == goopDefs[2];
                if (flag4)
                {
                    this.AddStat(PlayerStats.StatType.RateOfFire, 1.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    outlineMaterial.SetColor("_OverrideColor", new Color(0f, 0f, 255f, 50f));
                }
                bool flag5 = this.goop == goopDefs[3];
                if (flag5)
                {
                    this.AddStat(PlayerStats.StatType.AdditionalShotPiercing, 1f, StatModifier.ModifyMethod.ADDITIVE);
                    outlineMaterial.SetColor("_OverrideColor", new Color32(212, 58, 58, 255));
                }
                bool flag6 = this.goop == goopDefs[4];
                if (flag6)
                {
                    this.AddStat(PlayerStats.StatType.EnemyProjectileSpeedMultiplier, 0.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    outlineMaterial.SetColor("_OverrideColor", new Color(255f, 255f, 0f, 50f));
                }
                base.Owner.stats.RecalculateStats(base.Owner, true, false);
                this.goopLast = this.goop;
            }
        }
        private void PostProcessProjectile(Projectile proj, float f)
        {
            if (base.m_owner.CurrentGoop == goopDefs[3])
            {
                HomingModifier homingModifier = proj.gameObject.GetComponent<HomingModifier>();
                if (homingModifier == null)
                {
                    homingModifier = proj.gameObject.AddComponent<HomingModifier>();
                    homingModifier.HomingRadius = 15;
                    homingModifier.AngularVelocity = 100;
                }
            }
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
            Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(user.sprite);
            outlineMaterial.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
        }
        public override void Pickup(PlayerController player)
        {
            this.m_fireImmunity = new DamageTypeModifier();
            this.m_fireImmunity.damageMultiplier = 0f;
            this.m_fireImmunity.damageType = CoreDamageTypes.Fire;
            player.healthHaver.damageTypeModifiers.Add(this.m_fireImmunity);
            this.m_poisonImmunity = new DamageTypeModifier();
            this.m_poisonImmunity.damageMultiplier = 0f;
            this.m_poisonImmunity.damageType = CoreDamageTypes.Poison;
            player.healthHaver.damageTypeModifiers.Add(this.m_poisonImmunity);
            this.m_electricImmunity = new DamageTypeModifier();
            this.m_electricImmunity.damageMultiplier = 0f;
            this.m_electricImmunity.damageType = CoreDamageTypes.Electric;
            player.healthHaver.damageTypeModifiers.Add(this.m_electricImmunity);
            base.Pickup(player);
            player.OnKilledEnemyContext += this.OnEnemyKilled;
            player.PostProcessProjectile += this.PostProcessProjectile;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            if(this.m_fireImmunity != null)
            {
                player.healthHaver.damageTypeModifiers.Remove(this.m_fireImmunity);
            }
            if(this.m_poisonImmunity != null)
            {
                player.healthHaver.damageTypeModifiers.Remove(this.m_poisonImmunity);
            }
            if (this.m_electricImmunity != null)
            {
                player.healthHaver.damageTypeModifiers.Remove(this.m_electricImmunity);
            }
            player.OnKilledEnemyContext -= this.OnEnemyKilled;
            player.PostProcessProjectile -= this.PostProcessProjectile;
            return base.Drop(player);
        }
        private DamageTypeModifier m_fireImmunity;
        private DamageTypeModifier m_poisonImmunity;
        private DamageTypeModifier m_electricImmunity;
        private static string[] goops = new string[]
        {
            "assets/data/goops/napalmgoopthatworks.asset",
            "assets/data/goops/poison goop.asset",
            "assets/data/goops/water goop.asset",
            "assets/data/goops/blobulongoop.asset",
    };
        public static List<GoopDefinition> goopDefs;
        GoopDefinition goop;
        GoopDefinition goopLast = goopDefs[-1];
        public static GoopDefinition cheeseGoop = (PickupObjectDatabase.GetById(626) as Gun).DefaultModule.projectiles[0].cheeseEffect.CheeseGoop;
    }
}