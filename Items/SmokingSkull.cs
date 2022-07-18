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
    class SmokingSkull : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Smoking Skull";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/skull";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<SmokingSkull>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Hold Of The Iron Ring";
            string longDesc = "On use, chooses one of 3 of the Cannonbalrog's attacks to execute.\n\nAn artifact discovered in the binding tomb of a legendary beast. Its sole eye stares straight into your soul.";
            item.consumable = false;
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 400);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Curse, 1, StatModifier.ModifyMethod.ADDITIVE);
            ItemBuilder.AddToSubShop(item, ItemBuilder.ShopType.Cursula, 1f);
            //Set some other fields
            item.quality = ItemQuality.A;
        }
        protected override void DoEffect(PlayerController user)
        {
            if (base.LastOwner && base.LastOwner.CurrentRoom != null && x % 3 == 1)
            {
                base.StartCoroutine(this.sprinkler());
            }
            else if (base.LastOwner && base.LastOwner.CurrentRoom != null && x % 3 == 2)
            {
                base.StartCoroutine(this.twinBeams());
            }
            else if(x % 3 == 0)
            {
                this.StealthEffect();
                base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.BreakStealth)));
                base.StartCoroutine(this.handleCannonFire());
            }
            x++;
        }
        private IEnumerator handleCannonFire()
        {
            base.StartCoroutine(this.cannonFire());
            yield return new WaitForSeconds(2f);
            base.StartCoroutine(this.cannonFire());
            yield return new WaitForSeconds(2f);
            base.StartCoroutine(this.cannonFire());
        }
        private IEnumerator cannonFire()
        {
            Projectile projectile = ((Gun)ETGMod.Databases.Items[37]).DefaultModule.projectiles[0];
            AkSoundEngine.PostEvent("Play_ENM_cannonball_eyes_01", base.gameObject);
            float angle = base.LastOwner.FacingDirection;
            for (int i = 0; i < 5; i++)
            {
                float y = Mathf.Lerp(-4.5f, 4.5f, (float)i / 4f);
                GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter + new Vector2(1f, y), Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : angle), true);
                Projectile proj1 = obj1.GetComponent<Projectile>();
                proj1.Owner = base.LastOwner;
                proj1.Shooter = base.LastOwner.specRigidbody;
                proj1.baseData.speed = 0f;
                proj1.AdditionalScaleMultiplier = 0.75f;
                proj1.specRigidbody.CollideWithTileMap = false;
                if (proj1.GetComponent<BounceProjModifier>() == null)
                {
                    proj1.gameObject.AddComponent<BounceProjModifier>();
                }
                BounceProjModifier boing = proj1.GetComponent<BounceProjModifier>();
                boing.numberOfBounces = 1;
                proj1.StartCoroutine(this.delayedShot(proj1));
                yield return new WaitForSeconds(0.1f);
            }
        }
        private IEnumerator delayedShot(Projectile projectile)
        {
            yield return new WaitForSeconds(0.5f);
            projectile.baseData.speed = 13f;
            projectile.UpdateSpeed();
            AkSoundEngine.PostEvent("Play_WPN_elephantgun_shot_01", base.gameObject);
        }
        private IEnumerator twinBeams()
        {
            AkSoundEngine.PostEvent("Play_ENM_cannonball_whips_01", base.gameObject);
            Projectile projectile = ((Gun)ETGMod.Databases.Items[35]).DefaultModule.projectiles[0];
            for (int i = 0; i < 50; i++)
            {
                GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter + new Vector2(1, 0), Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : i * 10f), true);
                GameObject obj2 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter + new Vector2(-1, 0), Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : i * 10f), true);
                Projectile proj1 = obj1.GetComponent<Projectile>();
                Projectile proj2 = obj2.GetComponent<Projectile>();
                if (proj1 != null)
                {
                    proj1.Owner = base.LastOwner;
                    proj1.Shooter = base.LastOwner.specRigidbody;
                }
                if (proj2 != null)
                {
                    proj2.Owner = base.LastOwner;
                    proj2.Shooter = base.LastOwner.specRigidbody;
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
        private IEnumerator sprinkler()
        {
            AkSoundEngine.PostEvent("Play_ENM_cannonball_spray_01", base.gameObject);
            Projectile projectile = ((Gun)ETGMod.Databases.Items[35]).DefaultModule.projectiles[0];
            for(int i = 0; i < 50; i++)
            {
                float angle = base.LastOwner.FacingDirection;
                GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.LastOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (base.LastOwner.CurrentGun == null) ? 0f : UnityEngine.Random.Range(angle - 30, angle + 30)), true);
                Projectile proj1 = obj1.GetComponent<Projectile>();
                proj1.Owner = base.LastOwner;
                proj1.Shooter = base.LastOwner.specRigidbody;
                proj1.baseData.speed = 12f;
                yield return new WaitForSeconds(0.03f);
            }
        }
        private void StealthEffect()
        {
            PlayerController owner = base.LastOwner;
            this.BreakStealth(owner);
            owner.ChangeSpecialShaderFlag(1, 1f);
            owner.SetIsStealthed(true, "skull");
            owner.SetCapableOfStealing(true, "skull", null);
            owner.healthHaver.IsVulnerable = false;
            owner.IsGunLocked = true;
        }
        private void BreakStealth(PlayerController player)
        {
            player.ChangeSpecialShaderFlag(1, 0f);
            player.SetIsStealthed(false, "skull");
            player.SetCapableOfStealing(false, "skull", null);
            player.healthHaver.IsVulnerable = true;
            player.MovementModifiers -= this.NoMotionModifier;
            player.IsGunLocked = false;
            AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", base.gameObject);
        }
        private void NoMotionModifier(ref Vector2 voluntaryVel, ref Vector2 involuntaryVel)
        {
            voluntaryVel = Vector2.zero;
        }
        public override void Update()
        {
            base.Update();
            if(this.m_isCurrentlyActive && base.LastOwner && base.LastOwner.sprite != null)
            {
                HandlePickupCurseParticles(base.LastOwner.sprite);
            }
        }
        public override void Pickup(PlayerController user)
        {
            base.Pickup(user);
        }
        protected override void OnPreDrop(PlayerController user)
        {
            if (this.m_isCurrentlyActive)
            {
                this.BreakStealth(user);
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.m_isCurrentlyActive && base.LastOwner != null)
            {
                this.BreakStealth(base.LastOwner);
            }
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        private int x = 0;
        private float duration = 6f;
        //private float max;
    }
}