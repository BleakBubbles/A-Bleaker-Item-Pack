using System;
using System.Collections.Generic;
using Gungeon;
using ItemAPI;
using UnityEngine;

namespace BleakMod
{
	public class CannonFactory : GunBehaviour
	{
		public static void Add()
		{
			//Setting the display name of the gun and telling Gungeon what sprite prefix to look for in the mod's sprite files
			Gun gun = ETGMod.Databases.Items.NewGun("PPPPP", "cannon_factory");
			//Necessary line when using GunAPI
			Game.Items.Rename("outdated_gun_mods:ppppp", "bb:ppppp");
			//Adding this script to the gun

			gun.gameObject.AddComponent<CannonFactory>();

			//Setting the short description of the gun
			GunExt.SetShortDescription(gun, "Man the Cannons!");
            //Setting the long description of the gun
            GunExt.SetLongDescription(gun, "The Pirate's Personal Portable Plunder Producer, often abbreviated as the PPPPP, is a pirate's dream, as the name would suggest.\n\nWho wouldn't want a gun that shoots bullets that are cannons that shoot bullets?\n\nUsage: Shoots rotating cannons that can be fired by pressing E or interacting near them.");
			//Setting up the default sprite for the Ammonomicon and FPS of the gun
			GunExt.SetupSprite(gun, null, "cannon_factory_idle_001", 10);
			//Making it so that when the fire button is held down for a prolonged period of time, the firing animation loops from frame 2 rather than starting from the beginning
			tk2dSpriteAnimationClip fireClip = gun.sprite.spriteAnimator.GetClipByName("cannon_factory_fire");
			fireClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
			fireClip.loopStart = 2;
			//Setting up the FPS for each of the gun's animations
			GunExt.SetAnimationFPS(gun, gun.shootAnimation, 10);
			GunExt.SetAnimationFPS(gun, gun.reloadAnimation, 10);
			GunExt.SetAnimationFPS(gun, gun.idleAnimation, 10);

			//Setting the gun's projectile, which must come from a gun in the base game. In this case, I chose the gun with an ID of 37, which is Serious Cannon
			GunExt.AddProjectileModuleFrom(gun, PickupObjectDatabase.GetById(37) as Gun, true, false);
			//Setting the ammo cost of the gun
			gun.DefaultModule.ammoCost = 1;
			//Setting the shoot style of the gun (between automatic, semi automatic, or charged)
			gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
			//This determines the order that the gun should fire each of its projectiles, which is useless here because the gun only has one projectile
			gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
			//Setting the reload time of the gun
			gun.reloadTime = 3f;
			//Setting the fire rate of the gun, in cooldown time between shots
			gun.DefaultModule.cooldownTime = 0.75f;
			//Setting the clip size of the gun
			gun.DefaultModule.numberOfShotsInClip = 5;
			//Setting the location at which projectiles are fired from the gun
			gun.barrelOffset.position += new Vector3(1f, 0.4f, 0f);
			//Setting the maximum ammo of the gun
			gun.SetBaseMaxAmmo(150);
			//Setting the rarity of the gun
			gun.quality = PickupObject.ItemQuality.A;

			//Setting the encounter GUID of the gun which can be anything as long as it's not the same as another gun's. In this case the GUID is "pirate" written in hexadecimal
			gun.encounterTrackable.EncounterGuid = "706972617465";
			//These lines allow the projectile to be easily cloned by things such as shadow clone or shadow bullets
			Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(((Gun)global::ETGMod.Databases.Items[37]).DefaultModule.chargeProjectiles[0].Projectile);
			projectile.gameObject.SetActive(false);
			FakePrefab.MarkAsFakePrefab(projectile.gameObject);
			UnityEngine.Object.DontDestroyOnLoad(projectile);
			gun.DefaultModule.projectiles[0] = projectile;
			//Setting the speed of the projectile
			projectile.baseData.speed = 0.87f;
			//Setting the number of bounces of the projectile to 0
			BounceProjModifier bounce = projectile.GetComponent<BounceProjModifier>();
			bounce.numberOfBounces = 0;
			//Setting the number of pierces of the projectile to 3
			PierceProjModifier pierce = projectile.GetComponent<PierceProjModifier>();
			pierce.penetratesBreakables = true;
			pierce.penetration = 3;
			pierce.MaxBossImpacts = 3;
			//Making it so that the projectile spawns at the correct location
			projectile.transform.parent = gun.barrelOffset;

			//Changing the sprite of the projectile to a custom one. In this case, a simple cannon
			projectile.SetProjectileSpriteRight("cannon_projectile", 19, 12, 15, 9);
			//Adding the gun to the item database
			ETGMod.Databases.Items.Add(gun, null, "ANY");
			//Setting the angular velocity of the projectile. This is important because it is what makes it rotate
			projectile.angularVelocity = 180;

			//Adding a few synergies to the gun
			CustomSynergies.Add("ARRRR-C", new List<string>
			{
				"bb:ppppp"
			}, new List<string>
			{
				"remote_bullets",
				"ibomb_companion_app",
				"fortunes_favor",
				"rc_rocket"
			}, true);

			CustomSynergies.Add("Pirate Prowess", new List<string>
			{
				"bb:ppppp"
			}, new List<string>
			{
				"serious_cannon",
				"corsair",
				"scope",
				"eyepatch",
				"ring_of_chest_friendship",
				"map",
				"crutch"
			}, true);
		}

        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
			//Adding a CannonInteractManager component to every projectile when it is fired. This component is what handles player interaction with the projectile
            if (projectile.sprite)
            {
				projectile.gameObject.GetOrAddComponent<CannonInteractManager>();
            }
        }

        public override void OnPostFired(PlayerController player, Gun gun)
		{
			//Playing a sound every time the gun is fired
			gun.PreventNormalFireAudio = true;
			AkSoundEngine.PostEvent("Play_WPN_seriouscannon_shot_01", base.gameObject);
		}

		protected void Update()
		{
			//Some boilerplate code that handles sounds and reloading
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

		public override void OnReloadPressed(PlayerController player, Gun gun, bool bSOMETHING)
		{
			//Playing a sound every time the gun is reloaded
			if (gun.IsReloading && this.HasReloaded)
			{
				this.HasReloaded = false;
				AkSoundEngine.PostEvent("Stop_WPN_All", base.gameObject);
				base.OnReloadPressed(player, gun, bSOMETHING);
				AkSoundEngine.PostEvent("Play_steampunk_sfx", base.gameObject);
			}
		}

		private bool HasReloaded;
	}
}
