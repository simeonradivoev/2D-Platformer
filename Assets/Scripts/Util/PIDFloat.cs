using System;
using UnityEngine;

namespace Trive.Core
{
	[Serializable]
	public struct PIDFloat
	{
		public float pFactor, iFactor, dFactor;
		private float integral;
		private float lastError;

		public PIDFloat(Vector3 factors)
		{
			pFactor = factors.x;
			iFactor = factors.y;
			dFactor = factors.z;
			integral = 0;
			lastError = 0;
		}

		public PIDFloat(float pFactor, float iFactor, float dFactor)
		{
			this.pFactor = pFactor;
			this.iFactor = iFactor;
			this.dFactor = dFactor;
			integral = 0;
			lastError = 0;
		}

		public void Reset()
		{
			integral = 0;
			lastError = 0;
		}

		public void SetFactors(float pFactor, float iFactor, float dFactor)
		{
			this.pFactor = pFactor;
			this.iFactor = iFactor;
			this.dFactor = dFactor;
		}

		public void SetFactors(Vector3 factors)
		{
			SetFactors(factors.x, factors.y, factors.y);
		}

		public PIDFloat Update(float setpoint, float actual, float timeFrame, out float val)
		{
			var present = setpoint - actual;
			var newIntegral = integral + present * timeFrame;
			var deriv = (present - lastError) / timeFrame;
			val = present * pFactor + newIntegral * iFactor + deriv * dFactor;
			return new PIDFloat(pFactor, iFactor, dFactor) { lastError = present, integral = newIntegral };
		}

		public float Update(float setpoint, float actual, float timeFrame)
		{
			return Update(setpoint, actual, timeFrame, ref lastError, ref integral);
		}

		public float Update(float setpoint, float actual, float timeFrame, ref float lastError, ref float integral)
		{
			var present = setpoint - actual;
			integral += present * timeFrame;
			var deriv = (present - lastError) / timeFrame;
			var val = present * pFactor + integral * iFactor + deriv * dFactor;
			lastError = present;
			return val;
		}

		public float UpdateAngle(float setpoint, float actual, float timeFrame)
		{
			return UpdateAngle(setpoint, actual, timeFrame, ref integral, ref lastError);
		}

		public float UpdateAngle(float setpoint, float actual, float timeFrame, ref float integral, ref float lastError)
		{
			var present = Mathf.DeltaAngle(actual, setpoint);
			integral += present * timeFrame;
			var deriv = Mathf.DeltaAngle(lastError, present) / timeFrame;
			lastError = present;
			return present * pFactor + integral * iFactor + deriv * dFactor;
		}
	}
}