using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;
using MultiplayerBasicExample;

namespace BleakMod
{
    public class WinchestersHat : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Winchester's Hat";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/winchester_hats/winchester_hat_right";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<WinchestersHat>();


            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "M'lady";
            string longDesc = "Permanently charms all enemies in a radius upon active item use.\n\n" +
                "Wearing this hat makes you feel as cold and as shady as its original owner. The enemies, however, seem to be all over your new look...";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
            CustomSynergies.Add("Wincheese-ter", new List<string>
            {
                "bb:winchester's_hat"
            }, new List<string>
            {
                "prize_pistol",
            }, true);
            BuildPrefab();
        }
        public override void Pickup(PlayerController player)
		{
			if (this.m_pickedUp)
			{
				return;
			}
			base.Pickup(player);
            this.InternalCooldown = 5f;
            this.m_lastUsedTime = -1000f;
            player.OnUsedPlayerItem += this.DoEffect;
		}
		private void DoEffect(PlayerController usingPlayer, PlayerItem usedItem)
		{
            if (Time.realtimeSinceStartup - this.m_lastUsedTime < this.InternalCooldown)
            {
                return;
            }
            this.m_lastUsedTime = Time.realtimeSinceStartup;
            usingPlayer.CurrentRoom.ApplyActionToNearbyEnemies(usingPlayer.transform.position.XY(), 20f, this.ProcessEnemy);
        }
        private void ProcessEnemy(AIActor a, float distance)
        {
            if (a && a.IsNormalEnemy && a.healthHaver && !a.IsGone && !a.healthHaver.IsBoss)
            {
                a.ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect, 1f, null);
                a.IgnoreForRoomClear = true;
                a.gameObject.AddComponent<KillOnRoomClear>();
            }
        }
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            player.OnUsedPlayerItem -= this.DoEffect;
            this.m_pickedUpThisRun = true;
            return debrisObject;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (base.Owner != null)
            {
                base.Owner.OnUsedPlayerItem -= this.DoEffect;
            }
        }
        public static void BuildPrefab()
        {
            GameObject gameObject = SpriteBuilder.SpriteFromResource("BleakMod/Resources/winchester_hats/winchester_hat_right", null);
            gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(gameObject);
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            GameObject gameObject2 = new GameObject("Badaboom");
            tk2dSprite tk2dSprite = gameObject2.AddComponent<tk2dSprite>();
            tk2dSprite.SetSprite(gameObject.GetComponent<tk2dBaseSprite>().Collection, gameObject.GetComponent<tk2dBaseSprite>().spriteId);
            WinchestersHat.spriteIds.Add(SpriteBuilder.AddSpriteToCollection("BleakMod/Resources/winchester_hats/winchester_hat_left", tk2dSprite.Collection));
            WinchestersHat.spriteIds.Add(SpriteBuilder.AddSpriteToCollection("BleakMod/Resources/winchester_hats/winchester_hat_back", tk2dSprite.Collection));
            tk2dSprite.GetCurrentSpriteDef().material.shader = ShaderCache.Acquire("Brave/PlayerShader");
            WinchestersHat.spriteIds.Add(tk2dSprite.spriteId);
            gameObject2.SetActive(false);
            tk2dSprite.SetSprite(WinchestersHat.spriteIds[0]);
            tk2dSprite.GetCurrentSpriteDef().material.shader = ShaderCache.Acquire("Brave/PlayerShader");
            tk2dSprite.SetSprite(WinchestersHat.spriteIds[1]);
            tk2dSprite.GetCurrentSpriteDef().material.shader = ShaderCache.Acquire("Brave/PlayerShader");
            tk2dSprite.SetSprite(WinchestersHat.spriteIds[2]);
            tk2dSprite.GetCurrentSpriteDef().material.shader = ShaderCache.Acquire("Brave/PlayerShader");
            FakePrefab.MarkAsFakePrefab(gameObject2);
            UnityEngine.Object.DontDestroyOnLoad(gameObject2);
            WinchestersHat.boomprefab = gameObject2;
        }
        private void SpawnVFXAttached()
        {
            GameObject boomprefab1 = UnityEngine.Object.Instantiate<GameObject>(WinchestersHat.boomprefab, base.Owner.transform.position + new Vector3(0.3f, 1.05f, -5f), Quaternion.identity);
            boomprefab1.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(base.Owner.specRigidbody.UnitCenter, tk2dBaseSprite.Anchor.MiddleCenter);
            GameManager.Instance.StartCoroutine(this.HandleSprite(boomprefab1));
            HatObject = boomprefab1;
        }
        protected override void Update()
        {
            if (base.Owner && !HatObject && base.Owner.CurrentGun.sprite)
            {
                this.SpawnVFXAttached();
            }
            if (!base.Owner.CurrentGun.sprite && HatObject)
            {
                Destroy(HatObject);
            }
            if (base.Owner.CurrentGun.PickupObjectId == 251)
            {
                base.Owner.CurrentGun.InfiniteAmmo = true;
            }
            base.Update();
        }
        private IEnumerator HandleSprite(GameObject prefab)
        {
            while (prefab != null && base.Owner != null)
            {
                prefab.transform.position = base.Owner.transform.position + new Vector3(0.3f, 1.05f, -5f);
                if (base.Owner.IsFalling)
                {
                    prefab.GetComponent<tk2dBaseSprite>().renderer.enabled = false;
                }
                else
                {
                    prefab.GetComponent<tk2dBaseSprite>().renderer.enabled = true;
                }
                if (base.Owner.IsBackfacing())
                {
                    prefab.GetComponent<tk2dBaseSprite>().SetSprite(WinchestersHat.spriteIds[1]);
                }
                if (!base.Owner.IsBackfacing() && this.m_owner.CurrentGun.sprite.WorldCenter.x - this.m_owner.specRigidbody.UnitCenter.x < 0f)
                {
                    prefab.GetComponent<tk2dBaseSprite>().SetSprite(WinchestersHat.spriteIds[0]);
                }
                if (!base.Owner.IsBackfacing() && this.m_owner.CurrentGun.sprite.WorldCenter.x - this.m_owner.specRigidbody.UnitCenter.x > 0f)
                {
                    prefab.GetComponent<tk2dBaseSprite>().SetSprite(WinchestersHat.spriteIds[2]);
                }

                yield return null;
            }
            Destroy(prefab.gameObject);
            yield break;
        }
        private static GameObject boomprefab;
        private GameObject HatObject;
        public static List<int> spriteIds = new List<int>();
        public float InternalCooldown;
        private float m_lastUsedTime;
    }
}