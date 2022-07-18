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
    class CapeOfTheResourcefulRat : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Cape Of The Resourceful Rat";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/cape_of_the_resourceful_rat";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<CapeOfTheResourcefulRat>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Caped Bullet Defender";
            string longDesc = "On use, chooses one of 3 of the Rat's attacks to execute.\n\nTurns out, the Rat wore capes before it was cool.";
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            item.consumable = false;
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 300);
            //Set the cooldown type and duration of the cooldown
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.quality = ItemQuality.B;
        }
        protected override void DoEffect(PlayerController user)
        {
            if (base.LastOwner && base.LastOwner.CurrentRoom != null && this.x % 3 == 1)
            {
                base.StartCoroutine(this.cheeseWheel1());
                base.StartCoroutine(this.cheeseWheel2());
            }
            else if (base.LastOwner && base.LastOwner.CurrentRoom != null && this.x % 3 == 0)
            {
                this.crosshair();
            }
            else if(base.LastOwner && this.x % 3 == 2)
            {
                for (int i = 0; i < 12; i++)
                {
                    try
                    {
                        AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid("be0683affb0e41bbb699cb7125fdded6");
                        IntVector2? intVector = new IntVector2?(base.LastOwner.CurrentRoom.GetRandomVisibleClearSpot(1, 1));
                        bool flag = intVector != null;
                        if (flag)
                        {
                            AIActor aiactor = AIActor.Spawn(orLoadByGuid.aiActor, intVector.Value, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector.Value), true, AIActor.AwakenAnimationType.Default, true);
                            aiactor.OverrideHitEnemies = true;
                            aiactor.CollisionDamage = 5f;
                            aiactor.CollisionDamageTypes = CoreDamageTypes.Electric;
                            aiactor.CollisionDamage = 5f;
                            aiactor.ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect, 1f, null);
                            aiactor.IgnoreForRoomClear = true;
                            //aiactor.ApplyEffect(this.CharmEffect, 1f, null);
                            aiactor.gameObject.AddComponent<KillOnRoomClear>();                        
                        }
                    }
                    catch (Exception ex)
                    {
                        ETGModConsole.Log(ex.Message, false);
                    }
                }
            }
            this.x++;
        }
        private void crosshair()
        {
            Projectile projectile = ((Gun)ETGMod.Databases.Items[22]).DefaultModule.projectiles[0];
            GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : base.LastOwner.FacingDirection), true);
            Projectile proj1 = obj1.GetComponent<Projectile>();
            proj1.Owner = base.gameActor;
            proj1.Shooter = base.LastOwner.specRigidbody;
            proj1.shouldRotate = true;
            proj1.baseData.damage *= 2f;
            proj1.baseData.speed *= 0.5f;
            proj1.angularVelocity = 180f;
            if (proj1.GetComponent<PierceProjModifier>() == null)
            {
                proj1.gameObject.AddComponent<PierceProjModifier>();
            }
            PierceProjModifier poke = proj1.GetComponent<PierceProjModifier>();
            poke.MaxBossImpacts = 3;
            poke.penetration = 6;
            proj1.specRigidbody.CollideWithTileMap = false;
            proj1.UpdateCollisionMask();
            proj1.pierceMinorBreakables = true;
            followCursorBehavior followCursorBehavior = proj1.gameObject.AddComponent<followCursorBehavior>();
            followCursorBehavior.parentItem = this;
            followCursorBehavior.user = base.LastOwner;
            for (int i = 0; i < 16; i++)
            {
                GameObject obj2 = SpawnManager.SpawnProjectile(projectile.gameObject, proj1.sprite.WorldCenter + BraveMathCollege.DegreesToVector(i * 22.5f, 2), Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : i * 22.5f), true);
                Projectile proj2 = obj2.GetComponent<Projectile>();
                proj2.Owner = base.gameActor;
                if (proj2.GetComponent<PierceProjModifier>() == null)
                {
                    proj2.gameObject.AddComponent<PierceProjModifier>();
                }
                PierceProjModifier poke2 = proj2.GetComponent<PierceProjModifier>();
                poke2.MaxBossImpacts = 3;
                poke2.penetration = 6;
                proj2.specRigidbody.CollideWithTileMap = false;
                proj2.Shooter = base.LastOwner.specRigidbody;
                proj2.baseData.damage *= 2f;
                proj2.baseData.speed = 0f;
                proj2.transform.SetParent(proj1.transform);
                proj2.collidesWithEnemies = true;
                if (i % 4 == 0)
                {
                    for (float j = 1; j < 4; j++)
                    {
                        GameObject obj3 = SpawnManager.SpawnProjectile(projectile.gameObject, proj1.sprite.WorldCenter + BraveMathCollege.DegreesToVector(i * 22.5f, 2f + j * (2/3f)), Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : i * 22.5f), true);
                        Projectile proj3 = obj3.GetComponent<Projectile>();
                        proj3.Owner = base.gameActor;
                        if (proj3.GetComponent<PierceProjModifier>() == null)
                        {
                            proj3.gameObject.AddComponent<PierceProjModifier>();
                        }
                        PierceProjModifier poke3 = proj3.GetComponent<PierceProjModifier>();
                        poke3.MaxBossImpacts = 3;
                        poke3.penetration = 6;
                        //proj3.specRigidbody.CollideWithTileMap = false;
                        proj3.Shooter = base.LastOwner.specRigidbody;
                        proj3.baseData.speed = 0f;
                        proj3.transform.SetParent(proj1.transform);
                    }
                }
            }
        }
        private IEnumerator cheeseWheel1()
        {
            for(int i = 0; i < 24; i++)
            {
                Projectile projectile = ((Gun)ETGMod.Databases.Items[668]).DefaultModule.projectiles[0];
                GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : i * 15), true);
                Projectile proj1 = obj1.GetComponent<Projectile>();
                proj1.Owner = base.gameActor;
                proj1.Shooter = base.LastOwner.specRigidbody;
                proj1.collidesWithEnemies = true;
                proj1.baseData.damage = 15f;
                proj1.baseData.speed *= 2f;
                yield return new WaitForSeconds(0.05f);
            }
        }
        private IEnumerator cheeseWheel2()
        {
            for(int i = 0; i < 18; i++)
            {
                Projectile projectile = ((Gun)ETGMod.Databases.Items[668]).DefaultModule.projectiles[0];
                GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : i * 20), true);
                Projectile proj1 = obj1.GetComponent<Projectile>();
                proj1.Owner = base.gameActor;
                proj1.Shooter = base.LastOwner.specRigidbody;
                proj1.collidesWithEnemies = true;
                proj1.baseData.damage = 15f;
                proj1.baseData.speed *= 1.5f;
                yield return new WaitForSeconds(0.05f);
            }
        }
        public class followCursorBehavior : MonoBehaviour
        {
            private void Start()
            {
                this.proj = base.GetComponent<Projectile>();
            }
            private void Update()
            {
                if (this.user != null && Time.frameCount % 10 == 0 && this.proj)
                {
                    GungeonActions m_activeActions = Stuff.ReflectGetField<GungeonActions>(typeof(PlayerController), "m_activeActions", this.user);
                    bool IsKeyboardAndMouse = BraveInput.GetInstanceForPlayer(this.user.PlayerIDX).IsKeyboardAndMouse(false);
                    if (IsKeyboardAndMouse)
                    {
                        this.spawnpos = this.user.unadjustedAimPoint.XY() - (this.user.CenterPosition - this.user.specRigidbody.UnitCenter);
                    }
                    else
                    {
                        if (m_activeActions != null)
                        {
                            spawnpos = m_activeActions.Aim.Vector;
                        }
                    }
                    this.proj.SendInDirection(spawnpos - this.proj.specRigidbody.UnitCenter, true);
                    this.proj.shouldRotate = false;
                }
            }
            private Projectile proj;
            public PlayerController user;
            Vector2 spawnpos = Vector2.zero;
            public CapeOfTheResourcefulRat parentItem = null;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        private int x = 0;
        //public GameActorCharmEffect CharmEffect;
    }
}