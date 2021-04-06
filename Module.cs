using ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BleakMod
{
    public class Module : ETGModule
    {
        public static readonly string MOD_NAME = "A Bleaker Item Pack";
        public static readonly string VERSION = "0.2.0";
        public static readonly string TEXT_COLOR = "#00FFFF";

        public override void Start()
        {
            ItemBuilder.Init();
            //Hooks.Init();
            //EnemyTools.Init();
            //Tools.Init();
            //EnemyBuilder.Init();
            LifeCube.Register();
            GungeonWind.Register();
            FriendshipBracelet.Register();
            FlamingSkull.Register();
            ShowoffBullets.Register();
            Carrot.Register();
            WinchestersHat.Register();
            GundromedaPain.Register();
            Bleaker.Register();
            CheeseAmmolet.Register();
            StrawberryJam.Register();
            WhiteBulletCell.Register();
            Distribullets.Register();
            HungryClips.Register();
            TatteredCape.Register();
            HeroicCape.Register();
            Popcorn.Register();
            RepurposedShellCasing.Register();
            GatlingGullets.Register();
            //SomeBunny.Register();
            FittedTankBarrel.Register();
            LeadCrown.Register();
            PiratesPendant.Register();
            PendantOfTheFirstOrder.Register();
            MimicBullets.Register();
            SellCreepItem.Register();
            BabyGoodShellicopter.Init();
            Overpill.Init();
            JammomancersHat.Init();
            ShotgunEnergyDrink.Init();
            BowlersBriefcase.Init();
            FatalOptics.Init();
            MicroEnhancement.Init();
            TargetingSpecs.Init();
            GunShredder.Init();
            GlassPrincesCannon.Add();
            Rewind.Init();
            Telegunesis.Init();
            Underpill.Init();
            AegisShield.Init();
            AmmocondasNest.Init();
            ShadesShades.Init();    
            BeholstersBelt.Init();
            SuspiciousLookingBell.Init();
            WowTasticPaintbrush.Init();
            SmokingSkull.Init();
            PortableSewerGrate.Init();
            GoldenCirclet.Init();
            PackOfLostItems.Init();
            CapeOfTheResourcefulRat.Init();
            LootersGloves.Init();
            DemonicBrick.Init();
            EffigyOfVengeance.Init();
            HornOfTheDragun.Init();
            //LoathesomeBunny.Init();
            Rhythminator.Add();
            MultiActiveReloadManager.SetupHooks();
            Log($"{MOD_NAME} v{VERSION} started successfully.", TEXT_COLOR);
        }


        public static void Log(string text, string color="#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }

        public override void Exit() { }
        public override void Init() { }
    }
}
