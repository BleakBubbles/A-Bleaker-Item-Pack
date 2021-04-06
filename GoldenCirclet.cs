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
    class GoldenCirclet : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Golden Circlet";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/golden_circlet";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<GoldenCirclet>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "NOT A Tiara";
            string longDesc = "Each shot has a chance to summon a homing ring of bullets to take down your enemies.\n\nSaid to have given the Old King telepathic powers, this circlet has been lost ever since the Old King was usurped.";
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.quality = ItemQuality.B;
        }
        private void PostProcessProjectile(Projectile proj, float x)
        {
            float num = this.activationChance;
            if (proj.PossibleSourceGun)
            {
                if (proj.PossibleSourceGun.PickupObjectId == 541)
                {
                    Projectile projectile = ((Gun)ETGMod.Databases.Items[53]).DefaultModule.projectiles[0];
                    GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.Owner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.Owner.CurrentGun == null) ? 0f : base.Owner.FacingDirection), true);
                    Projectile proj1 = obj1.GetComponent<Projectile>();
                    proj1.Owner = base.gameActor;
                    proj1.Shooter = base.Owner.specRigidbody;
                    proj1.OnHitEnemy += this.OnHitEnemy;
                    proj1.collidesWithProjectiles = true;
                    proj1.specRigidbody.OnPreRigidbodyCollision += this.HandlePreCollision;
                }
                float num2 = 1f / proj.PossibleSourceGun.DefaultModule.cooldownTime;
                if (proj.PossibleSourceGun.Volley != null && proj.PossibleSourceGun.Volley.UsesShotgunStyleVelocityRandomizer)
                {
                    num2 *= (float)Mathf.Max(1, proj.PossibleSourceGun.Volley.projectiles.Count);
                }
                num = Mathf.Clamp01(this.activationsPerSecond / num2);
                num = Mathf.Max(this.minActivationChance, num);
                if (UnityEngine.Random.value <= num)
                {
                    this.homingRing();
                }
            }
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody hitRigidBody, bool fatal)
        {
            if (proj)
            {
                proj.baseData.force = 0f;
            }
            AIActor aiActor = hitRigidBody.aiActor;
            KnockbackDoer knockbackDoer = hitRigidBody.knockbackDoer;
            if (aiActor)
            {
                if (aiActor.GetComponent<ExplodeOnDeath>())
                {
                    UnityEngine.Object.Destroy(aiActor.GetComponent<ExplodeOnDeath>());
                }
                hitRigidBody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox));
                hitRigidBody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(hitRigidBody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(this.HandleHitEnemyHitEnemy));
            }
            if (knockbackDoer && proj)
            {
                float num = -1f;
                AIActor nearestEnemyInDirection = aiActor.ParentRoom.GetNearestEnemyInDirection(aiActor.CenterPosition, proj.Direction, this.AngleTolerance, out num, true);
                Vector2 direction = proj.Direction;
                if (nearestEnemyInDirection)
                {
                    direction = nearestEnemyInDirection.CenterPosition - aiActor.CenterPosition;
                }
                knockbackDoer.ApplyKnockback(direction, this.KnockbackForce, true);
            }
        }
        private void HandleHitEnemyHitEnemy(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
        {
            if (otherRigidbody && otherRigidbody.aiActor && myRigidbody && myRigidbody.healthHaver)
            {
                AIActor aiActor = otherRigidbody.aiActor;
                myRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(myRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(this.HandleHitEnemyHitEnemy));
                if (aiActor.IsNormalEnemy && aiActor.healthHaver)
                {
                    aiActor.healthHaver.ApplyDamage(myRigidbody.healthHaver.GetMaxHealth() * 2f, myRigidbody.Velocity, "Pinball", CoreDamageTypes.None, DamageCategory.Normal, false, null, false);
                }
            }
        }
        private void HandlePreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
        {
            if (otherRigidbody && otherRigidbody.projectile)
            {
                if (otherRigidbody.projectile.Owner is AIActor)
                {
                    myRigidbody.projectile.DieInAir(false, true, true, false);
                    this.ReflectBullet(otherRigidbody.projectile, true, myRigidbody.projectile.Owner, 10f, 1f, 1f, 0f);
                }
                PhysicsEngine.SkipCollision = true;
            }
        }
        public void ReflectBullet(Projectile p, bool retargetReflectedBullet, GameActor newOwner, float minReflectedBulletSpeed, float scaleModifier = 1f, float damageModifier = 1f, float spread = 0f)
        {
            p.RemoveBulletScriptControl();
            AkSoundEngine.PostEvent("Play_OBJ_metalskin_deflect_01", GameManager.Instance.gameObject);
            bool flag = retargetReflectedBullet && p.Owner && p.Owner.specRigidbody;
            if (flag)
            {
                p.Direction = (p.Owner.specRigidbody.GetUnitCenter(ColliderType.HitBox) - p.specRigidbody.UnitCenter).normalized;
            }
            bool flag2 = spread != 0f;
            if (flag2)
            {
                p.Direction = p.Direction.Rotate(UnityEngine.Random.Range(-spread, spread));
            }
            bool flag3 = p.Owner && p.Owner.specRigidbody;
            if (flag3)
            {
                p.specRigidbody.DeregisterSpecificCollisionException(p.Owner.specRigidbody);
            }
            p.Owner = newOwner;
            p.SetNewShooter(newOwner.specRigidbody);
            p.allowSelfShooting = false;
            p.collidesWithPlayer = false;
            p.collidesWithEnemies = true;
            bool flag4 = scaleModifier != 1f;
            if (flag4)
            {
                SpawnManager.PoolManager.Remove(p.transform);
                p.RuntimeUpdateScale(scaleModifier);
            }
            bool flag5 = p.Speed < minReflectedBulletSpeed;
            if (flag5)
            {
                p.Speed = minReflectedBulletSpeed;
            }
            bool flag6 = p.baseData.damage < ProjectileData.FixedFallbackDamageToEnemies;
            if (flag6)
            {
                p.baseData.damage = ProjectileData.FixedFallbackDamageToEnemies;
            }
            p.baseData.damage *= damageModifier;
            bool flag7 = p.baseData.damage < 10f;
            if (flag7)
            {
                p.baseData.damage = 15f;
            }
            p.UpdateCollisionMask();
            p.Reflected();
        }
        private void homingRing()
        {
            Projectile projectile = ((Gun)ETGMod.Databases.Items[146]).DefaultModule.projectiles[0];
            GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.Owner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.Owner.CurrentGun == null) ? 0f : base.Owner.FacingDirection), true);
            Projectile proj1 = obj1.GetComponent<Projectile>();
            proj1.Owner = base.gameActor;
            proj1.Shooter = base.Owner.specRigidbody;
            proj1.shouldRotate = true;
            proj1.baseData.speed *= 0.15f;
            proj1.angularVelocity = -270f;
            proj1.baseData.damage *= 4f;
            PierceProjModifier poke = proj1.GetComponent<PierceProjModifier>();
            if (proj1.GetComponent<PierceProjModifier>() == null)
            {
                proj1.gameObject.AddComponent<PierceProjModifier>();
                poke.MaxBossImpacts = 3;
                poke.penetration = 6;
            }
            //proj1.specRigidbody.CollideWithTileMap = false;
            HomingModifier homingModifier = proj1.gameObject.GetComponent<HomingModifier>();
            if (homingModifier == null)
            {
                homingModifier = proj1.gameObject.AddComponent<HomingModifier>();
                homingModifier.HomingRadius = 10f;
                homingModifier.AngularVelocity = 270f;
            }
            proj1.gameObject.AddComponent<KillOnRoomClear>();
            bool flag = proj1 != null && proj1.sprite != null;
            if (flag)
            {
                proj1.sprite.renderer.enabled = false;
            }
            bool flag2 = proj1.GetComponentInChildren<TrailController>() != null;
            if (flag2)
            {
                UnityEngine.Object.Destroy(proj1.GetComponentInChildren<TrailController>());
            }
            proj1.StartCoroutine(this.DelayedDestroyParticles(proj1));
            bool flag3 = proj1.ParticleTrail != null;
            if (flag3)
            {
                BraveUtility.EnableEmission(proj1.particleSystem, false);
            }
            bool flag4 = proj1.particleSystem != null;
            if (flag4)
            {
                UnityEngine.Object.Destroy(proj1.particleSystem);
            }
            bool flag5 = proj1.CustomTrailRenderer != null;
            if (flag5)
            {
                proj1.CustomTrailRenderer.enabled = false;
            }
            bool flag6 = proj1.TrailRenderer != null;
            if (flag6)
            {
                proj1.TrailRenderer.enabled = false;
            }
            bool flag7 = proj1.TrailRendererController != null;
            if (flag7)
            {
                proj1.TrailRendererController.enabled = false;
            }
            for (int i = 0; i < 18; i++)
            {
                GameObject obj2 = SpawnManager.SpawnProjectile(projectile.gameObject, proj1.sprite.WorldCenter + BraveMathCollege.DegreesToVector(i * 20, 2), Quaternion.Euler(0f, 0f, (base.Owner.CurrentGun == null) ? 0f : i * 20f), true);
                Projectile proj2 = obj2.GetComponent<Projectile>();
                proj2.Owner = base.gameActor;
                PierceProjModifier poke2 = proj2.GetComponent<PierceProjModifier>();
                if (proj2.GetComponent<PierceProjModifier>() == null)
                {
                    proj2.gameObject.AddComponent<PierceProjModifier>();
                    poke2.MaxBossImpacts = 3;
                    poke2.penetration = 6;
                }
                //proj2.specRigidbody.CollideWithTileMap = false;
                proj2.Shooter = base.Owner.specRigidbody;
                proj2.baseData.speed = 0f;
                proj2.baseData.damage *= 4f;
                proj2.transform.SetParent(proj1.transform);
            }
        }
        public IEnumerator DelayedDestroyParticles(Projectile proj)
        {
            yield return null;
            yield break;
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.PostProcessProjectile += this.PostProcessProjectile;
            GameManager.Instance.RewardManager.SinglePlayerPickupIncrementModifier *= 100f;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            player.PostProcessProjectile -= this.PostProcessProjectile;
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            base.Owner.PostProcessProjectile -= this.PostProcessProjectile;
        }
        private float activationChance = 0.1f;
        private float activationsPerSecond = 1f;
        private float minActivationChance = 0.05f;
        public float KnockbackForce = 800f;
        public float AngleTolerance = 30f;
    }
}