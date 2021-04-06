using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using DirectionType = DirectionalAnimation.DirectionType;
using AnimationType = ItemAPI.CompanionBuilder.AnimationType;
using MultiplayerBasicExample;
using System.Timers;
using HutongGames.PlayMaker.Actions;
using HutongGames;

namespace BleakMod
{
    public class JammomancersHat : CompanionItem
    {
        public static GameObject prefab;
        private static readonly string guid = "jammomancer_hatless12345"; //give your companion some unique guid

        public static void Init()
        {
            string itemName = "Jammomancer's Hat";
            string resourceName = "BleakMod/Resources/jammomancers_hat";

            GameObject obj = new GameObject();
            GameObject ect = new GameObject();
            var item = obj.AddComponent<JammomancersHat>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "I want my hat back!";
            string longDesc = "1. Take hat from jammomancer.\n2. It starts following you around, trying to get its hat back.\n3. While it's reaching for its hat, it accidentally buffs your movement and attack speed.\n4. ???\n5. Profit.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            item.quality = PickupObject.ItemQuality.C;
            item.CompanionGuid = guid; //this will be used by the item later to pull your companion from the enemy database
            item.Synergies = new CompanionTransformSynergy[0]; //this just needs to not be null
            item.AddPassiveStatModifier(PlayerStats.StatType.Damage, 0.2f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.MovementSpeed, 1.5f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.Curse, 1f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddToSubShop(ItemBuilder.ShopType.Cursula, 1f);
            BuildPrefab();
            BuildPrefab2();
        }
        private void PostProcessProjectile(Projectile proj, float f)
        {
            if(proj.sprite != null && !proj.TreatedAsNonProjectileForChallenge)
            {
                proj.sprite.usesOverrideMaterial = true;
                proj.sprite.renderer.material.SetFloat("_BlackBullet", 1f);
                proj.sprite.renderer.material.SetFloat("_EmissivePower", -40f);
            }
        }
        public override void Pickup(PlayerController player)
        {
            player.PostProcessProjectile += this.PostProcessProjectile;
            base.Pickup(player);
        }
        public override DebrisObject Drop(PlayerController player)
        {
            player.PostProcessProjectile -= this.PostProcessProjectile;
            return base.Drop(player);
        }
        public static void BuildPrefab()
        {
            if (prefab != null || CompanionBuilder.companionDictionary.ContainsKey(guid))
                return;

            //Create the prefab with a starting sprite and hitbox offset/size
            prefab = CompanionBuilder.BuildPrefab("Jammomancer's Hat", guid, "BleakMod/Resources/idle/idle_left_001", new IntVector2(1, 0), new IntVector2(9, 9));

            //Add a companion component to the prefab (could be a custom class)
            var companion = prefab.AddComponent<CompanionController>();
            companion.aiActor.MovementSpeed = 5f;

            //Add all of the needed animations (most of the animations need to have specific names to be recognized, like idle_right or attack_left)
            prefab.AddAnimation("idle_right", "BleakMod/Resources/idle", fps: 5, AnimationType.Idle, DirectionType.TwoWayHorizontal);
            prefab.AddAnimation("idle_left", "BleakMod/Resources/idle", fps: 5, AnimationType.Idle, DirectionType.TwoWayHorizontal);
            prefab.AddAnimation("run_right", "BleakMod/Resources/move_right", fps: 5, AnimationType.Move, DirectionType.TwoWayHorizontal);
            prefab.AddAnimation("run_left", "BleakMod/Resources/move_left", fps: 5, AnimationType.Move, DirectionType.TwoWayHorizontal);

            //Add the behavior here, this too can be a custom class that extends AttackBehaviorBase or something like that
            var bs = prefab.GetComponent<BehaviorSpeculator>();
            bs.MovementBehaviors.Add(new CompanionFollowPlayerBehavior() { IdleAnimations = new string[] { "idle" } });
        }
        public static void BuildPrefab2()
        {
            GameObject gameObject = SpriteBuilder.SpriteFromResource("BleakMod/Resources/hat/jammomancer_hat_right", null);
            gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            GameObject gameObject2 = new GameObject("Badaboom");
            tk2dSprite tk2dSprite = gameObject2.AddComponent<tk2dSprite>();
            tk2dSprite.SetSprite(gameObject.GetComponent<tk2dBaseSprite>().Collection, gameObject.GetComponent<tk2dBaseSprite>().spriteId);
            JammomancersHat.spriteIds.Add(SpriteBuilder.AddSpriteToCollection("BleakMod/Resources/hat/jammomancer_hat_left", tk2dSprite.Collection));
            JammomancersHat.spriteIds.Add(SpriteBuilder.AddSpriteToCollection("BleakMod/Resources/hat/jammomancer_hat_back", tk2dSprite.Collection));
            tk2dSprite.GetCurrentSpriteDef().material.shader = ShaderCache.Acquire("Brave/PlayerShader");
            JammomancersHat.spriteIds.Add(tk2dSprite.spriteId);
            gameObject2.SetActive(false);
            tk2dSprite.SetSprite(JammomancersHat.spriteIds[0]);
            tk2dSprite.GetCurrentSpriteDef().material.shader = ShaderCache.Acquire("Brave/PlayerShader");
            tk2dSprite.SetSprite(JammomancersHat.spriteIds[1]);
            tk2dSprite.GetCurrentSpriteDef().material.shader = ShaderCache.Acquire("Brave/PlayerShader");
            tk2dSprite.SetSprite(JammomancersHat.spriteIds[2]);
            tk2dSprite.GetCurrentSpriteDef().material.shader = ShaderCache.Acquire("Brave/PlayerShader");
            FakePrefab.MarkAsFakePrefab(gameObject2);
            UnityEngine.Object.DontDestroyOnLoad(gameObject2);
            JammomancersHat.boomprefab = gameObject2;
        }
        private void SpawnVFXAttached()
        {
            GameObject boomprefab1 = UnityEngine.Object.Instantiate<GameObject>(JammomancersHat.boomprefab, base.Owner.transform.position + new Vector3(0.4f, 1.1f, -5f), Quaternion.identity);
            boomprefab1.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(base.Owner.specRigidbody.UnitCenter, tk2dBaseSprite.Anchor.MiddleCenter);
            GameManager.Instance.StartCoroutine(this.HandleSprite(boomprefab1));
            HatObject = boomprefab1;
        }
        protected override void Update()
        {
            if (base.Owner && !HatObject && base.Owner.CurrentGun.sprite && Time.frameCount % 10 == 0)
            {
                this.SpawnVFXAttached();
            }
            if (!base.Owner.CurrentGun.sprite && HatObject)
            {
                Destroy(HatObject);
            }
            base.Update();
        }
        private IEnumerator HandleSprite(GameObject prefab)
        {
            while (prefab != null && base.Owner != null)
            {
                if (base.Owner.specRigidbody.Velocity.sqrMagnitude != 0)
                {
                    elapsed += BraveTime.DeltaTime;
                }
                if (base.Owner.specRigidbody.Velocity.sqrMagnitude == 0)
                {
                    elapsed = 0;
                }
                prefab.transform.position = base.Owner.transform.position + new Vector3(0.4f, 1.1f + ((float)Math.Cos(elapsed * 18f) *
                0.05f), -5f);
                if (base.Owner.IsFalling)
                {
                    prefab.GetComponent<tk2dBaseSprite>().renderer.enabled = false;
                }
                else
                {
                    prefab.GetComponent<tk2dBaseSprite>().renderer.enabled = true;
                }
                if (base.Owner.IsBackfacing())
                {
                    prefab.GetComponent<tk2dBaseSprite>().SetSprite(JammomancersHat.spriteIds[1]);
                }
                if (!base.Owner.IsBackfacing() && this.m_owner.CurrentGun.sprite.WorldCenter.x - this.m_owner.specRigidbody.UnitCenter.x < 0f)
                {
                    prefab.GetComponent<tk2dBaseSprite>().SetSprite(JammomancersHat.spriteIds[0]);
                }
                if (!base.Owner.IsBackfacing() && this.m_owner.CurrentGun.sprite.WorldCenter.x - this.m_owner.specRigidbody.UnitCenter.x > 0f)
                {
                    prefab.GetComponent<tk2dBaseSprite>().SetSprite(JammomancersHat.spriteIds[2]);
                }
                yield return null;
            }
            Destroy(prefab.gameObject);
            yield break;
        }
        private static GameObject boomprefab;
        private GameObject HatObject;
        public static List<int> spriteIds = new List<int>();
        public float elapsed;

    }
}