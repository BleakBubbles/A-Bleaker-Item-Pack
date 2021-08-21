using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using System.Reflection;
using MultiplayerBasicExample;
using Dungeonator;

namespace BleakMod
{
    class WowTasticPaintbrush: PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Wow-Tastic Paintbrush";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/wow-tastic_paintbrush";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<WowTasticPaintbrush>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Heavy Art-illery";
            string longDesc = "Let your artistic skills run rampant with this fun little paintbrush. Use techniques such as perspective, shading, freehand, and... arson?\n\nAnyway, it comes with status immunities free of charge, so there's that I guess.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 1.0f);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.numberOfUses = 3;
            item.UsesNumberOfUsesBeforeCooldown = true;
            item.consumable = false;
            item.quality = ItemQuality.B;

            WowTasticPaintbrush.goopDefs = new List<GoopDefinition>();
            AssetBundle assetBundle = ResourceManager.LoadAssetBundle("shared_auto_001");
            foreach (string text in WowTasticPaintbrush.goops)
            {
                GoopDefinition goopDefinition;
                try
                {
                    GameObject gameObject2 = assetBundle.LoadAsset(text) as GameObject;
                    goopDefinition = gameObject2.GetComponent<GoopDefinition>();
                }
                catch
                {
                    goopDefinition = (assetBundle.LoadAsset(text) as GoopDefinition);
                }
                goopDefinition.name = text.Replace("assets/data/goops/", "").Replace(".asset", "");
                WowTasticPaintbrush.goopDefs.Add(goopDefinition);
            }
            //BuildPrefab2();
        }
        public override void Pickup(PlayerController player)
        {
            this.m_fireImmunity = new DamageTypeModifier();
            this.m_fireImmunity.damageMultiplier = 0f;
            this.m_fireImmunity.damageType = CoreDamageTypes.Fire;
            player.healthHaver.damageTypeModifiers.Add(this.m_fireImmunity);
            this.m_poisonImmunity = new DamageTypeModifier();
            this.m_poisonImmunity.damageMultiplier = 0f;
            this.m_poisonImmunity.damageType = CoreDamageTypes.Poison;
            player.healthHaver.damageTypeModifiers.Add(this.m_poisonImmunity);
            this.m_electricImmunity = new DamageTypeModifier();
            this.m_electricImmunity.damageMultiplier = 0f;
            this.m_electricImmunity.damageType = CoreDamageTypes.Electric;
            player.healthHaver.damageTypeModifiers.Add(this.m_electricImmunity);
            //this.engageEffect(player);
            base.Pickup(player);
        }
        protected override void OnPreDrop(PlayerController player)
        {
            if (this.m_fireImmunity != null)
            {
                player.healthHaver.damageTypeModifiers.Remove(this.m_fireImmunity);
            }
            if (this.m_poisonImmunity != null)
            {
                player.healthHaver.damageTypeModifiers.Remove(this.m_poisonImmunity);
            }
            if (this.m_electricImmunity != null)
            {
                player.healthHaver.damageTypeModifiers.Remove(this.m_electricImmunity);
            }
            //this.disengageEffect(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.LastOwner != null)
            {
                if (this.m_fireImmunity != null)
                {
                    base.LastOwner.healthHaver.damageTypeModifiers.Remove(this.m_fireImmunity);
                }
                if (this.m_poisonImmunity != null)
                {
                    base.LastOwner.healthHaver.damageTypeModifiers.Remove(this.m_poisonImmunity);
                }
                if (this.m_electricImmunity != null)
                {
                    base.LastOwner.healthHaver.damageTypeModifiers.Remove(this.m_electricImmunity);
                }
            }
        }
        protected override void DoEffect(PlayerController user)
		{
            base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.EndEffect)));
        }
        private void EndEffect(PlayerController user)
        {
            base.m_isCurrentlyActive = false;
        }
        public override void Update()
        {
            base.Update();
            if (base.LastOwner != null)
            {
                GungeonActions m_activeActions = WowTasticPaintbrush.ReflectGetField<GungeonActions>(typeof(PlayerController), "m_activeActions", base.LastOwner);
                bool IsKeyboardAndMouse = BraveInput.GetInstanceForPlayer(base.LastOwner.PlayerIDX).IsKeyboardAndMouse(false);
                if(IsKeyboardAndMouse)
                {
                    this.spawnpos = base.LastOwner.unadjustedAimPoint.XY() - (base.LastOwner.CenterPosition - base.LastOwner.specRigidbody.UnitCenter);
                }
                else
                {
                    if (m_activeActions != null)
                        {
                            spawnpos += m_activeActions.Aim.Vector.normalized * BraveTime.DeltaTime * 10f;
                        }
                }
                BraveInput input = BraveInput.GetInstanceForPlayer(base.LastOwner.PlayerIDX);
                if (this.IsCurrentlyActive && input.ActiveActions.GetActionFromType(GungeonActions.GungeonActionType.Shoot).IsPressed && !input.ActiveActions.GetActionFromType(GungeonActions.GungeonActionType.Shoot).WasPressed && Time.frameCount % 5 == 0)
                {
                    if(this.numberOfUses == 3)
                    {
                        DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(WowTasticPaintbrush.goopDefs[0]).AddGoopCircle(this.spawnpos, 0.9f);
                    }
                    else if (this.numberOfUses == 2)
                    {
                        DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(WowTasticPaintbrush.goopDefs[1]).AddGoopCircle(this.spawnpos, 0.9f);
                    }
                    else if (this.numberOfUses == 1)
                    {
                        DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(WowTasticPaintbrush.goopDefs[2]).AddGoopCircle(this.spawnpos, 0.9f);
                        for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
                        {
                            StaticReferenceManager.AllGoops[i].ElectrifyGoopCircle(base.LastOwner.specRigidbody.UnitBottomCenter, 50f);
                        }
                    }
                }
                /*if(Time.frameCount % 10 == 0 && this.instanceHat)
                {
                    if (this.instanceHatSprite)
                    {
                        this.disengageEffect(base.LastOwner);
                        this.engageEffect(base.LastOwner);
                        if (base.LastOwner.IsFalling)
                        {
                            this.instanceHatSprite.renderer.enabled = false;
                        }
                        else
                        {
                            this.instanceHatSprite.renderer.enabled = true;
                        }
                        if (base.LastOwner.IsBackfacing() && !this.instanceHatSprite.IsPlaying("back"))
                        {
                            this.instanceHatSprite.Play("back");
                        }
                        else if (!base.LastOwner.IsBackfacing() && !this.instanceHatSprite.IsPlaying("front"))
                        {
                            this.instanceHatSprite.Play("front");
                        }
                        if (this.instanceHatSprite.sprite.FlipX != base.LastOwner.sprite.FlipX)
                        {
                            this.instanceHatSprite.transform.localPosition.Set(this.instanceHatSprite.transform.localPosition.x * -1, this.instanceHatSprite.transform.localPosition.y, this.instanceHatSprite.transform.localPosition.z);
                        }
                        if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
                        {
                            this.disengageEffect(base.LastOwner);
                        }
                    }
                    else if(GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
                    {
                        this.engageEffect(base.LastOwner);
                    }
                }*/
            }
        }
        /*
        public static void BuildPrefab2()
        {
            GameObject gameObject = SpriteBuilder.SpriteFromResource("BleakMod/Resources/wow_tastic_paintbrush_front/wow_tastic_paintbrush_front_001", null);
            gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            tk2dSpriteAnimator animator = gameObject.AddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = gameObject.AddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip()
            {
                name = "front",
                frames = new tk2dSpriteAnimationFrame[0],
                wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop
            };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            List<string> frontSpritePaths = new List<string>
            {
                "BleakMod/Resources/wow_tastic_paintbrush_front/wow_tastic_paintbrush_front_001",
                "BleakMod/Resources/wow_tastic_paintbrush_front/wow_tastic_paintbrush_front_002",
                "BleakMod/Resources/wow_tastic_paintbrush_front/wow_tastic_paintbrush_front_002"
            };
            foreach (string path in frontSpritePaths)
            {
                tk2dSpriteCollectionData collection = gameObject.GetComponent<tk2dBaseSprite>().Collection;
                frames.Add(new tk2dSpriteAnimationFrame{spriteId = SpriteBuilder.AddSpriteToCollection(path, collection), spriteCollection = collection});
            };
            clip.frames = frames.ToArray();
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] {clip}).ToArray();
            tk2dSpriteAnimationClip clip2 = new tk2dSpriteAnimationClip()
            {
                name = "back",
                frames = new tk2dSpriteAnimationFrame[0],
                wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop
            };
            List<tk2dSpriteAnimationFrame> frames2 = new List<tk2dSpriteAnimationFrame>();
            List<string> backSpritePaths = new List<string>
            {
                "BleakMod/Resources/wow_tastic_paintbrush_back/wow_tastic_paintbrush_back_001",
                "BleakMod/Resources/wow_tastic_paintbrush_back/wow_tastic_paintbrush_back_002",
                "BleakMod/Resources/wow_tastic_paintbrush_back/wow_tastic_paintbrush_back_003"
            };
            foreach (string path in backSpritePaths)
            {   
                tk2dSpriteCollectionData collection = gameObject.GetComponent<tk2dBaseSprite>().Collection;
                frames2.Add(new tk2dSpriteAnimationFrame {spriteId = SpriteBuilder.AddSpriteToCollection(path, collection), spriteCollection = collection});
            }
            clip2.frames = frames2.ToArray();
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] {clip2}).ToArray();
            tk2dSpriteAnimationClip clip3 = new tk2dSpriteAnimationClip()
            {
                name = "idle front",
                frames = new tk2dSpriteAnimationFrame[0],
                wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop
            };
            List<tk2dSpriteAnimationFrame> frames3 = new List<tk2dSpriteAnimationFrame>();
            List<string> idleFrontSpritePaths = new List<string>
            {
                "BleakMod/Resources/wow_tastic_paintbrush_front/wow_tastic_paintbrush_front_001"
            };
            foreach (string path in idleFrontSpritePaths)
            {
                tk2dSpriteCollectionData collection = gameObject.GetComponent<tk2dBaseSprite>().Collection;
                frames3.Add(new tk2dSpriteAnimationFrame { spriteId = SpriteBuilder.AddSpriteToCollection(path, collection), spriteCollection = collection });
            }
            clip3.frames = frames3.ToArray();
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip3 }).ToArray();
            tk2dSpriteAnimationClip clip4 = new tk2dSpriteAnimationClip()
            {
                name = "idle back",
                frames = new tk2dSpriteAnimationFrame[0],
                wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop
            };
            List<tk2dSpriteAnimationFrame> frames4 = new List<tk2dSpriteAnimationFrame>();
            List<string> idleBackSpritePaths = new List<string>
            {
                "BleakMod/Resources/wow_tastic_paintbrush_front/wow_tastic_paintbrush_back_001"
            };
            foreach (string path in idleFrontSpritePaths)
            {
                tk2dSpriteCollectionData collection = gameObject.GetComponent<tk2dBaseSprite>().Collection;
                frames3.Add(new tk2dSpriteAnimationFrame { spriteId = SpriteBuilder.AddSpriteToCollection(path, collection), spriteCollection = collection });
            }
            clip4.frames = frames4.ToArray();
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip4 }).ToArray();
            WowTasticPaintbrush.instanceHatPrefab = gameObject;
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return base.CanBeUsed(user);
        }
        protected void engageEffect(PlayerController user)
        {
            if (!this.instanceHat)
            {
                this.instanceHat = user.RegisterAttachedObject(WowTasticPaintbrush.instanceHatPrefab, "jetpack", 0f);
            }
            this.instanceHatSprite = this.instanceHat.GetComponent<tk2dSpriteAnimator>();
        }
        protected void disengageEffect(PlayerController user)
        {
            if (this.instanceHat)
            {
                user.DeregisterAttachedObject(WowTasticPaintbrush.instanceHatPrefab, true);
            }
            this.instanceHat = null;
            this.instanceHatSprite = null;
        }*/
        public static T ReflectGetField<T>(Type classType, string fieldName, object o = null)
        {
            FieldInfo field = classType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | ((o != null) ? BindingFlags.Instance : BindingFlags.Static));
            return (T)field.GetValue(o);
        }
        public float duration = 6f;
        private static string[] goops = new string[]
        {
            "assets/data/goops/napalmgoopthatworks.asset",
            "assets/data/goops/poison goop.asset",
            "assets/data/goops/water goop.asset",
        };
        public static List<GoopDefinition> goopDefs;
        private DamageTypeModifier m_fireImmunity;
        private DamageTypeModifier m_poisonImmunity;
        private DamageTypeModifier m_electricImmunity;
        Vector2 spawnpos = Vector2.zero;

        //public static GameObject instanceHatPrefab;

        //private tk2dSpriteAnimator instanceHatSprite;

        //private GameObject instanceHat;
    }
}
    