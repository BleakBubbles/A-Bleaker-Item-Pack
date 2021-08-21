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
    public class FittedTankBarrel: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Fitted Tank Barrel";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/fitted_tank_barrel";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<FittedTankBarrel>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "I'd Rather Naught";
            string longDesc = "Each shot fired has a chance to shoot a special shot from the arsenal of the Treadnaught.\n\nWho would have thought that detaching the barrel from the treadnaught and attaching it to your guns is a great idea?";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            float num = this.activationChance * f;
            if (proj.PossibleSourceGun)
            {
                ETGModConsole.Log(num.ToString());
                if (this.x % 3 == 1 && UnityEngine.Random.value <= num)
                {
                    Projectile projectile = ((Gun)ETGMod.Databases.Items[486]).DefaultModule.projectiles[0];
                    GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.Owner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.Owner.CurrentGun == null) ? 0f : base.Owner.FacingDirection), true);
                    Projectile proj1 = obj1.GetComponent<Projectile>();
                    proj1.Owner = base.Owner;
                    proj1.Shooter = base.Owner.specRigidbody;
                }
                else if(this.x % 3 == 2 && UnityEngine.Random.value <= num)
                {
                    Projectile projectile = ((Gun)ETGMod.Databases.Items[372]).DefaultModule.projectiles[0];
                    GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.Owner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.Owner.CurrentGun == null) ? 0f : base.Owner.FacingDirection), true);
                    Projectile proj1 = obj1.GetComponent<Projectile>();
                    proj1.Owner = base.Owner;
                    proj1.Shooter = base.Owner.specRigidbody;
                    proj1.OnDestruction += this.OnDestruction;
                    this.shouldSpawnEnemy = false;
                }
                else if(this.x % 3 == 0 && UnityEngine.Random.value <= num)
                {
                    proj.OnDestruction += this.OnDestruction;
                    this.shouldSpawnEnemy = true;
                }
                x++;
            }
        }
        private void OnDestruction(Projectile proj)
        {
            if(!this.shouldSpawnEnemy)
            {
                for(int i = 0; i < 8; i++)
                {
                    Projectile projectile = ((Gun)ETGMod.Databases.Items[22]).DefaultModule.projectiles[0];
                    GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, proj.specRigidbody.UnitCenter, Quaternion.Euler(0f, 0f, (base.Owner.CurrentGun == null) ? 0f : 45 * i), true);
                    Projectile proj1 = obj1.GetComponent<Projectile>();
                    proj1.Owner = base.Owner;
                    proj1.Shooter = base.Owner.specRigidbody;
                }
            }
            else if(this.shouldSpawnEnemy)
            {
                try
                {
                    AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid("df7fb62405dc4697b7721862c7b6b3cd");
                    IntVector2? intVector = proj.specRigidbody.UnitCenter.ToIntVector2();
                    bool flag = intVector != null;
                    if (flag)
                    {
                        AIActor aiactor = AIActor.Spawn(orLoadByGuid.aiActor, intVector.Value, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector.Value), true, AIActor.AwakenAnimationType.Default, true);
                        aiactor.healthHaver.IsVulnerable = false;
                        aiactor.ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect, 1f, null);
                        aiactor.IgnoreForRoomClear = true;
                        aiactor.IsHarmlessEnemy = true;
                        aiactor.gameObject.AddComponent<KillOnRoomClear>();
                        aiactor.bulletBank.OnProjectileCreated += this.NotDamagePlayer;
                        this.charmedEnemies.Add(aiactor);
                    }
                }
                catch (Exception ex)
                {
                    ETGModConsole.Log(ex.Message, false);
                }
            }
        }
        private void killCharmedEnemies(PlayerController player)
        {
            foreach(AIActor aiactor in this.charmedEnemies)
            {
                if (aiactor)
                {
                    aiactor.healthHaver.IsVulnerable = true;
                    aiactor.healthHaver.ApplyDamage(float.PositiveInfinity, Vector2.zero, "room cleared");
                }
            }
        }
        public void NotDamagePlayer(Projectile proj)
        {
            proj.collidesWithPlayer = false;
            proj.UpdateCollisionMask();
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.PostProcessProjectile += this.PostProcessProjectile;
            player.OnRoomClearEvent += this.killCharmedEnemies;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            player.PostProcessProjectile -= this.PostProcessProjectile;
            player.OnRoomClearEvent -= this.killCharmedEnemies;
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.Owner != null)
            {
                base.Owner.PostProcessProjectile -= this.PostProcessProjectile;
                base.Owner.OnRoomClearEvent -= this.killCharmedEnemies;
            }
        }
        private float activationChance = 0.1f;
        private int x = 1;
        private bool shouldSpawnEnemy = false;
        private List<AIActor> charmedEnemies = new List<AIActor>();
    }
}