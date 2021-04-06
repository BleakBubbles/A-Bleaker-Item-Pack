using System;
using System.Collections;
using System.Collections.Generic;
using Gungeon;
using MonoMod;
using UnityEngine;
using ItemAPI;

namespace BleakMod
{

    public class Rhythminator : GunBehaviour
    {

        
        public static void Add()
        {
            // Get yourself a new gun "base" first.
            // Let's just call it "Basic Gun", and use "jpxfrd" for all sprites and as "codename" All sprites must begin with the same word as the codename. For example, your firing sprite would be named "jpxfrd_fire_001".
            Gun gun = ETGMod.Databases.Items.NewGun("Rhythminator", "rhythminator");
            // "kp:basic_gun determines how you spawn in your gun through the console. You can change this command to whatever you want, as long as it follows the "name:itemname" template.
            Game.Items.Rename("outdated_gun_mods:rhythminator", "bb:rhythminator");
            gun.gameObject.AddComponent<Rhythminator>();
            //These two lines determines the description of your gun, ".SetShortDescription" being the description that appears when you pick up the gun and ".SetLongDescription" being the description in the Ammonomicon entry. 
            gun.SetShortDescription("Keeping The Beat");
            gun.SetLongDescription("A strange gun that seems to pulse at a steady rate. As you hold the gun, the pulse finds its way into your every move...\n\nLet's dance.");
            // This is required, unless you want to use the sprites of the base gun.
            // That, by default, is the pea shooter.
            // SetupSprite sets up the default gun sprite for the ammonomicon and the "gun get" popup.
            // WARNING: Add a copy of your default sprite to Ammonomicon Encounter Icon Collection!
            // That means, "sprites/Ammonomicon Encounter Icon Collection/defaultsprite.png" in your mod .zip. You can see an example of this with inside the mod folder.
            gun.SetupSprite(null, "rhythminator_idle_001", 10);
            // ETGMod automatically checks which animations are available.
            // The numbers next to "shootAnimation" determine the animation fps. You can also tweak the animation fps of the reload animation and idle animation using this method.
            gun.SetAnimationFPS(gun.shootAnimation, 30);
            gun.SetAnimationFPS(gun.reloadAnimation, 10);
            // Every modded gun has base projectile it works with that is borrowed from other guns in the game. 
            // The gun names are the names from the JSON dump! While most are the same, some guns named completely different things. If you need help finding gun names, ask a modder on the Gungeon discord.
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(577) as Gun, true, false);
            // Here we just take the default projectile module and change its settings how we want it to be.
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 4f;
            gun.DefaultModule.cooldownTime = 0.133333f;
            gun.DefaultModule.numberOfShotsInClip = 25;
            gun.barrelOffset.position += new Vector3(1, 0, 0);
            gun.SetBaseMaxAmmo(500);
            // Here we just set the quality of the gun and the "EncounterGuid", which is used by Gungeon to identify the gun.
            gun.quality = PickupObject.ItemQuality.B;
            gun.encounterTrackable.EncounterGuid = "72687974686d";
            //This block of code helps clone our projectile. Basically it makes it so things like Shadow Clone and Hip Holster keep the stats/sprite of your custom gun's projectiles.
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
                projectile.AdditionalScaleMultiplier = 1.5f;
                projectile.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                gun.DefaultModule.projectiles[0] = projectile;
            //projectile.baseData allows you to modify the base properties of your projectile module.
            //In our case, our gun uses modified projectiles from the ak-47.
            //Setting static values for a custom gun's projectile stats prevents them from scaling with player stats and bullet modifiers (damage, shotspeed, knockback)
            //You have to multiply the value of the original projectile you're using instead so they scale accordingly. For example if the projectile you're using as a base has 10 damage and you want it to be 6 you use this
            //In our case, our projectile has a base damage of 5.5, so we multiply it by 1.1 so it does 10% more damage from the ak-47.
            projectile.baseData.damage *= 0.667f;
            projectile.baseData.speed *= 1.33333f;
            projectile.transform.parent = gun.barrelOffset;
            //This determines what sprite you want your projectile to use. Note this isn't necessary if you don't want to have a custom projectile sprite.
            //The x and y values determine the size of your custom projectile
            ETGMod.Databases.Items.Add(gun, null, "ANY");
            MultiActiveReloadController behav = gun.gameObject.AddComponent<MultiActiveReloadController>();
            behav.activeReloadEnabled = true;
            behav.reloads = new List<MultiActiveReloadData>
            {
                new MultiActiveReloadData(0.37f, 36, 38, 3, 1 * 3, false, true, new ActiveReloadData
                {
                    damageMultiply = 1.75f,
                }, true),
                new MultiActiveReloadData(0.5f, 49, 51, 5, 3 * 3, false, true, new ActiveReloadData
                {
                    damageMultiply = 2f,
                }, true),
                new MultiActiveReloadData(0.62f, 61, 63, 3, 1 * 3, false, true, new ActiveReloadData
                {
                    damageMultiply = 2.25f,
                }, true),
                new MultiActiveReloadData(0.75f, 74, 76, 3, 1 * 3, false, true, new ActiveReloadData
                {
                    damageMultiply = 2.5f,
                }, true),
                new MultiActiveReloadData(0.87f, 86, 88, 3 ,1 * 3, false, true, new ActiveReloadData
                {
                    damageMultiply = 2.75f,
                }, true)
            };
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            //This determines what sound you want to play when you fire a gun.
            //Sounds names are based on the Gungeon sound dump, which can be found at EnterTheGungeon/Etg_Data/StreamingAssets/Audio/GeneratedSoundBanks/Windows/sfx.txt
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_WPN_megablaster_shot_02", gameObject);
        }
        private bool HasReloaded;
        //This block of code allows us to change the reload sounds.
        protected void Update()
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

        //All that's left now is sprite stuff. 
        //Your sprites should be organized, like how you see in the mod folder. 
        //Every gun requires that you have a .json to match the sprites or else the gun won't spawn at all
        //.Json determines the hand sprites for your character. You can make a gun two handed by having both "SecondaryHand" and "PrimaryHand" in the .json file, which can be edited through Notepad or Visual Studios
        //By default this gun is a one-handed weapon
        //If you need a basic two handed .json. Just use the jpxfrd2.json.
        //And finally, don't forget to add your Gun to your ETGModule class!

    }
}