using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;
using MultiplayerBasicExample;

namespace BleakMod
{
    public class Distribullets: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Distribullets";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/distribullets";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Distribullets>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "a(b + c) = ab + ac";
            string longDesc = "Slightly increases most offensive stats. Each shot's damage is distributed evenly to every enemy in the room.\nSharing is caring!";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item
            item.AddPassiveStatModifier(PlayerStats.StatType.Damage, 0.15f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.RateOfFire, 0.15f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.ReloadSpeed, -0.15f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.ProjectileSpeed, 0.15f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.RangeMultiplier, 0.15f, StatModifier.ModifyMethod.ADDITIVE);
            item.AddPassiveStatModifier(PlayerStats.StatType.Accuracy, 0.15f, StatModifier.ModifyMethod.ADDITIVE);


            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.A;
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            if (base.Owner.IsInCombat)
            {
                this.activeEnemies = base.Owner.CurrentRoom.GetActiveEnemies(Dungeonator.RoomHandler.ActiveEnemyType.RoomClear);
                this.vulnEnemies = this.activeEnemies.Count;
                foreach (AIActor enemy in this.activeEnemies)
                {
                    if (!enemy.healthHaver.IsVulnerable || !enemy.healthHaver.CanCurrentlyBeKilled)
                    {
                        this.vulnEnemies -= 1;
                    }
                }
            }
            proj.baseData.damage /= this.vulnEnemies;
            proj.OnHitEnemy += this.OnHitEnemy;
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            foreach (AIActor actor in base.Owner.CurrentRoom.GetActiveEnemies(Dungeonator.RoomHandler.ActiveEnemyType.RoomClear))
            {
                if(actor.healthHaver != enemy.healthHaver)
                {
                    actor.healthHaver.ApplyDamage(proj.baseData.damage, Vector2.zero, this.Owner.ActorName, CoreDamageTypes.None, DamageCategory.Normal, false, null, false);
                }
            }
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
            user.PostProcessProjectile += this.PostProcessProjectile;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            player.PostProcessProjectile -= this.PostProcessProjectile;
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.Owner != null)
            {
                base.Owner.PostProcessProjectile -= this.PostProcessProjectile;
            }
        }
        public List<AIActor> activeEnemies = new List<AIActor>();
        public int vulnEnemies;
    }
}