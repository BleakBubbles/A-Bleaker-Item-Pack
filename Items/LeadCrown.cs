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
    public class LeadCrown: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Lead Crown";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/lead_crown";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<LeadCrown>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Halls Of The Usurper";
            string longDesc = "Scares all enemies for a brief moment upon entering a room.\n\nA legendary crown worn by many a great king. Its owners were so brutal that just the sight of the crown instills fear in those who dare glance upon it.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
        }
        public void OnEnteredRoom()
        {
            if(this.fleeData == null || this.fleeData.Player != base.Owner)
            {
                this.fleeData = new FleePlayerData();
                this.fleeData.Player = base.Owner;
                this.fleeData.StartDistance *= 10f;
            }
            base.StartCoroutine(this.HandleDuration(base.Owner));
        }
        private IEnumerator HandleDuration(PlayerController user)
        {
            this.time = 0f;
            while (this.time < this.duration && base.Owner.CurrentRoom != null)
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
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnEnteredCombat += this.OnEnteredRoom;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            player.OnEnteredCombat -= this.OnEnteredRoom;
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            base.Owner.OnEnteredCombat -= this.OnEnteredRoom;
        }
        private FleePlayerData fleeData;
        private float duration = 4f;
        private float time;
    }
}