using System;
using SaveAPI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;

namespace BleakMod
{
    public class TatteredCape : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Tattered Cape";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/tattered_cape";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<TatteredCape>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Am I A Bullet?";
            string longDesc = "Each room now has a small chance to have a red caped bullet kin. Sparing 5 of them will unlock a reward.\n\nA torn, dirty cape found somewhere in the depths of the Gungeon. It radiates a strange warmth, as if calling out to something.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item
            item.SetupUnlockOnFlag(GungeonFlags.SECRET_BULLETMAN_SEEN_05, true);
            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.A;
        }
        public void OnEnteredCombat()
        {
            this.isBossRoom = false;
            foreach (AIActor enemy in base.Owner.CurrentRoom.GetActiveEnemies(Dungeonator.RoomHandler.ActiveEnemyType.RoomClear))
            {
                if (enemy.healthHaver.IsBoss)
                {
                    this.isBossRoom = true;
                }
            }
            if (UnityEngine.Random.value <= 0.1f && base.Owner && !this.isBossRoom)
            {
                try
                {
                    AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid("fa6a7ac20a0e4083a4c2ee0d86f8bbf7");
                    IntVector2? intVector = new IntVector2?(base.Owner.CurrentRoom.GetRandomVisibleClearSpot(1, 1));
                    bool flag = intVector != null;
                    if (flag)
                    {
                        AIActor aiactor = AIActor.Spawn(orLoadByGuid.aiActor, intVector.Value, GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector.Value), true, AIActor.AwakenAnimationType.Default, true);
                        aiactor.healthHaver.OnPreDeath += this.onPreDeath;
                    }
                }
                catch (Exception ex)
                {
                    ETGModConsole.Log(ex.Message, false);
                }
                this.timesSpawned += 1;
            }
        }
        private void onPreDeath(Vector2 vector)
        {
            this.timesSpawned -= 1;
        }
        private void OnRoomClearEvent(PlayerController player)
        {
            if (this.timesSpawned == 5)
            {
                player.DropPassiveItem(this);
                player.GiveItem("bb:heroic_cape");
            }
        }
        public void Break()
        {
            this.m_pickedUp = true;
            UnityEngine.Object.Destroy(base.gameObject, 1f);
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnEnteredCombat += this.OnEnteredCombat;
            player.OnRoomClearEvent += this.OnRoomClearEvent;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            player.OnEnteredCombat -= this.OnEnteredCombat;
            player.OnRoomClearEvent -= this.OnRoomClearEvent;
            DebrisObject debrisObject = base.Drop(player);
            TatteredCape component = debrisObject.GetComponent<TatteredCape>();
            component.Break();
            return base.Drop(player);
        }
        private bool isBossRoom;
        private int timesSpawned = 0;
    }
}