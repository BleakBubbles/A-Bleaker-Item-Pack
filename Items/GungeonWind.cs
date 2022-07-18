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
    public class GungeonWind : PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Gungeon Wind";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/gungeon_wind";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<GungeonWind>();
			

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Whispers one truth";
            string longDesc = "Has a high chance to reveal the floor, including secret rooms.\n\n" +
                "As the Gungeon Wind flows past your face, you can make out a few disctinct words: Body the floor... Bullet through the roof.. Ammo willin', past killin', gun shall shoot.. ";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
			CustomSynergies.Add("By Beholster Eye, Dragun Tooth", new List<string>
			{
				"bb:gungeon_wind"
			}, new List<string>
			{
				"eye_of_the_beholster",
				"dragunfire"
			}, true);
		}
		public override void Pickup(PlayerController player)
		{
			if (this.m_pickedUp)
			{
				return;
			}
			bool flag = false;
			if (this.executeOnPickup && !this.m_pickedUpThisRun)
			{
				flag = true;
			}
			base.Pickup(player);
			if (flag)
			{
				this.PossiblyRevealMap();
			}
			GameManager.Instance.OnNewLevelFullyLoaded += this.PossiblyRevealMap;
		}
		public void PossiblyRevealMap()
		{
			if (base.Owner.HasMTGConsoleID("eye_of_the_beholster") || base.Owner.HasMTGConsoleID("dragunfire"))
            {
				this.revealChanceOnLoad = 1f;
			}
            else
            {
				this.revealChanceOnLoad = 0.67f;
            }
			if (UnityEngine.Random.value < this.revealChanceOnLoad && Minimap.Instance != null)
			{
				Minimap.Instance.RevealAllRooms(this.revealSecretRooms);
			}
		}
		public override DebrisObject Drop(PlayerController player)
		{
			DebrisObject debrisObject = base.Drop(player);
			debrisObject.GetComponent<GungeonWind>().m_pickedUpThisRun = true;
			GameManager.Instance.OnNewLevelFullyLoaded -= this.PossiblyRevealMap;
			return debrisObject;
		}
		protected override void OnDestroy()
		{
			if (this.m_pickedUp && GameManager.HasInstance)
			{
				GameManager.Instance.OnNewLevelFullyLoaded -= this.PossiblyRevealMap;
			}
			base.OnDestroy();
		}
		public float revealChanceOnLoad = 0.67f;
		public bool revealSecretRooms = true;
		public bool executeOnPickup = true;
	}
}