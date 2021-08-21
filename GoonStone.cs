using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using MultiplayerBasicExample;

namespace BleakMod
{
    class GoonStone : AdvancedPlayerOrbitalItem
    {

        public static PlayerOrbital orbitalPrefab;
        //public static PlayerOrbital upgradeOrbitalPrefab;
        public static void Init()
        {
            string itemName = "Goon Stone"; //The name of the item
            string resourceName = "BleakMod/Resources/goon_stone"; //Refers to an embedded png in the project. Make sure to embed your resources!

            GameObject obj = new GameObject();

            var item = obj.AddComponent<GoonStone>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Get Down, Mr. President!";
            string longDesc = "A legendary guon stone which is said to hold great power. yeah that's about it";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            item.quality = PickupObject.ItemQuality.A;

            BuildPrefab();
            //BuildSynergyPrefab();
            item.OrbitalPrefab = orbitalPrefab;

            item.HasAdvancedUpgradeSynergy = true; //This allows you to have a synergy that upgrades the guon to a new prefab, like the 'Greener Guon Stone' or the 'Redder Guon Stone' etc
            item.AdvancedUpgradeSynergy = "Gooner Guon Stone"; //Name of the synergy to check for
            //item.AdvancedUpgradeOrbitalPrefab = GoonStone.upgradeOrbitalPrefab.gameObject;
            CustomSynergies.Add("Gooner Guon Stone", new List<string>
            {
                "bb:goon_stone"
            }, new List<string>
            {
                "+1_bullets",
                "amulet_of_the_pit_lord",
                "bullet_time"
            }, true);
        }
        public static void BuildPrefab()
        {
            if (GoonStone.orbitalPrefab != null) return;
            GameObject prefab = ItemBuilder.AddSpriteToObject("goon_orbital", "BleakMod/Resources/goon_stone/bodyguard_guon_stone_001", null);
            FakePrefab.MarkAsFakePrefab(prefab);
            UnityEngine.Object.DontDestroyOnLoad(prefab);
            tk2dSpriteAnimator animator = prefab.AddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimationClip animationClip = new tk2dSpriteAnimationClip();
            animationClip.fps = 10;
            animationClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
            animationClip.name = "start";

            GameObject spriteObject = new GameObject("spriteObject");
            ItemBuilder.AddSpriteToObject("spriteObject", $"BleakMod/Resources/goon_stone/bodyguard_guon_stone_001", spriteObject);
            tk2dSpriteAnimationFrame starterFrame = new tk2dSpriteAnimationFrame();
            starterFrame.spriteId = spriteObject.GetComponent<tk2dSprite>().spriteId;
            starterFrame.spriteCollection = spriteObject.GetComponent<tk2dSprite>().Collection;
            tk2dSpriteAnimationFrame[] frameArray = new tk2dSpriteAnimationFrame[]
            {
                starterFrame
            };
            animationClip.frames = frameArray;
            for(int i = 2; i < 10; i++)
            {
                GameObject spriteForObject = new GameObject("spriteForObject");
                ItemBuilder.AddSpriteToObject("spriteForObject", $"BleakMod/Resources/goon_stone/bodyguard_guon_stone_00{i}", spriteForObject);
                tk2dSpriteAnimationFrame frame = new tk2dSpriteAnimationFrame();
                frame.spriteId = spriteForObject.GetComponent<tk2dBaseSprite>().spriteId;
                frame.spriteCollection = spriteForObject.GetComponent<tk2dBaseSprite>().Collection;
                animationClip.frames = animationClip.frames.Concat(new tk2dSpriteAnimationFrame[] { frame }).ToArray();
            }
            for(int i = 10; i < 29; i++)
            {
                GameObject spriteForObject = new GameObject("spriteForObject");
                ItemBuilder.AddSpriteToObject("spriteForObject", $"BleakMod/Resources/goon_stone/bodyguard_guon_stone_0{i}", spriteForObject);
                tk2dSpriteAnimationFrame frame = new tk2dSpriteAnimationFrame();
                frame.spriteId = spriteForObject.GetComponent<tk2dBaseSprite>().spriteId;
                frame.spriteCollection = spriteForObject.GetComponent<tk2dBaseSprite>().Collection;
                animationClip.frames = animationClip.frames.Concat(new tk2dSpriteAnimationFrame[] { frame }).ToArray();
            }
            animator.Library = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
            animator.Library.clips = new tk2dSpriteAnimationClip[] { animationClip };
            animator.DefaultClipId = animator.GetClipIdByName("start");
            animator.playAutomatically = true;

            GameObject gameObject = animator.gameObject;
            gameObject.name = "Goon Orbital";
            var body = gameObject.GetComponent<tk2dSprite>().SetUpSpeculativeRigidbody(IntVector2.Zero, new IntVector2(15, 16)); //Numbers at the end are the dimensions of the hitbox          
            body.CollideWithTileMap = false;
            body.CollideWithOthers = true;
            body.PrimaryPixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
            orbitalPrefab = gameObject.AddComponent<PlayerOrbital>();
            orbitalPrefab.motionStyle = PlayerOrbital.OrbitalMotionStyle.ORBIT_PLAYER_ALWAYS;
            orbitalPrefab.shouldRotate = false;
            orbitalPrefab.orbitRadius = 2f;
            orbitalPrefab.orbitDegreesPerSecond = 120; //Guon Stats
            orbitalPrefab.perfectOrbitalFactor = 1000f;
            orbitalPrefab.SetOrbitalTier(0);

            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.SetActive(false);
        }
        //public static void BuildSynergyPrefab() //here is where the synergy form is set up
        //{
        //    bool flag = GoonStone.upgradeOrbitalPrefab == null;
        //    if (flag)
        //    {
        //        GameObject prefab = ItemBuilder.AddSpriteToObject("prismatic_guon_synergy", "BleakMod/Resources/prismatic_guon_stone/prismatismer_guon_stone_001", null);
        //        prefab.name = "Prismatic Guon Orbital Synergy Form";
        //        FakePrefab.MarkAsFakePrefab(prefab);
        //        UnityEngine.Object.DontDestroyOnLoad(prefab);
        //        tk2dSpriteAnimator animator = prefab.AddComponent<tk2dSpriteAnimator>();
        //        tk2dSpriteAnimationClip animationClip = new tk2dSpriteAnimationClip();
        //        animationClip.fps = 8;
        //        animationClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
        //        animationClip.name = "start";

        //        GameObject spriteObject = new GameObject("spriteObject");
        //        ItemBuilder.AddSpriteToObject("spriteObject", $"BleakMod/Resources/prismatic_guon_stone/prismatismer_guon_stone_001", spriteObject);
        //        tk2dSpriteAnimationFrame starterFrame = new tk2dSpriteAnimationFrame();
        //        starterFrame.spriteId = spriteObject.GetComponent<tk2dSprite>().spriteId;
        //        starterFrame.spriteCollection = spriteObject.GetComponent<tk2dSprite>().Collection;
        //        tk2dSpriteAnimationFrame[] frameArray = new tk2dSpriteAnimationFrame[]
        //        {
        //        starterFrame
        //        };
        //        animationClip.frames = frameArray;
        //        for (int i = 2; i < 6; i++)
        //        {
        //            GameObject spriteForObject = new GameObject("spriteForObject");
        //            ItemBuilder.AddSpriteToObject("spriteForObject", $"BleakMod/Resources/prismatic_guon_stone/prismatismer_guon_stone_00{i}", spriteForObject);
        //            tk2dSpriteAnimationFrame frame = new tk2dSpriteAnimationFrame();
        //            frame.spriteId = spriteForObject.GetComponent<tk2dBaseSprite>().spriteId;
        //            frame.spriteCollection = spriteForObject.GetComponent<tk2dBaseSprite>().Collection;
        //            animationClip.frames = animationClip.frames.Concat(new tk2dSpriteAnimationFrame[] { frame }).ToArray();
        //        }
        //        animator.Library = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
        //        animator.Library.clips = new tk2dSpriteAnimationClip[] { animationClip };
        //        animator.DefaultClipId = animator.GetClipIdByName("start");
        //        animator.playAutomatically = true;

        //        GameObject gameObject = animator.gameObject;
        //        gameObject.name = "Prismatic Guon Orbital Synergy Form";
        //        var body = gameObject.GetComponent<tk2dSprite>().SetUpSpeculativeRigidbody(IntVector2.Zero, new IntVector2(13, 15)); //Numbers at the end are the dimensions of the hitbox          
        //        body.CollideWithTileMap = false;
        //        body.CollideWithOthers = true;
        //        body.PrimaryPixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
        //        upgradeOrbitalPrefab = gameObject.AddComponent<PlayerOrbital>();
        //        upgradeOrbitalPrefab.motionStyle = PlayerOrbital.OrbitalMotionStyle.ORBIT_PLAYER_ALWAYS;
        //        upgradeOrbitalPrefab.shouldRotate = true;
        //        upgradeOrbitalPrefab.orbitRadius = 1.5f;
        //        upgradeOrbitalPrefab.orbitDegreesPerSecond = 180; //Guon Stats
        //        upgradeOrbitalPrefab.perfectOrbitalFactor = 1000f;
        //        upgradeOrbitalPrefab.SetOrbitalTier(0);

        //        UnityEngine.Object.DontDestroyOnLoad(gameObject);
        //        FakePrefab.MarkAsFakePrefab(gameObject);
        //        gameObject.SetActive(false);
        //    }
        //}

        protected override void Update()
        {
            if (base.Owner && this.m_extantOrbital != null && this.m_extantOrbital.GetComponent<PlayerOrbital>() != null)
            {
                if (this.m_extantOrbital.GetComponent<OrbitalHaltInteractManager>() == null)
                {
                    this.m_extantOrbital.AddComponent<OrbitalHaltInteractManager>();
                }
            }
            base.Update();
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();    
        }
    }
}