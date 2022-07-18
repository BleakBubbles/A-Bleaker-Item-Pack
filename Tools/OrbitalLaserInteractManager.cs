using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dungeonator;
using UnityEngine;

namespace BleakMod
{
    public class OrbitalLaserInteractManager : BraveBehaviour, IPlayerInteractable
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
            PrismaticGuonStone.cooldownBehavior cooldownBehavior = interactor.GetComponent<PrismaticGuonStone.cooldownBehavior>();
            Projectile projectile = (PickupObjectDatabase.GetById(508) as Gun).DefaultModule.projectiles[0];
            if (projectile.GetComponentInChildren<tk2dTiledSprite>() != null)
            {
                projectile.GetComponentInChildren<tk2dTiledSprite>().usesOverrideMaterial = true;
                if (interactor.PlayerHasActiveSynergy("Prismatismer Guon Stone"))
                {
                    projectile.GetComponentInChildren<tk2dTiledSprite>().renderer.material.shader = ShaderCache.Acquire("Brave/Internal/RainbowChestShader");
                    projectile.GetComponentInChildren<tk2dTiledSprite>().renderer.material.SetFloat("_HueTestValue", 0);
                }
                else
                {
                    tk2dTiledSprite baseSprite = projectile.GetComponentInChildren<tk2dTiledSprite>();
                    baseSprite.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
                    Material material = baseSprite.renderer.material;
                    float value = 0f;
                    float value2 = 0f;
                    value = material.GetFloat("_EmissivePower");
                    value2 = material.GetFloat("_EmissiveColorPower");
                    Shader shader;
                    shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                    if (baseSprite.renderer.material.shader != shader)
                    {
                        baseSprite.renderer.material.shader = shader;
                        baseSprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                        baseSprite.renderer.material.SetFloat("_EmissivePower", value);
                        baseSprite.renderer.material.SetFloat("_EmissiveColorPower", value2);
                    }
                    baseSprite.renderer.sharedMaterial.SetColor("_OverrideColor", this.outColors[colorCount % 6]);
                    colorCount++;
                }
            }
            float angle = base.transform.rotation.eulerAngles.z;
            GameObject obj1 = SpawnManager.SpawnProjectile(projectile.gameObject, base.specRigidbody.transform.localPosition.XY(), Quaternion.Euler(0f, 0f, (interactor.CurrentGun == null) ? 0f : angle), true);
            Projectile proj1 = obj1.GetComponent<Projectile>();
            proj1.Owner = interactor;
            proj1.Shooter = interactor.specRigidbody;
            if(interactor.PlayerHasActiveSynergy("Prismatismer Guon Stone"))
            {
                proj1.baseData.damage = 75f;
            }
            else
            {
                proj1.baseData.damage = 50f;
            }
            cooldownBehavior.parentItem.beamCooldown = 0f;
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
            PrismaticGuonStone.cooldownBehavior cooldownBehavior = player.GetComponent<PrismaticGuonStone.cooldownBehavior>();
            if (cooldownBehavior.parentItem.beamCooldown > (player.PlayerHasActiveSynergy("Prismatismer Guon Stone") ? 1f : 2f) && player.IsInCombat)
            {
                return 2f;
            }
            else
            {
                return 0.1f;
            }
        }

        // Token: 0x06007CE8 RID: 31976 RVA: 0x0003AB5B File Offset: 0x00038D5B
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        private RoomHandler m_room;
        //private List<Color32> outColors = new List<Color32>
        //{
        //    new Color32(255, 80, 64, 255),
        //    new Color32(255, 172, 64, 255),
        //    new Color32(239, 255, 64, 255),
        //    new Color32(64, 255, 80, 255),
        //    new Color32(80, 64, 255, 255),
        //    new Color32(182, 64, 255, 255)
        //};
        private List<Color> outColors = new List<Color>
        {
            Color.red,
            new Color(1f, 0.5f, 0f, 1f),
            Color.yellow,
            Color.green,
            Color.blue,
            new Color(0.5f, 0f, 1f, 1f)
        };
        private int colorCount = 0;
    }
}
