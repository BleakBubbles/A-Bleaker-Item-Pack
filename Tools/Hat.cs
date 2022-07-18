using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Gungeon;
using Dungeonator;
using System.Reflection;
using ItemAPI;
using System.Collections;
using System.Globalization;


namespace cAPI
{
    public class Hat : BraveBehaviour
    {

        public string   hatName;
        public Vector3 hatOffset;
        public HatDirectionality hatDirectionality;
        public HatRollReaction hatRollReaction;
        public HatAttachLevel attachLevel;
        public string FlipStartedSound;
        public string FlipEndedSound;
        public HatDepthType hatDepthType;
        public PlayerController hatOwner;
        public float SpinSpeed;
        public float flipHeightMultiplier;

        private FieldInfo commandedField, lastNonZeroField, lockedDodgeRollDirection, m_currentGunAngle;
        private HatDirection currentDirection;
        private HatState currentState;
        private tk2dSprite hatSprite;
        private tk2dSpriteAnimator hatSpriteAnimator;
        private tk2dSpriteAnimator hatOwnerAnimator;

        private float RollLength = 0.65f; //The time it takes for a player with no dodge roll effects to roll

        public Hat()
        {
            hatOffset = new Vector2(0, 0);
            hatOwner = null;
            SpinSpeed = 1;
            flipHeightMultiplier = 1;
            hatRollReaction = HatRollReaction.FLIP;
            hatDirectionality = HatDirectionality.NONE;
            attachLevel = HatAttachLevel.HEAD_TOP;
            hatDepthType = HatDepthType.AlwaysInFront;
            FlipEndedSound = null;
            FlipStartedSound = null;
        }

        private void Start()
        {
            hatSprite = base.GetComponent<tk2dSprite>();
            hatSpriteAnimator = base.GetComponent<tk2dSpriteAnimator>();

            commandedField = typeof(PlayerController).GetField("m_playerCommandedDirection", BindingFlags.NonPublic | BindingFlags.Instance);
            lastNonZeroField = typeof(PlayerController).GetField("m_lastNonzeroCommandedDirection", BindingFlags.NonPublic | BindingFlags.Instance);           
            lockedDodgeRollDirection = typeof(PlayerController).GetField("lockedDodgeRollDirection", BindingFlags.NonPublic | BindingFlags.Instance);
            m_currentGunAngle = typeof(PlayerController).GetField("m_currentGunAngle", BindingFlags.NonPublic | BindingFlags.Instance);
            


            if (hatOwner != null)
            {
                SpriteOutlineManager.AddOutlineToSprite(hatSprite, Color.black, 1);
                GameObject playerSprite = hatOwner.transform.Find("PlayerSprite").gameObject;
                hatOwnerAnimator = playerSprite.GetComponent<tk2dSpriteAnimator>();
                hatOwner.OnPreDodgeRoll += this.HatReactToDodgeRoll;
                UpdateHatFacingDirection(FetchOwnerFacingDirection());
            }
            else Debug.LogError("hatOwner was somehow null in hat Start() ???");
        }

        protected override void OnDestroy()
        {
            if (hatOwner)
            {
                hatOwner.OnPreDodgeRoll -= this.HatReactToDodgeRoll;
            }
            base.OnDestroy();
        }

        private void HatReactToDodgeRoll(PlayerController player)
        {

        }

        private void Update()
        {

            if (hatOwner)
            {
                
                //Make the Hat vanish upon pitfall, or when the player rolls if the hat is VANISH type
                HandleVanish();
                //if(ETGModGUI.CurrentMenu != ETGModGUI.MenuOpened.Console)
                    //ETGModConsole.Log(hatOwnerAnimator.CurrentClip.name);
                //UPDATE DIRECTIONS
                HatDirection checkedDir = FetchOwnerFacingDirection();
                if (checkedDir != currentDirection) UpdateHatFacingDirection(checkedDir);

                HandleFlip();
                HandleAttachedSpriteDepth(m_currentGunAngle.GetTypedValue<float>(hatOwner));
            }
        }

		#region vanishing
		private void HandleVanish()
        {
            bool isntVanished = base.sprite.renderer.enabled;
            bool shouldBeVanished = false;

            if (hatOwner.IsFalling) 
                shouldBeVanished = true;

            if(hatOwnerAnimator.CurrentClip.name == "doorway" || hatOwnerAnimator.CurrentClip.name == "spinfall")
                shouldBeVanished = true;

            if ((PlayerHasAdditionalVanishOverride() || hatRollReaction == HatRollReaction.VANISH) && hatOwner.IsDodgeRolling) 
                shouldBeVanished = true;

            if(hatOwner.IsSlidingOverSurface)
                shouldBeVanished = true;

            if (!isntVanished && !shouldBeVanished)
            {
                base.sprite.renderer.enabled = true;
                SpriteOutlineManager.AddOutlineToSprite(hatSprite, Color.black, 1);
                if (GameManager.AUDIO_ENABLED && !string.IsNullOrEmpty(FlipEndedSound))
                {
                    AkSoundEngine.PostEvent(FlipEndedSound, gameObject);
                }
            }
            else if (isntVanished && shouldBeVanished)
            {
                base.sprite.renderer.enabled = false;
                SpriteOutlineManager.RemoveOutlineFromSprite(hatSprite);
                StickHatToPlayer(hatOwner);
                if (GameManager.AUDIO_ENABLED && !string.IsNullOrEmpty(FlipStartedSound))
                {
                    AkSoundEngine.PostEvent(FlipStartedSound, gameObject);
                }
            }
        }

        private bool PlayerHasAdditionalVanishOverride()
        {
            bool shouldActuallyVanish = false;
            if (hatOwner && hatOwner.HasPickupID(436)) shouldActuallyVanish = true;
            return shouldActuallyVanish;
        }
		#endregion
		#region facingCode
		public void UpdateHatFacingDirection(HatDirection targetDir)
        {
            string animToPlay = "null";
            if (hatDirectionality == HatDirectionality.NONE)
            {
                animToPlay = "hat_south";
                currentDirection = HatDirection.SOUTH;
            }
            else
            {
                switch (targetDir)
                {
                    case HatDirection.SOUTH:
                        if (hatDirectionality != HatDirectionality.TWOWAYHORIZONTAL) { animToPlay = "hat_south"; }
                        break;
                    case HatDirection.NORTH:
                        if (hatDirectionality != HatDirectionality.TWOWAYHORIZONTAL) { animToPlay = "hat_north"; }
                        break;
                    case HatDirection.WEST:
                        if (hatDirectionality != HatDirectionality.TWOWAYVERTICAL) { animToPlay = "hat_west"; }
                        break;
                    case HatDirection.EAST:
                        if (hatDirectionality != HatDirectionality.TWOWAYVERTICAL) { animToPlay = "hat_east"; }
                        break;
                    case HatDirection.SOUTHWEST:
                        if (hatDirectionality == HatDirectionality.EIGHTWAY) { animToPlay = "hat_southwest"; }
                        break;
                    case HatDirection.SOUTHEAST:
                        if (hatDirectionality == HatDirectionality.EIGHTWAY) { animToPlay = "hat_southeast"; }
                        break;
                    case HatDirection.NORTHWEST:
                        if (hatDirectionality == HatDirectionality.SIXWAY || hatDirectionality == HatDirectionality.EIGHTWAY) { animToPlay = "hat_northwest"; }
                        break;
                    case HatDirection.NORTHEAST:
                        if (hatDirectionality == HatDirectionality.SIXWAY || hatDirectionality == HatDirectionality.EIGHTWAY) { animToPlay = "hat_northeast"; }
                        break;
                    case HatDirection.NONE:
                        ETGModConsole.Log("ERROR: TRIED TO ROTATE HAT TO A NULL DIRECTION! (wtf?)");
                        break;
                }
                currentDirection = targetDir;
            }
            if (animToPlay != "null")
            {
                hatSpriteAnimator.Play(animToPlay);
            }
        }

        public HatDirection FetchOwnerFacingDirection()
        {
            HatDirection hatDir = HatDirection.NONE;
            if (hatOwner != null)
            {
                if (hatOwner.CurrentGun == null)
                {
                    Vector2 m_playerCommandedDirection = commandedField.GetTypedValue<Vector2>(hatOwner);
                    Vector2 m_lastNonzeroCommandedDirection = lastNonZeroField.GetTypedValue<Vector2>(hatOwner);

                    float playerCommandedDir = BraveMathCollege.Atan2Degrees((!(m_playerCommandedDirection == Vector2.zero)) ? m_playerCommandedDirection : m_lastNonzeroCommandedDirection);

                    switch (playerCommandedDir)
                    {
                        case 90:
                            hatDir = HatDirection.NORTH;
                            break;
                        case 45:
                            hatDir = HatDirection.NORTHEAST;
                            break;
                        case -90:
                            hatDir = HatDirection.SOUTH;
                            break;
                        case -135:
                            hatDir = HatDirection.SOUTHWEST;
                            break;
                        case -180:
                            hatDir = HatDirection.WEST;
                            break;
                        case 135:
                            hatDir = HatDirection.NORTHWEST;
                            break;
                        case -45:
                            hatDir = HatDirection.SOUTHEAST;
                            break;
                        case 180:
                            hatDir = HatDirection.WEST;
                            break;
                    }
                    if (playerCommandedDir == 0 && hatOwner.Velocity != new Vector2(0f, 0))
                    {
                        hatDir = HatDirection.EAST;
                    }
                }
                else
                {
                    int FacingDirection = Mathf.RoundToInt(hatOwner.FacingDirection / 45) * 45;
                    switch (FacingDirection)
                    {
                        case 90:
                            hatDir = HatDirection.NORTH;
                            break;
                        case 45:
                            hatDir = HatDirection.NORTHEAST;
                            break;
                        case 0:
                            hatDir = HatDirection.EAST;
                            break;
                        case -45:
                            hatDir = HatDirection.SOUTHEAST;
                            break;
                        case -90:
                            hatDir = HatDirection.SOUTH;
                            break;
                        case -135:
                            hatDir = HatDirection.SOUTHWEST;
                            break;
                        case -180:
                            hatDir = HatDirection.WEST;
                            break;
                        case 135:
                            hatDir = HatDirection.NORTHWEST;
                            break;
                        case 180:                           
                            hatDir = HatDirection.WEST;
                            break;
                    }
                }
            }

            else Debug.LogError("Attempted to get hatOwner facing direction with a null hatOwner!");
            if (hatDir == HatDirection.NONE) hatDir = HatDirection.SOUTH;
            
            return hatDir;
        }
		#endregion

        public int GetPlayerAnimFrame(PlayerController player)
		{
            return player.spriteAnimator.CurrentFrame;

        }

		public Vector3 GetHatPosition(PlayerController player)
        {
            Vector3 vec = new Vector3();
            if (attachLevel == HatAttachLevel.HEAD_TOP)
            {
                if (PlayerHatDatabase.CharacterNameHatHeadLevel.ContainsKey(player.name))
                {
                    vec = new Vector3(player.sprite.WorldCenter.x, player.sprite.WorldCenter.y + PlayerHatDatabase.CharacterNameHatHeadLevel[player.name], player.transform.position.z +1);
                }
                else { vec = (player.sprite.WorldCenter + new Vector2(0, PlayerHatDatabase.defaultHeadLevelOffset)); }
            }
            else if (attachLevel == HatAttachLevel.EYE_LEVEL)
            {
                if (PlayerHatDatabase.CharacterNameEyeLevel.ContainsKey(player.name))
                {
                    vec = (player.sprite.WorldCenter + new Vector2(0, PlayerHatDatabase.CharacterNameEyeLevel[player.name]));
                }
                else { vec = (player.sprite.WorldCenter + new Vector2(0, PlayerHatDatabase.defaultEyeLevelOffset)); }
            }
            vec += new Vector3(0, 0.03f, player.transform.position.z + 1);
            vec += hatOffset;
            return vec;
        }

        public void StickHatToPlayer(PlayerController player)
        {
            currentState = HatState.SITTING;
            if (hatOwner == null) hatOwner = player;
            Vector2 vec = GetHatPosition(player);
            transform.position = vec;
            transform.rotation = hatOwner.transform.rotation;
            transform.parent = player.transform;
            player.sprite.AttachRenderer(gameObject.GetComponent<tk2dBaseSprite>());
            
            
        }
        // Token: 0x0600818D RID: 33165 RVA: 0x0033787C File Offset: 0x00335A7C
        private void HandleAttachedSpriteDepth(float gunAngle)
        {
			if (hatDepthType == HatDepthType.BehindWhenFacingBack || hatDepthType == HatDepthType.InFrontWhenFacingBack)
			{
				float num = 1f;
				if (hatOwner.CurrentGun is null)
				{
                    Vector2 m_playerCommandedDirection = commandedField.GetTypedValue<Vector2>(hatOwner);
                    Vector2 m_lastNonzeroCommandedDirection = lastNonZeroField.GetTypedValue<Vector2>(hatOwner);
                    gunAngle = BraveMathCollege.Atan2Degrees((!(m_playerCommandedDirection == Vector2.zero)) ? m_playerCommandedDirection : m_lastNonzeroCommandedDirection);
                }
				float num2;
				if (gunAngle <= 155f && gunAngle >= 25f)
				{
					num = -1f;
					if (gunAngle < 120f && gunAngle >= 60f)
					{
						num2 = 0.15f;
					}
					else
					{
						num2 = 0.15f;
					}
				}
				else if (gunAngle <= -60f && gunAngle >= -120f)
				{
					num2 = -0.15f;
				}
				else
				{
					num2 = -0.15f;
				}

                if(hatDepthType == HatDepthType.BehindWhenFacingBack)
				    hatSprite.HeightOffGround = num2 + num * 1;
                else
                    hatSprite.HeightOffGround = num2 + num * -1;
            }
			else
			{
                if(hatDepthType == HatDepthType.AlwaysInFront)
                    hatSprite.HeightOffGround = 0.6f;
                else
                    hatSprite.HeightOffGround = -0.6f;
            }
        }
        private IEnumerator FlipHatIENum()
        {
            currentState = HatState.FLIPPING;
			yield return new WaitForSeconds(RollLength);
            currentState = HatState.SITTING;
            StickHatToPlayer(hatOwner);
            if (GameManager.AUDIO_ENABLED && !string.IsNullOrEmpty(FlipEndedSound))
            {
                AkSoundEngine.PostEvent(FlipEndedSound, gameObject);
            }
        }
        
        

        private void HandleFlip()
        {
            if (hatRollReaction == HatRollReaction.FLIP && !PlayerHasAdditionalVanishOverride())
            {
                if (hatOwnerAnimator == null) Debug.LogError("Attempted to flip a hat with a null hatOwnerAnimator!");
                else
                {
                    RollLength = hatOwner.rollStats.GetModifiedTime(hatOwner);
                    if (hatOwner.IsDodgeRolling && currentState == HatState.SITTING && hatOwnerAnimator.ClipTimeSeconds < RollLength * 0.1)
                    {
                        //ETGModConsole.Log(RollLength.ToString());
                        StartCoroutine(FlipHatIENum());
                    }
                    

                    if (currentState == HatState.FLIPPING)
                    {
                        if (!GameManager.Instance.IsPaused)
                        {
                            
                           
                            if(GameManager.AUDIO_ENABLED && !string.IsNullOrEmpty(FlipStartedSound) )
							{
                                AkSoundEngine.PostEvent(FlipStartedSound, gameObject);
							}

                            if (hatOwnerAnimator.CurrentClip == null || !hatOwnerAnimator.CurrentClip.name.StartsWith("dodge"))
                                Debug.LogError("hatOwnerAnimator.CurrentClip is NULL! (or the current anim isnt a roll)");
                            else
                            {
								float CurrentRollTime = hatOwnerAnimator.ClipTimeSeconds;
								Vector3 rotatePoint = sprite.WorldCenter;
								this.transform.RotateAround(this.sprite.WorldCenter, Vector3.forward, 15 * SpinSpeed);

								if (CurrentRollTime < RollLength * 0.1)
									this.transform.position += new Vector3(0, flipHeightMultiplier * 0.3f, 0);
								else if (CurrentRollTime < RollLength * 0.2)
									this.transform.position += new Vector3(0, flipHeightMultiplier * 0.25f, 0);
								else if (CurrentRollTime < RollLength * 0.3)
									this.transform.position += new Vector3(0, flipHeightMultiplier * 0.15f, 0);
								else if (CurrentRollTime < RollLength * 0.4)
									this.transform.position += new Vector3(0, flipHeightMultiplier * 0.1f, 0);
								else if (CurrentRollTime < RollLength * 0.5)
									this.transform.position += new Vector3(0, flipHeightMultiplier * 0.01f, 0);
								else if (CurrentRollTime < RollLength * 0.6)
									this.transform.position -= new Vector3(0, flipHeightMultiplier * 0.01f, 0);
								else if (CurrentRollTime < RollLength * 0.7)
									this.transform.position -= new Vector3(0, flipHeightMultiplier * 0.1f, 0);
								else if (CurrentRollTime < RollLength * 0.8)
									this.transform.position -= new Vector3(0, flipHeightMultiplier * 0.15f, 0);
								else if (CurrentRollTime < RollLength * 0.9)
									this.transform.position -= new Vector3(0, flipHeightMultiplier * 0.25f, 0);
								else if (CurrentRollTime < RollLength * 1)
									this.transform.position -= new Vector3(0, flipHeightMultiplier * 0.3f, 0);
								


							}
                        }
                        else
                            StickHatToPlayer(hatOwner);
                    }
                }
            }

        }
        #region enums
        public enum HatDepthType
        {
            AlwaysInFront,
            AlwaysBehind,
            BehindWhenFacingBack,
            InFrontWhenFacingBack
		}

        public enum HatDirectionality
        {
            NONE,
            TWOWAYHORIZONTAL,
            TWOWAYVERTICAL,
            FOURWAY,
            SIXWAY,
            EIGHTWAY,
        }
        public enum HatRollReaction
        {
            FLIP,
            VANISH,
            NONE,
        }
        public enum HatAttachLevel
        {
            HEAD_TOP,
            EYE_LEVEL,
        }
        public enum HatDirection
        {
            NORTH,
            SOUTH,
            WEST,
            EAST,
            NORTHWEST,
            NORTHEAST,
            SOUTHWEST,
            SOUTHEAST,
            NONE,
        }
        public enum HatState
        {
            SITTING,
            FLIPPING,
        }
		#endregion
	}


	static class ExtensionMethods { 
        public static T GetTypedValue<T>(this FieldInfo This, object instance) { return (T)This.GetValue(instance); }

        public static void MakeOffset(this tk2dSpriteDefinition def, Vector2 offset, bool changesCollider = false)
        {
            float xOffset = offset.x;
            float yOffset = offset.y;
            def.position0 += new Vector3(xOffset, yOffset, 0);
            def.position1 += new Vector3(xOffset, yOffset, 0);
            def.position2 += new Vector3(xOffset, yOffset, 0);
            def.position3 += new Vector3(xOffset, yOffset, 0);
            def.boundsDataCenter += new Vector3(xOffset, yOffset, 0);
            def.boundsDataExtents += new Vector3(xOffset, yOffset, 0);
            def.untrimmedBoundsDataCenter += new Vector3(xOffset, yOffset, 0);
            def.untrimmedBoundsDataExtents += new Vector3(xOffset, yOffset, 0);
            if (def.colliderVertices != null && def.colliderVertices.Length > 0 && changesCollider)
            {
                def.colliderVertices[0] += new Vector3(xOffset, yOffset, 0);
            }
        }

        public static void ConstructOffsetsFromAnchor(this tk2dSpriteDefinition def, tk2dBaseSprite.Anchor anchor, Vector2? scale = null, bool fixesScale = false, bool changesCollider = true)
        {
            if (!scale.HasValue)
            {
                scale = new Vector2?(def.position3);
            }
            if (fixesScale)
            {
                Vector2 fixedScale = scale.Value - def.position0.XY();
                scale = new Vector2?(fixedScale);
            }
            float xOffset = 0;
            if (anchor == tk2dBaseSprite.Anchor.LowerCenter || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.UpperCenter)
            {
                xOffset = -(scale.Value.x / 2f);
            }
            else if (anchor == tk2dBaseSprite.Anchor.LowerRight || anchor == tk2dBaseSprite.Anchor.MiddleRight || anchor == tk2dBaseSprite.Anchor.UpperRight)
            {
                xOffset = -scale.Value.x;
            }
            float yOffset = 0;
            if (anchor == tk2dBaseSprite.Anchor.MiddleLeft || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.MiddleLeft)
            {
                yOffset = -(scale.Value.y / 2f);
            }
            else if (anchor == tk2dBaseSprite.Anchor.UpperLeft || anchor == tk2dBaseSprite.Anchor.UpperCenter || anchor == tk2dBaseSprite.Anchor.UpperRight)
            {
                yOffset = -scale.Value.y;
            }
            def.MakeOffset(new Vector2(xOffset, yOffset), false);
            if (changesCollider && def.colliderVertices != null && def.colliderVertices.Length > 0)
            {
                float colliderXOffset = 0;
                if (anchor == tk2dBaseSprite.Anchor.LowerLeft || anchor == tk2dBaseSprite.Anchor.MiddleLeft || anchor == tk2dBaseSprite.Anchor.UpperLeft)
                {
                    colliderXOffset = (scale.Value.x / 2f);
                }
                else if (anchor == tk2dBaseSprite.Anchor.LowerRight || anchor == tk2dBaseSprite.Anchor.MiddleRight || anchor == tk2dBaseSprite.Anchor.UpperRight)
                {
                    colliderXOffset = -(scale.Value.x / 2f);
                }
                float colliderYOffset = 0;
                if (anchor == tk2dBaseSprite.Anchor.LowerLeft || anchor == tk2dBaseSprite.Anchor.LowerCenter || anchor == tk2dBaseSprite.Anchor.LowerRight)
                {
                    colliderYOffset = (scale.Value.y / 2f);
                }
                else if (anchor == tk2dBaseSprite.Anchor.UpperLeft || anchor == tk2dBaseSprite.Anchor.UpperCenter || anchor == tk2dBaseSprite.Anchor.UpperRight)
                {
                    colliderYOffset = -(scale.Value.y / 2f);
                }
                def.colliderVertices[0] += new Vector3(colliderXOffset, colliderYOffset, 0);
            }
        }

    }
}
