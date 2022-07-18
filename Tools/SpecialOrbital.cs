using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gungeon;
using ItemAPI;
using UnityEngine;
using Dungeonator;
using System.Collections;

namespace BleakMod
{
    public class SpecialOrbital : MonoBehaviour, IPlayerOrbital
    {
        public SpecialOrbital()
        {
            orbitalSpeed = 90;
            orbitalRadius = 3f;
            canShoot = false;
            doesPostProcess = false;
            hasLifetime = false;
            lifetime = 100;
            currentOrbitTarget = null;
            currentCustomOrbitTarget = null;
            spread = 0;
            doesMultishot = false;
            doesBurstshot = false;
            outlineColor = Color.black;
            fireCooldown = .1f;
            cooldownAffectedByPlayerStats = false;
            damageAffectedByPlayerStats = true;
            preventOutline = false;
            affectedByLute = false;
            canShootEnemyOrbiter = true;
            targetToShoot = null;
            specBodyOffsets = new IntVector2(0, 0);
            specBodyDimensions = new IntVector2(3, 3);
            firingMode = FiringSequenceMode.SEQUENCE;
            orbitingMode = OrbitingMode.PLAYER;
            shouldRotate = false;
            rotatesTowardsTargetEnemy = true;
            hasSinWaveMovement = false;
            sinAmplitude = 1;
            sinWavelength = 3;
            shootRange = 6.5f;
            currentShootIndex = 0;
            affectedByBattleStandard = false;
        }

        private void Start()
        {
            //you'll need code from PlayerOrbital and HoveringGunController (the base game one) to make this thing work
            //Add Lute support too
            initialized = true;           
            if(base.gameObject.GetComponent<tk2dSprite>() != null && !preventOutline) // needs a sprite to work!
            {
                sprite = base.gameObject.GetComponent<tk2dSprite>();
                SpriteOutlineManager.AddOutlineToSprite(sprite, outlineColor);
            }
            this.SetOrbitalTier(PlayerOrbital.CalculateTargetTier(owner, this));
            this.SetOrbitalTierIndex(PlayerOrbital.GetNumberOfOrbitalsInTier(owner, orbitalTier));
            owner.orbitals.Add(this);
            ownerCenterPos = owner.CenterPosition;
            if(base.gameObject.GetComponent<SpeculativeRigidbody>() == null)
            {
                body = sprite.SetUpSpeculativeRigidbody(specBodyOffsets, specBodyDimensions);
                if (pixelColliders.Any())
                {
                    body.PixelColliders.Clear();
                    body.PixelColliders.AddRange(pixelColliders);
                    body.CollideWithOthers = true;
                }
                else
                {
                    body.PixelColliders.Clear();
                    body.PixelColliders.Add(new PixelCollider
                    {
                        ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                        CollisionLayer = CollisionLayer.EnemyBlocker,
                        IsTrigger = false,
                        BagleUseFirstFrameOnly = false,
                        SpecifyBagelFrame = string.Empty,
                        BagelColliderNumber = 0,
                        ManualOffsetX = specBodyOffsets.x,
                        ManualOffsetY = specBodyOffsets.y,
                        ManualWidth = specBodyDimensions.x,
                        ManualHeight = specBodyDimensions.y,
                        ManualDiameter = 0,
                        ManualLeftX = 0,
                        ManualLeftY = 0,
                        ManualRightX = 0,
                        ManualRightY = 0,
                    });
                    body.CollideWithOthers = false;
                }
                body.CollideWithTileMap = false;
            }
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }
            FindAIActorTarget();
            HandleMotion();
            if (this.canShoot && isOnCooldown)
            {
                HandleShootCooldown();
            }
            if(this.canShoot && !isOnCooldown)
            {
                HandleShooting(); //finish after doing motion
            }
        }

        private void HandleShootCooldown()
        {
            if (cooldownAffectedByPlayerStats)
            {
                fireCooldown *= owner.stats.GetStatModifier(PlayerStats.StatType.RateOfFire);
            }
            if(cooldownTimer < fireCooldown)
            {
                cooldownTimer += BraveTime.DeltaTime;
            }
            if(cooldownTimer >= fireCooldown)
            {
                cooldownTimer = 0;
                isOnCooldown = false;
                
            }
        }
        private void HandleShooting()
        {
            if (GameManager.Instance.IsPaused || !this.owner || this.owner.CurrentInputState != PlayerInputState.AllInput || this.owner.IsInputOverridden)
            {
                return;
            }
            if(targetToShoot == null)
            {
                return;
            }
            if (owner.IsStealthed)
            {
                return;
            }
            Projectile projectile = projectiles[currentShootIndex];
            if (firingMode == FiringSequenceMode.SEQUENCE)
            {                
                currentShootIndex++;
                if (currentShootIndex == projectiles.Count)
                {
                    currentShootIndex = 0;
                }
            }
            else
            {
                projectile = projectiles[UnityEngine.Random.Range(0, projectiles.Count)];
            }
            Vector2 a = FindPredictedTargetPosition(projectile);
            canShoot = false;
            if (!doesBurstshot && !doesMultishot)
            {
                Shoot(a, Vector2.zero, projectile.gameObject);
                isOnCooldown = true;
            }
            else if(!doesBurstshot && doesMultishot)
            {
                for(int i = 0; i < multishotAmount; i++)
                {
                    Shoot(a, Vector2.zero, projectile.gameObject);
                }
                isOnCooldown = true;
            }
            else if (doesBurstshot)
            {
                GameManager.Instance.StartCoroutine(HandleBurstShot(a, Vector2.zero, projectile.gameObject));
            }
            if (this.shouldRotate)
            {
                float num = BraveMathCollege.Atan2Degrees(targetToShoot.CenterPosition - base.transform.position.XY());
                base.transform.localRotation = Quaternion.Euler(0f, 0f, num - 90f);
            }
            //Do burst shot and burst shot + multishot code! shouldnt be too bad. also fucking do collision stuff you nerd
        }
        private IEnumerator HandleBurstShot(Vector2 targetPos, Vector2 offset, GameObject projectileGameObject)
        {
           
            int num = 0;
            do
            {
                if (doesMultishot)
                {
                    for (int i = 0; i < multishotAmount; i++)
                    {
                        Shoot(targetPos, offset, projectileGameObject);
                        yield return null;
                    }
                }
                else
                {
                    Shoot(targetPos, offset, projectileGameObject);
                    yield return null;
                }
                yield return new WaitForSeconds(timeBetweenBurstShots);
                num++;
            } while (num < burstAmount);
            isOnCooldown = true;
            yield break;
        }
        private void Shoot(Vector2 targetPos, Vector2 offset, GameObject projectileGameObject)
        {
            Vector2 vector = base.transform.position.XY() + offset;
            //Vector2 vector2 = targetPos - vector;
            //float z = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f; // Base game stuff dont know why it exists but it might be useful if my method is unreliable or doesnt work at all.
            float angle = Vector2.Angle(vector, targetPos);
            GameObject gameObject2 = SpawnManager.SpawnProjectile(projectileGameObject, vector, Quaternion.Euler(0f, 0f, angle + (UnityEngine.Random.Range(-spread, spread))), true);
            Projectile component = gameObject2.GetComponent<Projectile>();
            if (component != null)
            {
                component.collidesWithEnemies = true;
                component.collidesWithPlayer = false;
                component.Owner = owner;
                component.Shooter = owner.specRigidbody;
                component.TreatedAsNonProjectileForChallenge = true;
                component.specRigidbody.CollideWithTileMap = false;
                if (damageAffectedByPlayerStats)
                {
                    component.baseData.damage *= owner.stats.GetStatModifier(PlayerStats.StatType.Damage);
                }
                component.UpdateCollisionMask();
                GameManager.Instance.StartCoroutine(DelayedAddTileCollision(component));
                if (affectedByBattleStandard)
                {
                    if (PassiveItem.IsFlagSetForCharacter(owner, typeof(BattleStandardItem)))
                    {
                        component.baseData.damage *= BattleStandardItem.BattleStandardCompanionDamageMultiplier;
                    }
                }
                if (affectedByLute)
                {
                    if (owner.CurrentGun && owner.CurrentGun.LuteCompanionBuffActive)
                    {
                        component.baseData.damage *= 2f;
                        component.RuntimeUpdateScale(1.75f);
                    }
                }
                if (doesPostProcess)
                {
                    owner.DoPostProcessProjectile(component);
                }
            }
            
        }
        private IEnumerator DelayedAddTileCollision(Projectile proj)
        {
            yield return new WaitForSeconds(.3f);
            proj.specRigidbody.CollideWithTileMap = true;
            proj.UpdateCollisionMask();
            yield break;
        }
        private Vector2 FindPredictedTargetPosition(Projectile proj)
        {
            float num = proj.baseData.speed;
            if (num < 0f)
            {
                num = float.MaxValue;
            }
            Vector2 a = base.transform.position.XY();
            Vector2 vector = (targetToShoot.specRigidbody.HitboxPixelCollider == null) ? this.targetToShoot.specRigidbody.UnitCenter : this.targetToShoot.specRigidbody.HitboxPixelCollider.UnitCenter;
            float d = Vector2.Distance(a, vector) / num;
            return vector + this.targetToShoot.specRigidbody.Velocity * d;
        }
        private void HandleMotion() // Add rotations to facing the target enemy
        {
            Vector2 centerPosition = owner.CenterPosition;
            if (Vector2.Distance(centerPosition, base.transform.position.XY()) > 20f)
            {
                base.transform.position = centerPosition.ToVector3ZUp(0f);
                body.Reinitialize();
            }
            if (orbitingMode == OrbitingMode.ENEMY && currentOrbitTarget != null)
            {
                centerPosition = currentOrbitTarget.CenterPosition;
            }
            else if(orbitingMode == OrbitingMode.CUSTOM && currentCustomOrbitTarget != null)
            {
                centerPosition = currentCustomOrbitTarget.transform.position;
            }
            Vector2 vector = centerPosition - ownerCenterPos;
            float num = Mathf.Lerp(0.1f, 15f, vector.magnitude / 4f);
            float d = Mathf.Min(num * BraveTime.DeltaTime, vector.magnitude);
            float num2 = 360f / (float)PlayerOrbital.GetNumberOfOrbitalsInTier(owner, this.GetOrbitalTier()) * (float)this.GetOrbitalTierIndex() + BraveTime.ScaledTimeSinceStartup * orbitalSpeed;
            Vector2 vector2 = ownerCenterPos + (centerPosition - ownerCenterPos).normalized * d;
            vector2 = Vector2.Lerp(vector2, centerPosition, perfectOrbitFactor);
            Vector2 vector3 = vector2 + (Quaternion.Euler(0f, 0f, num2) * Vector3.right * orbitalRadius).XY();
            if (hasSinWaveMovement)
            {
                float d2 = Mathf.Sin(Time.time * sinWavelength) * sinAmplitude;
                vector3 += (Quaternion.Euler(0f, 0f, num2) * Vector3.right).XY().normalized * d2;
            }
            ownerCenterPos = vector2;
            vector3 = vector3.Quantize(0.0625f);
            Vector2 velocity = (vector3 - base.transform.position.XY()) / BraveTime.DeltaTime;
            body.Velocity = velocity;
            this.currentAngle = num2 % 360f;
            if (this.shouldRotate)
            {
                 base.transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

            }

        }

        private void FindAIActorTarget()
        {
            if(owner.CurrentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) != 0)
            {
                bool flag = orbitingMode == OrbitingMode.ENEMY && currentOrbitTarget == null || currentOrbitTarget.healthHaver.IsDead;                
                if (flag)
                {
                    currentOrbitTarget = owner.CurrentRoom.GetNearestEnemy(ownerCenterPos, out closestEnemyDistance);
                }
                else
                {
                    currentOrbitTarget = null;
                }
                if (canShoot)
                {
                    if(canShootEnemyOrbiter)
                    {
                        if(currentOrbitTarget != null)
                        {
                            targetToShoot = currentOrbitTarget;
                        }
                        else
                        {
                            targetToShoot = null;
                        }
                    }
                    else if(!canShootEnemyOrbiter)
                    {
                        if(owner.CurrentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) > 1)
                        {
                            int capper = 0;
                            do
                            {
                                owner.CurrentRoom.GetNearestEnemy(currentOrbitTarget.CenterPosition, out closestEnemyDistance);
                                capper++;
                            }while (targetToShoot == currentOrbitTarget && capper < 5);
                            if(targetToShoot == currentOrbitTarget)
                            {
                                targetToShoot = null;
                            }
                        }
                        else
                        {
                            targetToShoot = null;
                        }
                        //if there is more than one enemy in the room, run a do while check to find an enemy that isnt the current orbiter with a capper int of 5;
                    }
                }
                
            }
           
            
        }

        public void Reinitialize()
        {

        }

        public void ToggleRenderer(bool visible)
        {

        }

        public Transform GetTransform()
        {
            return base.transform;
        }

        public float GetOrbitalRadius()
        {
            return orbitalRadius;
        }

        public float GetOrbitalRotationalSpeed()
        {
            return orbitalSpeed;
        }

        public int GetOrbitalTier()
        {
            return orbitalTier;
        }

        public void SetOrbitalTier(int tier)
        {
            orbitalTier = tier;
        }

        public int GetOrbitalTierIndex()
        {
            return orbitalTierIndex;
        }

        public void SetOrbitalTierIndex(int tierIndex)
        {
            orbitalTierIndex = tierIndex;
        }
        
        public enum OrbitingMode //What the orbital orbits around
        {
            PLAYER, //Makes orbital orbit around the player
            ENEMY, //Makes orbital orbit around enemies
            CUSTOM //Makes orbital orbit around a chosen game object
        }
       
        public enum FiringSequenceMode //If it can shoot, determines the firing sequence of the orbital.
        {
            SEQUENCE, //Each time it shoots, it will cycle through each projectile in the list of projectiles it can shoot
            RANDOM, //Each time it shoots, it will pick a random projectile from the list of projectiles it can shoot
        }
        /// <summary>
        /// The radius at which it orbits from its target
        /// </summary>
        public float orbitalRadius;
        /// <summary>
        /// The speed at which it orbits
        /// </summary>
        public float orbitalSpeed;
        /// <summary>
        /// If you want the orbital to have a lifetime, this is the amount of time, in seconds. Only works if hasLifetime is true. !!Not Implemented Yet!!
        /// </summary>
        public float lifetime;
        /// <summary>
        /// The spread of the projectiles the orbital shoots. 
        /// </summary>
        public float spread;
        /// <summary>
        /// The time between shots.
        /// </summary>
        public float fireCooldown;
        /// <summary>
        /// How tightly it orbits the target. Set super high to make it stick to the target.
        /// </summary>
        public float perfectOrbitFactor;
        /// <summary>
        /// The length of the sin wave motion
        /// </summary>
        public float sinWavelength;
        /// <summary>
        /// Amplitude of the sin waves
        /// </summary>
        public float sinAmplitude;
        /// <summary>
        /// Range of the orbital. !!Not Implemented Yet!!
        /// </summary>
        public float shootRange;
        /// <summary>
        /// Time between shots in a burst. Only works if doesBurstshot is true
        /// </summary>
        public float timeBetweenBurstShots;

        public int orbitalTierIndex;
        /// <summary>
        /// Orbital tier
        /// </summary>
        public int orbitalTier;
        /// <summary>
        /// Amount of shots in a multishot shotgun-like attack. Only works if doesMultishot is true
        /// </summary>
        public int multishotAmount;
        /// <summary>
        /// Amount of shots in a burst. Only works if doesBurstshot is true
        /// </summary>
        public int burstAmount;
        /// <summary>
        /// If the orbital can shoot
        /// </summary>
        public bool canShoot;
        /// <summary>
        /// If the orbital's projectiles are post processed
        /// </summary>
        public bool doesPostProcess;
        /// <summary>
        /// If the orbital has a lifetime. !!Not Implemented Yet!!
        /// </summary>
        public bool hasLifetime;
        /// <summary>
        /// Enables shotgun-like shooting. Can be used with burst shooting
        /// </summary>
        public bool doesMultishot; //Enables shotgun-like shooting
        /// <summary>
        /// Enables burst shooting. Can be used with multishot
        /// </summary>
        public bool doesBurstshot; //Enables burst-like shooting. Can be used with multishot.
        /// <summary>
        /// If the cooldown between shots is based on your rate of fire
        /// </summary>
        public bool cooldownAffectedByPlayerStats;
        /// <summary>
        /// If the orbital's projectile damage is based on your damage
        /// </summary>
        public bool damageAffectedByPlayerStats;
        /// <summary>
        /// Prevents sprite outline
        /// </summary>
        public bool preventOutline;
        /// <summary>
        /// If the orbital is affected by Really Special Lute
        /// </summary>
        public bool affectedByLute;
        /// <summary>
        /// If the orbital can shoot the enemy it's orbiting, if it orbits an enemy
        /// </summary>
        public bool canShootEnemyOrbiter; //If the orbital is set to orbit an enemy, this changes if they can shoot that enemy or not.
        /// <summary>
        /// If the sprite should rotate
        /// </summary>
        public bool shouldRotate;
        /// <summary>
        /// If the sprite should rotate towards the target enemy. !!Not Implemented Yet!!
        /// </summary>
        public bool rotatesTowardsTargetEnemy;
        /// <summary>
        /// If the orbital has sin wave movement (Think Baby Dragun)
        /// </summary>
        public bool hasSinWaveMovement; //Gives orbital sin wave movement (think Baby Dragun)
        /// <summary>
        /// If the orbital is affected by Battle Standard
        /// </summary>
        public bool affectedByBattleStandard;
        /// <summary>
        /// The player the orbital is tied to
        /// </summary>
        public PlayerController owner;
        /// <summary>
        /// A list of the projectiles the orbital can shoot. 
        /// </summary>
        public List<Projectile> projectiles = new List<Projectile> { };
        /// <summary>
        /// The enemy the orbital is orbiting, if it can
        /// </summary>
        public AIActor currentOrbitTarget;
        /// <summary>
        /// The gameobject the orbital is orbiting, if it can
        /// </summary>
        public GameObject currentCustomOrbitTarget;
        /// <summary>
        /// The color of the sprite's outline
        /// </summary>
        public Color outlineColor = new Color();
        /// <summary>
        /// Offsets of the specRigidbody
        /// </summary>
        public IntVector2 specBodyOffsets = new IntVector2();
        /// <summary>
        /// Dimensions of the specRigidbody
        /// </summary>
        public IntVector2 specBodyDimensions = new IntVector2();
        /// <summary>
        /// Pixel colliders of the orbital (for when collision events are added) Best left empty for now
        /// </summary>
        public List<PixelCollider> pixelColliders = new List<PixelCollider> { };
        public FiringSequenceMode firingMode;
        public OrbitingMode orbitingMode;
        private Vector2 ownerCenterPos;
        private tk2dSprite sprite;
        private bool initialized;
        private SpeculativeRigidbody body; //Add collsion with enemies code eventually
        private float cooldownTimer;
        private float currentAngle;
        private float closestEnemyDistance;
        private int currentShootIndex;
        private bool isOnCooldown;
        private AIActor targetToShoot;
    }
    
}
