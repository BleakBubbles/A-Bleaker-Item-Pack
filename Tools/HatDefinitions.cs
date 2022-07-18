using System;
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
    class HatDefinitions
    {
        public static void Init()
        {
            string hatName = "The Berserker";
            GameObject obj = new GameObject(hatName);
            Hat hat = obj.AddComponent<Hat>();
            hat.hatDepthType = Hat.HatDepthType.AlwaysInFront;
            hat.hatName = hatName;
            hat.hatDirectionality = Hat.HatDirectionality.FOURWAY;
            hat.hatRollReaction = Hat.HatRollReaction.FLIP;
            hat.hatOffset = new Vector2(0, -0.12f);

            List<string> spritePaths = new List<string>()
            {
                "ExampleMod/Resources/theberserker_south_001",
                "ExampleMod/Resources/theberserker_north_001",
                "ExampleMod/Resources/theberserker_west_001",
                "ExampleMod/Resources/theberserker_east_001",
            };

            HatUtility.SetupHatSprites(spritePaths, obj, 1, new Vector2(14, 11));
            obj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(obj);
            UnityEngine.Object.DontDestroyOnLoad(obj);
            HatUtility.AddHatToDatabase(obj);

            string stovepipeName = "The Stovepipe";
            GameObject stovepipeObj = new GameObject(stovepipeName);
            Hat stovepipe = stovepipeObj.AddComponent<Hat>();
            stovepipe.hatName = stovepipeName;
            stovepipe.hatDirectionality = Hat.HatDirectionality.NONE;
            stovepipe.hatRollReaction = Hat.HatRollReaction.FLIP;
            stovepipe.hatOffset = new Vector2(0, -0.12f);
            List<string> stovepipeSprites = new List<string>()
            {
                "ExampleMod/Resources/thestovepipe_south_001",
            };
            HatUtility.SetupHatSprites(stovepipeSprites, stovepipeObj, 1, new Vector2(14, 12));
            stovepipeObj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(stovepipeObj);
            UnityEngine.Object.DontDestroyOnLoad(stovepipeObj);
            HatUtility.AddHatToDatabase(stovepipeObj);

            var headDressName = "HeadDress";
            GameObject headDresseObj = new GameObject(headDressName);
            Hat headDress = headDresseObj.AddComponent<Hat>();
            headDress.hatName = headDressName;
            headDress.hatDirectionality = Hat.HatDirectionality.FOURWAY;
            headDress.hatRollReaction = Hat.HatRollReaction.VANISH;
            headDress.hatOffset = new Vector2(0, -0.3f);
            headDress.hatDepthType = Hat.HatDepthType.InFrontWhenFacingBack;
            List<string> headDressSprites = new List<string>()
            {
                "ExampleMod/Resources/headdress_north_001",
                 "ExampleMod/Resources/headdress_east_001",
                "ExampleMod/Resources/headdress_south_001",
                "ExampleMod/Resources/headdress_west_001",
            };
            HatUtility.SetupHatSprites(headDressSprites, headDresseObj, 1, new Vector2(14, 12));
            headDresseObj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(headDresseObj);
            UnityEngine.Object.DontDestroyOnLoad(headDresseObj);
            HatUtility.AddHatToDatabase(headDresseObj);
        }
    }
}
