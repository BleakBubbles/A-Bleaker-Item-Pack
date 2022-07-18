using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Dungeonator;
using UnityEngine;

namespace BleakMod
{
    public class OrbitalHaltInteractManager : BraveBehaviour, IPlayerInteractable
    {
        private void Start()
        {
            this.m_room = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Round));
            this.m_room.RegisterInteractable(this);
            this.m_room.TransferInteractableOwnershipToDungeon(this);
        }

        // Token: 0x06007CE2 RID: 31970 RVA: 0x00315F54 File Offset: 0x00314154	
        public float GetDistanceToPoint(Vector2 point)
        {
            if (!base.sprite) return float.MaxValue;
            Bounds bounds = base.sprite.GetBounds();
            bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
            float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
            float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
            return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
        }

        // Token: 0x06007CE3 RID: 31971 RVA: 0x0026DAFF File Offset: 0x0026BCFF
        public void OnEnteredRange(PlayerController interactor)
        {
            SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
            SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
        }

        // Token: 0x06007CE4 RID: 31972 RVA: 0x00316033 File Offset: 0x00314233
        public void OnExitRange(PlayerController interactor)
        {
            SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
            SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
        }

        // Token: 0x06007CE5 RID: 31973 RVA: 0x00316054 File Offset: 0x00314254
        public void Interact(PlayerController interactor)
        {
            if (!this.isOnCooldown)
            {
                base.StartCoroutine(this.Halt());
            }
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
            PlayerController player = GameManager.Instance.PrimaryPlayer;
            if (!this.isOnCooldown && player.IsInCombat)
            {
                return 3f;
            }
            else
            {
                return 0.1f;
            }
        }

        private IEnumerator Halt()
        {
            if(base.GetComponent<PlayerOrbital>() != null)
            {
                //base.GetComponent<PlayerOrbital>().orbitDegreesPerSecond = 360f;
                base.GetComponent<PlayerOrbital>().specRigidbody.Initialize();

                TextBoxManager.ShowTextBox(base.transform.localPosition + new Vector3(0, 0.5f, 0), base.transform, 2, this.voicelines[UnityEngine.Random.Range(0, this.voicelines.Count)], "", false, TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, false, false);
                this.isOnCooldown = true;
            }
            yield return new WaitForSeconds(10);
            if(base.GetComponent<PlayerOrbital>() != null)
            {
                //base.GetComponent<PlayerOrbital>().orbitDegreesPerSecond = 120f;
                base.GetComponent<PlayerOrbital>().specRigidbody.Initialize();
                isOnCooldown = false;
            }
        }

        // Token: 0x06007CE8 RID: 31976 RVA: 0x0003AB5B File Offset: 0x00038D5B
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        private RoomHandler m_room;
        private bool isOnCooldown;
        private List<String> voicelines = new List<string>
        {
            "Stay down!",
            "Roger that.",
            "On it, chief.",
            "I got your back.",
            "Affirmative."
        };
    }
}
