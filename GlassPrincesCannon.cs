using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemAPI;

namespace BleakMod
{

    public class GlassPrincesCannon: GunBehaviour
    {
        public static void Add()
        {
            Gun gun = ETGMod.Databases.Items.NewGun("Glass Princes Cannon", PickupObjectDatabase.GetById(197) as Gun, "gpcannon");
            Game.Items.Rename("outdated_gun_mods:glass_princes_cannon", "bb:glass_princes_cannon");
            gun.gameObject.AddComponent<GlassPrincesCannon>();
            gun.SetShortDescription("Royal Weaponry");
            gun.SetLongDescription("A special glass cannon issued by the guild of glass.\nThese weapons were used to defend the palace of panes in the great war.");
            gun.AddProjectileModuleFrom("glass_cannon", true, false);
            gun.SetupSprite(null, "glass_cannon", 1);
            gun.TransformToTargetGun(PickupObjectDatabase.GetById(540) as Gun);
            gun.DefaultModule.ammoCost = 1;
            gun.reloadTime = 1f;
            gun.DefaultModule.numberOfShotsInClip = 3;
            gun.InfiniteAmmo = true;
            gun.damageModifier -= 70;
            gun.quality = PickupObject.ItemQuality.EXCLUDED;
            gun.encounterTrackable.EncounterGuid = "676c617373";
            ETGMod.Databases.Items.Add(gun, null, "ANY");
        }
    }
}