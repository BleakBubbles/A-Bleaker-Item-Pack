using System;
using System.Collections.Generic;
using Gungeon;
using ItemAPI;
using UnityEngine;
using Dungeonator;

namespace BleakMod
{
	// Token: 0x02000030 RID: 48
	public class StartStriker : GunBehaviour
	{
		// Token: 0x06000192 RID: 402 RVA: 0x0000CD0C File Offset: 0x0000AF0C
		public static void Add()
		{
			Gun gun = ETGMod.Databases.Items.NewGun("Star Splitter", "star_striker");
			Game.Items.Rename("outdated_gun_mods:star_splitter", "bb:star_splitter");
			gun.gameObject.AddComponent<StartStriker>();
			GunExt.SetShortDescription(gun, "Ricochet");
			GunExt.SetLongDescription(gun, "A laser gun from a far away star that is prone to splitting when hitting enemies and obstacles.");
			GunExt.SetupSprite(gun, null, "star_striker_idle_001", 1);
			GunExt.SetAnimationFPS(gun, gun.reloadAnimation, 6);
			GunExt.AddProjectileModuleFrom(gun, PickupObjectDatabase.GetById(54) as Gun, true, false);
			gun.DefaultModule.ammoCost = 1;
			gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
			gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
			gun.reloadTime = 1f;
			gun.DefaultModule.cooldownTime = 0.16666667f;
			gun.DefaultModule.numberOfShotsInClip = 25;
			gun.barrelOffset.position += new Vector3(0.4f, 0f, 0f);
			gun.SetBaseMaxAmmo(500);
			gun.quality = PickupObject.ItemQuality.B;
			gun.encounterTrackable.EncounterGuid = "7269636f63686574";
			Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
			projectile.gameObject.SetActive(false);
			FakePrefab.MarkAsFakePrefab(projectile.gameObject);
			UnityEngine.Object.DontDestroyOnLoad(projectile);
			gun.DefaultModule.projectiles[0] = projectile;
			projectile.baseData.damage = 2f;
			PierceProjModifier orAddComponent = projectile.gameObject.GetOrAddComponent<PierceProjModifier>();
			orAddComponent.penetration = 3;
			BounceProjModifier orAddComponent1 = projectile.gameObject.GetOrAddComponent<BounceProjModifier>();
			orAddComponent1.numberOfBounces = 1;
			projectile.transform.parent = gun.barrelOffset;
			ETGMod.Databases.Items.Add(gun, null, "ANY");
		}

		// Token: 0x06000193 RID: 403 RVA: 0x0000CFCC File Offset: 0x0000B1CC
		public override void OnPostFired(PlayerController player, Gun gun)
		{
			gun.PreventNormalFireAudio = true;
			AkSoundEngine.PostEvent("Play_WPN_megablaster_shot_02", base.gameObject);
		}
        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
            //projectile.gameObject.GetOrAddComponent<bounceCounter>();
            //         PierceProjModifier orAddComponent = projectile.gameObject.GetOrAddComponent<PierceProjModifier>();
            //         orAddComponent.penetratesBreakables = true;
            //         orAddComponent.penetration++;
            //BounceProjModifier orAddComponent1 = projectile.gameObject.GetOrAddComponent<BounceProjModifier>();
            //orAddComponent1.numberOfBounces = 1;
            //projectile.OnHitEnemy += this.OnHitEnemy;
            projectile.specRigidbody.OnPreRigidbodyCollision += this.OnPreRigidBodyCollision;
        }
		private void OnPreRigidBodyCollision(SpeculativeRigidbody mySpec, PixelCollider myPixCo, SpeculativeRigidbody otherSpec, PixelCollider otherPixCo)
        {
			this.projectilesCount++;
			if(mySpec.projectile && mySpec.projectile.Owner is PlayerController)
            {
				if(otherSpec.aiActor != null)
                {
					this.bounce(mySpec.projectile.Owner as PlayerController, mySpec.projectile);
                }
                else
                {
					this.bounce(mySpec.projectile.Owner as PlayerController, mySpec.projectile, otherSpec.aiActor);
                }
            }
			mySpec.OnPreRigidbodyCollision -= this.OnPreRigidBodyCollision;
        }
		private void bounce(PlayerController player, Projectile proj, AIActor enemy = null)
        {
			if(enemy == null && player.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
            {
				float num = -1f;
				AIActor enemy1 = player.CurrentRoom.GetNearestEnemy(proj.transform.position.XY(), out num, true, true);
				if (enemy1 != null)
				{
					float angle = (enemy1.CenterPosition - proj.transform.position.XY()).ToAngle();
					if (this.projectilesCount < 5)
					{
						Projectile projectile = (PickupObjectDatabase.GetById(881) as Gun).DefaultModule.projectiles[0];
						GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, proj.transform.localPosition.XY(), Quaternion.Euler(0f, 0f, angle), true);
						Projectile proj1 = obj1.GetComponent<Projectile>();
						proj1.baseData.damage = 1f;
						proj1.Owner = player;
						proj1.Shooter = player.specRigidbody;
						proj1.specRigidbody.OnPreRigidbodyCollision += this.OnPreRigidBodyCollision;
					}
                    else
                    {
						this.projectilesCount = 0;
                    }
				}
			}
			else if(enemy != null && player.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
            {
				AIActor enemy1 = player.CurrentRoom.GetRandomActiveEnemy(true);
				if (enemy1 != null && enemy1 != enemy)
				{
					float angle = (enemy1.CenterPosition - proj.transform.position.XY()).ToAngle();
					if (this.projectilesCount < 5)
					{
						Projectile projectile = (PickupObjectDatabase.GetById(881) as Gun).DefaultModule.projectiles[0];
						GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, proj.transform.localPosition.XY(), Quaternion.Euler(0f, 0f, angle), true);
						Projectile proj1 = obj1.GetComponent<Projectile>();
						proj1.baseData.damage = 1f;
						proj1.Owner = player;
						proj1.Shooter = player.specRigidbody;
						proj1.specRigidbody.OnPreRigidbodyCollision += this.OnPreRigidBodyCollision;
					}
					else
					{
						this.projectilesCount = 0;
					}
				}
				else if(enemy1 != null && enemy1 == enemy)
                {
					if (this.projectilesCount < 5)
					{
						Vector2 dirVec = UnityEngine.Random.insideUnitCircle;
						Projectile projectile = (PickupObjectDatabase.GetById(881) as Gun).DefaultModule.projectiles[0];
						GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, proj.transform.localPosition.XY(), Quaternion.Euler(0f, 0f, dirVec.ToAngle()), true);
						Projectile proj1 = obj1.GetComponent<Projectile>();
						proj1.baseData.damage = 1f;
						proj1.Owner = player;
						proj1.Shooter = player.specRigidbody;
						proj1.specRigidbody.OnPreRigidbodyCollision += this.OnPreRigidBodyCollision;
					}
					else
					{
						this.projectilesCount = 0;
					}
				}
			}
		}
		// Token: 0x06000194 RID: 404 RVA: 0x0000CFE8 File Offset: 0x0000B1E8
		public override void Update()
		{
			if (this.gun.CurrentOwner)
			{
				if (!this.gun.PreventNormalFireAudio)
				{
					this.gun.PreventNormalFireAudio = true;
				}
				if (!this.gun.IsReloading && !this.HasReloaded)
				{
					this.HasReloaded = true;
				}
			}
		}

		// Token: 0x06000195 RID: 405 RVA: 0x0000D03C File Offset: 0x0000B23C
		public override void OnReloadPressed(PlayerController player, Gun gun, bool bSOMETHING)
		{
			if (gun.IsReloading && this.HasReloaded)
			{
				this.HasReloaded = false;
				AkSoundEngine.PostEvent("Stop_WPN_All", base.gameObject);
				base.OnReloadPressed(player, gun, bSOMETHING);
				AkSoundEngine.PostEvent("Play_WPN_SAA_reload_01", base.gameObject);
			}
		}

		// Token: 0x04000075 RID: 117
		private bool HasReloaded;
		private int projectilesCount;
	}
}
