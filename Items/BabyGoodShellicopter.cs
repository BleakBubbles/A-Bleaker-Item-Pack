using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using DirectionType = DirectionalAnimation.DirectionType;
using AnimationType = ItemAPI.CompanionBuilder.AnimationType;
using MultiplayerBasicExample;
using HutongGames.PlayMaker.Actions;
using HutongGames;
using Dungeonator;

namespace BleakMod
{
    public class BabyGoodShellicopter : CompanionItem
    {
        public static GameObject prefab;
        private static readonly string guid = "68656c69636f707465720a"; //give your companion some unique guid

        public static void Init()
        {
            string itemName = "Baby Good Shellicopter";
            string resourceName = "BleakMod/Resources/mini_agunim";

            GameObject obj = new GameObject();
            GameObject ect = new GameObject();
            var item = obj.AddComponent<BabyGoodShellicopter>();
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Get To The Choppa!";
            string longDesc = "A strange companion found in the R&G Department.\nIt shoots bullets and rockets, burns them alive, and... talks?";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            item.quality = PickupObject.ItemQuality.S;
            item.CompanionGuid = guid; //this will be used by the item later to pull your companion from the enemy database
            item.Synergies = new CompanionTransformSynergy[0]; //this just needs to not be null
            BabyGoodShellicopter.BuildPrefab();
			BabyGoodShellicopter.napalmGoop = new GoopDefinition();
			AssetBundle assetBundle = ResourceManager.LoadAssetBundle("shared_auto_001");
			GoopDefinition goopDefinition;
			try
			{
				GameObject gameObject2 = assetBundle.LoadAsset(BabyGoodShellicopter.fireGoop) as GameObject;
				goopDefinition = gameObject2.GetComponent<GoopDefinition>();
			}
			catch
			{
				goopDefinition = (assetBundle.LoadAsset(BabyGoodShellicopter.fireGoop) as GoopDefinition);
			}
			goopDefinition.name = BabyGoodShellicopter.fireGoop.Replace("assets/data/goops/", "").Replace(".asset", "");
			BabyGoodShellicopter.napalmGoop = goopDefinition;
		}
		public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
        }
		//protected override void OnDestroy()
		//{
		//	base.OnDestroy();
		//}
		public static void BuildPrefab()
        {
            if (prefab != null || CompanionBuilder.companionDictionary.ContainsKey(guid))return;

            //Create the prefab with a starting sprite and hitbox offset/size
            BabyGoodShellicopter.ChopperPrefab = CompanionBuilder.BuildPrefab("Baby Good Shellicopter", guid, BabyGoodShellicopter.spritePaths[0], new IntVector2(3, 3), new IntVector2(9, 9));
            BabyGoodShellicopter.ChopperBehavior chopperBehavior = BabyGoodShellicopter.ChopperPrefab.AddComponent<BabyGoodShellicopter.ChopperBehavior>();
            AIAnimator aiAnimator = chopperBehavior.aiAnimator;
			aiAnimator.directionalType = AIAnimator.DirectionalType.Rotation;
			bool flag3 = BabyGoodShellicopter.chopperCollection == null;
            if (flag3)
            {
                BabyGoodShellicopter.chopperCollection = SpriteBuilder.ConstructCollection(BabyGoodShellicopter.ChopperPrefab, "Penguin_Collection");
                UnityEngine.Object.DontDestroyOnLoad(BabyGoodShellicopter.chopperCollection);
                for (int i = 0; i < BabyGoodShellicopter.spritePaths.Length; i++)
                {
                    SpriteBuilder.AddSpriteToCollection(BabyGoodShellicopter.spritePaths[i], BabyGoodShellicopter.chopperCollection);
                }
                SpriteBuilder.AddAnimation(chopperBehavior.spriteAnimator, BabyGoodShellicopter.chopperCollection, new List<int>
                    {
                        0,
                        1,
                        2
                    }, "idle", tk2dSpriteAnimationClip.WrapMode.Loop).fps = 15f;
            }
            chopperBehavior.aiActor.MovementSpeed = 9f;
            chopperBehavior.specRigidbody.Reinitialize();
            chopperBehavior.specRigidbody.CollideWithTileMap = false;
            chopperBehavior.aiActor.CanTargetEnemies = true;
            chopperBehavior.aiActor.SetIsFlying(true, "Flying Enemy", true, true);
            BehaviorSpeculator behaviorSpeculator = chopperBehavior.behaviorSpeculator;
            behaviorSpeculator.AttackBehaviors.Add(new BabyGoodShellicopter.ChopperAttackBehavior());
            behaviorSpeculator.MovementBehaviors.Add(new BabyGoodShellicopter.ApproachEnemiesBehavior());
            behaviorSpeculator.MovementBehaviors.Add(new CompanionFollowPlayerBehavior());
            UnityEngine.Object.DontDestroyOnLoad(BabyGoodShellicopter.ChopperPrefab);
            FakePrefab.MarkAsFakePrefab(BabyGoodShellicopter.ChopperPrefab);
            BabyGoodShellicopter.ChopperPrefab.SetActive(false);
        }
        public class ChopperBehavior : CompanionController
        {
            // Token: 0x06000119 RID: 281 RVA: 0x0000A9D9 File Offset: 0x00008BD9
            private void Start()
            {
                base.spriteAnimator.Play("idle");
                this.Owner = this.m_owner;
            }

            // Token: 0x04000079 RID: 121
            public PlayerController Owner;
        }
        public static GameObject ChopperPrefab;

        private static tk2dSpriteCollectionData chopperCollection;

        private static string[] spritePaths = new string[]
        {
            "BleakMod/Resources/mini_agunim_idle/mini_agunim_idle_001",
            "BleakMod/Resources/mini_agunim_idle/mini_agunim_idle_002",
            "BleakMod/Resources/mini_agunim_idle/mini_agunim_idle_003"
        };
		public class ChopperAttackBehavior : AttackBehaviorBase
		{
			public override void Destroy()
			{

				base.Destroy();
			}

			public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
			{
				base.Init(gameObject, aiActor, aiShooter);
				this.Owner = this.m_aiActor.GetComponent<BabyGoodShellicopter.ChopperBehavior>().Owner;
			}

			public override BehaviorResult Update()
			{
				bool flag = this.attackTimer > 0f && this.isAttacking;
				if (flag)
				{
					base.DecrementTimer(ref this.attackTimer, false);
				}
				else
				{

					bool flag2 = this.attackCooldownTimer > 0f && !this.isAttacking;
					if (flag2)
					{

						base.DecrementTimer(ref this.attackCooldownTimer, false);
					}
				}
				bool flag3 = this.IsReady();
				bool flag4 = (!flag3 || this.attackCooldownTimer > 0f || this.m_aiActor.TargetRigidbody == null) && this.isAttacking;
				BehaviorResult result;
				if (flag4)
				{
					this.StopAttacking();
					result = BehaviorResult.Continue;
				}
				else
				{

					bool flag5 = flag3 && this.attackCooldownTimer == 0f && !this.isAttacking;
					if (flag5)
					{

						this.attackTimer = this.attackDuration;
						this.isAttacking = true;
					}
					bool flag6 = this.attackTimer > 0f && flag3;
					if (flag6)
					{

						this.Attack();
						result = BehaviorResult.SkipAllRemainingBehaviors;
					}
					else
					{
						result = BehaviorResult.Continue;
					}
				}	
				return result;
			}

			// Token: 0x0600011E RID: 286 RVA: 0x0000AB30 File Offset: 0x00008D30
			private void StopAttacking()
			{
				this.isAttacking = false;
				this.attackTimer = 0f;
				this.attackCooldownTimer = this.attackCooldown;
			}

			// Token: 0x0600011F RID: 287 RVA: 0x0000AB54 File Offset: 0x00008D54
			public AIActor GetNearestEnemy(List<AIActor> activeEnemies, Vector2 position, out float nearestDistance, string[] filter)
			{
				AIActor aiactor = null;
				nearestDistance = float.MaxValue;
				bool flag = activeEnemies == null;
				bool flag2 = flag;
				bool flag3 = flag2;
				AIActor result;
				if (flag3)
				{
					result = null;
				}
				else
				{
					for (int i = 0; i < activeEnemies.Count; i++)
					{
						AIActor aiactor2 = activeEnemies[i];
						bool flag4 = aiactor2.healthHaver && aiactor2.healthHaver.IsVulnerable;
						bool flag5 = flag4;
						bool flag6 = flag5;
						if (flag6)
						{
							bool flag7 = !aiactor2.healthHaver.IsDead;
							bool flag8 = flag7;
							bool flag9 = flag8;
							if (flag9)
							{
								bool flag10 = filter == null || !filter.Contains(aiactor2.EnemyGuid);
								bool flag11 = flag10;
								bool flag12 = flag11;
								if (flag12)
								{
									float num = Vector2.Distance(position, aiactor2.CenterPosition);
									bool flag13 = num < nearestDistance;
									bool flag14 = flag13;
									bool flag15 = flag14;
									if (flag15)
									{
										nearestDistance = num;
										aiactor = aiactor2;
									}
								}
							}
						}
					}
					result = aiactor;
				}
				return result;
			}

			// Token: 0x06000120 RID: 288 RVA: 0x0000AC74 File Offset: 0x00008E74
			private void Attack()
			{

				bool flag = this.Owner == null;
				if (flag)
				{
					this.Owner = this.m_aiActor.GetComponent<BabyGoodShellicopter.ChopperBehavior>().Owner;
				}
				float num = -1f;

				List<AIActor> activeEnemies = this.Owner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
				bool flag2 = activeEnemies == null | activeEnemies.Count <= 0;
				if (!flag2)
				{
					AIActor nearestEnemy = this.GetNearestEnemy(activeEnemies, this.m_aiActor.sprite.WorldCenter, out num, null);
					bool flag3 = nearestEnemy && num < 10f;
					if (flag3)
					{
						bool flag4 = this.IsInRange(nearestEnemy);
						if (flag4)
						{
							bool flag5 = !nearestEnemy.IsHarmlessEnemy && nearestEnemy.IsNormalEnemy && !nearestEnemy.healthHaver.IsDead && nearestEnemy != this.m_aiActor;
							if (flag5)
							{
								if(UnityEngine.Random.value <= 0.1f)
                                {
									this.m_aiActor.StartCoroutine(this.sayVoiceLine());
                                }
								float x = UnityEngine.Random.value;
								Vector2 unitCenter = this.m_aiActor.specRigidbody.UnitCenter;
								Vector2 unitCenter2 = nearestEnemy.specRigidbody.HitboxPixelCollider.UnitCenter;
									float z = BraveMathCollege.Atan2Degrees((unitCenter2 - unitCenter).normalized);
								if (x <= 0.2f)
                                {
									this.m_aiActor.StartCoroutine(this.IgniteColumn1(z));
									this.m_aiActor.StartCoroutine(this.IgniteColumn2(z));
									DeadlyDeadlyGoopManager gooper = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(BabyGoodShellicopter.napalmGoop);
									gooper.TimedAddGoopLine(this.m_aiActor.sprite.WorldCenter, this.m_aiActor.sprite.WorldCenter + BraveMathCollege.DegreesToVector(z, 8), 0.8f, 2f);
								}
								else if(x <= 0.4f)
                                {
									this.m_aiActor.StartCoroutine(shootRockets(z));
                                }
								else if(x <= 0.7f)
                                {
									this.m_aiActor.StartCoroutine(eightShots());
                                }
                                else
                                {
									this.m_aiActor.StartCoroutine(eightShots2(z));
                                }
							}
						}
					}
				}
			}
			private IEnumerator sayVoiceLine()
			{
				yield return null;
				if (this.m_aiActor)
				{
					string text = this.voicelines[UnityEngine.Random.Range(0, this.voicelines.Count)];
					TextBoxManager.ShowTextBox(this.m_aiActor.transform.position + Vector3.up / 2, this.m_aiActor.transform, 2, text, "", false, TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, false, false);
				}
				yield break;
			}
			private IEnumerator IgniteColumn1(float angle)
            {
				Projectile projectile = ((Gun)ETGMod.Databases.Items[146]).DefaultModule.projectiles[0];
				Vector2 pos = this.m_aiActor.sprite.WorldCenter + BraveMathCollege.DegreesToVector(angle - 90, 0.75f);
				for (int i = 0; i < 9; i++)
                {
                    GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, pos + BraveMathCollege.DegreesToVector(angle, 1f * i), Quaternion.Euler(0f, 0f, angle), true);
					Projectile component = gameObject.GetComponent<Projectile>();
                    if (component)
                    {
						component.Owner = this.m_aiActor.GetComponent<ChopperBehavior>().Owner;
						component.Shooter = this.m_aiActor.specRigidbody;
						component.collidesWithPlayer = false;
						component.baseData.speed = 0f;
						component.AdditionalScaleMultiplier = 0.8f;
						component.StartCoroutine(this.delayedDisappear(component));
					}
					yield return new WaitForSeconds(0.222f);
				}
			}
			private IEnumerator IgniteColumn2(float angle)
            {
				Projectile projectile = ((Gun)ETGMod.Databases.Items[146]).DefaultModule.projectiles[0];
				Vector2 pos = this.m_aiActor.sprite.WorldCenter + BraveMathCollege.DegreesToVector(angle + 90, 0.75f);
				for (int i = 0; i < 9; i++)
                {
                    GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, pos + BraveMathCollege.DegreesToVector(angle, 1f * i), Quaternion.Euler(0f, 0f, angle), true);
					Projectile component = gameObject.GetComponent<Projectile>();
                    if (component)
                    {
						component.Owner = this.m_aiActor.GetComponent<ChopperBehavior>().Owner;
						component.Shooter = this.m_aiActor.specRigidbody;
						component.collidesWithPlayer = false;
						component.baseData.speed = 0f;
						component.AdditionalScaleMultiplier = 0.8f;
						component.StartCoroutine(this.delayedDisappear(component));
                    }
					yield return new WaitForSeconds(0.222f);
				}
			}
			private IEnumerator eightShots()
            {
				Projectile projectile = ((Gun)ETGMod.Databases.Items[53]).DefaultModule.projectiles[0];
				for(int i = 0; i < 2; i++)
                {
					for(int j = 0; j < 8; j++)
                    {
						GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, this.m_aiActor.sprite.WorldCenter, Quaternion.Euler(0f, 0f, j * 45), true);
						Projectile component = gameObject.GetComponent<Projectile>();
						if (component)
						{
							component.Owner = this.m_aiActor.GetComponent<ChopperBehavior>().Owner;
							component.Shooter = this.m_aiActor.specRigidbody;
							component.collidesWithPlayer = false;
							component.baseData.speed *= 0.5f;
                            component.AdditionalScaleMultiplier *= 1.5f;
						}
					}
					yield return new WaitForSeconds(0.3f);
                }
			}
			private IEnumerator eightShots2(float z)
            {
				Projectile projectile = ((Gun)ETGMod.Databases.Items[53]).DefaultModule.projectiles[0];
				for(int i = 0; i < 2; i++)
                {
					for(int j = 0; j < 4; j++)
                    {
						GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, this.m_aiActor.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (z - 40) + 20 * j), true);
						Projectile component = gameObject.GetComponent<Projectile>();
						if (component)
						{
							component.Owner = this.m_aiActor.GetComponent<ChopperBehavior>().Owner;
							component.Shooter = this.m_aiActor.specRigidbody;
							component.collidesWithPlayer = false;
							component.baseData.speed *= 0.5f;
							component.AdditionalScaleMultiplier *= 1.5f;
						}
					}
					yield return new WaitForSeconds(0.3f);
				}
			}
			private IEnumerator shootRockets(float z)
            {
				Projectile projectile = ((Gun)ETGMod.Databases.Items[16]).DefaultModule.projectiles[0];
				for (int i = 0; i < 8; i++)
                {
					GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, this.m_aiActor.sprite.WorldCenter, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(z - 30, z + 30)), true);
					Projectile component = gameObject.GetComponent<Projectile>();
					if (component)
					{
						component.Owner = this.m_aiActor.GetComponent<ChopperBehavior>().Owner;
						component.Shooter = this.m_aiActor.specRigidbody;
						component.collidesWithPlayer = false;
					}
					yield return new WaitForSeconds(0.1f);
				}
            }
			private IEnumerator delayedDisappear(Projectile proj)
            {
				yield return new WaitForSeconds(5);
				proj.DieInAir();
            }
			// Token: 0x06000121 RID: 289 RVA: 0x0000AE80 File Offset: 0x00009080
			public override float GetMaxRange()
			{
				return 5;
			}

			public override float GetMinReadyRange()
			{
				return 5;
			}

			public override bool IsReady()
			{
				AIActor aiActor = this.m_aiActor;
				bool flag;
				if (aiActor == null)
				{
					flag = true;
				}
				else
				{
					SpeculativeRigidbody targetRigidbody = aiActor.TargetRigidbody;
					Vector2? vector = (targetRigidbody != null) ? new Vector2?(targetRigidbody.UnitCenter) : null;
					flag = (vector == null);
				}
				bool flag2 = flag;
				return !flag2 && Vector2.Distance(this.m_aiActor.specRigidbody.UnitCenter, this.m_aiActor.TargetRigidbody.UnitCenter) <= this.GetMinReadyRange();
			}

			// Token: 0x06000124 RID: 292 RVA: 0x0000AF30 File Offset: 0x00009130
			public bool IsInRange(AIActor enemy)
			{
				bool flag;
				if (enemy == null)
				{
					flag = true;
				}
				else
				{
					SpeculativeRigidbody specRigidbody = enemy.specRigidbody;
					Vector2? vector = (specRigidbody != null) ? new Vector2?(specRigidbody.UnitCenter) : null;
					flag = (vector == null);
				}
				bool flag2 = flag;
				return !flag2 && Vector2.Distance(this.m_aiActor.specRigidbody.UnitCenter, enemy.specRigidbody.UnitCenter) <= this.GetMinReadyRange();
			}

			// Token: 0x0400007A RID: 122
			private bool isAttacking;

			// Token: 0x0400007B RID: 123
			private float attackCooldown = 1.5f;

			// Token: 0x0400007C RID: 124
			private float attackDuration = 0.01f;

			// Token: 0x0400007D RID: 125
			private float attackTimer;

			// Token: 0x0400007E RID: 126
			private float attackCooldownTimer;

			// Token: 0x0400007F RID: 127
			private PlayerController Owner;

			// Token: 0x04000080 RID: 128
			private List<AIActor> roomEnemies = new List<AIActor>();

			private List<string> voicelines = new List<string>()
			{
				"I'll kill you, then my past!",
				"You bothersome insect!",
				"Die!",
				"Eat this!",
				"You'll make a beautiful sacrifice...",
				"Bested by your own slow reflexes!",
				"Next time, try dodgerolling...",
				"The gun is mine!",
				"Oh, not enough firepower it seems?"
			};
		}
		public class ApproachEnemiesBehavior : MovementBehaviorBase
		{
			// Token: 0x06000126 RID: 294 RVA: 0x00009E97 File Offset: 0x00008097
			public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
			{
				base.Init(gameObject, aiActor, aiShooter);
			}

			// Token: 0x06000127 RID: 295 RVA: 0x0000AFCF File Offset: 0x000091CF
			public override void Upkeep()
			{
				base.Upkeep();
				base.DecrementTimer(ref this.repathTimer, false);
			}

			// Token: 0x06000128 RID: 296 RVA: 0x0000AFE8 File Offset: 0x000091E8
			public override BehaviorResult Update()
			{
				SpeculativeRigidbody overrideTarget = this.m_aiActor.OverrideTarget;
				bool flag = this.repathTimer > 0f;
				BehaviorResult result;
				if (flag)
				{
					result = ((overrideTarget == null) ? BehaviorResult.Continue : BehaviorResult.SkipRemainingClassBehaviors);
				}
				else
				{
					bool flag2 = overrideTarget == null;
					if (flag2)
					{
						this.PickNewTarget();
						result = BehaviorResult.Continue;
					}
					else
					{
						this.isInRange = (Vector2.Distance(this.m_aiActor.specRigidbody.UnitCenter, overrideTarget.UnitCenter) <= this.DesiredDistance);
						bool flag3 = overrideTarget != null && !this.isInRange;
						if (flag3)
						{
							this.m_aiActor.PathfindToPosition(overrideTarget.UnitCenter, null, true, null, null, null, false);

							this.repathTimer = this.PathInterval;
							result = BehaviorResult.SkipRemainingClassBehaviors;
						}
						else
						{
							bool flag4 = overrideTarget != null && this.repathTimer >= 0f;
							if (flag4)
							{
								this.m_aiActor.ClearPath();
								this.repathTimer = -1f;
							}
							result = BehaviorResult.Continue;
						}
					}
				}
				return result;
			}

			// Token: 0x06000129 RID: 297 RVA: 0x0000B104 File Offset: 0x00009304
			private void PickNewTarget()
			{

				bool flag = this.m_aiActor == null;
				if (!flag)
				{
					bool flag2 = this.Owner == null;
					if (flag2)
					{
						this.Owner = this.m_aiActor.GetComponent<BabyGoodShellicopter.ChopperBehavior>().Owner;
					}
					this.Owner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref this.roomEnemies);
					for (int i = 0; i < this.roomEnemies.Count; i++)
					{
						AIActor aiactor = this.roomEnemies[i];
						bool flag3 = aiactor.IsHarmlessEnemy || !aiactor.IsNormalEnemy || aiactor.healthHaver.IsDead || aiactor == this.m_aiActor || aiactor.EnemyGuid == "ba928393c8ed47819c2c5f593100a5bc";
						if (flag3)
						{

							this.roomEnemies.Remove(aiactor);

						}
					}
					bool flag4 = this.roomEnemies.Count == 0;
					BabyGoodShellicopter.ChopperBehavior chopperBehavior = BabyGoodShellicopter.ChopperPrefab.AddComponent<BabyGoodShellicopter.ChopperBehavior>();
					if (flag4)
					{
						//this.m_aiActor.OverrideTarget = this.Owner.specRigidbody;
						this.m_aiActor.OverrideTarget = null;
						chopperBehavior.aiAnimator.facingType = AIAnimator.FacingType.Movement;
					}
					else
					{
						AIActor aiActor = this.m_aiActor;
						AIActor aiactor2 = this.roomEnemies[UnityEngine.Random.Range(0, this.roomEnemies.Count)];
						aiActor.OverrideTarget = ((aiactor2 != null) ? aiactor2.specRigidbody : null);
						chopperBehavior.aiAnimator.facingType = AIAnimator.FacingType.Target;
					}
				}
			}
			public float PathInterval = 0.25f;

			// Token: 0x04000082 RID: 130
			public float DesiredDistance = 5f;

			// Token: 0x04000083 RID: 131
			private float repathTimer;

			// Token: 0x04000084 RID: 132
			private List<AIActor> roomEnemies = new List<AIActor>();

			// Token: 0x04000085 RID: 133
			private bool isInRange;

			// Token: 0x04000086 RID: 134
			private PlayerController Owner;
		}
		private static string fireGoop = "assets/data/goops/napalmgoopthatworks.asset";
		public static GoopDefinition napalmGoop;
	}
}