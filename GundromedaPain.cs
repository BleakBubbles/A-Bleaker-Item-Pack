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
    public class GundromedaPain: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Gundromeda Pain";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/gundromeda_pain";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<GundromedaPain>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Some enemies weaker!";
            string longDesc = "Changes enemy health.\n\nDisease spreads somewhat slowly among the Gundead, as they sleep far apart from each other in magazines.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
   
            //Adds the actual passive effect to the item
            

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
        }
        public class AffectedEnemyFlag : MonoBehaviour
        {
        }

        protected override void Update()
        {
            if (this.m_owner != null && this.m_pickedUp && this.m_owner.CurrentRoom != null)
            {
                foreach (AIActor aiactor in base.Owner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
                {
                    if (aiactor.GetComponent<AffectedEnemyFlag>() == null)
                    {
                        this.OnNewEnemyAppeared(aiactor);
                        aiactor.gameObject.AddComponent<AffectedEnemyFlag>();
                    }
                }
            }
        }
        private void OnNewEnemyAppeared(AIActor aiactor)
        {
            this.lowerHealthBoundary = aiactor.healthHaver.GetMaxHealth() * 0.6f;
            this.upperHealthBoundary = aiactor.healthHaver.GetMaxHealth() * 1.1f;
            aiactor.healthHaver.SetHealthMaximum(UnityEngine.Random.Range(this.lowerHealthBoundary, this.upperHealthBoundary), null, true);
        }
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            debrisObject.GetComponent<GundromedaPain>().m_pickedUpThisRun = true;
            return debrisObject;
        }
        public float lowerHealthBoundary;
        public float upperHealthBoundary;
    }
}