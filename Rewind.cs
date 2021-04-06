using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using Dungeonator;
using InControl;

namespace BleakMod
{
    class Rewind: PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Rewind";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/rewind";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<Rewind>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "It's Rewind Time";
            string longDesc = "Rewinds time and sends you back to the previous room upon use. Hearts, shields, blanks, and ammo are restored to their values previous to entering the room. Master rounds can still be obtained after rewinding, even if the user was hit previously.\n\n" +
				"This artifact has for long be thought as a part of the Gun, although it proved to only allow to revive the past once more. Despite it being limited to quite recent moments, it still wields one of the most respected tales of all the Gungeon.";

			//Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
			//"kts" here is the item pool. In the console you'd type kts:sweating_bullets
			ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 500);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.S;
        }
        protected override void DoEffect(PlayerController user)
        {
			user.CurrentRoom.PlayerHasTakenDamageInThisRoom = false;
			this.RewindRoom();
        }
        public override void Pickup(PlayerController user)
        {
			user.OnEnteredCombat += this.OnEnteredCombat;
            base.Pickup(user);
        }
		protected override void OnPreDrop(PlayerController user)
		{
			user.OnEnteredCombat -= this.OnEnteredCombat;
		}
		public void OnEnteredCombat()
        {
			PlayerController owner = base.LastOwner;
			this.savedHealth = owner.healthHaver.GetCurrentHealth();
			this.savedArmor = owner.healthHaver.Armor;
			this.savedBlanks = owner.Blanks;
			this.savedCasings = owner.carriedConsumables.Currency;
			this.savedGuns.Clear();
			foreach(Gun gun in owner.inventory.AllGuns)
            {
				this.savedGuns.Add(gun.PickupObjectId, gun.CurrentAmmo);
            }
		}
		private void RewindRoom()
		{
			AkSoundEngine.PostEvent("State_Bullet_Time_on", base.gameObject);
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_HIT_WITH_THE_GRIPPY, 1f);
			int num = 1;
			bool flag = num < 1;
			if (flag)
			{
				num = 1;
			}
			List<RoomHandler> list = new List<RoomHandler>();
			List<RoomHandler> list2 = new List<RoomHandler>();
			PlayerController owner = base.LastOwner;
			list.Add(owner.CurrentRoom);
			while (list.Count - 1 < num)
			{
				RoomHandler roomHandler = list[list.Count - 1];
				list2.Clear();
				foreach (RoomHandler roomHandler2 in roomHandler.connectedRooms)
				{
					bool flag2 = roomHandler2.hasEverBeenVisited && roomHandler2.distanceFromEntrance < roomHandler.distanceFromEntrance && !list.Contains(roomHandler2);
					if (flag2)
					{
						bool flag3 = !roomHandler2.area.IsProceduralRoom || roomHandler2.area.proceduralCells == null;
						if (flag3)
						{
							list2.Add(roomHandler2);
						}
					}
				}
				bool flag4 = list2.Count == 0;
				if (flag4)
				{
					break;
				}
				list.Add(BraveUtility.RandomElement<RoomHandler>(list2));
			}
			bool flag5 = list.Count > 1;
			if (flag5)
			{
				base.LastOwner.RespawnInPreviousRoom(false, PlayerController.EscapeSealedRoomStyle.GRIP_MASTER, true, list[list.Count - 1]);
				for (int i = 1; i < list.Count - 1; i++)
				{
					list[i].ResetPredefinedRoomLikeDarkSouls();
				}
			}
			else
			{
				base.LastOwner.RespawnInPreviousRoom(false, PlayerController.EscapeSealedRoomStyle.GRIP_MASTER, true, null);
			}
			base.LastOwner.specRigidbody.Velocity = Vector2.zero;
			base.LastOwner.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
			if (owner.healthHaver.GetCurrentHealth() < this.savedHealth)
            {
				owner.healthHaver.ForceSetCurrentHealth(this.savedHealth);
			}
			if(owner.healthHaver.Armor < this.savedArmor)
            {
				owner.healthHaver.Armor = this.savedArmor;
            }
			if (owner.Blanks < this.savedBlanks)
			{
				owner.Blanks = this.savedBlanks;
			}
			if(owner.carriedConsumables.Currency < this.savedCasings)
            {
				owner.carriedConsumables.Currency = this.savedCasings;
            }
			ICollection keys = this.savedGuns.Keys;
			foreach(Gun gun in owner.inventory.AllGuns)
            {
				foreach(int n in keys)
                {
					if(gun && this.savedGuns[n] != null && gun.PickupObjectId == n)
                    {
						gun.CurrentAmmo = (int)this.savedGuns[n];
						gun.ClipShotsRemaining = gun.ClipCapacity;
                    }
                }
            }
		}

        //Disable or enable the active whenever you need!
        public override bool CanBeUsed(PlayerController user)
        {
            return base.CanBeUsed(user);
        }
		public float savedHealth;
		public float savedArmor;
		public int savedBlanks;
		public int savedCasings;
		public Hashtable savedGuns = new Hashtable();
	}
}