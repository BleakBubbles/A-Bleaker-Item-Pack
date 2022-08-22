using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using ItemAPI;
using System.Collections.Generic;

namespace BleakMod
{

    public class PouchLauncher : GunBehaviour
    {


        public static void Add()
        {
            // Get yourself a new gun "base" first.
            // Let's just call it "Basic Gun", and use "jpxfrd" for all sprites and as "codename" All sprites must begin with the same word as the codename. For example, your firing sprite would be named "jpxfrd_fire_001".
            Gun gun = ETGMod.Databases.Items.NewGun("Pouch Launcher", "pouch_launcher");
            // "kp:basic_gun determines how you spawn in your gun through the console. You can change this command to whatever you want, as long as it follows the "name:itemname" template.
            Game.Items.Rename("outdated_gun_mods:pouch_launcher", "bb:pouch_launcher");
            gun.gameObject.AddComponent<PouchLauncher>();
            //These two lines determines the description of your gun, ".SetShortDescription" being the description that appears when you pick up the gun and ".SetLongDescription" being the description in the Ammonomicon entry. 
            GunExt.SetShortDescription(gun, "Plunderbuss");
            GunExt.SetLongDescription(gun, "A gun that uses bags of money as ammunition. These pouches burst into a volley of deadly coins upon hitting an enemy, doing massive damage to enemies slightly further away.");
            // This is required, unless you want to use the sprites of the base gun.
            // That, by default, is the pea shooter.
            // SetupSprite sets up the default gun sprite for the ammonomicon and the "gun get" popup.
            // WARNING: Add a copy of your default sprite to Ammonomicon Encounter Icon Collection!
            // That means, "sprites/Ammonomicon Encounter Icon Collection/defaultsprite.png" in your mod .zip. You can see an example of this with inside the mod folder.
            GunExt.SetupSprite(gun, null, "pouch_launcher_idle_001", 8);
            // ETGMod automatically checks which animations are available.
            // The numbers next to "shootAnimation" determine the animation fps. You can also tweak the animation fps of the reload animation and idle animation using this method.
            gun.SetAnimationFPS(gun.shootAnimation, 8);
            gun.SetAnimationFPS(gun.reloadAnimation, 8);
            // Every modded gun has base projectile it works with that is borrowed from other guns in the game. 
            // The gun names are the names from the JSON dump! While most are the same, some guns named completely different things. If you need help finding gun names, ask a modder on the Gungeon discord.
            GunExt.AddProjectileModuleFrom(gun, PickupObjectDatabase.GetById(47) as Gun, true, false);
            // Here we just take the default projectile module and change its settings how we want it to be.
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1f;
            gun.DefaultModule.cooldownTime = 0.66f;
            gun.DefaultModule.angleVariance = 0f;
            gun.DefaultModule.numberOfShotsInClip = 6;
            gun.SetBaseMaxAmmo(180);
            gun.barrelOffset.position += new Vector3(0.7f, -0.2f, 0f);
            // Here we just set the quality of the gun and the "EncounterGuid", which is used by Gungeon to identify the gun.
            gun.quality = PickupObject.ItemQuality.B;
            gun.encounterTrackable.EncounterGuid = "70656E6E79";
            //This block of code helps clone our projectile. Basically it makes it so things like Shadow Clone and Hip Holster keep the stats/sprite of your custom gun's projectiles.
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(((Gun)global::ETGMod.Databases.Items[47]).DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;
            //projectile.baseData allows you to modify the base properties of your projectile module.
            //In our case, our gun uses modified projectiles from the ak-47.
            //You can modify a good number of stats but for now, let's just modify the damage and speed.
            projectile.baseData.damage = 15f;
            projectile.baseData.speed = 20f;
            projectile.transform.parent = gun.barrelOffset;
            //This determines what sprite you want your projectile to use. Note this isn't necessary if you don't want to have a custom projectile sprite.
            //The x and y values determine the size of your custom projectile
            projectile.SetProjectileSpriteRight("pouch_projectile", 24, 14, 24, 14);
            ETGMod.Databases.Items.Add(gun, null, "ANY");

            Projectile coinProjectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(86) as Gun).DefaultModule.projectiles[0]);
            coinProjectile.gameObject.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(coinProjectile);
            FakePrefab.MarkAsFakePrefab(coinProjectile.gameObject);
            coinProjectile.baseData.damage *= 2f;
            coinProjectile.baseData.speed *= 0.5f;
            coinProjectile.shouldRotate = true;
            coinProjectile.SetProjectileSpriteRight("coinproj_1", 7, 6, false, tk2dBaseSprite.Anchor.MiddleCenter, 7, 6);
            PierceProjModifier pierce = coinProjectile.gameObject.GetOrAddComponent<PierceProjModifier>();
            pierce.penetration = 5;
            pierce.UsesMaxBossImpacts = false;
            coinProjectile.AnimateProjectile(new List<string> {
                "coinproj_1",
                "coinproj_2",
                "coinproj_3",
                "coinproj_4"
            }, 12, true, new List<IntVector2> {
                new IntVector2(7, 6), //1
                new IntVector2(7, 6), //2            
                new IntVector2(7, 6), //3
                new IntVector2(7, 6), //4
            }, AnimateBullet.ConstructListOfSameValues(false, 4), AnimateBullet.ConstructListOfSameValues(tk2dBaseSprite.Anchor.MiddleCenter, 4), AnimateBullet.ConstructListOfSameValues(true, 4), AnimateBullet.ConstructListOfSameValues(false, 4),
                AnimateBullet.ConstructListOfSameValues<Vector3?>(null, 4), AnimateBullet.ConstructListOfSameValues<IntVector2?>(null, 4), AnimateBullet.ConstructListOfSameValues<IntVector2?>(null, 4), AnimateBullet.ConstructListOfSameValues<Projectile>(null, 4));
        }


        public static Projectile coinProjectile;
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            //This determines what sound you want to play when you fire a gun.
            //Sounds names are based on the Gungeon sound dump, which can be found at EnterTheGungeon/Etg_Data/StreamingAssets/Audio/GeneratedSoundBanks/Windows/sfx.txt
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_WPN_smileyrevolver_shot_01", gameObject);
        }
        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
            projectile.OnHitEnemy += this.OnHitEnemy;
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody spec, bool isFatal)
        {
            GameObject obj1 = SpawnManager.SpawnProjectile(coinProjectile.gameObject, spec.sprite.WorldCenter, Quaternion.Euler(0f, 0f, proj.Direction.ToAngle()), true);
            Projectile proj1 = obj1.GetComponent<Projectile>();
            if (proj1 != null)
            {
                proj1.Owner = proj.Owner;
                proj1.Shooter = proj.Owner.specRigidbody;
                proj1.baseData.damage *= (proj.Owner as PlayerController).stats.GetStatValue(PlayerStats.StatType.Damage);
            }
        }
        private bool HasReloaded;
        //This block of code allows us to change the reload sounds.
        public override void Update()
        {
            if (gun.CurrentOwner)
            {

                if (!gun.PreventNormalFireAudio)
                {
                    this.gun.PreventNormalFireAudio = true;
                }
                if (!gun.IsReloading && !HasReloaded)
                {
                    this.HasReloaded = true;
                }
            }
        }

        public override void OnReloadPressed(PlayerController player, Gun gun, bool bSOMETHING)
        {
            if (gun.IsReloading && this.HasReloaded)
            {
                HasReloaded = false;
                AkSoundEngine.PostEvent("Stop_WPN_All", base.gameObject);
                base.OnReloadPressed(player, gun, bSOMETHING);
                AkSoundEngine.PostEvent("Play_WPN_SAA_reload_01", base.gameObject);
            }
        }
        //Now add the Tools class to your project.
        //All that's left now is sprite stuff. 
        //Your sprites should be organized, like how you see in the mod folder. 
        //Every gun requires that you have a .json to match the sprites or else the gun won't spawn at all
        //.Json determines the hand sprites for your character. You can make a gun two handed by having both "SecondaryHand" and "PrimaryHand" in the .json file, which can be edited through Notepad or Visual Studios
        //By default this gun is a one-handed weapon
        //If you need a basic two handed .json. Just use the jpxfrd2.json.
        //And finally, don't forget to add your Gun to your ETGModule class!
    }
}