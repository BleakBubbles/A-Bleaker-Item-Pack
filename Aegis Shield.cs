using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using Dungeonator;
using HutongGames.PlayMaker.Actions;

namespace BleakMod
{
    class AegisShield : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Aegis Shield";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/aegis_shield";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<AegisShield>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Killiad";
            string longDesc = "A shield with the head of the Gorgun in the middle.\n\n" +
                "It was said to be given to a wise goddess as a gift from her father, but was lost to time after she misplaced it.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 500);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.A;
        }
        protected override void DoEffect(PlayerController user)
        {
            Exploder.DoDistortionWave(user.sprite.WorldCenter, 0.5f, 0.04f, 20f, 1.5f);
            List<AIActor> activeEnemies = this.LastOwner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
            bool flag = activeEnemies != null;
            if (flag)
            {
                AkSoundEngine.PostEvent("Play_ENM_gorgun_gaze_01", base.gameObject);
                int count = activeEnemies.Count;
                for (int i = 0; i < count; i++)
                {
                    bool flag2 = activeEnemies[i] && activeEnemies[i].HasBeenEngaged && activeEnemies[i].healthHaver && activeEnemies[i].IsNormalEnemy && !activeEnemies[i].healthHaver.IsDead && !activeEnemies[i].healthHaver.IsBoss && !activeEnemies[i].IsTransmogrified;
                    if (flag2)
                    {
                        activeEnemies[i].CanTargetPlayers = false;
                        activeEnemies[i].CanTargetEnemies = false;
                        activeEnemies[i].MovementModifiers += this.NoMotionModifier;
                        activeEnemies[i].RegisterOverrideColor(Color.grey, this.gameObject.name);
                        if (activeEnemies[i].behaviorSpeculator)
                        {
                            activeEnemies[i].behaviorSpeculator.InterruptAndDisable(); //Uncomment this line if you want your item to disable ALL behaviors of the target enemy.
                        }
                        if (activeEnemies[i].spriteAnimator != null && activeEnemies[i].spriteAnimator.Library != null && activeEnemies[i].spriteAnimator.Library.clips != null)
                        {
                            foreach (tk2dSpriteAnimationClip clip in activeEnemies[i].spriteAnimator.Library.clips)
                            {
                                if (clip != null && clip.frames != null)
                                {
                                    foreach (tk2dSpriteAnimationFrame frame in clip.frames)
                                    {
                                        if (frame != null && frame.invulnerableFrame)
                                        {
                                            frame.invulnerableFrame = false;
                                        }
                                    }
                                }
                            }
                        }
                        activeEnemies[i].ReflectsProjectilesWhileInvulnerable = false;
                        activeEnemies[i].specRigidbody.ReflectProjectiles = false;
                        activeEnemies[i].specRigidbody.ReflectBeams = false;
                        activeEnemies[i].healthHaver.damageTypeModifiers.Clear();
                        activeEnemies[i].healthHaver.PreventAllDamage = false;
                    }
                    if (activeEnemies[i].healthHaver.IsBoss)
                    {
                        Gun gun = ETGMod.Databases.Items["triple_crossbow"] as Gun;
                        GameActorSpeedEffect speedEffect = gun.DefaultModule.projectiles[0].speedEffect;
                        speedEffect.duration = 15f;
                        activeEnemies[i].ApplyEffect(speedEffect, 1f, null);
                        base.StartCoroutine(this.TurnBossGrey(activeEnemies[i]));
                    }
                }
            }
            if (user.CurrentRoom.IsShop)
            {
                foreach (BaseShopController controller in user.CurrentRoom.GetComponentsInRoom<BaseShopController>())
                {
                    controller.SetCapableOfBeingStolenFrom(true, "AegisShield", new float?(15f));
                    this.TurnShopkeeperGrey(controller);
                }
            }
        }

        private void TurnShopkeeperGrey(BaseShopController shop)
        {
            FakeGameActorEffectHandler effectHandler = shop.GetComponentInChildren<FakeGameActorEffectHandler>();
            if (effectHandler)
            {
                effectHandler.StartCoroutine(this.TurnShopkeeperGreyCR(effectHandler));
            }
        }

        private IEnumerator TurnShopkeeperGreyCR(FakeGameActorEffectHandler effectHandler)
        {
            effectHandler.RegisterOverrideColor(Color.grey, "AegisShield");
            yield return new WaitForSeconds(15f);
            effectHandler.DeregisterOverrideColor("AegisShield");
        }
        private IEnumerator TurnBossGrey(AIActor boss)
        {
            boss.RegisterOverrideColor(Color.grey, "AegisShield");
            yield return new WaitForSeconds(15);
            boss.DeregisterOverrideColor("AegisShield");
        }
        private void NoMotionModifier(ref Vector2 voluntaryVel, ref Vector2 involuntaryVel)
        {
            voluntaryVel = Vector2.zero;
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
        }
        protected override void OnPreDrop(PlayerController user)
        {
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return base.CanBeUsed(user);
        }
    }
}