using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Gungeon;
using MonoMod;
using MultiplayerBasicExample;
using Dungeonator;

namespace BleakMod
{
    class DemonicBrick : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Demonic Brick";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/brick";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);
            
            //Add a ActiveItem component to the object
            var item = obj.AddComponent<DemonicBrick>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Brick Slayer";
            string longDesc = "Summons a wall from the middle-left of the player's room to sweep across it horizontally, destroying any enemies in its path.\n\nA glowing brick straight from the villainous wall itself - you risked your life for this stone, so use it wisely.";
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            item.consumable = false;
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 1);
            //Set the cooldown type and duration of the cooldown
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.quality = ItemQuality.EXCLUDED;
        }
        protected override void DoEffect(PlayerController user)
        {
            this.wallBullets();
        }
        private void wallBullets()
        {
            CellArea area = base.LastOwner.CurrentRoom.area;
            Vector2 vector = area.basePosition.ToVector2();
            //float shrink = area.dimensions.x / 24;
            Projectile projectile = ((Gun)ETGMod.Databases.Items[22]).DefaultModule.projectiles[0];
                GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, new Vector2(vector.x + (area.dimensions.x - 24) / 2, vector.y + area.dimensions.y - 3), Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 270), true);
                Projectile proj1 = obj1.GetComponent<Projectile>();
                proj1.Owner = base.gameActor;
                proj1.Shooter = base.LastOwner.specRigidbody;
            proj1.IgnoreTileCollisionsFor(100f);
            proj1.SetProjectileSpriteRight("demonwal_move_001", 384, 103);
                proj1.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.BulletBlocker));
                proj1.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.BulletBreakable));
                proj1.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.HighObstacle));
                //proj1.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox));
                //proj1.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyCollider));
                //proj1.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyBlocker));
                proj1.baseData.range = 50;
                //ETGModConsole.Log("" + area.dimensions.x);
                proj1.baseData.damage = 200f;
                //proj1.AdditionalScaleMultiplier = vector.x / 12;
                proj1.baseData.speed *= 0.5f;
                proj1.specRigidbody.CollideWithTileMap = false;
                proj1.UpdateCollisionMask();
                PierceProjModifier pierce = proj1.gameObject.GetOrAddComponent<PierceProjModifier>();
                pierce.penetration = 100;
                pierce.MaxBossImpacts = 1;
                proj1.ImmuneToBlanks = true;
                proj1.pierceMinorBreakables = true;
                //    ETGModConsole.Log("Big man incoming");
                //}

            //else
            //{
            //    GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, new Vector2(vector.x, vector.y + area.dimensions.y - 1), Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 270), true);
            //    Projectile proj1 = obj1.GetComponent<Projectile>();
            //    proj1.Owner = base.gameActor;
            //    proj1.Shooter = base.LastOwner.specRigidbody;
            //    proj1.SetProjectileSpriteRight("demonwal_move_001", area.dimensions.x * 16, 103 * (area.dimensions.x / 24), area.dimensions.x * 8, 26);
            //    proj1.IgnoreTileCollisionsFor(100f);
            //    ETGModConsole.Log("" + area.dimensions.x);
            //    proj1.baseData.damage = 200f;
            //    //proj1.AdditionalScaleMultiplier = vector.x / 12;
            //    proj1.baseData.speed *= 0.5f;
            //    proj1.baseData.range *= 10f;
            //    proj1.specRigidbody.CollideWithTileMap = false;
            //    proj1.UpdateCollisionMask();
            //    PierceProjModifier pierce = proj1.gameObject.GetOrAddComponent<PierceProjModifier>();
            //    pierce.penetration = 100;
            //    pierce.MaxBossImpacts = 1;
            //    proj1.ImmuneToBlanks = true;
            //    proj1.pierceMinorBreakables = true;
            //    ETGModConsole.Log("Small man incoming");
            //}
            /*
            Projectile projectile2 = ((Gun)ETGMod.Databases.Items[22]).DefaultModule.projectiles[0];
            GameObject obj2 = SpawnManager.SpawnProjectile(projectile2.gameObject, new Vector2((vector.x + area.dimensions.x / 4) + 12, vector.y + area.dimensions.y - 5), Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : 270), true);
            Projectile proj2 = obj2.GetComponent<Projectile>();
            proj2.Owner = base.gameActor;
            proj2.Shooter = base.LastOwner.specRigidbody;
            proj2.IgnoreTileCollisionsFor(5f);
            ETGModConsole.Log("" + area.dimensions.x);
            proj2.SetProjectileSpriteRight("demonwal_move_002", 192, 103, 192, 32);
            proj2.baseData.damage = 200f;
            //proj2.AdditionalScaleMultiplier = vector.x / 12;
            proj2.baseData.speed *= 0.5f;
            proj2.baseData.range *= 10f;
            proj2.specRigidbody.CollideWithTileMap = false;
            proj2.UpdateCollisionMask();
            PierceProjModifier pierce2 = proj2.gameObject.GetOrAddComponent<PierceProjModifier>();
            pierce2.penetration = 100;
            pierce2.MaxBossImpacts = 1;
            proj2.ImmuneToBlanks = true;
            proj2.pierceMinorBreakables = true;
            */
        }
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        //public GameActorCharmEffect CharmEffect;
    }
}