using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using System.Runtime.CompilerServices;
using MultiplayerBasicExample;

namespace BleakMod
{
    class SpinGuonStone : AdvancedPlayerOrbitalItem
    {

        public static PlayerOrbital orbitalPrefab;
        public static PlayerOrbital upgradeOrbitalPrefab;
        public static void Init()
        {
            string itemName = "Steel Ball Guon Stone"; //The name of the item
            string resourceName = "BleakMod/Resources/spin_guon_stone"; //(inventory sprite) MAKE SURE TO CHANGE THE SPRITE PATH TO YOUR MOD'S RESOURCES.

            GameObject obj = new GameObject();
            var item = obj.AddComponent<SpinGuonStone>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Fall Off Your Horse!";
            string longDesc = "Long Description";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            item.quality = PickupObject.ItemQuality.A;

            BuildPrefab();
            item.OrbitalPrefab = orbitalPrefab;
            BuildSynergyPrefab();

            item.HasAdvancedUpgradeSynergy = true; //Set this to true if you want a synergy that changes the appearance of the Guon Stone. All base game guons have a [colour]-er Guon Stone synergy that makes them bigger and brighter.
            item.AdvancedUpgradeSynergy = "Steelier Guon Stone"; //This is the name of the synergy that changes the appearance, if you have one.
            item.AdvancedUpgradeOrbitalPrefab = SpinGuonStone.upgradeOrbitalPrefab.gameObject;

            CustomSynergies.Add("Steelier Guon Stone", new List<string>
            {
                "bb:steel_ball_guon_stone"
            }, new List<string>
            {
                "+1_bullets",
                "amulet_of_the_pit_lord",
                "bullet_time"
            }, true);
        }
        
        public static void BuildPrefab()
        {
            if (SpinGuonStone.orbitalPrefab != null) return;
            GameObject prefab = SpriteBuilder.SpriteFromResource("BleakMod/Resources/spin_stone"); //(ingame orbital sprite)MAKE SURE TO CHANGE THE SPRITE PATH TO YOUR MODS RESOURCES
            prefab.name = "Spin Guon Orbital"; //The name of the orbital used by the code. Barely ever used or seen, but important to change.
            var body = prefab.GetComponent<tk2dSprite>().SetUpSpeculativeRigidbody(IntVector2.Zero, new IntVector2(10, 10)); //This line sets up the hitbox of your guon, this one is set to 5 pixels across by 9 pixels high, but you can set it as big or small as you want your guon to be.           
            body.CollideWithTileMap = false;
            body.CollideWithOthers = true;
            body.PrimaryPixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
            orbitalPrefab = prefab.AddComponent<PlayerOrbital>();
            orbitalPrefab.motionStyle = PlayerOrbital.OrbitalMotionStyle.ORBIT_PLAYER_ALWAYS; //You can ignore most of this stuff, but I've commented on some of it.
            orbitalPrefab.shouldRotate = false; //This determines if the guon stone rotates. If set to true, the stone will rotate so that it always faces towards the player. Most Guons have this set to false, and you probably should too unless you have a good reason for changing it.
            orbitalPrefab.orbitRadius = 2f; //This determines how far away from you the guon orbits. The default for most guons is 2.5.
            orbitalPrefab.orbitDegreesPerSecond = 400; //This determines how many degrees of rotation the guon travels per second. The default for most guons is 120.
            orbitalPrefab.perfectOrbitalFactor = 9999f; //This determines how fast guons will move to catch up with their owner (regular guons have it set to 0 so they lag behind). You can probably ignore this unless you want or need your guon to stick super strictly to it's orbit.
            orbitalPrefab.SetOrbitalTier(0);

            GameObject.DontDestroyOnLoad(prefab);
            FakePrefab.MarkAsFakePrefab(prefab);
            prefab.SetActive(false);
        }

        public static void BuildSynergyPrefab()
        {
            bool flag = SpinGuonStone.upgradeOrbitalPrefab == null;
            if (flag)
            {
                GameObject gameObject = SpriteBuilder.SpriteFromResource("BleakMod/Resources/spinner_guon_stone", null); //(The orbital appearance with it's special synergy) MAKE SURE TO CHANGE THE SPRITE PATH TO YOUR OWN MODS
                gameObject.name = "Spin Guon Orbital Synergy Form";
                SpeculativeRigidbody speculativeRigidbody = gameObject.GetComponent<tk2dSprite>().SetUpSpeculativeRigidbody(IntVector2.Zero, new IntVector2(14, 14));
                SpinGuonStone.upgradeOrbitalPrefab = gameObject.AddComponent<PlayerOrbital>();
                speculativeRigidbody.CollideWithTileMap = false;
                speculativeRigidbody.CollideWithOthers = true;
                speculativeRigidbody.PrimaryPixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;
                SpinGuonStone.upgradeOrbitalPrefab.shouldRotate = false; //Determines if your guon rotates with it's special synergy
                SpinGuonStone.upgradeOrbitalPrefab.orbitRadius = 1.5f; //Determines how far your guon orbits with it's special synergy
                SpinGuonStone.upgradeOrbitalPrefab.orbitDegreesPerSecond = 600; //Determines how fast your guon orbits with it's special synergy
                SpinGuonStone.upgradeOrbitalPrefab.perfectOrbitalFactor = 9999f; //Determines how fast your guon will move to catch up with its owner with it's special synergy. By default, even though the regular guons have it at 0, the upgraded synergy guons all have a higher perfectOrbitalFactor. I find 10 to be about the same.
                SpinGuonStone.upgradeOrbitalPrefab.SetOrbitalTier(0);
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                FakePrefab.MarkAsFakePrefab(gameObject);
                gameObject.SetActive(false);
            }
        }

        public override void OnOrbitalCreated(GameObject orbital)
        {
            SpeculativeRigidbody orbBody = orbital.GetComponent<SpeculativeRigidbody>();
            if (orbBody)
            {
                orbBody.specRigidbody.OnPreRigidbodyCollision += this.HandlePreCollision;
            }
            base.OnOrbitalCreated(orbital);
        }

        private void HandlePreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
        {
            if (otherRigidbody && otherRigidbody.projectile)
            {
                if (otherRigidbody.projectile.Owner is AIActor)
                {
                    this.ReflectBullet(otherRigidbody.projectile, true, base.Owner, 10f, 1f, 1f, 0f);
                }
                PhysicsEngine.SkipCollision = true;
            }
        }

        // Token: 0x06007297 RID: 29335 RVA: 0x002CA4A0 File Offset: 0x002C86A0
        public void ReflectBullet(Projectile p, bool retargetReflectedBullet, GameActor newOwner, float minReflectedBulletSpeed, float scaleModifier = 1f, float damageModifier = 1f, float spread = 0f)
        {
            p.RemoveBulletScriptControl();
            AkSoundEngine.PostEvent("Play_OBJ_metalskin_deflect_01", GameManager.Instance.gameObject);
            bool flag = retargetReflectedBullet && p.Owner && p.Owner.specRigidbody;
            if (flag)
            {
                p.Direction = (p.Owner.specRigidbody.GetUnitCenter(ColliderType.HitBox) - p.specRigidbody.UnitCenter).normalized;
            }
            bool flag2 = spread != 0f;
            if (flag2)
            {
                p.Direction = p.Direction.Rotate(UnityEngine.Random.Range(-spread, spread));
            }
            bool flag3 = p.Owner && p.Owner.specRigidbody;
            if (flag3)
            {
                p.specRigidbody.DeregisterSpecificCollisionException(p.Owner.specRigidbody);
            }
            p.Owner = newOwner;
            p.SetNewShooter(newOwner.specRigidbody);
            p.allowSelfShooting = false;
            p.collidesWithPlayer = false;
            p.collidesWithEnemies = true;
            bool flag4 = scaleModifier != 1f;
            if (flag4)
            {
                SpawnManager.PoolManager.Remove(p.transform);
                p.RuntimeUpdateScale(scaleModifier);
            }
            bool flag5 = p.Speed < minReflectedBulletSpeed;
            if (flag5)
            {
                p.Speed = minReflectedBulletSpeed;
            }
            bool flag6 = p.baseData.damage < ProjectileData.FixedFallbackDamageToEnemies;
            if (flag6)
            {
                p.baseData.damage = ProjectileData.FixedFallbackDamageToEnemies;
            }
            p.baseData.damage *= damageModifier;
            bool flag7 = p.baseData.damage < 10f;
            if (flag7)
            {
                p.baseData.damage = 15f;
            }
            p.UpdateCollisionMask();
            p.Reflected();
            p.SendInDirection(p.Direction, true, true);
        }
        protected override void Update()
        {
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