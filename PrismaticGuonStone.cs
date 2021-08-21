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
    class PrismaticGuonStone : AdvancedPlayerOrbitalItem
    {

        public static PlayerOrbital orbitalPrefab;
        public static PlayerOrbital upgradeOrbitalPrefab;
        public static void Init()
        {
            string itemName = "Prismatic Guon Stone"; //The name of the item
            string resourceName = "BleakMod/Resources/prismatic_guon_stone"; //Refers to an embedded png in the project. Make sure to embed your resources!

            GameObject obj = new GameObject();

            var item = obj.AddComponent<PrismaticGuonStone>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "First Prism";
            string longDesc = "Press E or your interact button in combat to shoot a colorful, devastating laser from this guon stone.\n\nA legendary guon stone which is said to hold great power. It is able to refract light into a powerful laser.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            item.quality = PickupObject.ItemQuality.S;

            BuildPrefab();
            BuildSynergyPrefab();
            item.OrbitalPrefab = orbitalPrefab;

            item.HasAdvancedUpgradeSynergy = true; //This allows you to have a synergy that upgrades the guon to a new prefab, like the 'Greener Guon Stone' or the 'Redder Guon Stone' etc
            item.AdvancedUpgradeSynergy = "Prismatismer Guon Stone"; //Name of the synergy to check for
            item.AdvancedUpgradeOrbitalPrefab = PrismaticGuonStone.upgradeOrbitalPrefab.gameObject;
            CustomSynergies.Add("Prismatismer Guon Stone", new List<string>
            {
                "bb:prismatic_guon_stone"
            }, new List<string>
            {
                "+1_bullets",
                "amulet_of_the_pit_lord",
                "bullet_time"
            }, true);
        }
        public static void BuildPrefab()
        {
            if (PrismaticGuonStone.orbitalPrefab != null) return;
            GameObject prefab = ItemBuilder.AddSpriteToObject("prismatic_guon", "BleakMod/Resources/prismatic_guon_stone/prismatic_guon_stone_001", null);
            FakePrefab.MarkAsFakePrefab(prefab);
            UnityEngine.Object.DontDestroyOnLoad(prefab);
            tk2dSpriteAnimator animator = prefab.AddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimationClip animationClip = new tk2dSpriteAnimationClip();
            animationClip.fps = 8;
            animationClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
            animationClip.name = "start";

            GameObject spriteObject = new GameObject("spriteObject");
            ItemBuilder.AddSpriteToObject("spriteObject", $"BleakMod/Resources/prismatic_guon_stone/prismatic_guon_stone_001", spriteObject);
            tk2dSpriteAnimationFrame starterFrame = new tk2dSpriteAnimationFrame();
            starterFrame.spriteId = spriteObject.GetComponent<tk2dSprite>().spriteId;
            starterFrame.spriteCollection = spriteObject.GetComponent<tk2dSprite>().Collection;
            tk2dSpriteAnimationFrame[] frameArray = new tk2dSpriteAnimationFrame[]
            {
                starterFrame
            };
            animationClip.frames = frameArray;
            for(int i = 2; i < 6; i++)
            {
                GameObject spriteForObject = new GameObject("spriteForObject");
                ItemBuilder.AddSpriteToObject("spriteForObject", $"BleakMod/Resources/prismatic_guon_stone/prismatic_guon_stone_00{i}", spriteForObject);
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
            gameObject.name = "Prismatic Guon Orbital";
            var body = gameObject.GetComponent<tk2dSprite>().SetUpSpeculativeRigidbody(IntVector2.Zero, new IntVector2(12, 11)); //Numbers at the end are the dimensions of the hitbox          
            body.CollideWithTileMap = false;
            body.CollideWithOthers = true;
            body.PrimaryPixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
            orbitalPrefab = gameObject.AddComponent<PlayerOrbital>();
            orbitalPrefab.motionStyle = PlayerOrbital.OrbitalMotionStyle.ORBIT_PLAYER_ALWAYS;
            orbitalPrefab.shouldRotate = true;
            orbitalPrefab.orbitRadius = 1.5f;
            orbitalPrefab.orbitDegreesPerSecond = 135; //Guon Stats
            orbitalPrefab.perfectOrbitalFactor = 1000f;
            orbitalPrefab.SetOrbitalTier(0);

            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            FakePrefab.MarkAsFakePrefab(gameObject);
            gameObject.SetActive(false);
        }
        public static void BuildSynergyPrefab() //here is where the synergy form is set up
        {
            bool flag = PrismaticGuonStone.upgradeOrbitalPrefab == null;
            if (flag)
            {
                GameObject prefab = ItemBuilder.AddSpriteToObject("prismatic_guon_synergy", "BleakMod/Resources/prismatic_guon_stone/prismatismer_guon_stone_001", null);
                prefab.name = "Prismatic Guon Orbital Synergy Form";
                FakePrefab.MarkAsFakePrefab(prefab);
                UnityEngine.Object.DontDestroyOnLoad(prefab);
                tk2dSpriteAnimator animator = prefab.AddComponent<tk2dSpriteAnimator>();
                tk2dSpriteAnimationClip animationClip = new tk2dSpriteAnimationClip();
                animationClip.fps = 8;
                animationClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop;
                animationClip.name = "start";

                GameObject spriteObject = new GameObject("spriteObject");
                ItemBuilder.AddSpriteToObject("spriteObject", $"BleakMod/Resources/prismatic_guon_stone/prismatismer_guon_stone_001", spriteObject);
                tk2dSpriteAnimationFrame starterFrame = new tk2dSpriteAnimationFrame();
                starterFrame.spriteId = spriteObject.GetComponent<tk2dSprite>().spriteId;
                starterFrame.spriteCollection = spriteObject.GetComponent<tk2dSprite>().Collection;
                tk2dSpriteAnimationFrame[] frameArray = new tk2dSpriteAnimationFrame[]
                {
                starterFrame
                };
                animationClip.frames = frameArray;
                for (int i = 2; i < 6; i++)
                {
                    GameObject spriteForObject = new GameObject("spriteForObject");
                    ItemBuilder.AddSpriteToObject("spriteForObject", $"BleakMod/Resources/prismatic_guon_stone/prismatismer_guon_stone_00{i}", spriteForObject);
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
                gameObject.name = "Prismatic Guon Orbital Synergy Form";
                var body = gameObject.GetComponent<tk2dSprite>().SetUpSpeculativeRigidbody(IntVector2.Zero, new IntVector2(13, 15)); //Numbers at the end are the dimensions of the hitbox          
                body.CollideWithTileMap = false;
                body.CollideWithOthers = true;
                body.PrimaryPixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
                upgradeOrbitalPrefab = gameObject.AddComponent<PlayerOrbital>();
                upgradeOrbitalPrefab.motionStyle = PlayerOrbital.OrbitalMotionStyle.ORBIT_PLAYER_ALWAYS;
                upgradeOrbitalPrefab.shouldRotate = true;
                upgradeOrbitalPrefab.orbitRadius = 1.5f;
                upgradeOrbitalPrefab.orbitDegreesPerSecond = 175f; //Guon Stats
                upgradeOrbitalPrefab.perfectOrbitalFactor = 1000f;
                upgradeOrbitalPrefab.SetOrbitalTier(0);

                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                FakePrefab.MarkAsFakePrefab(gameObject);
                gameObject.SetActive(false);
            }
        }

        protected override void Update()
        {
            if (base.Owner && this.m_extantOrbital != null && this.m_extantOrbital.GetComponent<PlayerOrbital>() != null)
            {
                this.beamCooldown += BraveTime.DeltaTime;
                if (base.Owner.IsInCombat)
                {
                    if(this.m_extantOrbital.GetComponent<PlayerOrbital>().GetOrbitalRadius() != 1.5f)
                    {
                        this.m_extantOrbital.GetComponent<PlayerOrbital>().orbitRadius = 1.5f;
                    }
                    
                }
                else if (!base.Owner.IsInCombat)
                {
                    if (this.m_extantOrbital.GetComponent<PlayerOrbital>().GetOrbitalRadius() != 3f)
                    {
                        this.m_extantOrbital.GetComponent<PlayerOrbital>().orbitRadius = 3f;
                    }
                }
                if (this.m_extantOrbital.GetComponent<OrbitalLaserInteractManager>() == null)
                {
                    this.m_extantOrbital.AddComponent<OrbitalLaserInteractManager>();
                }
            }
            base.Update();
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            cooldownBehavior cooldownBehavior = player.gameObject.GetOrAddComponent<cooldownBehavior>();
            cooldownBehavior.parentItem = this;
        }

        public override DebrisObject Drop(PlayerController player)
        {
            cooldownBehavior cooldownBehavior = player.gameObject.GetComponent<cooldownBehavior>();
            UnityEngine.Object.Destroy(cooldownBehavior);
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();    
        }
        public class cooldownBehavior : MonoBehaviour
        {
            private void Start()
            {
            }
            public PrismaticGuonStone parentItem = null;
        }
        public float beamCooldown = 0f;
    }
}