using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dungeonator;
using UnityEngine;

namespace BleakMod
{
    public class EnemyInteractManager: BraveBehaviour, IPlayerInteractable
    {
		private void Start()
		{
			if (!PassiveItem.IsFlagSetAtAll(typeof(PickpocketGuide)))
			{
				return;
			}
			this.m_room = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Round));
			this.m_room.RegisterInteractable(this);
		}

		// Token: 0x06007CE2 RID: 31970 RVA: 0x00315F54 File Offset: 0x00314154	
		public float GetDistanceToPoint(Vector2 point)
		{
			Bounds bounds = base.sprite.GetBounds();
			bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
			float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
			float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
			return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
		}

		// Token: 0x06007CE3 RID: 31971 RVA: 0x0026DAFF File Offset: 0x0026BCFF
		public void OnEnteredRange(PlayerController interactor)
		{
            if (!PassiveItem.IsFlagSetAtAll(typeof(PickpocketGuide)))
            {
				return;
            }
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
		}

		// Token: 0x06007CE4 RID: 31972 RVA: 0x00316033 File Offset: 0x00314233
		public void OnExitRange(PlayerController interactor)
		{
			if (!PassiveItem.IsFlagSetAtAll(typeof(PickpocketGuide)))
			{
				return;
			}
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
		}

		// Token: 0x06007CE5 RID: 31973 RVA: 0x00316054 File Offset: 0x00314254
		public void Interact(PlayerController interactor)
		{
			if (!PassiveItem.IsFlagSetAtAll(typeof(PickpocketGuide)))
			{
				return;
			}
			PlayerController player = GameManager.Instance.PrimaryPlayer;
			FloorRewardData currentRewardData = GameManager.Instance.RewardManager.CurrentRewardData;
			LootEngine.AmmoDropType ammoDropType = LootEngine.AmmoDropType.DEFAULT_AMMO;
			bool flag = LootEngine.DoAmmoClipCheck(currentRewardData, out ammoDropType);
			string path = (ammoDropType != LootEngine.AmmoDropType.SPREAD_AMMO) ? "Ammo_Pickup" : "Ammo_Pickup_Spread";
			float value = UnityEngine.Random.value;
			float num = currentRewardData.ChestSystem_ChestChanceLowerBound;
            if (value <= 0.2f)
            {
                IntVector2 bestRewardLocation = base.sprite.WorldCenter.ToIntVector2();
                LootEngine.SpawnItem((GameObject)BraveResources.Load(path, ".prefab"), bestRewardLocation.ToVector3(), Vector2.up, 1f, true, true, false);
            }
            else if (value <= 0.95f)
            {
				GameObject gameObject;
				if(value <= 0.6f)
                {
					gameObject = currentRewardData.SingleItemRewardTable.SelectByWeight(false);
				}
                else
                {
					gameObject = ((UnityEngine.Random.value >= 0.9f) ? GameManager.Instance.RewardManager.FullHeartPrefab.gameObject : GameManager.Instance.RewardManager.HalfHeartPrefab.gameObject);
				}
				DebrisObject debrisObject = LootEngine.SpawnItem(gameObject, base.sprite.WorldCenter.ToIntVector2().ToVector3() + new Vector3(0.25f, 0f, 0f), Vector2.up, 1f, true, true, false);
				AkSoundEngine.PostEvent("Play_OBJ_item_spawn_01", debrisObject.gameObject);
			}
            else
            {
				GameManager.Instance.RewardManager.SpawnTotallyRandomItem(base.sprite.WorldCenter);
			}
			this.m_room.DeregisterInteractable(this);
            PickpocketGuide.unstealthBehavior unstealthBehavior = player.gameObject.GetOrAddComponent<PickpocketGuide.unstealthBehavior>();
			unstealthBehavior.parentItem.BreakStealth(player);
		}

		// Token: 0x06007CE6 RID: 31974 RVA: 0x001A24CF File Offset: 0x001A06CF
		public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
		{
			shouldBeFlipped = false;
			return string.Empty;
		}

		// Token: 0x06007CE7 RID: 31975 RVA: 0x0002A087 File Offset: 0x00028287
		public float GetOverrideMaxDistance()
		{
			return 3f;
		}

		// Token: 0x06007CE8 RID: 31976 RVA: 0x0003AB5B File Offset: 0x00038D5B
		protected override void OnDestroy()
		{
			base.OnDestroy();
		}
		private RoomHandler m_room;
	}
}
