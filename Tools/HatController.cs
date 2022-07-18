﻿using System;
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
    class HatController : MonoBehaviour
    {
        private Hat m_currentHat;
        private GameObject m_extantHatObject;
        private PlayerController m_WearingPlayer;

        public HatController()
        {

        }
        private void Start()
        {
            this.m_WearingPlayer = base.GetComponent<PlayerController>();
            if (m_WearingPlayer != null)
            {
                if (PlayerHatDatabase.StoredHats.ContainsKey(m_WearingPlayer.name)) //Checks if the player character is in the dictionary
                {
                    RecalculateHat();
                }
                else //If the player character is NOT in the dictionary, we add them with a 'Null' hat
                {
                    PlayerHatDatabase.StoredHats.Add(m_WearingPlayer.name, "Null");
                }
            }
        }

        public void RecalculateHat()
        {
            string StoredHatName = PlayerHatDatabase.StoredHats[m_WearingPlayer.name]; //Fetches the name of the hat stored for the wearer
            if (m_currentHat != null && StoredHatName != m_currentHat.hatName)
            {
                RemoveCurrentHat(); //Removes the current hat if it's not equal to the stored one
                if (StoredHatName != "Null") //If the stored hat is not a null hat, therefore we'll need to add another one back
                {
                    if (HatLibrary.Hats.ContainsKey(StoredHatName)) // Makes sure the stored hat is a real hat
                    {
                        Hat StoredHat = HatLibrary.Hats[StoredHatName]; //Fetches the hat data from the library
                        SetHat(StoredHat); //Sets the current hat to the fetched one
                    }
                }
            }
            else if (m_currentHat == null && StoredHatName != "Null") //If the current hat is null, but it shouldn't be
            {
                if (HatLibrary.Hats.ContainsKey(StoredHatName)) //Makes sure the stored hat is a real hat
                {
                    Hat StoredHat = HatLibrary.Hats[StoredHatName];  //Fetches the hat data from the library
                    SetHat(StoredHat); //Sets the current hat to the fetched one
                }
            }
        }
        public void SetHat(Hat hat)
        {
            if (m_extantHatObject != null) RemoveCurrentHat(); //Makes sure we're not trying to add a hat where one already exists.

            //Set up the code for spawning the hat in here
            GameObject createdHat = UnityEngine.Object.Instantiate(hat.gameObject);
            createdHat.SetActive(true);
            Hat newlyMadeHat = createdHat.GetComponent<Hat>();
            newlyMadeHat.hatOwner = m_WearingPlayer;
            newlyMadeHat.StickHatToPlayer(this.m_WearingPlayer);

            this.m_currentHat = newlyMadeHat;
            SetStoredHatName(newlyMadeHat.hatName); //Handles storing the new hat in the dictionary

            this.m_extantHatObject = createdHat;
        }

        public void RemoveCurrentHat()
        {
            UnityEngine.Object.Destroy(m_extantHatObject); //Deletes the hat
            SetStoredHatName("Null"); //Sets the stored hat name to 'Null'
            this.m_extantHatObject = null;
            this.m_currentHat = null; //Nulls some variables
        }

        public void SetStoredHatName(string hatNameToStore) //Sets the stored name of the hat for that player
        {
            if (!PlayerHatDatabase.StoredHats.ContainsKey(m_WearingPlayer.name))
            { //Makes absolutely sure that the player is in the database before we try and change the stored hat
                PlayerHatDatabase.StoredHats.Add(m_WearingPlayer.name, "Null");
            }
            string StoredHatName = PlayerHatDatabase.StoredHats[m_WearingPlayer.name];
            if (StoredHatName != hatNameToStore) //Makes sure we're not faffing around trying to re-store a hat that's already stored
            {
                PlayerHatDatabase.StoredHats[m_WearingPlayer.name] = hatNameToStore; //Replaces the current stored hat name with the new one
            }
        }

        public Hat CurrentHat
        {
            get
            {
                return m_currentHat;
            }
        }
    }

    public class PlayerHatDatabase
    {
        public static Dictionary<string, string> StoredHats = new Dictionary<string, string>()
        {
        };
        public static Dictionary<string, float> CharacterNameHatHeadLevel = new Dictionary<string, float>()
        { //Vertical offset for where head-top hats should be placed on different characters (using character name)
            {"PlayerRogue(Clone)",  0.43f},
            {"PlayerMarine(Clone)", 0.37f },
            {"PlayerGuide(Clone)", 0.31f },
            {"PlayerConvict(Clone)", 0.37f},
            {"PlayerRobot(Clone)", 0.56f},
            {"PlayerBullet(Clone)", 0.68f },
            {"PlayerGunslinger(Clone)", 0.56f },
            {"PlayerCoopCultist(Clone)", 0.43f },
        };
        public static Dictionary<string, float> CharacterNameEyeLevel = new Dictionary<string, float>()
        { //Vertical offset for where head-top hats should be placed on different characters (using character name)
            {"PlayerRogue(Clone)",  0.18f},
            {"PlayerMarine(Clone)", 0.12f },
            {"PlayerGuide(Clone)", -0.18f },
            {"PlayerConvict(Clone)", -0.06f},
            {"PlayerRobot(Clone)", 0.25f},
            {"PlayerBullet(Clone)", -0.31f },
            {"PlayerGunslinger(Clone)", 0.25f },
            {"PlayerCoopCultist(Clone)", 0.06f },
        };
        public static float defaultHeadLevelOffset = 0.37f;
        public static float defaultEyeLevelOffset = 0.06f;
    }

    public static class HatLibrary
    {
        public static Dictionary<string, Hat> Hats = new Dictionary<string, Hat>()
        {
        };
    }
}
