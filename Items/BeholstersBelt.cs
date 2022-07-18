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
    class BeholstersBelt : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Beholster's Belt";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/beholsters_belt";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<BeholstersBelt>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Belt of Many Guns";
            string longDesc = "Pulls 6 random guns from the player's inventory and sends them into orbit around the player for a brief period of time. Unfortunately, it does not work with beam weapons.\n\nThis stolen belt is awesome and all, but seeing the Beholster without it is something you are still trying to forget.";
            item.consumable = false;
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 300);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.quality = ItemQuality.S;
        }
        protected override void DoEffect(PlayerController user)
        {
            this.StartEffect(user);
            base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.EndEffect)));
        }
        private void StartEffect(PlayerController user)
        {
            int x = UnityEngine.Random.Range(0, user.inventory.AllGuns.Count);
            Gun gun = user.inventory.AllGuns[x];
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ResourceCache.Acquire("Global Prefabs/HoveringGun") as GameObject, base.LastOwner.CenterPosition.ToVector3ZisY(0f), Quaternion.identity);
            gameObject.transform.parent = base.LastOwner.transform;
            HoveringGunController hover = gameObject.GetComponent<HoveringGunController>();
            hover.ConsumesTargetGunAmmo = false;
            hover.ChanceToConsumeTargetGunAmmo = 0f;
            hover.Position = HoveringGunController.HoverPosition.CIRCULATE;
            hover.Aim = HoveringGunController.AimType.NEAREST_ENEMY;
            hover.Trigger = HoveringGunController.FireType.ON_FIRED_GUN;
            hover.CooldownTime = gun.DefaultModule.cooldownTime;
            hover.ShootDuration = 0.1f;
            hover.OnlyOnEmptyReload = false;
            hover.Initialize(gun, base.LastOwner);
            this.hovers.Add(hover);
            Gun gun2 = user.inventory.AllGuns[(x + 1) % user.inventory.AllGuns.Count];
            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(ResourceCache.Acquire("Global Prefabs/HoveringGun") as GameObject, base.LastOwner.CenterPosition.ToVector3ZisY(0f), Quaternion.identity);
            gameObject2.transform.parent = base.LastOwner.transform;
            HoveringGunController hover2 = gameObject2.GetComponent<HoveringGunController>();
            this.hovers.Add(hover2);
            hover2.ConsumesTargetGunAmmo = false;
            hover2.ChanceToConsumeTargetGunAmmo = 0f;
            hover2.Position = HoveringGunController.HoverPosition.CIRCULATE;
            hover2.Aim = HoveringGunController.AimType.NEAREST_ENEMY;
            hover2.Trigger = HoveringGunController.FireType.ON_FIRED_GUN;
            hover2.CooldownTime = gun2.DefaultModule.cooldownTime;
            hover2.ShootDuration = 0.1f;
            hover2.OnlyOnEmptyReload = false;
            hover2.Initialize(gun2, base.LastOwner);
            Gun gun3 = user.inventory.AllGuns[(x + 2) % user.inventory.AllGuns.Count];
            GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(ResourceCache.Acquire("Global Prefabs/HoveringGun") as GameObject, base.LastOwner.CenterPosition.ToVector3ZisY(0f), Quaternion.identity);
            gameObject3.transform.parent = base.LastOwner.transform;
            HoveringGunController hover3 = gameObject3.GetComponent<HoveringGunController>();
            this.hovers.Add(hover3);
            hover3.ConsumesTargetGunAmmo = false;
            hover3.ChanceToConsumeTargetGunAmmo = 0f;
            hover3.Position = HoveringGunController.HoverPosition.CIRCULATE;
            hover3.Aim = HoveringGunController.AimType.NEAREST_ENEMY;
            hover3.Trigger = HoveringGunController.FireType.ON_FIRED_GUN;
            hover3.CooldownTime = gun3.DefaultModule.cooldownTime;
            hover3.ShootDuration = 0.1f;
            hover3.OnlyOnEmptyReload = false;
            hover3.Initialize(gun3, base.LastOwner);
            Gun gun4 = user.inventory.AllGuns[(x + 3) % user.inventory.AllGuns.Count];
            GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(ResourceCache.Acquire("Global Prefabs/HoveringGun") as GameObject, base.LastOwner.CenterPosition.ToVector3ZisY(0f), Quaternion.identity);
            gameObject4.transform.parent = base.LastOwner.transform;
            HoveringGunController hover4 = gameObject4.GetComponent<HoveringGunController>();
            this.hovers.Add(hover4);
            hover4.ConsumesTargetGunAmmo = false;
            hover4.ChanceToConsumeTargetGunAmmo = 0f;
            hover4.Position = HoveringGunController.HoverPosition.CIRCULATE;
            hover4.Aim = HoveringGunController.AimType.NEAREST_ENEMY;
            hover4.Trigger = HoveringGunController.FireType.ON_FIRED_GUN;
            hover4.CooldownTime = gun4.DefaultModule.cooldownTime;
            hover4.ShootDuration = 0.1f;
            hover4.OnlyOnEmptyReload = false;
            hover4.Initialize(gun4, base.LastOwner);
            Gun gun5 = user.inventory.AllGuns[(x + 4) % user.inventory.AllGuns.Count];
            GameObject gameObject5 = UnityEngine.Object.Instantiate<GameObject>(ResourceCache.Acquire("Global Prefabs/HoveringGun") as GameObject, base.LastOwner.CenterPosition.ToVector3ZisY(0f), Quaternion.identity);
            gameObject5.transform.parent = base.LastOwner.transform;
            HoveringGunController hover5 = gameObject5.GetComponent<HoveringGunController>();
            this.hovers.Add(hover5);
            hover5.ConsumesTargetGunAmmo = false;
            hover5.ChanceToConsumeTargetGunAmmo = 0f;
            hover5.Position = HoveringGunController.HoverPosition.CIRCULATE;
            hover5.Aim = HoveringGunController.AimType.NEAREST_ENEMY;
            hover5.Trigger = HoveringGunController.FireType.ON_FIRED_GUN;
            hover5.CooldownTime = gun5.DefaultModule.cooldownTime;
            hover5.ShootDuration = 0.1f;
            hover5.OnlyOnEmptyReload = false;
            hover5.Initialize(gun5, base.LastOwner);
            Gun gun6 = user.inventory.AllGuns[(x + 5) % user.inventory.AllGuns.Count];
            GameObject gameObject6 = UnityEngine.Object.Instantiate<GameObject>(ResourceCache.Acquire("Global Prefabs/HoveringGun") as GameObject, base.LastOwner.CenterPosition.ToVector3ZisY(0f), Quaternion.identity);
            gameObject6.transform.parent = base.LastOwner.transform;
            HoveringGunController hover6 = gameObject6.GetComponent<HoveringGunController>();
            this.hovers.Add(hover6);
            hover6.ConsumesTargetGunAmmo = false;
            hover6.ChanceToConsumeTargetGunAmmo = 0f;
            hover6.Position = HoveringGunController.HoverPosition.CIRCULATE;
            hover6.Aim = HoveringGunController.AimType.NEAREST_ENEMY;
            hover6.Trigger = HoveringGunController.FireType.ON_FIRED_GUN;
            hover6.CooldownTime = gun6.DefaultModule.cooldownTime;
            hover6.ShootDuration = 0.1f;
            hover6.OnlyOnEmptyReload = false;
            hover6.Initialize(gun6, base.LastOwner);
        }
        private void EndEffect(PlayerController user)
        {
            for (int i = 0; i < this.hovers.Count; i++)
            {
                if (this.hovers[i])
                {
                    UnityEngine.Object.Destroy(this.hovers[i].gameObject);
                }
            }
            this.hovers.Clear();
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        public float duration = 10f;
        private List<HoveringGunController> hovers = new List<HoveringGunController>();
    }
}