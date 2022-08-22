using System;
using System.Collections.Generic;
using System.Collections;
using Gungeon;
using ItemAPI;
using UnityEngine;

namespace BleakMod
{
	// Token: 0x02000030 RID: 48
	public class PrizeRifle : GunBehaviour
	{
		// Token: 0x06000192 RID: 402 RVA: 0x0000CD0C File Offset: 0x0000AF0C
		public static void Add()
		{
			Gun gun = ETGMod.Databases.Items.NewGun("Prize Rifle", "prize_rifle");
			Game.Items.Rename("outdated_gun_mods:prize_rifle", "bb:prize_rifle");
			gun.gameObject.AddComponent<PrizeRifle>();
			GunExt.SetShortDescription(gun, "Definitely Not Fair");
			GunExt.SetLongDescription(gun, "Hit targets while reloading to gain a damage boost for your next clip.\n\nA personal gun of Winchester's. He used it all the time for his leisure, but it seems that it has other uses as well...");
			GunExt.SetupSprite(gun, null, "prize_rifle_idle_001", 1);
			GunExt.SetAnimationFPS(gun, gun.reloadAnimation, 1);
			GunExt.SetAnimationFPS(gun, gun.idleAnimation, 1);
			GunExt.SetAnimationFPS(gun, gun.shootAnimation, 10);
			tk2dSpriteAnimationClip fireClip = gun.sprite.spriteAnimator.GetClipByName("prize_rifle_fire");
			fireClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
			fireClip.loopStart = 1;
			GunExt.AddProjectileModuleFrom(gun, PickupObjectDatabase.GetById(251) as Gun, true, false);
			gun.DefaultModule.ammoCost = 1;
			gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
			gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
			gun.reloadTime = 3f;
			gun.DefaultModule.cooldownTime = 0.133333f;
			gun.DefaultModule.numberOfShotsInClip = 20;
			gun.barrelOffset.position += new Vector3(1f, 0f, 0f);
			gun.carryPixelOffset = new IntVector2(1, 0);
			gun.SetBaseMaxAmmo(500);
			gun.quality = PickupObject.ItemQuality.B;
			gun.encounterTrackable.EncounterGuid = "77696e63686573746572";
			Projectile projectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0]);
			projectile.gameObject.SetActive(false);
			FakePrefab.MarkAsFakePrefab(projectile.gameObject);
			UnityEngine.Object.DontDestroyOnLoad(projectile);
			gun.DefaultModule.projectiles[0] = projectile;
			projectile.baseData.damage = 6;
			projectile.baseData.range = 60;
			projectile.SetProjectileSpriteRight("game_projectile_001", 18, 8, 15, 5);
			projectile.shouldRotate = true;
			projectile.name = "ArtfulDodgerRifleProjectile";
			BounceProjModifier bounce = projectile.gameObject.GetOrAddComponent<BounceProjModifier>();
			bounce.numberOfBounces = 5;
			projectile.transform.parent = gun.barrelOffset;
			GameObject trail = UnityEngine.Object.Instantiate<GameObject>((PickupObjectDatabase.GetById(251) as Gun).DefaultModule.projectiles[0].transform.Find("Trail").gameObject);
			ItemAPI.FakePrefab.MarkAsFakePrefab(trail);
			UnityEngine.Object.DontDestroyOnLoad(trail);
			trail.SetActive(true);
			trail.transform.parent = projectile.transform;
			ETGMod.Databases.Items.Add(gun, null, "ANY");
			BuildTarget();
		}
		public static GameObject targetPrefab;
		private static List<int> spriteIds = new List<int>();
		private static void BuildTarget()
        {
			targetPrefab = SpriteBuilder.SpriteFromResource("BleakMod/Resources/winchester_targets/winchester_target_001");
			tk2dBaseSprite vfxSprite = targetPrefab.GetComponent<tk2dBaseSprite>();
			vfxSprite.GetCurrentSpriteDef().ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerCenter, vfxSprite.GetCurrentSpriteDef().position3);
			FakePrefab.MarkAsFakePrefab(targetPrefab);
			UnityEngine.Object.DontDestroyOnLoad(targetPrefab);
			targetPrefab.SetActive(false);
			List<string> vfxIdleSprites = new List<string>
				{
					"winchester_target_001.png",
					"winchester_target_002.png",
					"winchester_target_003.png",
					"winchester_target_004.png",
					"winchester_target_005.png",
					"winchester_target_006.png",
					"winchester_target_007.png",
					"winchester_target_008.png",
					"winchester_target_009.png",
					"winchester_target_010.png",
					"winchester_target_011.png",
					"winchester_target_012.png",
					"winchester_target_013.png",
					"winchester_target_014.png",
				};

			var collection = targetPrefab.GetComponent<tk2dSprite>().Collection;
			var idleIdsList = new List<int>();
			foreach (string sprite in vfxIdleSprites)
			{
				idleIdsList.Add(SpriteBuilder.AddSpriteToCollection("BleakMod/Resources/winchester_targets/" + sprite, collection));
			}
			tk2dSpriteAnimator spriteAnimator = targetPrefab.AddComponent<tk2dSpriteAnimator>();
			spriteAnimator.playAutomatically = true;
            SpriteBuilder.AddAnimation(spriteAnimator, collection, idleIdsList, "winchetser_target_idle", tk2dSpriteAnimationClip.WrapMode.Loop, 7);
			var body = targetPrefab.GetComponent<tk2dSprite>().SetUpSpeculativeRigidbody(IntVector2.Zero, new IntVector2(20, 23)); //Numbers at the end are the dimensions of the hitbox          
			body.CollideWithTileMap = false;
			body.PrimaryPixelCollider.CollisionLayer = CollisionLayer.BulletBlocker;
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
			projectile.baseData.damage *= damageBoost;
			if (projectile.gameObject.GetComponent<BounceProjModifier>() != null)
            {
				BounceProjModifier boing = projectile.GetComponent<BounceProjModifier>();
				projectile.ChangeTintColorShader(0f, BraveUtility.GetRainbowColor(boing.numberOfBounces));
				boing.OnBounceContext += this.OnBounce;
            }
        }
		private void OnBounce(BounceProjModifier boing, SpeculativeRigidbody body)
        {
			int num = boing.numberOfBounces;
			boing.projectile.ChangeTintColorShader(0f, BraveUtility.GetRainbowColor(num));
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
					this.ClearTargets();
				}
			}
		}

		// Token: 0x06000195 RID: 405 RVA: 0x0000D03C File Offset: 0x0000B23C
		public override void OnReloadPressed(PlayerController player, Gun gun, bool bSOMETHING)
		{
			if (gun.IsReloading && this.HasReloaded)
			{
				this.HasReloaded = false;
				this.damageBoost = 1;
				AkSoundEngine.PostEvent("Stop_WPN_All", base.gameObject);
				base.OnReloadPressed(player, gun, bSOMETHING);
				AkSoundEngine.PostEvent("Play_WPN_SAA_reload_01", base.gameObject);
				GameManager.Instance.StartCoroutine(this.SpawnTargets(player, gun));
			}
		}

		private IEnumerator SpawnTargets(PlayerController player, Gun gun)
        {
			float num = (gun.reloadTime.RoundToNearest(1) - 1) * 2;
			for (int i = 0; i < (gun.reloadTime.RoundToNearest(1) - 1) * 2; i++)
			{
				Vector2 position = player.CurrentRoom.GetRandomVisibleClearSpot(1, 1).ToVector2();
				position = BraveMathCollege.ClampToBounds(position, GameManager.Instance.MainCameraController.MinVisiblePoint, GameManager.Instance.MainCameraController.MaxVisiblePoint);
				targetObjects.Add(UnityEngine.Object.Instantiate<GameObject>(PrizeRifle.targetPrefab, position, Quaternion.identity));
				tk2dBaseSprite targetSprite = targetObjects[i].GetComponent<tk2dBaseSprite>();
				SpriteOutlineManager.AddOutlineToSprite(targetSprite, Color.white);
				LootEngine.DoDefaultItemPoof(targetSprite.WorldCenter, false, false);
				SpeculativeRigidbody body = targetObjects[i].GetComponent<SpeculativeRigidbody>();
				body.OnRigidbodyCollision += this.HandleRigidbodyCollision;
			}
			for(int i = 0; i < (gun.reloadTime.RoundToNearest(1) - 1) * 2; i++)
            {
				yield return new WaitForSeconds(0.5f);
				GameObject obj3 = SpawnManager.SpawnProjectile(gun.DefaultModule.projectiles[0].gameObject, (gun.CurrentOwner as PlayerController).sprite.WorldCenter, Quaternion.Euler(0f, 0f, gun.CurrentAngle));
				Projectile proj3 = obj3.GetComponent<Projectile>();
				proj3.Owner = gun.CurrentOwner;
				proj3.collidesWithEnemies = false;
				if (proj3.gameObject.GetComponent<BounceProjModifier>() != null)
				{
					BounceProjModifier boing = proj3.GetComponent<BounceProjModifier>();
					proj3.ChangeTintColorShader(0f, BraveUtility.GetRainbowColor(boing.numberOfBounces));
					boing.OnBounceContext += this.OnBounce;
				}
			}
		}
		private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
		{
			if (rigidbodyCollision.OtherRigidbody.projectile != null)
			{
				Projectile projectile = rigidbodyCollision.OtherRigidbody.projectile;
				if (projectile.name.StartsWith("ArtfulDodger"))
				{
					PierceProjModifier component2 = projectile.GetComponent<PierceProjModifier>();
					if (component2 == null)
					{
						projectile.DieInAir(false, true, true, false);
					}
                    SpawnManager.SpawnVFX(Stuff.WinchesterTargetHitVFX, rigidbodyCollision.MyRigidbody.UnitCenter, Quaternion.identity);
					this.targetObjects.Remove(rigidbodyCollision.MyRigidbody.gameObject);
					UnityEngine.Object.Destroy(rigidbodyCollision.MyRigidbody.gameObject);
					AkSoundEngine.PostEvent("Play_OBJ_item_pickup_01", base.gameObject);
					this.damageBoost += 0.25f;
				}
			}
		}

		private void ClearTargets()
        {
			foreach(GameObject target in this.targetObjects)
            {
				SpeculativeRigidbody body = target.GetComponent<SpeculativeRigidbody>();
				LootEngine.DoDefaultItemPoof(body.UnitCenter);
				UnityEngine.Object.Destroy(target);
            }
			this.targetObjects.Clear();
        }
		// Token: 0x04000075 RID: 117
		private bool HasReloaded;
		private float damageBoost = 1;
		private List<GameObject> targetObjects = new List<GameObject>();
	}
}
