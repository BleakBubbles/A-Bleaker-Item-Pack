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
    class HornOfTheDragun : PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Horn Of The Dragun";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/draguns_horn";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<HornOfTheDragun>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "ROAR!";
            string longDesc = "Lets out a bone-piercing roar that makes enemies run for their lives.\n\nA horn of the legendary beast found in the depths of the forge.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 300);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.B;
        }
        protected override void DoEffect(PlayerController user)
        {
            if (this.fleeData == null || this.fleeData.Player != base.LastOwner)
            {
                this.fleeData = new FleePlayerData();
                this.fleeData.Player = base.LastOwner;
                this.fleeData.StartDistance *= 10f;
            }
            base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.EndEffect)));
            base.StartCoroutine(this.HandleDuration(user));
            base.StartCoroutine(this.FearDuration(user));
            GameActorEffect greenFire = (PickupObjectDatabase.GetById(722) as Gun).DefaultModule.projectiles[0].fireEffect;
            foreach (AIActor aiactor in user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
            {
                aiactor.ApplyEffect(greenFire);
            }
        }
        private IEnumerator HandleDuration(PlayerController user)
        {
            user.SetIsFlying(true, "horn");
            user.AdditionalCanDodgeRollWhileFlying.SetOverride("horn", true);
            Exploder.DoDistortionWave(user.sprite.WorldCenter, 0.5f, 0.04f, 20f, 1.5f);
            StatModifier item = new StatModifier
            {
                statToBoost = PlayerStats.StatType.Damage,
                amount = 2f,
                modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE
            };
            base.LastOwner.ownerlessStatModifiers.Add(item);
            StatModifier item2 = new StatModifier
            {
                statToBoost = PlayerStats.StatType.RateOfFire,
                amount = 2f,
                modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE
            };
            base.LastOwner.ownerlessStatModifiers.Add(item2);
            StatModifier item3 = new StatModifier
            {
                statToBoost = PlayerStats.StatType.Accuracy,
                amount = 0.5f,
                modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE
            };
            base.LastOwner.ownerlessStatModifiers.Add(item3);
            base.LastOwner.stats.RecalculateStats(base.LastOwner, true, false);
            yield return new WaitForSeconds(this.duration);
            base.LastOwner.ownerlessStatModifiers.Remove(item);
            base.LastOwner.ownerlessStatModifiers.Remove(item2);
            base.LastOwner.ownerlessStatModifiers.Remove(item3);
            base.LastOwner.stats.RecalculateStats(base.LastOwner, true, false);
            user.SetIsFlying(false, "horn");
            user.AdditionalCanDodgeRollWhileFlying.SetOverride("horn", false);
        }
        private IEnumerator FearDuration(PlayerController user)
        {
            this.time = 0f;
            while (this.time < this.duration && base.LastOwner.CurrentRoom != null)
            {
                this.HandleFear(user, true);
                this.time += BraveTime.DeltaTime;
                yield return null;
            }
            this.HandleFear(user, false);
            yield break;
        }
        private void HandleFear(PlayerController user, bool active)
        {
            RoomHandler currentRoom = user.CurrentRoom;
            bool flag = !currentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.All);
            if (!flag)
            {
                if (active)
                {
                    foreach (AIActor aiactor in currentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
                    {
                        bool flag2 = aiactor.behaviorSpeculator != null;
                        if (flag2)
                        {
                            aiactor.behaviorSpeculator.FleePlayerData = this.fleeData;
                            FleePlayerData fleePlayerData = new FleePlayerData();
                        }
                    }
                }
                else
                {
                    foreach (AIActor aiactor2 in currentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
                    {
                        bool flag3 = aiactor2.behaviorSpeculator != null && aiactor2.behaviorSpeculator.FleePlayerData != null;
                        if (flag3)
                        {
                            aiactor2.behaviorSpeculator.FleePlayerData.Player = null;
                        }
                    }
                }
            }
        }
        private void EndEffect(PlayerController user)
        {

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
            return base.CanBeUsed(user);
        }
        public float duration = 10f;
        private FleePlayerData fleeData;
        private float time;
    }
}