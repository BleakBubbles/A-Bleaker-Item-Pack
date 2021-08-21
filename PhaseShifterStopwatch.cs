using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;
using System.Reflection;
using Random = System.Random;
using FullSerializer;
using System.Collections;
using Gungeon;
using MonoMod.RuntimeDetour;
using MonoMod;


namespace BleakMod
{
    public class PhaseShifterStopwatch : PlayerItem
    {

        public static void Init()
        {
            //The name of the item
            string itemName = "Phase-Shifter Stopwatch";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/phase_shifter_stopwatch";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<PhaseShifterStopwatch>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Sneaky";
            string longDesc = "Upon first use, a visual marker appears at the player's location. The second use will teleport the player to the marker, restoring any health lost before the teleportation. Can telefrag enemeies.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 200f);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.B;
            BuildIcon();
        }
        public static GameObject TeleportIcon;
        public static List<int> spriteIds = new List<int>();
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }
        public static void BuildIcon()
        {
            TeleportIcon = SpriteBuilder.SpriteFromResource("BleakMod/Resources/phase_shift_vfx/phase_shift_vfx_idle_001");
            tk2dBaseSprite vfxSprite = TeleportIcon.GetComponent<tk2dBaseSprite>();
            vfxSprite.GetCurrentSpriteDef().ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleCenter, vfxSprite.GetCurrentSpriteDef().position3);
            FakePrefab.MarkAsFakePrefab(TeleportIcon);
            UnityEngine.Object.DontDestroyOnLoad(TeleportIcon);
            TeleportIcon.SetActive(false);
            List<string> vfxIdleSprites = new List<string>
                {
                    "phase_shift_vfx_idle_001.png",
                    "phase_shift_vfx_idle_002.png",
                    "phase_shift_vfx_idle_003.png",
                    "phase_shift_vfx_idle_004.png",
                    "phase_shift_vfx_idle_005.png",
                    "phase_shift_vfx_idle_006.png",
                    "phase_shift_vfx_idle_007.png",
                    "phase_shift_vfx_idle_008.png"
                };

            var collection = TeleportIcon.GetComponent<tk2dSprite>().Collection;
            var idleIdsList = new List<int>();
            foreach (string sprite in vfxIdleSprites)
            {
                idleIdsList.Add(SpriteBuilder.AddSpriteToCollection("BleakMod/Resources/phase_shift_vfx/" + sprite, collection));
            }
            tk2dSpriteAnimator spriteAnimator = TeleportIcon.AddComponent<tk2dSpriteAnimator>();
            spriteAnimator.playAutomatically = true;
            SpriteBuilder.AddAnimation(spriteAnimator, collection, idleIdsList, "phase_shift_vfx_idle", tk2dSpriteAnimationClip.WrapMode.Loop, 8);
        }
        public override bool CanBeUsed(PlayerController user)
        {
            if(!hasTriggeredVFX)
            {
                return true;
            }
            else if (hasTriggeredVFX)
            {
                if(teleportObject != null)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
            return false;
        }
        protected override void DoEffect(PlayerController user)
        {
            if (this.hasTriggeredVFX != true)
            {
                teleportObject = UnityEngine.Object.Instantiate<GameObject>(PhaseShifterStopwatch.TeleportIcon, user.sprite.WorldCenter, Quaternion.identity);
                tk2dBaseSprite vfxSprite = teleportObject.GetComponent<tk2dBaseSprite>();
                vfxSprite.sprite.usesOverrideMaterial = true;
                vfxSprite.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/HologramShader");
                base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.EndEffect)));
                this.health = user.healthHaver.GetCurrentHealth();
                this.armor = user.healthHaver.Armor;
                this.hasTriggeredVFX = true;
            }
        }
        protected override void DoActiveEffect(PlayerController user)
        {
            base.DoActiveEffect(user);
            if (teleportObject != null)
            {
                Vector2 position = teleportObject.GetComponent<tk2dBaseSprite>().WorldCenter;
                TeleportPlayerToCursorPosition.StartTeleport(user, position);
                this.TeleportEffect(teleportObject, user);
                user.healthHaver.ForceSetCurrentHealth(this.health);
                user.healthHaver.Armor = this.armor;
                EndEffect(user);
                base.IsCurrentlyActive = false;
            }
        }
        private void EndEffect(PlayerController user)
        {
            Destroy(teleportObject);
            this.hasTriggeredVFX = false;
            this.health = 0;
            this.armor = 0;
            base.m_isCurrentlyActive = false;
        }
        private GameObject teleportObject;
        private bool hasTriggeredVFX;

        protected override void OnPreDrop(PlayerController user)
        {
            base.OnPreDrop(user);
            if (teleportObject != null)
            {
                Destroy(teleportObject);
            }
        }
        protected override void OnDestroy()
        {
            if (teleportObject != null)
            {
                Destroy(teleportObject);
            }
            base.OnDestroy();
        }
        private int duration = 10;
        private void TeleportEffect(GameObject g, PlayerController p)
        {
            Vector2 position = p.sprite.WorldCenter;
            if(g != null && g.GetComponent<tk2dBaseSprite>() != null)
            {
                position = g.GetComponent<tk2dBaseSprite>().WorldCenter;
            }
            AkSoundEngine.PostEvent("Play_OBJ_teleport_depart_01", base.gameObject);
            for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
            {
                if (!GameManager.Instance.AllPlayers[i].IsGhost)
                {
                    GameManager.Instance.AllPlayers[i].healthHaver.TriggerInvulnerabilityPeriod(1f);
                    GameManager.Instance.AllPlayers[i].knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
                }
            }
            GameObject gameObject = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Teleport_Beam");
            if (gameObject != null && g != null)
            {
                GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
                gameObject2.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(position + new Vector2(0f, -0.5f), tk2dBaseSprite.Anchor.LowerCenter);
                gameObject2.transform.position = gameObject2.transform.position.Quantize(0.0625f);
                gameObject2.GetComponent<tk2dBaseSprite>().UpdateZDepth();
            }
            p.CurrentRoom.ApplyActionToNearbyEnemies(position, 2f, new Action<AIActor, float>(this.ProcessEnemy));
        }
        private void ProcessEnemy(AIActor a, float distance)
        {
            if (a.IsNormalEnemy && a.healthHaver && !a.healthHaver.IsBoss)
            {
                a.healthHaver.ApplyDamage(100000f, Vector2.zero, "Telefrag", CoreDamageTypes.None, DamageCategory.Normal, false, null, false);
            }
        }
        float health = 0;
        float armor = 0;
    }
}

