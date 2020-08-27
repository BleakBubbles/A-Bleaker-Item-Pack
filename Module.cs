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
        public static readonly string MOD_NAME = "BleakBubbles's Mod";
        public static readonly string VERSION = "0.0.6";
        public static readonly string TEXT_COLOR = "#00FFFF";

        public override void Start()
        {
            ItemBuilder.Init();
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
