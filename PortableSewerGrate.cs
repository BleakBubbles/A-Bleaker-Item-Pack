using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using Dungeonator;

namespace BleakMod
{
    class PortableSewerGrate : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Portable Sewer Grate";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/portable_sewer_grate";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<PortableSewerGrate>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "He Vented!";
            string longDesc = "Causes the player to seep into the ground, becoming invincible and having increased speed for a few seconds. Also shoots bullets upon seeping into the ground.\n\nStolen from the Blobulord himself, this apparatus seems to have been some sort of bed, or resting device. Either way, it's quite satisfying to use...";
            item.consumable = false;
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 150);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.quality = ItemQuality.B;
        }
        protected override void DoEffect(PlayerController user)
        {
            this.StealthEffect();
            base.StartCoroutine(this.HandleDuration());
            base.StartCoroutine(this.bounceBullets());
            base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.BreakStealth)));
        }
        private IEnumerator bounceBullets()
        {
            Projectile projectile = ((Gun)ETGMod.Databases.Items[404]).DefaultModule.projectiles[0];
            GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 0f), true);
            Projectile proj1 = obj1.GetComponent<Projectile>();
            proj1.Owner = base.LastOwner;
            proj1.Shooter = base.LastOwner.specRigidbody;
            proj1.AdditionalScaleMultiplier = 2f;
            proj1.baseData.speed = UnityEngine.Random.Range(7, 11);
            proj1.AdjustPlayerProjectileTint(new Color32(212, 58, 58, 255), 5, 0f);
            proj1.baseData.damage = 25f;
            if (proj1.GetComponent<BounceProjModifier>() == null)
            {
                proj1.gameObject.AddComponent<BounceProjModifier>();
            }
            BounceProjModifier boing1 = proj1.GetComponent<BounceProjModifier>();
            boing1.numberOfBounces = 1;
            boing1.OnBounceContext += this.BounceTowardsPlayer;
            if (proj1.GetComponent<PierceProjModifier>() == null)
            {
                proj1.gameObject.AddComponent<PierceProjModifier>();
            }
            PierceProjModifier poke1 = proj1.GetComponent<PierceProjModifier>();
            poke1.MaxBossImpacts = 3;
            poke1.penetration = 6;
            GameObject obj2 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 45f), true);
            Projectile proj2 = obj2.GetComponent<Projectile>();
            proj2.Owner = base.LastOwner;
            proj2.Shooter = base.LastOwner.specRigidbody;
            proj2.AdditionalScaleMultiplier = 2f;
            proj2.baseData.speed = UnityEngine.Random.Range(7, 11);
            proj2.AdjustPlayerProjectileTint(new Color32(212, 58, 58, 255), 5, 0f);
            proj2.baseData.damage = 25f;
            if (proj2.GetComponent<BounceProjModifier>() == null)
            {
                proj2.gameObject.AddComponent<BounceProjModifier>();
            }
            BounceProjModifier boing2 = proj2.GetComponent<BounceProjModifier>();
            boing2.numberOfBounces = 1;
            boing2.OnBounceContext += this.BounceTowardsPlayer;
            if (proj2.GetComponent<PierceProjModifier>() == null)
            {
                proj2.gameObject.AddComponent<PierceProjModifier>();
            }
            PierceProjModifier poke2 = proj2.GetComponent<PierceProjModifier>();
            poke2.MaxBossImpacts = 3;
            poke2.penetration = 6;
            GameObject obj3 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 90f), true);
            Projectile proj3 = obj3.GetComponent<Projectile>();
            proj3.Owner = base.LastOwner;
            proj3.Shooter = base.LastOwner.specRigidbody;
            proj3.AdditionalScaleMultiplier = 2f;
            proj3.baseData.speed = UnityEngine.Random.Range(7, 11);
            proj3.AdjustPlayerProjectileTint(new Color32(212, 58, 58, 255), 5, 0f);
            proj3.baseData.damage = 25f;
            if (proj3.GetComponent<BounceProjModifier>() == null)
            {
                proj3.gameObject.AddComponent<BounceProjModifier>();
            }
            BounceProjModifier boing3 = proj3.GetComponent<BounceProjModifier>();
            boing3.numberOfBounces = 1;
            boing3.OnBounceContext += this.BounceTowardsPlayer;
            if (proj3.GetComponent<PierceProjModifier>() == null)
            {
                proj3.gameObject.AddComponent<PierceProjModifier>();
            }
            PierceProjModifier poke3 = proj3.GetComponent<PierceProjModifier>();
            poke3.MaxBossImpacts = 3;
            poke3.penetration = 6;
            GameObject obj4 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 135f), true);
            Projectile proj4 = obj4.GetComponent<Projectile>();
            proj4.Owner = base.LastOwner;
            proj4.Shooter = base.LastOwner.specRigidbody;
            proj4.AdditionalScaleMultiplier = 2f;
            proj4.baseData.speed = UnityEngine.Random.Range(7, 11);
            proj4.AdjustPlayerProjectileTint(new Color32(212, 58, 58, 255), 5, 0f);
            proj4.baseData.damage = 25f;
            if (proj4.GetComponent<BounceProjModifier>() == null)
            {
                proj4.gameObject.AddComponent<BounceProjModifier>();
            }
            BounceProjModifier boing4 = proj4.GetComponent<BounceProjModifier>();
            boing4.numberOfBounces = 1;
            boing4.OnBounceContext += this.BounceTowardsPlayer;
            if (proj4.GetComponent<PierceProjModifier>() == null)
            {
                proj4.gameObject.AddComponent<PierceProjModifier>();
            }
            PierceProjModifier poke4 = proj4.GetComponent<PierceProjModifier>();
            poke4.MaxBossImpacts = 3;
            poke4.penetration = 6;
            GameObject obj5 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 180f), true);
            Projectile proj5 = obj5.GetComponent<Projectile>();
            proj5.Owner = base.LastOwner;
            proj5.Shooter = base.LastOwner.specRigidbody;
            proj5.AdditionalScaleMultiplier = 2f;
            proj5.baseData.speed = UnityEngine.Random.Range(7, 11);
            proj5.AdjustPlayerProjectileTint(new Color32(212, 58, 58, 255), 5, 0f);
            proj5.baseData.damage = 25f;
            if (proj5.GetComponent<BounceProjModifier>() == null)
            {
                proj5.gameObject.AddComponent<BounceProjModifier>();
            }
            BounceProjModifier boing5 = proj5.GetComponent<BounceProjModifier>();
            boing5.numberOfBounces = 1;
            boing5.OnBounceContext += this.BounceTowardsPlayer;
            if (proj5.GetComponent<PierceProjModifier>() == null)
            {
                proj5.gameObject.AddComponent<PierceProjModifier>();
            }
            PierceProjModifier poke5 = proj5.GetComponent<PierceProjModifier>();
            poke5.MaxBossImpacts = 3;
            poke5.penetration = 6;
            GameObject obj6 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 225f), true);
            Projectile proj6 = obj6.GetComponent<Projectile>();
            proj6.Owner = base.LastOwner;
            proj6.Shooter = base.LastOwner.specRigidbody;
            proj6.AdditionalScaleMultiplier = 2f;
            proj6.baseData.speed = UnityEngine.Random.Range(7, 11);
            proj6.AdjustPlayerProjectileTint(new Color32(212, 58, 58, 255), 5, 0f);
            proj6.baseData.damage = 25f;
            if (proj6.GetComponent<BounceProjModifier>() == null)
            {
                proj6.gameObject.AddComponent<BounceProjModifier>();
            }
            BounceProjModifier boing6 = proj6.GetComponent<BounceProjModifier>();
            boing6.numberOfBounces = 1;
            boing6.OnBounceContext += this.BounceTowardsPlayer;
            if (proj6.GetComponent<PierceProjModifier>() == null)
            {
                proj6.gameObject.AddComponent<PierceProjModifier>();
            }
            PierceProjModifier poke6 = proj6.GetComponent<PierceProjModifier>();
            poke6.MaxBossImpacts = 3;
            poke6.penetration = 6;
            GameObject obj7 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 270f), true);
            Projectile proj7 = obj7.GetComponent<Projectile>();
            proj7.Owner = base.LastOwner;
            proj7.Shooter = base.LastOwner.specRigidbody;
            proj7.AdditionalScaleMultiplier = 2f;
            proj7.baseData.speed = UnityEngine.Random.Range(7, 11);
            proj7.AdjustPlayerProjectileTint(new Color32(212, 58, 58, 255), 5, 0f);
            proj7.baseData.damage = 25f;
            if (proj7.GetComponent<BounceProjModifier>() == null)
            {
                proj7.gameObject.AddComponent<BounceProjModifier>();
            }
            BounceProjModifier boing7 = proj7.GetComponent<BounceProjModifier>();
            boing7.numberOfBounces = 1;
            boing7.OnBounceContext += this.BounceTowardsPlayer;
            if (proj7.GetComponent<PierceProjModifier>() == null)
            {
                proj7.gameObject.AddComponent<PierceProjModifier>();
            }
            PierceProjModifier poke7 = proj7.GetComponent<PierceProjModifier>();
            poke7.MaxBossImpacts = 3;
            poke7.penetration = 6;
            GameObject obj8 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 315f), true);
            Projectile proj8 = obj8.GetComponent<Projectile>();
            proj8.Owner = base.LastOwner;
            proj8.Shooter = base.LastOwner.specRigidbody;
            proj8.AdditionalScaleMultiplier = 2f;
            proj8.baseData.speed = UnityEngine.Random.Range(7, 11);
            proj8.AdjustPlayerProjectileTint(new Color32(212, 58, 58, 255), 5, 0f);
            proj8.baseData.damage = 25f;
            if (proj8.GetComponent<BounceProjModifier>() == null)
            {
                proj8.gameObject.AddComponent<BounceProjModifier>();
            }
            BounceProjModifier boing8 = proj8.GetComponent<BounceProjModifier>();
            boing8.numberOfBounces = 1;
            boing8.OnBounceContext += this.BounceTowardsPlayer;
            if (proj8.GetComponent<PierceProjModifier>() == null)
            {
                proj8.gameObject.AddComponent<PierceProjModifier>();
            }
            PierceProjModifier poke8 = proj8.GetComponent<PierceProjModifier>();
            poke1.MaxBossImpacts = 3;
            poke1.penetration = 6;
            yield return new WaitForSeconds(4);
            //boing1.OnBounceContext -= this.BounceTowardsPlayer;
            //proj1.specRigidbody.CollideWithTileMap = false;
            proj1.collidesWithPlayer = true;
            proj1.SendInDirection(base.LastOwner.specRigidbody.UnitCenter - proj1.specRigidbody.UnitCenter, true);
            proj1.baseData.speed = 10f;
            proj1.UpdateSpeed();
            //boing2.OnBounceContext -= this.BounceTowardsPlayer;
            //proj2.specRigidbody.CollideWithTileMap = false;
            proj2.collidesWithPlayer = true;
            proj2.SendInDirection(base.LastOwner.specRigidbody.UnitCenter - proj2.specRigidbody.UnitCenter, true);
            proj2.baseData.speed = 10f;
            proj2.UpdateSpeed();
            //boing3.OnBounceContext -= this.BounceTowardsPlayer;
            //proj3.specRigidbody.CollideWithTileMap = false;
            proj3.collidesWithPlayer = true;
            proj3.SendInDirection(base.LastOwner.specRigidbody.UnitCenter - proj3.specRigidbody.UnitCenter, true);
            proj3.baseData.speed = 10f;
            proj3.UpdateSpeed();
            //boing4.OnBounceContext -= this.BounceTowardsPlayer;
            //proj4.specRigidbody.CollideWithTileMap = false;
            proj4.collidesWithPlayer = true;
            proj4.SendInDirection(base.LastOwner.specRigidbody.UnitCenter - proj4.specRigidbody.UnitCenter, true);
            proj4.baseData.speed = 10f;
            proj4.UpdateSpeed();
            //boing5.OnBounceContext -= this.BounceTowardsPlayer;
            //proj5.specRigidbody.CollideWithTileMap = false;
            proj5.collidesWithPlayer = true;
            proj5.SendInDirection(base.LastOwner.specRigidbody.UnitCenter - proj5.specRigidbody.UnitCenter, true);
            proj5.baseData.speed = 10f;
            proj5.UpdateSpeed();
            //boing6.OnBounceContext -= this.BounceTowardsPlayer;
            //proj6.specRigidbody.CollideWithTileMap = false;
            proj6.collidesWithPlayer = true;
            proj6.SendInDirection(base.LastOwner.specRigidbody.UnitCenter - proj6.specRigidbody.UnitCenter, true);
            proj6.baseData.speed = 10f;
            proj6.UpdateSpeed();
            //boing7.OnBounceContext -= this.BounceTowardsPlayer;
            //proj7.specRigidbody.CollideWithTileMap = false;
            proj7.collidesWithPlayer = true;
            proj7.SendInDirection(base.LastOwner.specRigidbody.UnitCenter - proj7.specRigidbody.UnitCenter, true);
            proj7.baseData.speed = 10f;
            proj7.UpdateSpeed();
            //boing8.OnBounceContext -= this.BounceTowardsPlayer;
            //proj8.specRigidbody.CollideWithTileMap = false;
            proj8.collidesWithPlayer = true;
            proj8.SendInDirection(base.LastOwner.specRigidbody.UnitCenter - proj8.specRigidbody.UnitCenter, true);
            proj8.baseData.speed = 10f;
            proj8.UpdateSpeed();
        }
        private IEnumerator HandleDuration()
        {
            if(base.LastOwner)
            {
                if (base.LastOwner.sprite)
                {
                    for(int i = 3; i > 0; i--)
                    {
                        base.LastOwner.sprite.transform.localScale *= (i * 0.25f);
                        yield return new WaitForSeconds(0.33f);
                    }
                }
                StatModifier item = new StatModifier
                {
                    statToBoost = PlayerStats.StatType.MovementSpeed,
                    amount = 4f,
                    modifyType = StatModifier.ModifyMethod.ADDITIVE
                };
                base.LastOwner.ownerlessStatModifiers.Add(item);
                base.LastOwner.stats.RecalculateStats(base.LastOwner, true, false);
                yield return new WaitForSeconds(3);
                if (base.LastOwner.sprite)
                {
                    for (int i = 1; i < 4; i++)
                    {
                        base.LastOwner.sprite.transform.localScale *= (1/(i * 0.25f));
                        yield return new WaitForSeconds(0.33f);
                    }
                }
                base.LastOwner.ownerlessStatModifiers.Remove(item);
                base.LastOwner.stats.RecalculateStats(base.LastOwner, true, false);
            }
        }
        private void StealthEffect()
        {
            PlayerController owner = base.LastOwner;
            owner.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
            owner.ToggleShadowVisiblity(false);
            owner.ToggleGunRenderers(false, "vent");
            //if (owner.CurrentGun.sprite)
            //{
            //    owner.CurrentGun.sprite.renderer.enabled = false;
            //}
            //this.BreakStealth(owner);
            //owner.SetIsStealthed(true, "vent");
            owner.healthHaver.IsVulnerable = false;
            owner.IsGunLocked = true;
        }
        private void BreakStealth(PlayerController player)
        {
            player.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
            player.ToggleShadowVisiblity(true);
            player.ToggleGunRenderers(true, "vent");
            //player.SetIsStealthed(false, "vent");
            //if (player.CurrentGun.sprite)
            //{
            //    player.CurrentGun.sprite.renderer.enabled = true;
            //}
            player.healthHaver.IsVulnerable = true;
            player.IsGunLocked = false;
            AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", base.gameObject);
        }
        private void BounceTowardsPlayer(BounceProjModifier bouncer, SpeculativeRigidbody body)
        {
            Projectile proj = bouncer.GetComponent<Projectile>();
            proj.baseData.speed = 0f;
            proj.UpdateSpeed();
            bouncer.OnBounceContext -= this.BounceTowardsPlayer;
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
        }
        protected override void OnPreDrop(PlayerController user)
        {
            if (this.m_isCurrentlyActive)
            {
                this.BreakStealth(user);
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.m_isCurrentlyActive)
            {
                this.BreakStealth(base.LastOwner);
            }
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        private float duration = 4f;
        //private float max;
    }
}