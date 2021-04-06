using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;

namespace BleakMod
{
    public class WhiteBulletCell: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "White Bullet Cell";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/white_bullet_cell";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<WhiteBulletCell>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Leuko-Sight";
            string longDesc = "The more you kill an enemy, the more damage you will do to that type of enemy.\n\nThis cell might seem weak, but billions of years in the Oubliette taught it to adapt and evolve. It learns the weaknesses of enemies after each kill, and becomes stronger and stronger";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            if (!proj.TreatedAsNonProjectileForChallenge)
            {
                proj.OnHitEnemy += this.OnHitEnemy;
            }
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            if(!fatal && enemy.aiActor.healthHaver && enemy.aiActor.EnemyGuid != null && this.affectedEnemies.ContainsKey(enemy.aiActor.EnemyGuid))
            {
                enemy.aiActor.healthHaver.ApplyDamage((proj.baseData.damage / 10) * this.affectedEnemies[enemy.aiActor.EnemyGuid], Vector2.zero, this.Owner.ActorName, CoreDamageTypes.None, DamageCategory.Normal, false, null, false);
            }
        }
        private void OnKilledEnemyContext(PlayerController player, HealthHaver enemy)
        {
            if (this.affectedEnemies.ContainsKey(enemy.aiActor.EnemyGuid))
            {
                this.affectedEnemies[enemy.aiActor.EnemyGuid] += 1;
            }
            else
            {
                this.affectedEnemies.Add(enemy.aiActor.EnemyGuid, 1);
            }
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
            user.PostProcessProjectile += this.PostProcessProjectile;
            user.OnKilledEnemyContext += this.OnKilledEnemyContext;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            player.PostProcessProjectile -= this.PostProcessProjectile;
            player.OnKilledEnemyContext -= this.OnKilledEnemyContext;
            return base.Drop(player);
        }
        public Dictionary<string, int> affectedEnemies = new Dictionary<string, int>();
    }
}