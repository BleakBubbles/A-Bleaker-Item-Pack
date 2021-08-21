using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using Dungeonator;

namespace BleakMod
{
    class HealthyBullets: PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Register()
        {
            //The name of the item
            string itemName = "Healthy Bullets";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/healthy_bullets";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<HealthyBullets>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Eat Your Greens";
            string longDesc = "Enemy bullets near the player have a chance to turn green. Touch these bullets to get a damage boost while at full health or a half-heart heal while not.\n\nStudies show that eating your greens have several positive effects, including but not limited to: helth";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.quality = ItemQuality.B;
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }
        protected override void Update()
        {
            base.Update();
            if (base.Owner)
            {
                ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
                if (allProjectiles != null)
                {
                    foreach (Projectile proj in allProjectiles)
                    {
                        bool x = Vector2.Distance(proj.sprite.WorldCenter, base.Owner.sprite.WorldCenter) < 6 && proj != null && proj.sprite != null && proj.Owner != base.Owner && proj.gameObject.GetComponent<check>() == null;
                        if (x)
                        {
                            proj.gameObject.AddComponent<check>();
                            if (UnityEngine.Random.value <= 0.2f){
                                Material sharedMaterial = proj.sprite.renderer.sharedMaterial;
                                proj.sprite.usesOverrideMaterial = true;
                                Material material = new Material(ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive"));
                                material.SetTexture("_MainTex", sharedMaterial.GetTexture("_MainTex"));
                                material.SetColor("_OverrideColor", Color.green);
                                material.SetFloat("_EmissivePower", Mathf.Lerp(0, 22, 0.4f));
                                proj.sprite.renderer.material = material;
                                proj.specRigidbody.OnPreRigidbodyCollision += this.eatBullet;
                            }
                        }
                    }
                }
            }
        }
        private void eatBullet(SpeculativeRigidbody mySpec, PixelCollider myPixCo, SpeculativeRigidbody otherSpec, PixelCollider otherPixCo)
        {
            if(otherSpec.GetComponent<PlayerController>() != null)
            {
                PlayerController player = otherSpec.GetComponent<PlayerController>();
                if (mySpec.projectile)
                {
                    mySpec.projectile.DieInAir(false, true, true, false);
                    if (player.healthHaver.GetCurrentHealth() < player.healthHaver.GetMaxHealth())
                    {
                        player.healthHaver.ApplyHealing(1f);
                    }
                    else
                    {
                        base.StartCoroutine(this.tempBuff(player));
                    }
                    PhysicsEngine.SkipCollision = true;
                    mySpec.projectile.specRigidbody.OnPreRigidbodyCollision -= this.eatBullet;
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
        public class check : MonoBehaviour
        {
            private void Start()
            {

            }
        }
        private IEnumerator tempBuff(PlayerController player)
        {
            this.AddStat(PlayerStats.StatType.Damage, 0.25f);
            player.stats.RecalculateStats(player, true, false);
            yield return new WaitForSeconds(10f);
            this.RemoveStat(PlayerStats.StatType.Damage);
        }
        public override DebrisObject Drop(PlayerController player)
        {
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
    