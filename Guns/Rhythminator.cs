using System;
using System.Collections.Generic;
using Gungeon;
using ItemAPI;
using UnityEngine;

namespace BleakMod
{
	// Token: 0x02000030 RID: 48
	public class Rhythminator : GunBehaviour
	{
		// Token: 0x06000192 RID: 402 RVA: 0x0000CD0C File Offset: 0x0000AF0C
		public static void Add()
		{
			Gun gun = ETGMod.Databases.Items.NewGun("Rhythminator", "rhythminator");
			Game.Items.Rename("outdated_gun_mods:rhythminator", "bb:rhythminator");
			gun.gameObject.AddComponent<Rhythminator>();
			GunExt.SetShortDescription(gun, "Keeping The Beat");
			GunExt.SetLongDescription(gun, "A strange gun that seems to pulse at a steady rate. As you hold the gun, the pulse finds its way into your every move...\n\nLet's dance.");
			GunExt.SetupSprite(gun, null, "rhythminator_idle_001", 10);
			GunExt.SetAnimationFPS(gun, gun.reloadAnimation, 10);
			GunExt.AddProjectileModuleFrom(gun, PickupObjectDatabase.GetById(577) as Gun, true, false);
			gun.DefaultModule.ammoCost = 1;
			gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
			gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
			gun.reloadTime = 4f;
			gun.DefaultModule.cooldownTime = 0.133333f;
			gun.DefaultModule.numberOfShotsInClip = 25;
			gun.barrelOffset.position += new Vector3(1f, 0f, 0f);
			gun.SetBaseMaxAmmo(500);
			gun.quality = PickupObject.ItemQuality.B;
			gun.encounterTrackable.EncounterGuid = "72687974686d";
			Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
			projectile.AdditionalScaleMultiplier = 1.5f;
			projectile.gameObject.SetActive(false);
			FakePrefab.MarkAsFakePrefab(projectile.gameObject);
			UnityEngine.Object.DontDestroyOnLoad(projectile);
			gun.DefaultModule.projectiles[0] = projectile;
			projectile.baseData.damage *= 0.667f;
			projectile.baseData.speed *= 1.33333f;
			projectile.transform.parent = gun.barrelOffset;
			ETGMod.Databases.Items.Add(gun, null, "ANY");
			MultiActiveReloadController multiActiveReloadController = gun.gameObject.AddComponent<MultiActiveReloadController>();
			multiActiveReloadController.activeReloadEnabled = true;
			multiActiveReloadController.reloads = new List<MultiActiveReloadData>
			{
				new MultiActiveReloadData(0.37f, 36, 38, 3, 3, false, true, new ActiveReloadData
				{
					damageMultiply = 1.75f
				}, true),
				new MultiActiveReloadData(0.5f, 49, 51, 5, 9, false, true, new ActiveReloadData
				{
					damageMultiply = 2f
				}, true),
				new MultiActiveReloadData(0.62f, 61, 63, 3, 3, false, true, new ActiveReloadData
				{
					damageMultiply = 2.25f
				}, true),
				new MultiActiveReloadData(0.75f, 74, 76, 3, 3, false, true, new ActiveReloadData
				{
					damageMultiply = 2.5f
				}, true),
				new MultiActiveReloadData(0.87f, 86, 88, 3, 3, false, true, new ActiveReloadData
				{
					damageMultiply = 2.75f
				}, true)
			};
		}

		// Token: 0x06000193 RID: 403 RVA: 0x0000CFCC File Offset: 0x0000B1CC
		public override void OnPostFired(PlayerController player, Gun gun)
		{
			gun.PreventNormalFireAudio = true;
			AkSoundEngine.PostEvent("Play_WPN_megablaster_shot_02", base.gameObject);
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
	}
}
