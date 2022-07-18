using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;

namespace BleakMod
{
    public class ChamberOfFrogs: PassiveItem
    {   
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Chamber Of Frogs";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/chamber_of_frogs";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<ChamberOfFrogs>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Ribbit!";
            string longDesc = "Bullets have a chance to shoot out a tongue that snatches enemies. A strange chamber taken from one of the ponds of Gunymede. It seems friendly.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.A;
        }
        public void PostProcessProjectile(Projectile proj, float f)
        {
            float chance = 0.2f * f;
            if(UnityEngine.Random.value <= chance)
            {
                proj.baseData.speed *= 0.5f;
                proj.gameObject.GetOrAddComponent<FrogTongueBehavior>();
            }
        }
        public class FrogTongueBehavior : MonoBehaviour
        {
            public FrogTongueBehavior()
            {
                m_beamDuration = 0.2f;
                //tongue = null;
                hasKnockedBackEnemy = false;
            }
            public void Start()
            {
                try
                {
                    this.m_projectile = base.GetComponent<Projectile>();
                    this.cooldownTime = m_projectile.baseData.speed / 50f;
                    this.timer = cooldownTime;
                }
                catch (Exception e)
                {
                    ETGModConsole.Log(e.Message);
                    ETGModConsole.Log(e.StackTrace);
                }
            }
            private void Update()
            {
                if (m_projectile != null)
                {
                    if (timer > 0)
                    {
                        timer -= BraveTime.DeltaTime;
                    }
                    if (timer <= 0)
                    {
                        this.tongue = this.DoTongueFlick(m_projectile);
                        cooldownTime *= 2f;
                        timer = cooldownTime;
                        hasKnockedBackEnemy = false;
                    }
                    if (tongue != null && !this.hasKnockedBackEnemy)
                    {
                        RaidenBeamController slurp = tongue.GetComponent<RaidenBeamController>();
                        List<AIActor> targets = Stuff.ReflectGetField<List<AIActor>>(typeof(RaidenBeamController), "m_targets", slurp);
                        foreach (AIActor enemy in targets)
                        {
                            if (enemy && enemy.healthHaver && enemy.healthHaver.IsAlive && enemy.knockbackDoer && !enemy.gameActor.behaviorSpeculator.IsStunned)
                            {
                                hasKnockedBackEnemy = true;
                                float origWeight = enemy.knockbackDoer.weight;
                                enemy.knockbackDoer.weight = 10;
                                enemy.knockbackDoer.shouldBounce = false;
                                float distance = m_projectile.baseData.speed * 0.2f;
                                Vector2 angle = m_projectile.Direction;
                                Vector2 vector = BraveMathCollege.DegreesToVector(angle.ToAngle(), distance);
                                enemy.knockbackDoer.ApplyKnockback((enemy.specRigidbody.UnitCenter - (m_projectile.sprite.WorldCenter + vector)), -5f * Vector2.Distance(enemy.specRigidbody.UnitCenter, m_projectile.sprite.WorldCenter));
                                if (!enemy.gameActor.behaviorSpeculator.IsStunned)
                                {
                                    enemy.gameActor.behaviorSpeculator.Stun(2f, true);
                                }
                                enemy.knockbackDoer.weight = origWeight;
                            }
                        }
                    }
                }
            }
            private BeamController DoTongueFlick(Projectile proj)
            {
                Func<AIActor, bool> isValid = (AIActor a) => a && a.HasBeenEngaged && a.healthHaver && a.healthHaver.IsVulnerable;
                AIActor closestToPosition = BraveUtility.GetClosestToPosition<AIActor>(m_projectile.Owner.GetAbsoluteParentRoom().GetActiveEnemies(RoomHandler.ActiveEnemyType.All), m_projectile.sprite.WorldCenter, isValid, affectedEnemies.ToArray());
                this.affectedEnemies.Add(closestToPosition);
                BeamController tongue = Stuff.FreeFireBeamFromAnywhere((PickupObjectDatabase.GetById(759) as Gun).DefaultModule.projectiles[0], m_projectile.Owner as PlayerController, m_projectile.gameObject, Vector2.zero, false, (closestToPosition.specRigidbody.UnitCenter - m_projectile.sprite.WorldCenter).ToAngle(), m_beamDuration);
                tongue.projectile.baseData.damage *= 0.5f;
                return tongue;
            }
            public float timer;
            private float m_beamDuration;
            private Projectile m_projectile;
            private BeamController tongue;
            private List<AIActor> affectedEnemies = new List<AIActor>();
            private float cooldownTime;
            private bool hasKnockedBackEnemy;

        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.PostProcessProjectile += this.PostProcessProjectile;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            player.PostProcessProjectile -= this.PostProcessProjectile;
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (base.Owner != null)
            {
                base.Owner.PostProcessProjectile -= this.PostProcessProjectile;
            }
        }
    }
}