using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dungeonator;
using UnityEngine;

namespace BleakMod
{
    public class CannonInteractManager : BraveBehaviour, IPlayerInteractable
    {
        private void Start()
        {
            //Some boilerplate code that associates and register the interactable with its current room
            this.m_room = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Round));
            this.m_room.RegisterInteractable(this);
        }

        public float GetDistanceToPoint(Vector2 point)
        {
            //A method to get the distance from the interactable from any point
            if (!base.sprite) return float.MaxValue;
            Bounds bounds = base.sprite.GetBounds();
            bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
            float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
            float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
            return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
        }

        public void OnEnteredRange(PlayerController interactor)
        {
            //A method that runs whenever the player enters the interaction range of the interactable. This is what outlines it in white to show that it can be interacted with
            SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
            SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
        }

        public void OnExitRange(PlayerController interactor)
        {
            //A method that runs whenever the player exits the interaction range of the interactable. This is what removed the white outline to show that it cannot be currently interacted with
            SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
            SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
        }

        public void Interact(PlayerController interactor)
        {
            //This method is what runs when the player actually interacts with the interactable. I put an if statement around the code to ensure that the projectile still exists
            if (base.projectile)
            {
                //This float is equal to the direction the projectile is facing (not the direction it is going in). This will used later on to fire a projectile in that direction
                float rotation = base.projectile.transform.rotation.eulerAngles.z;
                //Getting the projectile of Serious Cannon via ID
                Projectile projectile = ((Gun)global::ETGMod.Databases.Items[37]).DefaultModule.chargeProjectiles[0].Projectile;
                //Spawning the projectile and setting a variable equal to it for future access
                GameObject obj = SpawnManager.SpawnProjectile(projectile.gameObject, base.projectile.sprite.WorldTopCenter, Quaternion.Euler(0, 0, rotation));
                //Retrieving the spawned projectile using GetComponent
                Projectile proj = obj.GetComponent<Projectile>();
                //Setting the projectile's owner and shooter to the player
                proj.Owner = interactor;
                proj.Shooter = interactor.specRigidbody;
                //Scaling down the size of the projectile a little bit
                proj.AdditionalScaleMultiplier = 0.75f;
                //If the player does not have the Pirate Prowess synergy, set the number of bounces the projectile has to 0
                if(!interactor.PlayerHasActiveSynergy("Pirate Prowess"))
                {
                    BounceProjModifier boing = proj.GetComponent<BounceProjModifier>();
                    boing.numberOfBounces = 0;
                }
                //Deregister the interactable so that it cannot be further intereacted with
                this.m_room.DeregisterInteractable(this);
            }
        }

        public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
        {
            //Some boilerplate code for determining if the interactable should be flipped
            shouldBeFlipped = false;
            return string.Empty;
        }

        public float GetOverrideMaxDistance()
        {
            //This float determines the range at which the player can interact with the interactable
            //If the player has the synergy ARRRR-C, the range will be much larger compared to if the player does not have the synergy.
            if (GameManager.Instance.PrimaryPlayer.PlayerHasActiveSynergy("ARRRR-C"))
            {
                return 20f;
            }
            return 3f;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        private RoomHandler m_room;
    }
}
