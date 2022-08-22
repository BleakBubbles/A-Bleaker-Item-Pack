using ItemAPI;
using SaveAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BepInEx;

using UnityEngine;

namespace BleakMod
{
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInDependency(Alexandria.Alexandria.GUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Module : BaseUnityPlugin
    {
        public const string GUID = "bleak.etg.abip";
        public const string NAME = "A Bleaker Item Pack";
        public const string VERSION = "1.0.3";
        public static readonly string MOD_NAME = "A Bleaker Item Pack";
        public static readonly string TEXT_COLOR = "#00FFFF";

        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
            ItemBuilder.Init();
            //Hooks.Init();
            //EnemyTools.Init();
            //Tools.Init();
            //EnemyBuilder.Init();
            ETGMod.Assets.SetupSpritesFromFolder(Path.Combine(this.FolderPath(), "sprites"));
            LifeCube.Register();
            GungeonWind.Register();
            FriendshipBracelet.Register();
            FlamingSkull.Register();
            Carrot.Register();
            WinchestersHat.Register();
            GundromedaPain.Register();
            Bleaker.Register();
            CheeseAmmolet.Register();
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
            //SpinGuonStone.Init();
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
            PouchLauncher.Add();
            MultiActiveReloadManager.SetupHooks();
            EasyGoopDefinitions.DefineDefaultGoops();
            //ZipFilePath = this.Metadata.Archive;
            //FilePath = this.Metadata.Directory;
            //AudioResourceLoader.InitAudio;
            try
            {
                SaveAPIManager.Setup("bb");
            }
            catch (Exception E)
            {
                ETGModConsole.Log($"{E}");
            }
            Log($"{MOD_NAME} v{VERSION} started successfully.", TEXT_COLOR);
        }

        public static void Log(string text, string color="#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }

        public void Awake() { }
    }
}
