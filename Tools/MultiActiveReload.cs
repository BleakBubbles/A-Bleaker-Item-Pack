using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Utils;

namespace BleakMod
{
    public static class MultiActiveReloadManager
    {
        public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
        public static void SetupHooks()
        {
            Hook hook = new Hook(
                typeof(GameUIReloadBarController).GetMethod("TriggerReload", BindingFlags.Public | BindingFlags.Instance),
                typeof(MultiActiveReloadManager).GetMethod("TriggerReloadHook")
            );
            /*Hook hook2 = new Hook(
                typeof(GameUIReloadBarController).GetMethod("AttemptActiveReload", BindingFlags.Public | BindingFlags.Instance),
                typeof(MultiActiveReloadManager).GetMethod("AttemptActiveReloadHook")
            );*/
            Hook hook3 = new Hook(
                typeof(Gun).GetMethod("OnActiveReloadPressed", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(MultiActiveReloadManager).GetMethod("OnActiveReloadPressedHook")
            );
            Hook hook4 = new Hook(
                typeof(Gun).GetMethod("Reload", BindingFlags.Public | BindingFlags.Instance),
                typeof(MultiActiveReloadManager).GetMethod("ReloadHook")
            );
        }

        public static bool ReloadHook(Func<Gun, bool> orig, Gun self)
        {
            bool result = orig(self);
            if (result && self.GetComponent<MultiActiveReloadController>() != null)
            {
                self.GetComponent<MultiActiveReloadController>().canAttemptActiveReload = true;
                self.GetComponent<MultiActiveReloadController>().damageMult = 1f;
            }
            return result;
        }

        public static void TriggerReloadHook(Action<GameUIReloadBarController, PlayerController, Vector3, float, float, int> orig, GameUIReloadBarController self, PlayerController attachParent, Vector3 offset, float duration, float activeReloadStartPercent,
            int pixelWidth)
        {
            if (tempraryActiveReloads.ContainsKey(self))
            {
                foreach (MultiActiveReload multiactivereload in tempraryActiveReloads[self])
                {
                    if (multiactivereload.sprite != null && multiactivereload.sprite.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(multiactivereload.sprite.gameObject);
                    }
                    if (multiactivereload.celebrationSprite != null && multiactivereload.celebrationSprite.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(multiactivereload.celebrationSprite.gameObject);
                    }
                }
                tempraryActiveReloads[self].Clear();
            }
            orig(self, attachParent, offset, duration, activeReloadStartPercent, pixelWidth);
            if (attachParent != null && attachParent.CurrentGun != null && attachParent.CurrentGun.GetComponent<MultiActiveReloadController>() != null)
            {
                foreach (MultiActiveReloadData data in attachParent.CurrentGun.GetComponent<MultiActiveReloadController>().reloads)
                {
                    dfSprite sprite = UnityEngine.Object.Instantiate(self.activeReloadSprite);
                    self.activeReloadSprite.Parent.AddControl(sprite);
                    sprite.enabled = true;
                    float width = self.progressSlider.Width;
                    float maxValue = self.progressSlider.MaxValue;
                    float num = data.startValue / maxValue * width;
                    float num2 = data.endValue / maxValue * width;
                    float x = num + (num2 - num) * data.activeReloadStartPercentage;
                    float width2 = (float)pixelWidth * Pixelator.Instance.CurrentTileScale;
                    sprite.RelativePosition = self.activeReloadSprite.RelativePosition;
                    sprite.RelativePosition = GameUIUtility.QuantizeUIPosition(sprite.RelativePosition.WithX(x));
                    sprite.Width = width2;
                    sprite.IsVisible = true;
                    dfSprite celebrationSprite = UnityEngine.Object.Instantiate(self.celebrationSprite);
                    self.activeReloadSprite.Parent.AddControl(celebrationSprite);
                    celebrationSprite.enabled = true;
                    dfSpriteAnimation component = celebrationSprite.GetComponent<dfSpriteAnimation>();
                    component.Stop();
                    component.SetFrameExternal(0);
                    celebrationSprite.enabled = false;
                    celebrationSprite.RelativePosition = sprite.RelativePosition + new Vector3(Pixelator.Instance.CurrentTileScale * -1f, Pixelator.Instance.CurrentTileScale * -2f, 0f);
                    int activeReloadStartValue = Mathf.RoundToInt((float)(data.endValue - data.startValue) * data.activeReloadStartPercentage) + data.startValue - data.activeReloadLastTime / 2;
                    MultiActiveReload reload = new MultiActiveReload
                    {
                        sprite = sprite,
                        celebrationSprite = celebrationSprite,
                        startValue = activeReloadStartValue,
                        endValue = activeReloadStartValue + data.activeReloadLastTime,
                        stopsReload = data.stopsReload,
                        canAttemptActiveReloadAfterwards = data.canAttemptActiveReloadAfterwards,
                        reloadData = data.reloadData,
                        usesActiveReloadData = data.usesActiveReloadData
                    };
                    if (tempraryActiveReloads.ContainsKey(self))
                    {
                        tempraryActiveReloads[self].Add(reload);
                    }
                    else
                    {
                        tempraryActiveReloads.Add(self, new List<MultiActiveReload> { reload });
                    }
                }
            }
        }

        public static bool AttemptActiveReloadHook(Func<GameUIReloadBarController, bool> orig, GameUIReloadBarController self)
        {
            if (!self.ReloadIsActive)
            {
                return false;
            }
            if (tempraryActiveReloads.ContainsKey(self))
            {
                foreach (MultiActiveReload reload in tempraryActiveReloads[self])
                {
                    if (self.progressSlider.Value >= (float)reload.startValue && self.progressSlider.Value <= (float)reload.endValue)
                    {
                        self.progressSlider.Color = Color.green;
                        AkSoundEngine.PostEvent("Play_WPN_active_reload_01", self.gameObject);
                        reload.celebrationSprite.enabled = true;
                        reload.sprite.enabled = false;
                        if (reload.stopsReload)
                        {
                            self.progressSlider.Thumb.enabled = false;
                            info.SetValue(self, false);
                        }
                        reload.celebrationSprite.GetComponent<dfSpriteAnimation>().Play();
                        return true;
                    }
                }
            }
            bool result = orig(self);
            return result;
        }

        public static bool AttemptActiveReloadOnlyMultireload(this GameUIRoot self, PlayerController targetPlayer)
        {
            int index = (!targetPlayer.IsPrimaryPlayer) ? 1 : 0;
            bool flag = ((List<GameUIReloadBarController>)info3.GetValue(self))[index].AttemptActiveReloadOnlyMultireload();
            return flag;
        }

        public static bool AttemptActiveReloadOnlyMultireload(this GameUIReloadBarController self)
        {
            if (!self.ReloadIsActive)
            {
                return false;
            }
            if (tempraryActiveReloads.ContainsKey(self))
            {
                foreach (MultiActiveReload reload in tempraryActiveReloads[self])
                {
                    if (self.progressSlider.Value >= (float)reload.startValue && self.progressSlider.Value <= (float)reload.endValue)
                    {
                        self.progressSlider.Color = Color.green;
                        AkSoundEngine.PostEvent("Play_WPN_active_reload_01", self.gameObject);
                        reload.celebrationSprite.enabled = true;
                        reload.sprite.enabled = false;
                        if (reload.stopsReload)
                        {
                            self.progressSlider.Thumb.enabled = false;
                            info.SetValue(self, false);
                        }
                        reload.celebrationSprite.GetComponent<dfSpriteAnimation>().Play();
                        return true;
                    }
                }
            }
            self.progressSlider.Color = Color.red;
            return false;
        }

        public static void OnActiveReloadPressedHook(Action<Gun, PlayerController, Gun, bool> orig, Gun self, PlayerController p, Gun g, bool actualPress)
        {
            orig(self, p, g, actualPress);
            if (self.IsReloading || self.reloadTime < 0f)
            {
                PlayerController playerController = self.CurrentOwner as PlayerController;
                if (playerController && (actualPress || true))
                {
                    MultiActiveReloadController controller = self.GetComponent<MultiActiveReloadController>();
                    if (controller != null && controller.activeReloadEnabled && controller.canAttemptActiveReload && !GameUIRoot.Instance.GetReloadBarForPlayer(self.CurrentOwner as PlayerController).IsActiveReloadGracePeriod())
                    {
                        bool flag2 = GameUIRoot.Instance.AttemptActiveReloadOnlyMultireload(self.CurrentOwner as PlayerController);
                        MultiActiveReload reload = GameUIRoot.Instance.GetReloadBarForPlayer(self.CurrentOwner as PlayerController).GetMultiActiveReloadForController();
                        if (flag2)
                        {
                            controller.OnActiveReloadSuccess(reload);
                            GunFormeSynergyProcessor component = self.GetComponent<GunFormeSynergyProcessor>();
                            if (component)
                            {
                                component.JustActiveReloaded = true;
                            }
                            ChamberGunProcessor component2 = self.GetComponent<ChamberGunProcessor>();
                            if (component2)
                            {
                                component2.JustActiveReloaded = true;
                            }
                        }
                        else
                        {
                            controller.OnActiveReloadFailure(reload);
                        }
                        if (reload == null || !reload.canAttemptActiveReloadAfterwards)
                        {
                            ETGModConsole.Log("yes");
                            controller.canAttemptActiveReload = false;
                            Action<PlayerController, Gun, bool> act = (Action<PlayerController, Gun, bool>)info2.CreateDelegate<Action<PlayerController, Gun, bool>>();
                            self.OnReloadPressed -= act;
                        }
                    }
                }
            }
        }

        public static MultiActiveReload GetMultiActiveReloadForController(this GameUIReloadBarController controller)
        {
            MultiActiveReload result = null;
            if (tempraryActiveReloads.ContainsKey(controller))
            {
                foreach (MultiActiveReload reload in tempraryActiveReloads[controller])
                {
                    if (controller.progressSlider.Value >= (float)reload.startValue && controller.progressSlider.Value <= (float)reload.endValue)
                    {
                        result = reload;
                        break;
                    }
                }
            }
            return result;
        }

        public static Dictionary<GameUIReloadBarController, List<MultiActiveReload>> tempraryActiveReloads = new Dictionary<GameUIReloadBarController, List<MultiActiveReload>>();
        public static FieldInfo info = typeof(GameUIReloadBarController).GetField("m_reloadIsActive", BindingFlags.NonPublic | BindingFlags.Instance);
        public static MethodInfo info2 = typeof(Gun).GetMethod("OnActiveReloadPressed", BindingFlags.NonPublic | BindingFlags.Instance);
        public static FieldInfo info3 = typeof(GameUIRoot).GetField("m_extantReloadBars", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public class MultiActiveReload
    {
        public dfSprite sprite;
        public dfSprite celebrationSprite;
        public int startValue;
        public int endValue;
        public bool stopsReload;
        public bool canAttemptActiveReloadAfterwards;
        public ActiveReloadData reloadData;
        public bool usesActiveReloadData;
    }

    public struct MultiActiveReloadData
    {
        public MultiActiveReloadData(float activeReloadStartPercentage, int startValue, int endValue, int pixelWidth, int activeReloadLastTime, bool stopsReload, bool canAttemptActiveReloadAfterwards, ActiveReloadData reloadData, bool usesActiveReloadData)
        {
            this.activeReloadStartPercentage = activeReloadStartPercentage;
            this.startValue = startValue;
            this.endValue = endValue;
            this.pixelWidth = pixelWidth;
            this.activeReloadLastTime = activeReloadLastTime;
            this.stopsReload = stopsReload;
            this.canAttemptActiveReloadAfterwards = canAttemptActiveReloadAfterwards;
            this.reloadData = reloadData;
            this.usesActiveReloadData = usesActiveReloadData;
        }

        public float activeReloadStartPercentage;
        public int startValue;
        public int endValue;
        public int pixelWidth;
        public int activeReloadLastTime;
        public bool stopsReload;
        public bool canAttemptActiveReloadAfterwards;
        public ActiveReloadData reloadData;
        public bool usesActiveReloadData;
    }

    class MultiActiveReloadController : AdvancedGunBehaviour
    {
        public virtual void OnActiveReloadSuccess(MultiActiveReload reload)
        {
            if (reload == null || reload.stopsReload)
            {
                info.Invoke(base.gun, new object[] { true, false, false });
            }
            float num = 1f;
            if (Gun.ActiveReloadActivated && this.PickedUpByPlayer && this.Player.IsPrimaryPlayer)
            {
                num *= CogOfBattleItem.ACTIVE_RELOAD_DAMAGE_MULTIPLIER;
            }
            if (Gun.ActiveReloadActivatedPlayerTwo && this.PickedUpByPlayer && !this.Player.IsPrimaryPlayer)
            {
                num *= CogOfBattleItem.ACTIVE_RELOAD_DAMAGE_MULTIPLIER;
            }
            if (reload == null || reload.usesActiveReloadData)
            {
                if (base.gun.LocalActiveReload && (reload == null || reload.reloadData == null))
                {
                    num *= Mathf.Pow(this.gun.activeReloadData.damageMultiply, (float)((int)info2.GetValue(base.gun) + 1));
                }
                else if (reload != null && reload.reloadData != null)
                {
                    num *= Mathf.Pow(reload.reloadData.damageMultiply, reload.reloadData.ActiveReloadStacks ? (float)((int)info2.GetValue(base.gun) + 1) : 1);
                }
            }
            this.damageMult = num;
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);
            projectile.baseData.damage *= this.damageMult;
        }

        public virtual void OnActiveReloadFailure(MultiActiveReload reload)
        {
        }

        public override void MidGameDeserialize(List<object> data, ref int dataIndex)
        {
            base.MidGameDeserialize(data, ref dataIndex);
            this.reloads = (List<MultiActiveReloadData>)data[dataIndex];
            this.activeReloadEnabled = (bool)data[dataIndex + 1];
            dataIndex += 2;
        }

        public override void MidGameSerialize(List<object> data, int dataIndex)
        {
            base.MidGameSerialize(data, dataIndex);
            data.Add(this.reloads);
            data.Add(this.activeReloadEnabled);
        }

        public override void InheritData(Gun source)
        {
            base.InheritData(source);
            MultiActiveReloadController component = source.GetComponent<MultiActiveReloadController>();
            if (component)
            {
                this.reloads = component.reloads;
                this.activeReloadEnabled = component.activeReloadEnabled;
            }
        }

        public static MethodInfo info = typeof(Gun).GetMethod("FinishReload", BindingFlags.NonPublic | BindingFlags.Instance);
        public static FieldInfo info2 = typeof(Gun).GetField("SequentialActiveReloads", BindingFlags.NonPublic | BindingFlags.Instance);
        public List<MultiActiveReloadData> reloads = new List<MultiActiveReloadData>();
        public bool canAttemptActiveReload;
        public bool activeReloadEnabled;
        public float damageMult = 1f;
    }

    class AdvancedGunBehaviour : MonoBehaviour, IGunInheritable
    {
        protected virtual void Update()
        {
            if (this.Player != null)
            {
                if (!this.everPickedUpByPlayer)
                {
                    this.everPickedUpByPlayer = true;
                }
            }
            if (this.Owner != null)
            {
                if (!this.everPickedUp)
                {
                    this.everPickedUp = true;
                }
            }
            if (this.lastOwner != this.Owner)
            {
                this.lastOwner = this.Owner;
            }
            if (this.Owner != null && !this.pickedUpLast)
            {
                this.OnPickup(this.Owner);
                this.pickedUpLast = true;
            }
            if (this.Owner == null && this.pickedUpLast)
            {
                if (this.lastOwner != null)
                {
                    this.OnPostDrop(this.lastOwner);
                    this.lastOwner = null;
                }
                this.pickedUpLast = false;
            }
            if (this.gun != null && !this.gun.IsReloading && !this.hasReloaded)
            {
                this.hasReloaded = true;
            }
            this.gun.PreventNormalFireAudio = this.preventNormalFireAudio;
            this.gun.OverrideNormalFireAudioEvent = this.overrideNormalFireAudio;
        }

        public virtual void InheritData(Gun source)
        {
            AdvancedGunBehaviour component = source.GetComponent<AdvancedGunBehaviour>();
            if (component != null)
            {
                this.preventNormalFireAudio = component.preventNormalFireAudio;
                this.preventNormalReloadAudio = component.preventNormalReloadAudio;
                this.overrideNormalReloadAudio = component.overrideNormalReloadAudio;
                this.overrideNormalFireAudio = component.overrideNormalFireAudio;
                this.everPickedUpByPlayer = component.everPickedUpByPlayer;
                this.everPickedUp = component.everPickedUp;
            }
        }

        public virtual void MidGameSerialize(List<object> data, int dataIndex)
        {
            data.Add(this.preventNormalFireAudio);
            data.Add(this.preventNormalReloadAudio);
            data.Add(this.overrideNormalReloadAudio);
            data.Add(this.overrideNormalFireAudio);
            data.Add(this.everPickedUpByPlayer);
            data.Add(this.everPickedUp);
        }

        public virtual void MidGameDeserialize(List<object> data, ref int dataIndex)
        {
            this.preventNormalFireAudio = (bool)data[dataIndex];
            this.preventNormalReloadAudio = (bool)data[dataIndex + 1];
            this.overrideNormalReloadAudio = (string)data[dataIndex + 2];
            this.overrideNormalFireAudio = (string)data[dataIndex + 3];
            this.everPickedUpByPlayer = (bool)data[dataIndex + 4];
            this.everPickedUp = (bool)data[dataIndex + 5];
            dataIndex += 6;
        }

        public virtual void Start()
        {
            this.gun = base.GetComponent<Gun>();
            this.gun.OnInitializedWithOwner += this.OnInitializedWithOwner;
            if (this.gun.CurrentOwner != null)
            {
                this.OnInitializedWithOwner(this.gun.CurrentOwner);
            }
            this.gun.PostProcessProjectile += this.PostProcessProjectile;
            this.gun.PostProcessVolley += this.PostProcessVolley;
            this.gun.OnDropped += this.OnDropped;
            this.gun.OnAutoReload += this.OnAutoReload;
            this.gun.OnReloadPressed += this.OnReloadPressed;
            this.gun.OnFinishAttack += this.OnFinishAttack;
            this.gun.OnPostFired += this.OnPostFired;
            this.gun.OnAmmoChanged += this.OnAmmoChanged;
            this.gun.OnBurstContinued += this.OnBurstContinued;
            this.gun.OnPreFireProjectileModifier += this.OnPreFireProjectileModifier;
        }

        public virtual void OnInitializedWithOwner(GameActor actor)
        {
        }

        public virtual void PostProcessProjectile(Projectile projectile)
        {
        }

        public virtual void PostProcessVolley(ProjectileVolleyData volley)
        {
        }

        public virtual void OnDropped()
        {
        }

        public virtual void OnAutoReload(PlayerController player, Gun gun)
        {
            if (player != null)
            {
                this.OnAutoReloadSafe(player, gun);
            }
        }

        public virtual void OnAutoReloadSafe(PlayerController player, Gun gun)
        {
        }

        public virtual void OnReloadPressed(PlayerController player, Gun gun, bool manualReload)
        {
            if (this.hasReloaded && gun.IsReloading)
            {
                this.OnReload(player, gun);
                this.hasReloaded = false;
            }
            if (player != null)
            {
                this.OnReloadPressedSafe(player, gun, manualReload);
            }
        }

        public virtual void OnReloadPressedSafe(PlayerController player, Gun gun, bool manualReload)
        {
            if (this.hasReloaded && gun.IsReloading)
            {
                this.OnReloadSafe(player, gun);
                this.hasReloaded = false;
            }
        }

        public virtual void OnReload(PlayerController player, Gun gun)
        {
            if (this.preventNormalReloadAudio)
            {
                AkSoundEngine.PostEvent("Stop_WPN_All", base.gameObject);
                if (!string.IsNullOrEmpty(this.overrideNormalReloadAudio))
                {
                    AkSoundEngine.PostEvent(this.overrideNormalReloadAudio, base.gameObject);
                }
            }
        }

        public virtual void OnReloadSafe(PlayerController player, Gun gun)
        {
            if (this.preventNormalReloadAudio)
            {
                AkSoundEngine.PostEvent("Stop_WPN_All", base.gameObject);
                if (!string.IsNullOrEmpty(this.overrideNormalReloadAudio))
                {
                    AkSoundEngine.PostEvent(this.overrideNormalReloadAudio, base.gameObject);
                }
            }
        }

        public virtual void OnFinishAttack(PlayerController player, Gun gun)
        {
        }

        public virtual void OnPostFired(PlayerController player, Gun gun)
        {
            if (gun.IsHeroSword)
            {
                if ((float)heroSwordCooldown.GetValue(gun) == 0.5f)
                {
                    this.OnHeroSwordCooldownStarted(player, gun);
                }
            }
        }

        public virtual void OnHeroSwordCooldownStarted(PlayerController player, Gun gun)
        {
        }

        public virtual void OnAmmoChanged(PlayerController player, Gun gun)
        {
            if (player != null)
            {
                this.OnAmmoChangedSafe(player, gun);
            }
        }

        public virtual void OnAmmoChangedSafe(PlayerController player, Gun gun)
        {
        }

        public virtual void OnBurstContinued(PlayerController player, Gun gun)
        {
            if (player != null)
            {
                this.OnBurstContinuedSafe(player, gun);
            }
        }

        public virtual void OnBurstContinuedSafe(PlayerController player, Gun gun)
        {
        }

        public virtual Projectile OnPreFireProjectileModifier(Gun gun, Projectile projectile, ProjectileModule mod)
        {
            return projectile;
        }

        public AdvancedGunBehaviour()
        {
        }

        protected virtual void OnPickup(GameActor owner)
        {
            if (owner is PlayerController)
            {
                this.OnPickedUpByPlayer(owner as PlayerController);
            }
            if (owner is AIActor)
            {
                this.OnPickedUpByEnemy(owner as AIActor);
            }
        }

        protected virtual void OnPostDrop(GameActor owner)
        {
            if (owner is PlayerController)
            {
                this.OnPostDroppedByPlayer(owner as PlayerController);
            }
            if (owner is AIActor)
            {
                this.OnPostDroppedByEnemy(owner as AIActor);
            }
        }

        protected virtual void OnPickedUpByPlayer(PlayerController player)
        {
        }

        protected virtual void OnPostDroppedByPlayer(PlayerController player)
        {
        }

        protected virtual void OnPickedUpByEnemy(AIActor enemy)
        {
        }

        protected virtual void OnPostDroppedByEnemy(AIActor enemy)
        {
        }

        public bool PickedUp
        {
            get
            {
                return this.gun.CurrentOwner != null;
            }
        }

        public PlayerController Player
        {
            get
            {
                if (this.gun.CurrentOwner is PlayerController)
                {
                    return this.gun.CurrentOwner as PlayerController;
                }
                return null;
            }
        }

        public float HeroSwordCooldown
        {
            get
            {
                if (this.gun != null)
                {
                    return (float)heroSwordCooldown.GetValue(this.gun);
                }
                return -1f;
            }
            set
            {
                if (this.gun != null)
                {
                    heroSwordCooldown.SetValue(this.gun, value);
                }
            }
        }

        public GameActor Owner
        {
            get
            {
                return this.gun.CurrentOwner;
            }
        }

        public bool PickedUpByPlayer
        {
            get
            {
                return this.Player != null;
            }
        }

        private bool pickedUpLast = false;
        private GameActor lastOwner = null;
        public bool everPickedUpByPlayer = false;
        public bool everPickedUp = false;
        protected Gun gun;
        private bool hasReloaded = true;
        public bool preventNormalFireAudio;
        public bool preventNormalReloadAudio;
        public string overrideNormalFireAudio;
        public string overrideNormalReloadAudio;
        private static FieldInfo heroSwordCooldown = typeof(Gun).GetField("HeroSwordCooldown", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}