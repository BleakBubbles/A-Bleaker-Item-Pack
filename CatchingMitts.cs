using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using Dungeonator;

namespace BleakMod
{
    class CatchingMitts: PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Register()
        {
            //The name of the item
            string itemName = "Catcher's Mitts";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/glove";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<CatchingMitts>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Grab 'Em While They're Hot!";
            string longDesc = "Press E or your interact button to catch nearby enemy bullets. Doing so has a chance to relplenish 1 ammo into your current gun.\n\nA tattered baseball glove found in the outskirts of the gungeon. Who could it have belonged to?";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.quality = ItemQuality.A;
            CustomSynergies.Add("Shortstop", new List<string>
            {
                "bb:catcher's_mitts"
            }, new List<string>
            {
                "casey"
            }, true);
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            PassiveItem.IncrementFlag(player, typeof(CatchingMitts));
            if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
            {
                PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
            }
            cooldownBehavior cooldownBehavior = player.gameObject.GetOrAddComponent<cooldownBehavior>();
            cooldownBehavior.parentItem = this;
        }
        protected override void Update()
        {
            base.Update();
            if (base.Owner)
            {
                this.cooldown += BraveTime.DeltaTime;
                ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
                if (allProjectiles != null)
                {
                    foreach (Projectile proj in allProjectiles)
                    {
                        bool x = Vector2.Distance(proj.sprite.WorldCenter, base.Owner.sprite.WorldCenter) < 5 && proj != null && proj.sprite != null && proj.Owner != base.Owner;
                        if (x)
                        {
                            //cooldownBehavior cooldownBehavior =  proj.gameObject.GetOrAddComponent<cooldownBehavior>();
                            //cooldownBehavior.parentItem = this;
                            proj.gameObject.GetOrAddComponent<ProjectileInteractManager>();
                        }
                    }
                }
            }
        }
        public class cooldownBehavior : MonoBehaviour
        {
            private void Start()
            {
            }
            public CatchingMitts parentItem = null;
        }
        public override DebrisObject Drop(PlayerController player)
        {
            PassiveItem.DecrementFlag(player, typeof(CatchingMitts));
            cooldownBehavior cooldownBehavior = player.gameObject.GetComponent<cooldownBehavior>();
            UnityEngine.Object.Destroy(cooldownBehavior);
            return base.Drop(player);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.Owner != null){
                PlayerController player = base.Owner;
                PassiveItem.DecrementFlag(player, typeof(CatchingMitts));
                cooldownBehavior cooldownBehavior = player.gameObject.GetComponent<cooldownBehavior>();
                UnityEngine.Object.Destroy(cooldownBehavior);
            }
        }
        public float cooldown = 0f;
    }
}
    