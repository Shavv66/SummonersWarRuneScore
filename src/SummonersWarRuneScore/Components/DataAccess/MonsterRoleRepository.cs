﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SummonersWarRuneScore.Components.Domain;
using SummonersWarRuneScore.Components.Domain.Constants;
using SummonersWarRuneScore.Components.Domain.Enumerations;

namespace SummonersWarRuneScore.Components.DataAccess
{
	public class MonsterRoleRepository : IMonsterRoleRepository
	{
		private readonly string mFilePath;
		private List<MonsterRole> mCachedAll;
		private DateTime mLastCachedTimestamp;

		public MonsterRoleRepository() : this(FileConstants.MONSTER_ROLES_PATH) { }

		public MonsterRoleRepository(string filePath)
		{
			mFilePath = filePath;
		}

		public List<MonsterRole> GetAll()
		{
			try
			{
				if (CacheIsValid())
				{
					return mCachedAll;
				}

				mLastCachedTimestamp = File.GetLastWriteTime(mFilePath);
				string json = File.ReadAllText(mFilePath);
				return mCachedAll = JsonConvert.DeserializeObject<List<MonsterRole>>(json);
			}
			catch(Exception)
			{
				return new List<MonsterRole>();
			}
		}

		public List<MonsterRole> GetByRuneSet(RuneSet runeSet)
		{
			return GetAll().Where(monsterRole => monsterRole.RuneSets.Contains(runeSet)).ToList();
		}

		public MonsterRole Add(MonsterRole monsterRole)
		{
			List<MonsterRole> allRoles = GetAll();
			int id = allRoles.Count > 0 ? allRoles.Select(existingMonsterRole => existingMonsterRole.Id).Max() + 1 : 0;

			MonsterRole newRole = new MonsterRole(id, monsterRole.Name, monsterRole.RuneSets, monsterRole.HpPercentWeight, monsterRole.AtkPercentWeight, monsterRole.DefPercentWeight, 
				monsterRole.ExpectedBaseHp, monsterRole.ExpectedBaseAtk, monsterRole.ExpectedBaseDef, monsterRole.SpdWeight, monsterRole.CritRateWeight, monsterRole.CritDmgWeight,
				monsterRole.ResistanceWeight, monsterRole.AccuracyWeight);

			allRoles.Add(newRole);
			WriteRoles(allRoles);

			return newRole;
		}

		public MonsterRole Update(MonsterRole monsterRole)
		{
			List<MonsterRole> allRoles = GetAll();
			MonsterRole roleToUpdate = allRoles.Find(existingMonsterRole => existingMonsterRole.Id == monsterRole.Id);

			if (roleToUpdate == null)
			{
				throw new Exception("Failed to update monster role: The given monster role was not found");
			}
			
			roleToUpdate.HpPercentWeight = monsterRole.HpPercentWeight;
			roleToUpdate.AtkPercentWeight = monsterRole.AtkPercentWeight;
			roleToUpdate.DefPercentWeight = monsterRole.DefPercentWeight;
			roleToUpdate.ExpectedBaseHp = monsterRole.ExpectedBaseHp;
			roleToUpdate.ExpectedBaseAtk = monsterRole.ExpectedBaseAtk;
			roleToUpdate.ExpectedBaseDef = monsterRole.ExpectedBaseDef;
			roleToUpdate.SpdWeight = monsterRole.SpdWeight;
			roleToUpdate.CritRateWeight = monsterRole.CritRateWeight;
			roleToUpdate.CritDmgWeight = monsterRole.CritDmgWeight;
			roleToUpdate.ResistanceWeight = monsterRole.ResistanceWeight;
			roleToUpdate.AccuracyWeight = monsterRole.AccuracyWeight;

			WriteRoles(allRoles);

			return roleToUpdate;
		}

		public void Delete(int id)
		{
			List<MonsterRole> allRoles = GetAll();
			allRoles.Remove(allRoles.Find(monsterRole => monsterRole.Id == id));
			WriteRoles(allRoles);
		}

		private void WriteRoles(List<MonsterRole> roles)
		{
			string json = JsonConvert.SerializeObject(roles);
			File.WriteAllText(mFilePath, json);
		}

		private bool CacheIsValid()
		{
			return mCachedAll != null && (DateTime.Compare(mLastCachedTimestamp, File.GetLastWriteTime(mFilePath)) == 0);
		}
	}
}
