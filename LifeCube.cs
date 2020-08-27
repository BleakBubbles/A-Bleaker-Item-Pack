using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;

namespace BleakMod
{
	using System;
    using System.Resources;
    using UnityEngine;

	// Token: 0x02001430 RID: 5168
	public class BleakOrbGunModifier : MonoBehaviour
	{
		// Token: 0x0600754A RID: 30026 RVA: 0x002DBB55 File Offset: 0x002D9D55
		public BleakOrbGunModifier()
		{
			this.damageFraction = 0.5f;
		}

		// Token: 0x0600754B RID: 30027 RVA: 0x002DBB68 File Offset: 0x002D9D68
		private void Awake()
		{
			this.m_gun = base.GetComponent<Gun>();
			LifeOrbGunModifier orig = (PickupObjectDatabase.GetById(595) as Gun).GetComponent<LifeOrbGunModifier>();
			OnBurstDamageVFX = orig.OnBurstDamageVFX;
			OnBurstGunVFX = orig.OnBurstGunVFX;
			OnKilledEnemyVFX = orig.OnKilledEnemyVFX;
			OverheadVFX = orig.OverheadVFX;
			Gun gun = this.m_gun;
			gun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun.OnReloadPressed, new Action<PlayerController, Gun, bool>(this.HandleReloadPressed));
		}

		// Token: 0x0600754C RID: 30028 RVA: 0x002DBBA0 File Offset: 0x002D9DA0
		private void HandleReloadPressed(PlayerController owner, Gun source, bool reloadSomething)
		{
			PickupObject pickupObject = Gungeon.Game.Items["bb:life_cube"];
			if (this.m_storedSoulDamage > 0f && owner.HasPickupID(pickupObject.PickupObjectId))
			{
				if (this.OnBurstGunVFX)
				{
					SpawnManager.SpawnVFX(this.OnBurstGunVFX, owner.CurrentGun.barrelOffset.position, Quaternion.identity);
				}
				this.m_isDealingBurstDamage = true;
				owner.CurrentRoom.ApplyActionToNearbyEnemies(owner.transform.position.XY(), 100f, new Action<AIActor, float>(this.ProcessEnemy));
				this.m_isDealingBurstDamage = false;
				this.ClearSoul(false);
			}
		}

		// Token: 0x0600754D RID: 30029 RVA: 0x002DBC2F File Offset: 0x002D9E2F
		private void OnDisable()
		{
			this.ClearSoul(true);
		}

		// Token: 0x0600754E RID: 30030 RVA: 0x002DBC38 File Offset: 0x002D9E38
		private void ClearSoul(bool disabling)
		{
			this.m_storedSoulDamage = 0f;
			this.m_gun.idleAnimation = string.Empty;
			if (!disabling)
			{
				this.m_gun.PlayIdleAnimation();
			}
			if (this.m_overheadVFXInstance)
			{
				SpawnManager.Despawn(this.m_overheadVFXInstance.gameObject);
				this.m_overheadVFXInstance = null;
			}
		}

		// Token: 0x0600754F RID: 30031 RVA: 0x002DBC9C File Offset: 0x002D9E9C
		private void ProcessEnemy(AIActor a, float distance)
		{
			if (a && a.IsNormalEnemy && a.healthHaver && !a.IsGone)
			{
				if (this.m_lastOwner)
				{
					a.healthHaver.ApplyDamage(this.m_storedSoulDamage * this.damageFraction, Vector2.zero, this.m_lastOwner.ActorName, CoreDamageTypes.None, DamageCategory.Normal, false, null, false);
				}
				else
				{
					a.healthHaver.ApplyDamage(this.m_storedSoulDamage * this.damageFraction, Vector2.zero, "projectile", CoreDamageTypes.None, DamageCategory.Normal, false, null, false);
				}
				if (this.OnBurstDamageVFX)
				{
					a.PlayEffectOnActor(this.OnBurstDamageVFX, Vector3.zero, true, false, false);
				}
			}
		}

		// Token: 0x06007550 RID: 30032 RVA: 0x002DBD68 File Offset: 0x002D9F68
		private void Update()
		{
			if (!this.m_connected && this.m_gun.CurrentOwner)
			{
				this.m_connected = true;
				this.m_lastOwner = (this.m_gun.CurrentOwner as PlayerController);
				this.m_lastOwner.OnDealtDamageContext += this.HandlePlayerDealtDamage;
			}
			else if (this.m_connected && !this.m_gun.CurrentOwner)
			{
				this.m_connected = false;
				this.m_lastOwner.OnDealtDamageContext -= this.HandlePlayerDealtDamage;
				Gun gun = this.m_gun;
				gun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Remove(gun.OnReloadPressed, new Action<PlayerController, Gun, bool>(this.HandleReloadPressed));
				this.m_overheadVFXInstance = null;
				this.OverheadVFX = null;
				this.OnKilledEnemyVFX = null;
				this.OnBurstGunVFX  = null;
				this.OnBurstDamageVFX = null;
				ETGModConsole.Log("Player has dropped the item." + this.OnBurstDamageVFX);
			}
		}

		// Token: 0x06007551 RID: 30033 RVA: 0x002DBE0C File Offset: 0x002DA00C
		private void HandlePlayerDealtDamage(PlayerController source, float damage, bool fatal, HealthHaver target)
		{
			PickupObject pickupObject = Gungeon.Game.Items["bb:life_cube"];
			if (source.HasPickupID(pickupObject.PickupObjectId))
            {
				if (source.CurrentGun != this.m_gun)
				{
					return;
				}
				if (this.m_isDealingBurstDamage)
				{
					return;
				}
				if (this.m_lastTargetDamaged != target)
				{
					this.m_lastTargetDamaged = target;
					this.m_totalDamageDealtToLastTarget = 0f;
				}
				this.m_totalDamageDealtToLastTarget += damage;
				if (fatal)
				{
					this.m_storedSoulDamage = this.m_totalDamageDealtToLastTarget;
					this.m_lastTargetDamaged = null;
					this.m_totalDamageDealtToLastTarget = 0f;
					if (this.OverheadVFX && !this.m_overheadVFXInstance)
					{
						this.m_overheadVFXInstance = source.PlayEffectOnActor(this.OverheadVFX, Vector3.up, true, false, false);
						this.m_overheadVFXInstance.transform.localPosition = this.m_overheadVFXInstance.transform.localPosition.Quantize(0.0625f);
						this.m_gun.idleAnimation = "life_orb_full_idle";
						this.m_gun.PlayIdleAnimation();
					}
					if (this.OnKilledEnemyVFX && target && target.aiActor)
					{
						target.aiActor.PlayEffectOnActor(this.OnKilledEnemyVFX, new Vector3(0f, 0.5f, 0f), false, false, false);
					}
				}
			}
		}

		// Token: 0x04007729 RID: 30505
		public float damageFraction;

		// Token: 0x0400772A RID: 30506
		public GameObject OverheadVFX;

		// Token: 0x0400772B RID: 30507
		public GameObject OnKilledEnemyVFX;

		// Token: 0x0400772C RID: 30508
		public GameObject OnBurstGunVFX;

		// Token: 0x0400772D RID: 30509
		public GameObject OnBurstDamageVFX;

		// Token: 0x0400772E RID: 30510
		private GameObject m_overheadVFXInstance;

		// Token: 0x0400772F RID: 30511
		private Gun m_gun;

		// Token: 0x04007730 RID: 30512
		private bool m_connected;

		// Token: 0x04007731 RID: 30513
		private PlayerController m_lastOwner;

		// Token: 0x04007732 RID: 30514
		private HealthHaver m_lastTargetDamaged;

		// Token: 0x04007733 RID: 30515
		private float m_totalDamageDealtToLastTarget;

		// Token: 0x04007734 RID: 30516
		private float m_storedSoulDamage;

		// Token: 0x04007735 RID: 30517
		private bool m_isDealingBurstDamage;
	}

	public class LifeCube: PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Life Cube";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/life_cube";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<LifeCube>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "All your soul are belong to us";
            string longDesc = "Sucks life from enemies, works with almost all guns.\n\n" +
                "An artifact only to be kept by Gungeoneers. Its purpose is to convert enemies' lives into an arsenal of your own.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
			ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Curse, 1, StatModifier.ModifyMethod.ADDITIVE);
			//Set the rarity of the item
			item.quality = PickupObject.ItemQuality.A;
			item.AddToSubShop(ItemBuilder.ShopType.Cursula, 1f);
			CustomSynergies.Add("Soulless", new List<string>
			{
				"bb:life_cube"
			}, new List<string>
			{
				"life_orb",
			}, true);
		}
        protected override void Update()
        {
            base.Update();
            if (base.Owner && base.Owner.CurrentGun && !base.Owner.CurrentGun.gameObject.GetComponent<BleakOrbGunModifier>())
            {
				base.Owner.CurrentGun.gameObject.AddComponent<BleakOrbGunModifier>();
            }
			if (base.Owner.HasMTGConsoleID("life_orb") && base.Owner.CurrentGun.gameObject.GetComponent<BleakOrbGunModifier>().damageFraction != 1f)
			{
				base.Owner.CurrentGun.gameObject.GetComponent<BleakOrbGunModifier>().damageFraction = 1f;
			}
			else if (!base.Owner.HasMTGConsoleID("life_orb") && base.Owner.CurrentGun.gameObject.GetComponent<BleakOrbGunModifier>().damageFraction == 1f)
            {
				base.Owner.CurrentGun.gameObject.GetComponent<BleakOrbGunModifier>().damageFraction = 0.5f;
			}
        }
    }
}
