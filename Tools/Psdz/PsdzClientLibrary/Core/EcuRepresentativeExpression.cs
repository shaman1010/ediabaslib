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
	public class EcuRepresentativeExpression : SingleAssignmentExpression
	{
		public EcuRepresentativeExpression()
		{
		}

		public EcuRepresentativeExpression(long ecuRepresentativeId)
		{
			this.value = ecuRepresentativeId;
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
			if (vec == null)
			{
				//Log.Warning("EcuRepresentativeExpression.Evaluate()", "vec was null", Array.Empty<object>());
				return false;
			}
            PdszDatabase.EcuReps ecuRepsById = ClientContext.GetDatabase(vec)?.GetEcuRepsById(this.value.ToString(CultureInfo.InvariantCulture));
			if (ecuRepsById == null)
			{
				return false;
			}
			if (vec.VCI != null && (vec.VehicleIdentLevel == IdentificationLevel.BasicFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINBasedFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINOnly))
			{
				return true;
			}
			if (vec.VehicleIdentLevel == IdentificationLevel.VINBasedOnlineUpdated && vec.ECU != null && vec.ECU.Count == 0)
			{
				return true;
			}
			bool flag = vec.getECUbyTITLE_ECUTREE(ecuRepsById.EcuShortcut) != null;
			return flag;
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			if (ecus.EcuRepresentatives.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.VALID;
			}
			if (ecus.UnknownEcuRepresentatives.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.MISSING_VARIANT;
			}
			return EEvaluationResult.INVALID;
		}

		public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
		{
			List<long> list = new List<long>();
			if (ecus.UnknownEcuRepresentatives.ToList<long>().BinarySearch(this.value) >= 0)
			{
				list.Add(this.value);
			}
			return list;
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(18);
			base.Serialize(ms);
		}

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(formulaConfig.CheckLongFunc);
            stringBuilder.Append("(\"EcuRepresentative\", ");
            stringBuilder.Append(this.value.ToString(CultureInfo.InvariantCulture));
            stringBuilder.Append(")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

		public override string ToString()
		{
			return "EcuRepresentative=" + this.value.ToString(CultureInfo.InvariantCulture);
		}
	}
}
