﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public class EquipmentExpression : SingleAssignmentExpression
	{
		public EquipmentExpression()
		{
		}

		public EquipmentExpression(long equipmentId)
		{
			this.value = equipmentId;
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
			IDatabaseProvider instance = DatabaseProviderFactory.Instance;
			if (vec == null)
			{
				//Log.Error("EquipmentExpression.Evaluate()", "vec was null", Array.Empty<object>());
				return false;
			}
			if (vec.VehicleIdentLevel == IdentificationLevel.None)
			{
				return false;
			}
			if (instance == null)
			{
				//Log.Error("EquipmentExpression.Evaluate()", "database connection invalid", Array.Empty<object>());
				return false;
			}
			XEP_EQUIPMENT equipmentById = instance.GetEquipmentById(this.value);
			if (equipmentById == null)
			{
				return false;
			}
			object obj = EquipmentExpression.evaluationLockObject;
			bool result;
			lock (obj)
			{
				bool? flag2 = vec.hasFFM(equipmentById.NAME);
				if (flag2 != null)
				{
					result = flag2.Value;
				}
				else
				{
					bool flag3 = instance.EvaluateXepRulesById(this.value, vec, ffmResolver, null);
					if (ffmResolver != null && flag3)
					{
						ObservableCollectionEx<IXepInfoObject> infoObjectsByDiagObjectControlId = DatabaseProviderFactory.Instance.GetInfoObjectsByDiagObjectControlId(this.value, vec, ffmResolver, true, null);
						if (infoObjectsByDiagObjectControlId != null && infoObjectsByDiagObjectControlId.Count != 0)
						{
							bool? flag4 = ffmResolver.Resolve(this.value, infoObjectsByDiagObjectControlId.First<IXepInfoObject>());
							vec.AddOrUpdateFFM(new FFMResult(equipmentById.ID, equipmentById.NAME, "FFMResolver", flag4, false));
							if (flag4 != null)
							{
								result = flag4.Value;
							}
							else
							{
								result = true;
							}
						}
						else
						{
							result = false;
						}
					}
					else if (flag3)
					{
						result = true;
					}
					else
					{
						result = false;
					}
				}
			}
			return result;
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			if (ecus.Equipments.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.VALID;
			}
			if (ecus.UnknownEquipments.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.MISSING_VARIANT;
			}
			return EEvaluationResult.INVALID;
		}

		public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
		{
			List<long> list = new List<long>();
			if (ecus.UnknownEquipments.ToList<long>().BinarySearch(this.value) >= 0)
			{
				list.Add(this.value);
			}
			return list;
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(13);
			base.Serialize(ms);
		}

		public override string ToString()
		{
			return "Equipment=" + this.value.ToString(CultureInfo.InvariantCulture);
		}

		private static object evaluationLockObject = new object();
	}
}
