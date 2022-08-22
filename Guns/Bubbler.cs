using System;
using System.Collections.Generic;
using Gungeon;
using ItemAPI;
using UnityEngine;

namespace BleakMod
{
	// Token: 0x02000030 RID: 48
	public class Bubbler : GunBehaviour
	{
		// Token: 0x06000192 RID: 402 RVA: 0x0000CD0C File Offset: 0x0000AF0C
		public static void Add()
		{
			Gun gun = ETGMod.Databases.Items.NewGun("Bubbler", "bubbler");
			Game.Items.Rename("outdated_gun_mods:bubbler", "bb:bubbler");
			gun.gameObject.AddComponent<Bubbler>();
			GunExt.SetShortDescription(gun, "A Bleaker Blaster");
			GunExt.SetLongDescription(gun, "A strange gun that seems to pulse at a steady rate. As you hold the gun, the pulse finds its way into your every move...\n\nLet's dance.");
			GunExt.SetupSprite(gun, null, "bubbler_idle_001", 10);
			tk2dSpriteAnimationClip fireClip = gun.sprite.spriteAnimator.GetClipByName("bubbler_fire");
			fireClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
			fireClip.loopStart = 1;
			GunExt.SetAnimationFPS(gun, gun.reloadAnimation, 10);
			GunExt.SetAnimationFPS(gun, gun.shootAnimation, 10);
			GunExt.SetAnimationFPS(gun, gun.idleAnimation, 10);
			GunExt.AddProjectileModuleFrom(gun, PickupObjectDatabase.GetById(36) as Gun, true, false);
			gun.DefaultModule.ammoCost = 1;
			gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
			gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
			gun.reloadTime = 2f;
			gun.DefaultModule.cooldownTime = 0.125f;
			gun.DefaultModule.numberOfShotsInClip = 16;
			gun.barrelOffset.position += new Vector3(0.8f, 0f, 0f);
			gun.SetBaseMaxAmmo(500);
			gun.InfiniteAmmo = true;
			gun.quality = PickupObject.ItemQuality.SPECIAL;
			gun.encounterTrackable.EncounterGuid = "627562626c65";
			Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
			projectile.gameObject.SetActive(false);
			FakePrefab.MarkAsFakePrefab(projectile.gameObject);
			UnityEngine.Object.DontDestroyOnLoad(projectile);
			gun.DefaultModule.projectiles[0] = projectile;
			projectile.baseData.damage = 4f;
			projectile.transform.parent = gun.barrelOffset;
            Projectile bubbleProjectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
			bubbleProjectile.gameObject.SetActive(false);
			FakePrefab.MarkAsFakePrefab(bubbleProjectile.gameObject);
			UnityEngine.Object.DontDestroyOnLoad(bubbleProjectile);
			bubbleProjectile.transform.parent = gun.barrelOffset;
			bubbleProjectile.SetProjectileSpriteRight("bubbler_projectile", 16, 16);
			bubbleProjectile.baseData.speed *= 0.5f;
			bubbleProjectile.baseData.damage *= 3f;
			gun.DefaultModule.usesOptionalFinalProjectile = true;
			gun.DefaultModule.numberOfFinalProjectiles = 1;
			gun.DefaultModule.finalProjectile = bubbleProjectile;	
			gun.DefaultModule.finalAmmoType = ((Gun)ETGMod.Databases.Items[599]).DefaultModule.ammoType;
			gun.DefaultModule.finalCustomAmmoType = ((Gun)ETGMod.Databases.Items[599]).DefaultModule.customAmmoType;
			ETGMod.Databases.Items.Add(gun, null, "ANY");
			Bubbler.goopDefs = new List<GoopDefinition>();
			AssetBundle assetBundle = ResourceManager.LoadAssetBundle("shared_auto_001");
			foreach (string text in Bubbler.goops)
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
				Bubbler.goopDefs.Add(goopDefinition);
			}
		}
		// Token: 0x06000193 RID: 403 RVA: 0x0000CFCC File Offset: 0x0000B1CC
		public override void OnPostFired(PlayerController player, Gun gun)
		{
			gun.PreventNormalFireAudio = true;
			if(gun.ClipShotsRemaining <= 1)
            {
				AkSoundEngine.PostEvent("Play_ENM_lizard_bubble_01", base.gameObject);
            }
            else
			{
				AkSoundEngine.PostEvent("Play_WPN_megablaster_shot_02", base.gameObject);
			}
		}

        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
			if(projectile.PossibleSourceGun.ClipShotsRemaining <= 1)
            {
				projectile.OnDestruction += this.Pop;
            }
        }

		private void Pop(Projectile proj)
        {
            if (proj.Owner is PlayerController && proj.Owner)
            {
				float duration = 0.75f;
				DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(Bubbler.goopDefs[UnityEngine.Random.Range(0, 3)]).TimedAddGoopCircle(proj.specRigidbody.UnitCenter, 4f, duration, false);
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

		private static string[] goops = new string[]
		{
			"assets/data/goops/napalmgoopthatworks.asset",
			"assets/data/goops/poison goop.asset",
			"assets/data/goops/water goop.asset",
			"assets/data/goops/blobulongoop.asset",
		};
		public static List<GoopDefinition> goopDefs;
	}
}
