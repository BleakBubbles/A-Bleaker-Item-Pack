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
    public class GundromedaPain : PassiveItem
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
            item.AddToSubShop(ItemBuilder.ShopType.Goopton, 1f);
            CustomSynergies.Add("No Pain, No Gain", new List<string>
            {
                "bb:gundromeda_pain"
            }, new List<string>
            {
                "gundromeda_strain"
            }, true);
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
            if (this.Owner.HasMTGConsoleID("gundromeda_strain"))
            {
                this.healthSizeMultiplier = 0.75f;
                aiactor.healthHaver.SetHealthMaximum(this.healthSizeMultiplier * this.baseHealth, null, true);
            }
            else
            {
                this.baseHealth = aiactor.healthHaver.GetMaxHealth();
                this.healthSizeMultiplier = UnityEngine.Random.Range(0.6f, 1.2f);
                aiactor.healthHaver.SetHealthMaximum(this.healthSizeMultiplier * this.baseHealth, null, true);
                aiactor.EnemyScale *= this.healthSizeMultiplier;
            }
            
        }
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            debrisObject.GetComponent<GundromedaPain>().m_pickedUpThisRun = true;
            return debrisObject;
        }
        public float healthSizeMultiplier;
        public float baseHealth;
    }
}