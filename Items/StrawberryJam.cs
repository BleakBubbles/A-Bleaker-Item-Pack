using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using UnityEngine;
using ItemAPI;
using Dungeonator;
using MultiplayerBasicExample;

namespace BleakMod
{
    public class StrawberryJam: PassiveItem
    {
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Strawberry Jam";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BleakMod/Resources/strawberry_jam";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<StrawberryJam>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Dread and Butter";
            string longDesc = "Jammed enemies have a high chance of being instantly charmed.\n\nStrangely, jammed enemies have a knack for the gooey, sweet substance known as jam." +
                " Maybe you can use it to your advantage and keep them in place...";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bb");

            //Adds the actual passive effect to the item
            item.AddPassiveStatModifier(PlayerStats.StatType.Curse, 1.5f, StatModifier.ModifyMethod.ADDITIVE);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
            item.AddToSubShop(ItemBuilder.ShopType.Cursula, 1f);
            item.AddToSubShop(ItemBuilder.ShopType.Goopton, 1f);
            CustomSynergies.Add("Ooey Gooey", new List<string>
            {
                "bb:strawberry_jam"
            }, new List<string>
            {
                "shotga_cola",
                "shotgun_coffee",
                "double_vision",
                "potion_of_gun_friendship",
                "potion_of_lead_skin",
                "old_knights_flask",
                "orange"
            }, true);
        }
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnEnteredCombat += this.OnEnteredCombat;
        }
        public class AffectedEnemyFlag : MonoBehaviour
        {

        }
        private void OnEnteredCombat()
        {
            this.affectedEnemies.Clear();
        }
        protected override void Update()
        {
            if (this.m_owner != null && this.m_pickedUp && this.m_owner.CurrentRoom != null)
            {
                foreach (AIActor aiactor in base.Owner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
                {
                    if (aiactor.GetComponent<AffectedEnemyFlag>() == null && !aiactor.healthHaver.IsBoss && aiactor)
                    {
                        this.OnNewEnemyAppeared(aiactor);
                        aiactor.gameObject.AddComponent<AffectedEnemyFlag>();
                    }
                }
                if (this.m_pickedUp && this.m_owner && this.affectedEnemies != null && (this.m_owner.CurrentRoom == null || this.m_owner.CurrentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) <= 0))
                {
                    foreach(AIActor a in this.affectedEnemies)
                    {
                        if (!a.IsMimicEnemy && a)
                        {
                            this.EatCharmedEnemy(a);
                        }
                    }
                }
            }
        }
        private void OnNewEnemyAppeared(AIActor aiactor)
        {
            if(this.Owner.HasMTGConsoleID("shotga_cola") || this.Owner.HasMTGConsoleID("shotgun_coffee") || this.Owner.HasMTGConsoleID("double_vision") || this.Owner.HasMTGConsoleID("potion_of_gun_friendship") || this.Owner.HasMTGConsoleID("potion_of_lead_skin") || this.Owner.HasMTGConsoleID("old_knight's_flask") || this.Owner.HasMTGConsoleID("Orange"))
            {
                this.charmChance = 1f;
            }
            else
            {
                this.charmChance = 0.75f;
            }
            if(aiactor != null && aiactor.IsBlackPhantom && UnityEngine.Random.value <= this.charmChance)
            {
                aiactor.ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect, 1f, null);
                aiactor.IgnoreForRoomClear = true;
                this.affectedEnemies.Add(aiactor);
            }
        }
        private void EatCharmedEnemy(AIActor a)
        {
            if (!a)
            {
                return;
            }
            if (a.behaviorSpeculator)
            {
                a.behaviorSpeculator.Stun(1f, true);
            }
            if (a.knockbackDoer)
            {
                a.knockbackDoer.SetImmobile(true, "YellowChamberItem");
            }
            //GameObject gameObject = a.PlayEffectOnActor(this.yellowchamber.EraseVFX, new Vector3(0f, -1f, 0f), false, false, false);
            this.DelayedDestroyEnemy(a);
        }
        private void DelayedDestroyEnemy(AIActor enemy)
	    {
            if(enemy && base.Owner)
            {
                this.TelefragVFXPrefab = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Tentacleport");
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.TelefragVFXPrefab);
                gameObject.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(enemy.sprite.WorldCenter, tk2dBaseSprite.Anchor.LowerCenter);
                gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
                gameObject.GetComponent<tk2dBaseSprite>().UpdateZDepth();
                if (enemy)
                {
                    enemy.EraseFromExistence(false);
                }
            }
	    }
        public override DebrisObject Drop(PlayerController player)
        {
            player.OnEnteredCombat -= this.OnEnteredCombat;
            DebrisObject debrisObject = base.Drop(player);
            debrisObject.GetComponent<StrawberryJam>().m_pickedUpThisRun = true;
            return debrisObject;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(base.Owner != null)
            {
                base.Owner.OnEnteredCombat -= this.OnEnteredCombat;
            }
        }
        public float charmChance;
        public List<AIActor> affectedEnemies = new List<AIActor>();
        YellowChamberItem yellowchamber = new YellowChamberItem();
        public GameObject TelefragVFXPrefab;
    }
}