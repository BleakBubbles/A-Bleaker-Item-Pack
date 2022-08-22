using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Brave.BulletScript;
using Dungeonator;
using Gungeon;
using ItemAPI;
using tk2dRuntime.TileMap;
using UnityEngine;

namespace BleakMod
{
    public static class Stuff
    {
        public static void Init()
        {
		}
		public static bool PlayerHasActiveSynergy(this PlayerController player, string synergyNameToCheck)
		{
			foreach (int num in player.ActiveExtraSynergies)
			{
				AdvancedSynergyEntry advancedSynergyEntry = GameManager.Instance.SynergyManager.synergies[num];
				bool flag = advancedSynergyEntry.NameKey == synergyNameToCheck;
				if (flag)
				{
					return true;
				}
			}
			return false;
		}
		public static T ReflectGetField<T>(Type classType, string fieldName, object o = null)
		{
			FieldInfo field = classType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | ((o != null) ? BindingFlags.Instance : BindingFlags.Static));
			return (T)field.GetValue(o);
		}
        public static BeamController FreeFireBeamFromAnywhere(Projectile projectileToSpawn, PlayerController owner, GameObject otherShooter, Vector2 fixedPosition, bool usesFixedPosition, float targetAngle, float duration, bool skipChargeTime = false)
        {
            Vector2 sourcePos;
            if (usesFixedPosition) sourcePos = fixedPosition;
            else sourcePos = otherShooter.GetComponent<SpeculativeRigidbody>().UnitCenter;
            if (sourcePos != null)
            {

                GameObject gameObject = SpawnManager.SpawnProjectile(projectileToSpawn.gameObject, sourcePos, Quaternion.identity, true);
                Projectile component = gameObject.GetComponent<Projectile>();
                component.Owner = owner;
                BeamController component2 = gameObject.GetComponent<BeamController>();
                if (skipChargeTime)
                {
                    component2.chargeDelay = 0f;
                    component2.usesChargeDelay = false;
                }
                component2.Owner = owner;
                component2.HitsPlayers = false;
                component2.HitsEnemies = true;
                Vector3 vector = BraveMathCollege.DegreesToVector(targetAngle, 1f);
                component2.Direction = vector;
                component2.Origin = sourcePos;
                GameManager.Instance.Dungeon.StartCoroutine(Stuff.HandleFreeFiringBeam(component2, otherShooter, fixedPosition, usesFixedPosition, targetAngle, duration));
                return component2;
            }
            else
            {
                ETGModConsole.Log("ERROR IN BEAM FREEFIRE CODE. SOURCEPOS WAS NULL, EITHER DUE TO INVALID FIXEDPOS OR SOURCE GAMEOBJECT.");
                return null;
            }
        }
        public static IEnumerator HandleFreeFiringBeam(BeamController beam, GameObject otherShooter, Vector2 fixedPosition, bool usesFixedPosition, float targetAngle, float duration)
        {
            float elapsed = 0f;
            yield return null;
            while (elapsed < duration)
            {
                Vector2 sourcePos;
                if (otherShooter == null || otherShooter.GetComponent<SpeculativeRigidbody>() == null) { break; }
                if (usesFixedPosition) sourcePos = fixedPosition;
                else sourcePos = otherShooter.GetComponent<SpeculativeRigidbody>().UnitCenter;

                elapsed += BraveTime.DeltaTime;
                if (sourcePos != null)
                {
                    beam.Origin = sourcePos;
                    beam.LateUpdatePosition(sourcePos);

                }
                else { ETGModConsole.Log("SOURCEPOS WAS NULL IN BEAM FIRING HANDLER"); }
                yield return null;
            }
            beam.CeaseAttack();
            yield break;
        }
        public static bool CanBlinkToPoint(PlayerController Owner, Vector2 point)
        {
            bool flag = Owner.IsValidPlayerPosition(point);
            if (flag && Owner.CurrentRoom != null)
            {
                CellData cellData = GameManager.Instance.Dungeon.data[point.ToIntVector2(VectorConversions.Floor)];
                if (cellData == null) { return false; }
                RoomHandler nearestRoom = cellData.nearestRoom;
                if (cellData.type != CellType.FLOOR) { flag = false; }
                if (Owner.CurrentRoom.IsSealed && nearestRoom != Owner.CurrentRoom) { flag = false; }
                if (Owner.CurrentRoom.IsSealed && cellData.isExitCell) { flag = false; }
                if (nearestRoom.visibility == RoomHandler.VisibilityStatus.OBSCURED || nearestRoom.visibility == RoomHandler.VisibilityStatus.REOBSCURED) { flag = false; }
            }
            if (Owner.CurrentRoom == null) { flag = false; }
            if (Owner.IsDodgeRolling | Owner.IsFalling | Owner.IsCurrentlyCoopReviving | Owner.IsInMinecart | Owner.IsInputOverridden) { return false; }
            return flag;
        }
        public static bool PositionIsInBounds(PlayerController Owner, Vector2 point)
        {
            bool flag = true;
            if (Owner.CurrentRoom != null)
            {
                CellData cellData = GameManager.Instance.Dungeon.data[point.ToIntVector2(VectorConversions.Floor)];
                if (cellData == null) { return false; }
                RoomHandler nearestRoom = cellData.nearestRoom;
                if (cellData.type != CellType.FLOOR) { flag = false; }
                if (Owner.CurrentRoom.IsSealed && nearestRoom != Owner.CurrentRoom) { flag = false; }
                if (Owner.CurrentRoom.IsSealed && cellData.isExitCell) { flag = false; }
                if (nearestRoom.visibility == RoomHandler.VisibilityStatus.OBSCURED || nearestRoom.visibility == RoomHandler.VisibilityStatus.REOBSCURED) { flag = false; }
            }
            if (Owner.CurrentRoom == null) { flag = false; }
            if (Owner.IsDodgeRolling | Owner.IsFalling | Owner.IsCurrentlyCoopReviving | Owner.IsInMinecart | Owner.IsInputOverridden) { return false; }
            return flag;
        }
        public static Vector2 AdjustInputVector(Vector2 rawInput, float cardinalMagnetAngle, float ordinalMagnetAngle)
        {
            float num = BraveMathCollege.ClampAngle360(BraveMathCollege.Atan2Degrees(rawInput));
            float num2 = num % 90f;
            float num3 = (num + 45f) % 90f;
            float num4 = 0f;
            if (cardinalMagnetAngle > 0f)
            {
                if (num2 < cardinalMagnetAngle)
                {
                    num4 = -num2;
                }
                else if (num2 > 90f - cardinalMagnetAngle)
                {
                    num4 = 90f - num2;
                }
            }
            if (ordinalMagnetAngle > 0f)
            {
                if (num3 < ordinalMagnetAngle)
                {
                    num4 = -num3;
                }
                else if (num3 > 90f - ordinalMagnetAngle)
                {
                    num4 = 90f - num3;
                }
            }
            num += num4;
            return (Quaternion.Euler(0f, 0f, num) * Vector3.right).XY() * rawInput.magnitude;
        }
        
        //VFX
        public static GameObject WinchesterTargetHitVFX = ResourceManager.LoadAssetBundle("shared_auto_001").LoadAsset<GameObject>("VFX_Explosion_Firework");
    }
    public class TeleportPlayerToCursorPosition : MonoBehaviour
    {
        private static Vector2 lockedDodgeRollDirection;
        public static BlinkPassiveItem m_BlinkPassive = PickupObjectDatabase.GetById(436).GetComponent<BlinkPassiveItem>();
        public GameObject BlinkpoofVfx = m_BlinkPassive.BlinkpoofVfx;
        public static void StartTeleport(PlayerController user, Vector2 newPosition)
        {
            user.healthHaver.TriggerInvulnerabilityPeriod(0.001f);
            user.DidUnstealthyAction();
            Vector2 clampedPosition = BraveMathCollege.ClampToBounds(newPosition, GameManager.Instance.MainCameraController.MinVisiblePoint, GameManager.Instance.MainCameraController.MaxVisiblePoint);
            BlinkToPoint(user, clampedPosition);
        }
        private static void BlinkToPoint(PlayerController Owner, Vector2 targetPoint)
        {

            lockedDodgeRollDirection = (targetPoint - Owner.specRigidbody.UnitCenter).normalized;

            Vector2 playerPos = Owner.transform.position;

            int x0 = (int)targetPoint.x, y0 = (int)targetPoint.y, x1 = (int)playerPos.x, y1 = (int)playerPos.y;
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2; /* error value e_xy */
            int maxiterations = 600;
            while (maxiterations > 0)
            {  /* loop */

                if (x0 == x1 && y0 == y1) break;
                if (CanBlinkToPoint(new Vector2(x0, y0), Owner))
                {
                    StaticCoroutine.Start(HandleBlinkTeleport(Owner, new Vector2(x0, y0), lockedDodgeRollDirection));
                    return;
                }
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; } /* e_xy+e_x > 0 */
                if (e2 <= dx) { err += dx; y0 += sy; } /* e_xy+e_y < 0 */

                maxiterations--;
            }
        }
        static bool CanBlinkToPoint(Vector2 point, PlayerController owner)
        {
            RoomHandler CurrentRoom = owner.CurrentRoom;
            bool flag = owner.IsValidPlayerPosition(point);
            if (flag && CurrentRoom != null)
            {
                CellData cellData = GameManager.Instance.Dungeon.data[point.ToIntVector2(VectorConversions.Floor)];
                if (cellData == null)
                {
                    return false;
                }
                RoomHandler nearestRoom = cellData.nearestRoom;
                if (cellData.type != CellType.FLOOR)
                {
                    flag = false;
                }
                if (CurrentRoom.IsSealed && nearestRoom != CurrentRoom)
                {
                    flag = false;
                }
                if (CurrentRoom.IsSealed && cellData.isExitCell)
                {
                    flag = false;
                }
                if (nearestRoom.visibility == RoomHandler.VisibilityStatus.OBSCURED || nearestRoom.visibility == RoomHandler.VisibilityStatus.REOBSCURED)
                {
                    flag = false;
                }
            }
            if (CurrentRoom == null)
            {
                flag = false;
            }
            return flag;
        }
        private static IEnumerator HandleBlinkTeleport(PlayerController Owner, Vector2 targetPoint, Vector2 targetDirection)
        {

            //targetPoint = (targetPoint - new Vector2(0.30f, 0.125f));

            //Owner.PlayEffectOnActor(BloodiedScarfPoofVFX, Vector3.zero, false, true, false);

            AkSoundEngine.PostEvent("Play_ENM_wizardred_vanish_01", Owner.gameObject);
            List<AIActor> m_rollDamagedEnemies = Stuff.ReflectGetField<List<AIActor>>(typeof(PlayerController), "m_rollDamagedEnemies", Owner);
            if (m_rollDamagedEnemies != null)
            {
                m_rollDamagedEnemies.Clear();
                FieldInfo m_rollDamagedEnemiesClear = typeof(PlayerController).GetField("m_rollDamagedEnemies", BindingFlags.Instance | BindingFlags.NonPublic);
                m_rollDamagedEnemiesClear.SetValue(Owner, m_rollDamagedEnemies);
            }

            if (Owner.knockbackDoer) { Owner.knockbackDoer.ClearContinuousKnockbacks(); }
            Owner.IsEthereal = true;
            Owner.IsVisible = false;
            float RecoverySpeed = GameManager.Instance.MainCameraController.OverrideRecoverySpeed;
            bool IsLerping = GameManager.Instance.MainCameraController.IsLerping;
            yield return new WaitForSeconds(0.1f);
            GameManager.Instance.MainCameraController.OverrideRecoverySpeed = 80f;
            GameManager.Instance.MainCameraController.IsLerping = true;
            if (Owner.IsPrimaryPlayer)
            {
                GameManager.Instance.MainCameraController.UseOverridePlayerOnePosition = true;
                GameManager.Instance.MainCameraController.OverridePlayerOnePosition = targetPoint;
                yield return new WaitForSeconds(0.12f);
                Owner.specRigidbody.Velocity = Vector2.zero;
                Owner.specRigidbody.Position = new Position(targetPoint);
                GameManager.Instance.MainCameraController.UseOverridePlayerOnePosition = false;
            }
            else
            {
                GameManager.Instance.MainCameraController.UseOverridePlayerTwoPosition = true;
                GameManager.Instance.MainCameraController.OverridePlayerTwoPosition = targetPoint;
                yield return new WaitForSeconds(0.12f);
                Owner.specRigidbody.Velocity = Vector2.zero;
                Owner.specRigidbody.Position = new Position(targetPoint);
                GameManager.Instance.MainCameraController.UseOverridePlayerTwoPosition = false;
            }
            GameManager.Instance.MainCameraController.OverrideRecoverySpeed = RecoverySpeed;
            GameManager.Instance.MainCameraController.IsLerping = IsLerping;
            Owner.IsEthereal = false;
            Owner.IsVisible = true;
            //Owner.PlayEffectOnActor(BloodiedScarfPoofVFX, Vector3.zero, false, true, false);
            //m_CurrentlyBlinking = false;
            if (Owner.CurrentFireMeterValue <= 0f) { yield break; }
            Owner.CurrentFireMeterValue = Mathf.Max(0f, Owner.CurrentFireMeterValue -= 0.5f);
            if (Owner.CurrentFireMeterValue == 0f)
            {
                Owner.IsOnFire = false;
                yield break;
            }
            // yield return null;
            //CorrectForWalls(Owner);
            yield break;
        }
        public static void CorrectForWalls(PlayerController portal)
        {
            bool flag = PhysicsEngine.Instance.OverlapCast(portal.specRigidbody, null, true, false, null, null, false, null, null, new SpeculativeRigidbody[0]);
            if (flag)
            {
                Vector2 vector = portal.transform.position.XY();
                IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
                int num = 0;
                int num2 = 1;
                for (; ; )
                {
                    for (int i = 0; i < cardinalsAndOrdinals.Length; i++)
                    {
                        //portal.transform.position = vector + PhysicsEngine.PixelToUnit(cardinalsAndOrdinals[i] * num2);
                        portal.specRigidbody.Position = new Position(vector + PhysicsEngine.PixelToUnit(cardinalsAndOrdinals[i] * num2));
                        portal.specRigidbody.Reinitialize();
                        if (!PhysicsEngine.Instance.OverlapCast(portal.specRigidbody, null, true, false, null, null, false, null, null, new SpeculativeRigidbody[0]))
                        {
                            return;
                        }
                    }
                    num2++;
                    num++;
                    if (num > 200)
                    {
                        goto Block_4;
                    }
                }
            Block_4:
                Debug.LogError("FREEZE AVERTED!  TELL RUBEL!  (you're welcome) 147");
                return;
            }
        }
        public static GameObject BloodiedScarfPoofVFX = PickupObjectDatabase.GetById(436).GetComponent<BlinkPassiveItem>().BlinkpoofVfx.gameObject;
    }
    public class StaticCoroutine : MonoBehaviour
    {
        private static StaticCoroutine m_instance;

        // OnDestroy is called when the MonoBehaviour will be destroyed.
        // Coroutines are not stopped when a MonoBehaviour is disabled, but only when it is definitely destroyed.
        private void OnDestroy()
        { m_instance.StopAllCoroutines(); }

        // OnApplicationQuit is called on all game objects before the application is closed.
        // In the editor it is called when the user stops playmode.
        private void OnApplicationQuit()
        { m_instance.StopAllCoroutines(); }

        // Build will attempt to retrieve the class-wide instance, returning it when available.
        // If no instance exists, attempt to find another StaticCoroutine that exists.
        // If no StaticCoroutines are present, create a dedicated StaticCoroutine object.
        private static StaticCoroutine Build()
        {
            if (m_instance != null)
            { return m_instance; }

            m_instance = (StaticCoroutine)FindObjectOfType(typeof(StaticCoroutine));

            if (m_instance != null)
            { return m_instance; }

            GameObject instanceObject = new GameObject("StaticCoroutine");
            instanceObject.AddComponent<StaticCoroutine>();
            m_instance = instanceObject.GetComponent<StaticCoroutine>();

            if (m_instance != null)
            { return m_instance; }



            ETGModConsole.Log("STATIC COROUTINE: Build did not generate a replacement instance. Method Failed!");

            return null;
        }
        //public static Coroutine Start(IEnumerator routine)
        //{ return Build().StartCoroutine(routine); }

        // Overloaded Static Coroutine Methods which use Unity's default Coroutines.
        // Polymorphism applied for best compatibility with the standard engine.
        public static void Start(string methodName)
        { Build().StartCoroutine(methodName); }
        public static void Start(string methodName, object value)
        { Build().StartCoroutine(methodName, value); }
        public static Coroutine Start(IEnumerator routine)
        { return Build().StartCoroutine(routine); }
        public class EasyTrailComponent : BraveBehaviour //----------------------------------------------------------------------------------------------
        {
            public EasyTrailComponent()
            {
                //=====
                this.TrailPos = new Vector3(0, 0, 0);
                //======
                this.BaseColor = Color.red;
                this.StartColor = Color.red;
                this.EndColor = Color.white;
                //======
                this.LifeTime = 1f;
                //======
                this.StartWidth = 1;
                this.EndWidth = 0;

            }
            /// <summary>
            /// Lets you add a trail to your projectile.    
            /// </summary>
            /// <param name="TrailPos">Where the trail attaches its center-point to. You can input a custom Vector3 but its best to use the base preset. (Namely"projectile.transform.position;").</param>
            /// <param name="BaseColor">The Base Color of your trail.</param>
            /// <param name="StartColor">The Starting color of your trail.</param>
            /// <param name="EndColor">The End color of your trail. Having it different to the StartColor will make it transition from the Starting/Base Color to its End Color during its lifetime.</param>
            /// <param name="LifeTime">How long your trail lives for.</param>
            /// <param name="StartWidth">The Starting Width of your Trail.</param>
            /// <param name="EndWidth">The Ending Width of your Trail. Not sure why youd want it to be something other than 0, but the options there.</param>
            public void Start()
            {
                obj = base.gameObject;
                {
                    TrailRenderer tr;
                    var tro = obj.AddChild("trail object");
                    tro.transform.position = obj.transform.position;
                    tro.transform.localPosition = TrailPos;

                    tr = tro.AddComponent<TrailRenderer>();
                    tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    tr.receiveShadows = false;
                    var mat = new Material(Shader.Find("Sprites/Default"));
                    mat.mainTexture = _gradTexture;
                    tr.material = mat;
                    tr.minVertexDistance = 0.1f;
                    //======
                    mat.SetColor("_Color", BaseColor);
                    tr.startColor = StartColor;
                    tr.endColor = EndColor;
                    //======
                    tr.time = LifeTime;
                    //======
                    tr.startWidth = StartWidth;
                    tr.endWidth = EndWidth;
                    tr.autodestruct = false;
                    //tr.
                }

            }

            public Texture _gradTexture;
            private GameObject obj;

            public Vector2 TrailPos;
            public Color BaseColor;
            public Color StartColor;
            public Color EndColor;
            public float LifeTime;
            public float StartWidth;
            public float EndWidth;

        }
    }
}