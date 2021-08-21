using ItemAPI;
using SaveAPI;
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
        public static readonly string VERSION = "0.2.3";
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
            Carrot.Register();
            WinchestersHat.Register();
            GundromedaPain.Register();
            Bleaker.Register();
            CheeseAmmolet.Register();
            StrawberryJam.Register();
            //WhiteBulletCell.Register();
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
            //PiratesPendant.Register();
            PendantOfTheFirstOrder.Register();
            MimicBullets.Register();
            CatchingMitts.Register();
            Protractor.Register();
            HealthyBullets.Register();
            ChamberOfFrogs.Register();
            TrickShot.Register();
            ShowoffBullets.Register();
            BabyGoodShellicopter.Init();
            PrismaticGuonStone.Init();
            //GoonStone.Init();
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
            //Underpill.Init();
            AegisShield.Init();
            AmmocondasNest.Init();
            ShadesShades.Init();    
            BeholstersBelt.Init();
            SuspiciousLookingBell.Init();
            WowTasticPaintbrush.Init();
            SmokingSkull.Init();
            PortableSewerGrate.Init();
            GoldenCirclet.Init();
            //PackOfLostItems.Init();
            CapeOfTheResourcefulRat.Init();
            //DemonicBrick.Init();
            EffigyOfVengeance.Init();
            HornOfTheDragun.Init();
            PickpocketGuide.Init();
            SpillOJar.Init();
            PhaseShifterStopwatch.Init();
            Rhythminator.Add();
            //Bubbler.Add();
            StartStriker.Add();
            PrizeRifle.Add();
            MultiActiveReloadManager.SetupHooks();
            EasyGoopDefinitions.DefineDefaultGoops();
            //ZipFilePath = this.Metadata.Archive;
            //FilePath = this.Metadata.Directory;
            //AudioResourceLoader.InitAudio;
            SaveAPIManager.Setup("bb:");
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
