using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;

namespace BleakMod
{
    public class ShadesShades : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Init()
        {
            //The name of the item
            string itemName = "Shades's Shades";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/shades'_shades";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<ShadesShades>();
			

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Some Glasses";
            string longDesc = "Does everything that a Trigger Twin can do.\n\n" +
                "Shades' prized possession, his namesake, his everything. Putting them on makes you feel... cool.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.A;
            item.AddPassiveStatModifier(PlayerStats.StatType.Coolness, 2f, StatModifier.ModifyMethod.ADDITIVE);
            item.consumable = false;
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 250);
		}
        public override void Update()
        {
            base.Update();
            this.currentDodgeState = base.LastOwner.CurrentRollState;
            if (this.currentDodgeState == PlayerController.DodgeRollState.OnGround && lastDodgeState != PlayerController.DodgeRollState.OnGround)
            {
                this.handleEndOfRoll();
            }
            this.lastDodgeState = this.currentDodgeState;
        }
        private void handleEndOfRoll()
        {
            Projectile projectile = ((Gun)ETGMod.Databases.Items[22]).DefaultModule.projectiles[0];
            GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 0f), true);
            GameObject gameObject2 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 45f), true);
            GameObject gameObject3 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 90f), true);
            GameObject gameObject4 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 135f), true);
            GameObject gameObject5 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 180f), true);
            GameObject gameObject6 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 225f), true);
            GameObject gameObject7 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 270f), true);
            GameObject gameObject8 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 315f), true);
            Projectile component = gameObject.GetComponent<Projectile>();
            Projectile component2 = gameObject2.GetComponent<Projectile>();
            Projectile component3 = gameObject3.GetComponent<Projectile>();
            Projectile component4 = gameObject4.GetComponent<Projectile>();
            Projectile component5 = gameObject5.GetComponent<Projectile>();
            Projectile component6 = gameObject6.GetComponent<Projectile>();
            Projectile component7 = gameObject7.GetComponent<Projectile>();
            Projectile component8 = gameObject8.GetComponent<Projectile>();
            if(component != null)
            {
                component.Owner = base.LastOwner;
                component.Shooter = base.LastOwner.specRigidbody;
                component.baseData.damage *= 2;
                component.baseData.speed *= 0.75f;
            }
            if(component2 != null)
            {
                component2.Owner = base.LastOwner;
                component2.Shooter = base.LastOwner.specRigidbody;
                component2.baseData.damage *= 2;
                component2.baseData.speed *= 0.75f;
            }
            if (component3 != null)
            {
                component3.Owner = base.LastOwner;
                component3.Shooter = base.LastOwner.specRigidbody;
                component3.baseData.damage *= 2;
                component3.baseData.speed *= 0.75f;
            }
            if (component4 != null)
            {
                component4.Owner = base.LastOwner;
                component4.Shooter = base.LastOwner.specRigidbody;
                component4.baseData.damage *= 2;
                component4.baseData.speed *= 0.75f;
            }
            if (component5 != null)
            {
                component5.Owner = base.LastOwner;
                component5.Shooter = base.LastOwner.specRigidbody;
                component5.baseData.damage *= 2;
                component5.baseData.speed *= 0.75f;
            }
            if (component6 != null)
            {
                component6.Owner = base.LastOwner;
                component6.Shooter = base.LastOwner.specRigidbody;
                component6.baseData.damage *= 2;
                component6.baseData.speed *= 0.75f;
            }
            if (component7 != null)
            {
                component7.Owner = base.LastOwner;
                component7.Shooter = base.LastOwner.specRigidbody;
                component7.baseData.damage *= 2;
                component7.baseData.speed *= 0.75f;
            }
            if (component8 != null)
            {
                component8.Owner = base.LastOwner;
                component8.Shooter = base.LastOwner.specRigidbody;
                component8.baseData.damage *= 2;
                component8.baseData.speed *= 0.75f;
            }
        }
        private void HandleReceivedDamage(PlayerController obj)
        {
            if (this.m_isRaged)
            {
                if (this.OverheadVFX && !this.instanceVFX)
                {
                    this.instanceVFX = base.LastOwner.PlayEffectOnActor(this.OverheadVFX, new Vector3(0f, 1.375f, 0f), true, true, false);
                }
                this.m_elapsed = 0f;
            }
            else
            {
                obj.StartCoroutine(this.HandleRage());
            }
        }
        private IEnumerator HandleRage()
        {
            this.m_isRaged = true;
            this.instanceVFX = null;
            RagePassiveItem rageitem = PickupObjectDatabase.GetById(353).GetComponent<RagePassiveItem>();
            this.OverheadVFX = rageitem.OverheadVFX.gameObject;
            if (this.OverheadVFX)
            {
                this.instanceVFX = base.LastOwner.PlayEffectOnActor(this.OverheadVFX, new Vector3(0f, 1.375f, 0f), true, true, false);
            }
            StatModifier damageStat = new StatModifier();
            damageStat.amount = this.boostAmount;
            damageStat.modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE;
            damageStat.statToBoost = PlayerStats.StatType.Damage;
            base.LastOwner.ownerlessStatModifiers.Add(damageStat);
            StatModifier speedStat = new StatModifier();
            speedStat.amount = this.boostAmount;
            speedStat.modifyType = StatModifier.ModifyMethod.ADDITIVE;
            speedStat.statToBoost = PlayerStats.StatType.MovementSpeed;
            base.LastOwner.ownerlessStatModifiers.Add(speedStat);
            base.LastOwner.stats.RecalculateStats(base.LastOwner, false, false);
            if (base.LastOwner.CurrentGun != null)
            {
                base.LastOwner.CurrentGun.ForceImmediateReload(false);
            }
            this.m_elapsed = 0f;
            float particleCounter = 0f;
            while (this.m_elapsed < this.rageDuration)
            {
                this.m_elapsed += BraveTime.DeltaTime;
                base.LastOwner.baseFlatColorOverride = this.flatColorOverride.WithAlpha(Mathf.Lerp(this.flatColorOverride.a, 0f, Mathf.Clamp01(this.m_elapsed - (this.rageDuration - 1f))));
                if (this.instanceVFX && this.m_elapsed > 1f)
                {
                    this.instanceVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out", null);
                    this.instanceVFX = null;
                }
                if (GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.LOW && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && base.LastOwner && base.LastOwner.IsVisible && !base.LastOwner.IsFalling)
                {
                    particleCounter += BraveTime.DeltaTime * 40f;
                    if (particleCounter > 1f)
                    {
                        int num = Mathf.FloorToInt(particleCounter);
                        particleCounter %= 1f;
                        GlobalSparksDoer.DoRandomParticleBurst(num, base.LastOwner.sprite.WorldBottomLeft.ToVector3ZisY(0f), base.LastOwner.sprite.WorldTopRight.ToVector3ZisY(0f), Vector3.up, 90f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
                    }
                }
                yield return null;
            }
            if (this.instanceVFX)
            {
                this.instanceVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out", null);
            }
            base.LastOwner.ownerlessStatModifiers.Remove(damageStat);
            base.LastOwner.ownerlessStatModifiers.Remove(speedStat);
            base.LastOwner.stats.RecalculateStats(base.LastOwner, false, false);
            this.m_isRaged = false;
            yield break;
        }
        protected override void DoEffect(PlayerController user)
        {
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    var BulletKin = EnemyDatabase.GetOrLoadByGuid("01972dee89fc4404a5c408d50007dad5");
                    //float radius = 5;
                    //var random = UnityEngine.Random.insideUnitCircle * radius;
                    //IntVector2 temp = random.ToIntVector2() + LastOwner.CurrentRoom.GetNearestCellToPosition(LastOwner.specRigidbody.UnitCenter).position; // or something like this to get the player's pos relative to the room
                    //IntVector2? spawnPos = LastOwner.CurrentRoom.GetRandomAvailableCell(temp);
                    IntVector2? spawnPos = LastOwner.CurrentRoom.GetRandomVisibleClearSpot(1, 1);
                    if (spawnPos != null)
                    {
                        AIActor TargetActor = AIActor.Spawn(BulletKin.aiActor, spawnPos.Value, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(spawnPos.Value), true, AIActor.AwakenAnimationType.Default, true);
                        TargetActor.ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect, 1f, null);
                        PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(TargetActor.specRigidbody, null, false);
                        TargetActor.gameObject.AddComponent<KillOnRoomClear>();
                        TargetActor.IsHarmlessEnemy = true;
                        TargetActor.IgnoreForRoomClear = true;
                        TargetActor.HandleReinforcementFallIntoRoom(0f);
                    }
                }
                catch (Exception e)
                {
                    ETGModConsole.Log(e.Message);
                }
            }
        }
        public override void Pickup(PlayerController player)
        {
            player.OnReceivedDamage += this.HandleReceivedDamage;
            base.Pickup(player);
        }
        protected override void OnPreDrop(PlayerController user)
        {
            user.OnReceivedDamage -= this.HandleReceivedDamage;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            base.LastOwner.OnReceivedDamage -= this.HandleReceivedDamage;
        }
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        PlayerController.DodgeRollState currentDodgeState;
        PlayerController.DodgeRollState lastDodgeState = PlayerController.DodgeRollState.InAir;
        public float rageDuration = 6f;
        public float boostAmount = 2f; 
        public Color flatColorOverride = new Color(0.5f, 0f, 0f, 0.75f);
        public GameObject OverheadVFX;
        private bool m_isRaged;
        private float m_elapsed;
        private GameObject instanceVFX;
    }
}