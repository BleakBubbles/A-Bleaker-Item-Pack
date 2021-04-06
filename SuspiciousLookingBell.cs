using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using Dungeonator;

namespace BleakMod
{
    class SuspiciousLookingBell : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Suspicious Looking Bell";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/suspicious_looking_bell";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<SuspiciousLookingBell>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Ding Dong";
            string longDesc = "On use, chooses one of 4 of the Mine Flayer's attacks to execute.\n\nA seemingly lost bell with something alive writhing inside it. It calls for its former owner.";
            item.consumable = false;
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 300);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.quality = ItemQuality.B;
        }
        protected override void DoEffect(PlayerController user)
        {
            if (base.LastOwner && base.LastOwner.CurrentRoom != null && x % 4 == 1)
            {
                for (int i = 0; i < 6; i++)
                {          
                    try
                    {
                        AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid("566ecca5f3b04945ac6ce1f26dedbf4f");
                        IntVector2? intVector = new IntVector2?(base.LastOwner.CurrentRoom.GetRandomVisibleClearSpot(1, 1));
                        bool flag = intVector != null;
                        if (flag)
                        {
                            AIActor aiactor = AIActor.Spawn(orLoadByGuid.aiActor, intVector.Value, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector.Value), true, AIActor.AwakenAnimationType.Default, true);
                            aiactor.ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect, 1f, null);
                            aiactor.IgnoreForRoomClear = true;
                            aiactor.CanTargetEnemies = true;
                            aiactor.IsHarmlessEnemy = true;
                            aiactor.HitByEnemyBullets = true;
                            aiactor.OverrideTarget = user.CurrentRoom.GetRandomActiveEnemy().specRigidbody;
                            aiactor.gameObject.AddComponent<KillOnRoomClear>();
                            MoveErraticallyBehavior movement = new MoveErraticallyBehavior();
                            movement.PathInterval = 0.25f;
                            movement.StayOnScreen = true;
                            movement.AvoidTarget = false;
                            aiactor.behaviorSpeculator.MovementBehaviors[0] = movement;
                        }
                    }
                    catch (Exception ex)
                    {
                        ETGModConsole.Log(ex.Message, false);
                    }
                }
            }
            else if (base.LastOwner && base.LastOwner.CurrentRoom != null && x % 4 == 2)
            {
                this.StealthEffect();
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid("78a8ee40dff2477e9c2134f6990ef297");
                        IntVector2? intVector = new IntVector2?(base.LastOwner.CurrentRoom.GetRandomVisibleClearSpot(1, 1));
                        bool flag = intVector != null;
                        if (flag)
                        {
                            AIActor aiactor = AIActor.Spawn(orLoadByGuid.aiActor, intVector.Value, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector.Value), true, AIActor.AwakenAnimationType.Default, true);
                            aiactor.ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect, 1f, null);
                            aiactor.IgnoreForRoomClear = true;
                            aiactor.CanTargetEnemies = true;
                            aiactor.HitByEnemyBullets = true;
                            aiactor.healthHaver.SetHealthMaximum(1f);
                            aiactor.gameObject.AddComponent<KillOnRoomClear>();
                            foreach (AIActor enemy in user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
                            {
                                if (enemy.OverrideTarget == null)
                                {
                                    enemy.OverrideTarget = aiactor.specRigidbody;
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        ETGModConsole.Log(ex.Message, false);
                    }
                    try
                    {
                        AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid("78a8ee40dff2477e9c2134f6990ef297");
                        IntVector2? intVector = new IntVector2?(base.LastOwner.CurrentRoom.GetRandomVisibleClearSpot(1, 1));
                        bool flag = intVector != null;
                        if (flag)
                        {
                            AIActor aiactor = AIActor.Spawn(orLoadByGuid.aiActor, intVector.Value, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector.Value), true, AIActor.AwakenAnimationType.Default, true);
                            aiactor.ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect, 1f, null);
                            aiactor.IgnoreForRoomClear = true;
                            aiactor.IsHarmlessEnemy = true;
                            aiactor.HitByEnemyBullets = true;
                            aiactor.healthHaver.SetHealthMaximum(1f);
                            aiactor.gameObject.AddComponent<KillOnRoomClear>();
                            aiactor.healthHaver.OnPreDeath += this.BreakStealthFromBell;
                            foreach (AIActor enemy in user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
                            {
                                if (enemy.OverrideTarget == null)
                                {
                                    enemy.OverrideTarget = aiactor.specRigidbody;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ETGModConsole.Log(ex.Message, false);
                    }
                }
            }
            else if(x % 4 == 3)
            {
                base.StartCoroutine(this.Spiral());
            }
            else if(x % 4 == 0)
            {
                base.StartCoroutine(this.Bounce());
            }
            x++;
        }
        private IEnumerator Spiral()
        {
            Projectile projectile = ((Gun)ETGMod.Databases.Items[35]).DefaultModule.projectiles[0];
            for (int i = 0; i < 50; i++)
            {
                GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : i * 10f), true);
                GameObject obj2 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : (i * 10f) + 180f), true);
                Projectile proj1 = obj1.GetComponent<Projectile>();
                Projectile proj2 = obj2.GetComponent<Projectile>();
                if(proj1 != null)
                {
                    proj1.Owner = base.LastOwner;
                    proj1.Shooter = base.LastOwner.specRigidbody;
                }
                if (proj2 != null)
                {
                    proj2.Owner = base.LastOwner;
                    proj2.Shooter = base.LastOwner.specRigidbody;
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
        private IEnumerator Bounce()
        {
            Projectile projectile = ((Gun)ETGMod.Databases.Items[35]).DefaultModule.projectiles[0];
            for (int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 12; j++)
                {
                    GameObject obj3 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : j * 30f), true);
                    Projectile proj3 = obj3.GetComponent<Projectile>();
                    if (proj3 != null)
                    {
                        proj3.Owner = base.LastOwner;
                        proj3.Shooter = base.LastOwner.specRigidbody;
                        proj3.baseData.speed *= 0.66f;
                        if(proj3.GetComponent<BounceProjModifier>() == null)
                        {
                            proj3.gameObject.AddComponent<BounceProjModifier>();
                        }
                        if(proj3.GetComponent<PierceProjModifier>() == null)
                        {
                            proj3.gameObject.AddComponent<PierceProjModifier>();
                        }
                        BounceProjModifier boing = proj3.GetComponent<BounceProjModifier>();
                        boing.chanceToDieOnBounce = 0;
                        boing.numberOfBounces = 2;
                        PierceProjModifier poke = proj3.GetComponent<PierceProjModifier>();
                        poke.MaxBossImpacts = 3;
                        poke.penetration = 6;
                        proj3.GetComponent<BounceProjModifier>().OnBounceContext += this.BounceTowardsPlayer;
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        private void BreakStealthFromBell(Vector2 v)
        {
            this.BreakStealth(base.LastOwner);
        }
        private void StealthEffect()
        {
            PlayerController owner = base.LastOwner;
            this.BreakStealth(owner);
            owner.ChangeSpecialShaderFlag(1, 1f);
            owner.SetIsStealthed(true, "bell");
            owner.SetCapableOfStealing(true, "bell", null);
        }
        private void BreakStealth(PlayerController player)
        {
            player.ChangeSpecialShaderFlag(1, 0f);
            player.SetIsStealthed(false, "bell");
            player.SetCapableOfStealing(false, "bell", null);
            AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", base.gameObject);
        }
        private void nullifyDamage(SpeculativeRigidbody mySpec, PixelCollider myPixCo, SpeculativeRigidbody otherSpec, PixelCollider otherPixCo)
        {
            if (otherSpec.projectile && otherSpec.projectile.Owner == null)
            {
                otherSpec.projectile.DieInAir(false, true, true, false);
                PhysicsEngine.SkipCollision = true;
            }
        }
        private void BounceTowardsPlayer(BounceProjModifier bouncer, SpeculativeRigidbody body)
        {
            Projectile proj = bouncer.GetComponent<Projectile>();
            proj.SendInDirection(proj.Shooter.UnitCenter - proj.specRigidbody.UnitCenter, true);
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
            user.specRigidbody.OnPreRigidbodyCollision += this.nullifyDamage;
        }
        protected override void OnPreDrop(PlayerController user)
        {
            if (this.m_isCurrentlyActive)
            {
                this.BreakStealth(user);
            }
            user.specRigidbody.OnPreRigidbodyCollision -= this.nullifyDamage;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.m_isCurrentlyActive)
            {
                this.BreakStealth(base.LastOwner);
            }
            base.LastOwner.specRigidbody.OnPreRigidbodyCollision -= this.nullifyDamage;
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        private int x = 0;
    }
}