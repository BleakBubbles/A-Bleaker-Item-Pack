using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Brave.BulletScript;
using Dungeonator;
using Gungeon;
using ItemAPI;
using tk2dRuntime.TileMap;
using UnityEngine;

namespace BleakMod
{
    public static class Stuff
    {
        public static void Init()
        {
		}
		public static bool PlayerHasActiveSynergy(this PlayerController player, string synergyNameToCheck)
		{
			foreach (int num in player.ActiveExtraSynergies)
			{
				AdvancedSynergyEntry advancedSynergyEntry = GameManager.Instance.SynergyManager.synergies[num];
				bool flag = advancedSynergyEntry.NameKey == synergyNameToCheck;
				if (flag)
				{
					return true;
				}
			}
			return false;
		}
		public static T ReflectGetField<T>(Type classType, string fieldName, object o = null)
		{
			FieldInfo field = classType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | ((o != null) ? BindingFlags.Instance : BindingFlags.Static));
			return (T)field.GetValue(o);
		}
	}
}