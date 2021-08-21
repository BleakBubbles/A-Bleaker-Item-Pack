using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;
using MultiplayerBasicExample;
using Dungeonator;

namespace BleakMod
{
    class PickpocketGuide: PlayerItem
    {
        //Call this method from the Start() method of your ETGModule extension class
        public static void Init()
        {
            //The name of the item
            string itemName = "Pilot's Guide to Pickpocketing";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it.
            string resourceName = "BleakMod/Resources/pickpocket_book";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a ActiveItem component to the object
            var item = obj.AddComponent<PickpocketGuide>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Yoink!";
            string longDesc = "While active, allows you to sneak up on enemies and steal from them by pressing E or your interact button.\n\nAn old but gold book that always accompanied the Pilot on his travels.\nLegends say he doesn't actually read it, he just wants to show off the fact that he wrote it.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //"kts" here is the item pool. In the console you'd type kts:sweating_bullets
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");
            //Set the cooldown type and duration of the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 200f);
            //Adds a passive modifier, like curse, coolness, damage, etc. to the item. Works for passives and actives.
            //Set some other fields
            item.consumable = false;
            item.quality = ItemQuality.C;
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            PassiveItem.IncrementFlag(player, typeof(PickpocketGuide));
            if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
            {
                PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
            }
            unstealthBehavior unstealthBehavior = player.gameObject.GetOrAddComponent<unstealthBehavior>();
            unstealthBehavior.parentItem = this;
        }
        protected override void OnPreDrop(PlayerController user)
        {
            PassiveItem.DecrementFlag(user, typeof(PickpocketGuide));
            unstealthBehavior unstealthBehavior = user.gameObject.GetOrAddComponent<unstealthBehavior>();
            UnityEngine.Object.DestroyImmediate(unstealthBehavior);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.LastOwner != null)
            {
                PassiveItem.DecrementFlag(base.LastOwner, typeof(PickpocketGuide));
                unstealthBehavior unstealthBehavior = base.LastOwner.gameObject.GetOrAddComponent<unstealthBehavior>();
                UnityEngine.Object.DestroyImmediate(unstealthBehavior);
            }
        }
        protected override void DoEffect(PlayerController user)
		{
            this.StealthEffect();
            base.StartCoroutine(ItemBuilder.HandleDuration(this, this.duration, user, new Action<PlayerController>(this.BreakStealth)));
            this.enemies = user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
            foreach(AIActor enemy in enemies)
            {
                if (enemy.sprite)
                {
                    enemy.gameObject.GetOrAddComponent<EnemyInteractManager>();
                }
            }
        }
        private void StealthEffect()
        {
            PlayerController owner = base.LastOwner;
            owner.SetIsStealthed(false, "thief");
            owner.SetIsStealthed(true, "thief");
            AkSoundEngine.PostEvent("Play_ENM_wizardred_vanish_01", base.gameObject);
        }
        public void BreakStealth(PlayerController player)
        {
            player.SetIsStealthed(false, "thief");
            AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", base.gameObject);
            foreach(AIActor enemy in enemies)
            {
                if(enemy.gameObject.GetComponent<EnemyInteractManager>() != null)
                {
                    RoomHandler room = enemy.GetAbsoluteParentRoom();
                    EnemyInteractManager interactManager = enemy.gameObject.GetComponent<EnemyInteractManager>();

                    room.DeregisterInteractable(interactManager);
                    UnityEngine.Object.Destroy(interactManager);
                }
            }
            base.IsCurrentlyActive = false;
        }
        public class unstealthBehavior : MonoBehaviour
        {
            private void Start()
            {
            }
            public PickpocketGuide parentItem = null;
        }
        public override bool CanBeUsed(PlayerController user)
        {
            return user.IsInCombat && base.CanBeUsed(user);
        }
        private float duration = 3f;
        private List<AIActor> enemies;
    }
}
    