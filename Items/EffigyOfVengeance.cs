using System;
using System.Collections;
using System.Collections.Generic;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Gungeon;
using Dungeonator;
using Brave.BulletScript;

namespace BleakMod
{
    class EffigyOfVengeance : PlayerItem
    {
        public static void Init()
        {
            string itemName = "Effigy Of Vengeance";
            string resourceName = "BleakMod/Resources/effigy_of_vengeance";
            GameObject obj = new GameObject(itemName);
            var item = obj.AddComponent<EffigyOfVengeance>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);
            string shortDesc = "Kall of Kaliber";
            string longDesc = "Summons one of kaliber's own statues to aid you in battle.\n\nDon't get too close, or its explosions will hurt you.";
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 350);
            item.consumable = false;
            item.quality = ItemQuality.A;
            EffigyOfVengeance.BuildPrefab();
        }

        public static void BuildPrefab()
        {
            GameObject pillar = SpriteBuilder.SpriteFromResource("BleakMod/Resources/kill_pillars/kali_statue_fire_002");
            pillar.SetActive(false);
            FakePrefab.MarkAsFakePrefab(pillar);
            UnityEngine.Object.DontDestroyOnLoad(pillar);
            GameObject kpillar = new GameObject("SKillPillar");
            tk2dSprite sprite = kpillar.AddComponent<tk2dSprite>();
            sprite.SetSprite(pillar.GetComponent<tk2dBaseSprite>().Collection, pillar.GetComponent<tk2dBaseSprite>().spriteId);
            spriteIds.Add(SpriteBuilder.AddSpriteToCollection("BleakMod/Resources/kill_pillars/kali_statue_fire_001", sprite.Collection));
            sprite.GetCurrentSpriteDef().material.shader = ShaderCache.Acquire("Brave/PlayerShader");
            spriteIds.Add(sprite.spriteId);
            kpillar.SetActive(false);
            sprite.SetSprite(spriteIds[0]);
            sprite.GetCurrentSpriteDef().material.shader = ShaderCache.Acquire("Brave/PlayerShader");
            sprite.SetSprite(spriteIds[1]);
            FakePrefab.MarkAsFakePrefab(kpillar);
            UnityEngine.Object.DontDestroyOnLoad(kpillar);
            killPillarPrefab = kpillar;
        }

        protected override void DoEffect(PlayerController user)
        {
            base.DoEffect(user);
            this.m_extantKillPillar = UnityEngine.Object.Instantiate(killPillarPrefab, user.CenterPosition, Quaternion.identity);
            if (user.CurrentRoom != null)
            {
                AIActor randomenemy = user.CurrentRoom.GetRandomActiveEnemy(true);
                this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(randomenemy.sprite.WorldBottomCenter + new Vector2(0, 50), tk2dBaseSprite.Anchor.LowerCenter);
                base.StartCoroutine(this.HandleAttack(randomenemy, user.CurrentRoom));
            }
        }

        public IEnumerator HandleAttack(AIActor enemyToStartWith, RoomHandler room)
        {
            this.m_isCurrentlyActive = true;
            AIActor currentEnemy = enemyToStartWith;
            KillPillarState currentState = KillPillarState.Falling;
            Vector2 targetPoint = currentEnemy.sprite.WorldBottomCenter;
            Vector2 velocity = default(Vector2);
            float cooldownTime = 0f;
            float chaseTime = 0f;
            Vector2 lastTargetPoint = Vector2.zero;
            while (true)
            {
                if (!this.m_pickedUp || this.LastOwner == null)
                {
                    break;
                }
                if (currentState == KillPillarState.Falling)
                {
                    if (this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().WorldBottomCenter.y > targetPoint.y)
                    {
                        this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().WorldBottomCenter + new Vector2(0, -(30f * BraveTime.DeltaTime)), tk2dBaseSprite.Anchor.LowerCenter);
                    }
                    else
                    {
                        Exploder.DoDefaultExplosion(this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().WorldBottomCenter, new Vector2());
                        Projectile projectile = ((Gun)ETGMod.Databases.Items[22]).DefaultModule.projectiles[0];
                        Projectile projectile2 = ((Gun)ETGMod.Databases.Items[337]).DefaultModule.projectiles[0];
                        for (int i = 0; i < 16; i++)
                        {
                            GameObject obj3 = SpawnManager.SpawnProjectile(projectile.gameObject, this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : i * 22.5f), true);
                            Projectile proj3 = obj3.GetComponent<Projectile>();
                            if (proj3 != null)
                            {
                                proj3.Owner = base.LastOwner;
                                proj3.Shooter = base.LastOwner.specRigidbody;
                                proj3.baseData.speed *= 0.66f;
                                proj3.ImmuneToBlanks = true;
                                proj3.ImmuneToSustainedBlanks = true;
                            }
                            if(i % 2 == 0)
                            {
                                GameObject obj4 = SpawnManager.SpawnProjectile(projectile2.gameObject, this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().WorldCenter + BraveMathCollege.DegreesToVector(i * 22.5f, 0.5f), Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : i * 22.5f), true);
                                Projectile proj4 = obj4.GetComponent<Projectile>();
                                if (proj4 != null)
                                {
                                    proj4.Owner = base.LastOwner;
                                    proj4.Shooter = base.LastOwner.specRigidbody;
                                    proj4.baseData.speed *= 0.66f;
                                    proj4.ImmuneToBlanks = true;
                                    proj4.ImmuneToSustainedBlanks = true;
                                }
                            }
                        }
                        currentState = KillPillarState.CoolingDown;
                        cooldownTime = 1f;
                    }
                }
                else if (currentState == KillPillarState.CoolingDown)
                {
                    if (cooldownTime > 0)
                    {
                        cooldownTime -= BraveTime.DeltaTime;
                    }
                    else
                    {
                        if (room.HasActiveEnemies(RoomHandler.ActiveEnemyType.All))
                        {
                            currentState = KillPillarState.Chasing;
                            this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().SetSprite(spriteIds[0]);
                            chaseTime = 2f;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else if (currentState == KillPillarState.Chasing)
                {
                    if (room.HasActiveEnemies(RoomHandler.ActiveEnemyType.All))
                    {
                        if (currentEnemy == null || currentEnemy.sprite == null || (currentEnemy.healthHaver != null && currentEnemy.healthHaver.IsDead))
                        {
                            currentEnemy = room.GetRandomActiveEnemy(true);
                        }
                        Vector2 a = (currentEnemy.sprite.WorldTopCenter + new Vector2(0, 1) - this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().WorldBottomCenter);
                        a.Normalize();
                        bool flag5 = Vector2.Distance((currentEnemy.sprite.WorldTopCenter + new Vector2(0, 1)), base.sprite.WorldBottomCenter) < 0.2f;
                        if (flag5)
                        {
                            velocity = Vector2.Lerp(velocity, Vector2.zero, 0.5f);
                        }
                        else
                        {
                            velocity = Vector2.Lerp(velocity, a * 10, 0.1f);
                        }
                        this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().WorldBottomCenter + (BraveTime.DeltaTime * velocity), tk2dBaseSprite.Anchor.LowerCenter);
                        lastTargetPoint = (this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().WorldBottomCenter + new Vector2(0, -1f - currentEnemy.sprite.GetRelativePositionFromAnchor(tk2dBaseSprite.Anchor.UpperCenter).y));
                        if (chaseTime > 0)
                        {
                            chaseTime -= BraveTime.DeltaTime;
                        }
                        else
                        {
                            targetPoint = (this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().WorldBottomCenter + new Vector2(0, -1f - currentEnemy.sprite.GetRelativePositionFromAnchor(tk2dBaseSprite.Anchor.UpperCenter).y));
                            this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().SetSprite(spriteIds[1]);
                            currentState = KillPillarState.Falling;
                        }
                    }
                    else
                    {
                        targetPoint = lastTargetPoint;
                        this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().SetSprite(spriteIds[1]);
                        currentState = KillPillarState.Falling;
                    }
                }
                yield return null;
            }
            this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().SetSprite(spriteIds[0]);
            float ela = 0f;
            while (ela < 2f)
            {
                this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(this.m_extantKillPillar.GetComponent<tk2dBaseSprite>().WorldBottomCenter + new Vector2(0, (30f * BraveTime.DeltaTime)), tk2dBaseSprite.Anchor.LowerCenter);
                ela += BraveTime.DeltaTime;
                yield return null;
            }
            Destroy(this.m_extantKillPillar);
            this.m_isCurrentlyActive = false;
            yield break;
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }
        public override bool CanBeUsed(PlayerController user)
        {
            return user != null && user.CurrentRoom != null && user.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.All) && base.CanBeUsed(user);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        private GameObject m_extantKillPillar;
        public static GameObject killPillarPrefab;
        public static List<int> spriteIds = new List<int>();
        public enum KillPillarState
        {
            Falling,
            Chasing,
            CoolingDown
        }
    }
}