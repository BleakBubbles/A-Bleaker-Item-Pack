using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using Dungeonator;
using JetBrains.Annotations;
using InControl.NativeProfile;

namespace BleakMod
{
    class FatalOptics : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Fatal Optics";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/fatal_optics";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<FatalOptics>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Step right up.";
            string longDesc = "Slows down time, rooting you to the spot and allowing you to aim a shot at every enemy in the room.\nThe more time you take to aim, the more damage the shot does. Press the fire button to shoot.";
            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 500f);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Curse, 1, StatModifier.ModifyMethod.ADDITIVE);
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.A;
        }
        public IEnumerator Timelapsed()
        {
            time = 0f;
            while (this.m_isCurrentlyActive)
            {
                time += BraveTime.DeltaTime;
                yield return null;
            }
            yield break;
        }
        protected override void DoEffect(PlayerController user)
        {
            this.StartEffect(user);
            base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.EndEffect)));
            base.StartCoroutine(this.Timelapsed());
            user.OnTriedToInitiateAttack += EndEffect;
        }
        private void StartEffect(PlayerController user)
        {
            test.RadialSlowHoldTime = 6f;
            test.RadialSlowOutTime = 1.5f;
            test.RadialSlowTimeModifier = 0.25f;
            test.DoesSepia = true;
            test.DoRadialSlow(user.CenterPosition, user.CurrentRoom);
            user.MovementModifiers += NoMotionModifier;
            user.IsGunLocked = true;
            user.IsStationary = true;
        }
        private void EndEffect(PlayerController user)
        {
            user.CurrentRoom.ApplyActionToNearbyEnemies(user.transform.position.XY(), 100f, new Action<AIActor, float>(this.ProcessEnemy));
            this.m_isCurrentlyActive = false;
            user.IsGunLocked = false;
            user.IsStationary = false;
            user.MovementModifiers -= NoMotionModifier;
            user.OnTriedToInitiateAttack -= EndEffect;
        }
        private void ProcessEnemy(AIActor a, float distance)
        {
            if (a && a.IsNormalEnemy && a.healthHaver && !a.IsGone)
            {
                a.healthHaver.ApplyDamage(time * 30, Vector2.zero, this.LastOwner.ActorName, CoreDamageTypes.None, DamageCategory.Normal, false, null, false);
            }
        }
        private void NoMotionModifier(ref Vector2 voluntaryVel, ref Vector2 involuntaryVel)
        {
            voluntaryVel = Vector2.zero;
        }
        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);

        }
        protected override void OnPreDrop(PlayerController user)
        {
            if (base.m_isCurrentlyActive)
            {
                this.EndEffect(user);
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.m_isCurrentlyActive && base.LastOwner != null)
            {
                this.EndEffect(base.LastOwner);
            }
        }
        public RadialSlowInterface test = new RadialSlowInterface();
        public float time;
        public float duration = 6f;
    }
}