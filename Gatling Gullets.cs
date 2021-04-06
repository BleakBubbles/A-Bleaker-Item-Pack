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
    public class GatlingGullets: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Gatling Gullets";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/gatling_gullets";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<GatlingGullets>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Rains From Above";
            string longDesc = "Bullets forged from the soul of the Gatling Gull.\n\nEach hit on an enemy has a chance to summon 3 rockets from the sky to the enemy's position.\nBe wary, as these rockets can damage the player as well.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            proj.OnHitEnemy += this.OnHitEnemy;
        }
        private void OnHitEnemy(Projectile proj, SpeculativeRigidbody enemy, bool fatal)
        {
            if (UnityEngine.Random.value <= 0.20f)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.FireRocket(enemy.aiActor);
                }

                if (this.m_spawnedRockets > 0 && (BossKillCam.BossDeathCamRunning || GameManager.Instance.PreventPausing))
                {
                    this.Cleanup();
                }
            }
        }
        private void FireRocket(AIActor enemy)
        {
            var cm = UnityEngine.Object.Instantiate<GameObject>((GameObject)BraveResources.Load("Global Prefabs/_ChallengeManager", ".prefab"));
            this.Rocket = (cm.GetComponent<ChallengeManager>().PossibleChallenges.Where(c => c.challenge is SkyRocketChallengeModifier).First().challenge as SkyRocketChallengeModifier).Rocket;
            UnityEngine.Object.Destroy(cm);
            if (BossKillCam.BossDeathCamRunning)
            {
                return;
            }
            if (GameManager.Instance.PreventPausing)
            {
                return;
            }
            SkyRocket component = SpawnManager.SpawnProjectile(this.Rocket, Vector3.zero, Quaternion.identity, true).GetComponent<SkyRocket>();
            component.Target = enemy.specRigidbody;
            tk2dSprite componentInChildren = component.GetComponentInChildren<tk2dSprite>();
            component.transform.position = component.transform.position.WithY(component.transform.position.y - componentInChildren.transform.localPosition.y);
            this.m_spawnedRockets++;
        }
        public void Cleanup()
        {
            this.m_spawnedRockets = 0;
            SkyRocket[] array = UnityEngine.Object.FindObjectsOfType<SkyRocket>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i])
                {
                    array[i].DieInAir();
                }
            }
        }
        public PlayerController yourMethod(PlayerController player)
        {
            return player;
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.PostProcessProjectile += this.PostProcessProjectile;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            player.PostProcessProjectile -= this.PostProcessProjectile;
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            base.Owner.PostProcessProjectile -= this.PostProcessProjectile;
        }
        public GameObject Rocket;
        private int m_spawnedRockets;
    }
}