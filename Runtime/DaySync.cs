using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Nappollen.Udon.DaySync {
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class DaySync : UdonSharpBehaviour {
		public TimeBased based;
		public int       timeZone;
		public bool      lerp = true;
		public Transform sun;

		[Header("Geographic Position")]
		[Range(-90f, 90f)]
		[Tooltip("Latitude in degrees (-90 to 90)")]
		public float latitude = 48.8566f; // Paris by default

		[Range(-180f, 180f)]
		[Tooltip("Longitude in degrees (-180 to 180)")]
		public float longitude = 2.3522f; // Paris by default

		[Header("Interpolation Speed")]
		[Range(0.1f, 10f)]
		public float lerpSpeed = 2f;

		private const float DegToRad = Mathf.PI / 180f;
		private const float RadToDeg = 180f     / Mathf.PI;

		public void Awake() {
			if (sun) return;
			Debug.LogError("[DaySync] Sun Transform is not assigned.");
			enabled = false;
		}

		public DateTime GetTime() {
			if (based == TimeBased.Local)
				return DateTime.Now;

			return Networking.GetNetworkDateTime()
				.AddHours(timeZone);
		}

		private void Update() {
			if (!sun) return;

			var time        = GetTime();
			var sunRotation = CalculateSunPosition(time);

			if (lerp) sun.localRotation = Quaternion.Slerp(sun.localRotation, sunRotation, Time.deltaTime * lerpSpeed);
			else sun.localRotation      = sunRotation;
		}

	/// <summary>
	/// Calculates realistic sun position based on astronomical formulas
	/// Reference: SunEarthTools algorithm
	/// </summary>
	private Quaternion CalculateSunPosition(DateTime dateTime) {
		// Calculate Julian Day
		var year  = dateTime.Year;
		var month = dateTime.Month;
		var day   = dateTime.Day;
		
		if (month <= 2) {
			year  -= 1;
			month += 12;
		}
		
		var a          = year / 100;
		var b          = 2 - a + a / 4;
		var julianDay  = Mathf.Floor(365.25f * (year + 4716)) + Mathf.Floor(30.6001f * (month + 1)) + day + b - 1524.5f;
		
		// Add time of day
		var hours = dateTime.Hour + dateTime.Minute / 60f + dateTime.Second / 3600f;
		julianDay += hours / 24f;
		
		// Calculate Julian Century
		var julianCentury = (julianDay - 2451545f) / 36525f;
		
		// Solar declination (more accurate formula)
		var geometricMeanLongSun = (280.46646f + julianCentury * (36000.76983f + julianCentury * 0.0003032f)) % 360f;
		var geometricMeanAnomSun = 357.52911f + julianCentury * (35999.05029f - 0.0001537f * julianCentury);
		var eccentEarthOrbit = 0.016708634f - julianCentury * (0.000042037f + 0.0000001267f * julianCentury);
		
		var sunEqOfCtr = Mathf.Sin(geometricMeanAnomSun * DegToRad) * (1.914602f - julianCentury * (0.004817f + 0.000014f * julianCentury))
		                 + Mathf.Sin(2f * geometricMeanAnomSun * DegToRad) * (0.019993f - 0.000101f * julianCentury)
		                 + Mathf.Sin(3f * geometricMeanAnomSun * DegToRad) * 0.000289f;
		
		var sunTrueLong     = geometricMeanLongSun + sunEqOfCtr;
		var sunAppLong      = sunTrueLong - 0.00569f - 0.00478f * Mathf.Sin((125.04f - 1934.136f * julianCentury) * DegToRad);
		var meanObliqEclip  = 23f + (26f + ((21.448f - julianCentury * (46.815f + julianCentury * (0.00059f - julianCentury * 0.001813f)))) / 60f) / 60f;
		var obliqCorr       = meanObliqEclip + 0.00256f * Mathf.Cos((125.04f - 1934.136f * julianCentury) * DegToRad);
		
		var declination = Mathf.Asin(Mathf.Sin(obliqCorr * DegToRad) * Mathf.Sin(sunAppLong * DegToRad)) * RadToDeg;
		
		// Equation of time (accurate formula)
		var varY = Mathf.Tan(obliqCorr / 2f * DegToRad);
		varY *= varY;
		
		var equationOfTime = 4f * (varY * Mathf.Sin(2f * geometricMeanLongSun * DegToRad)
		                           - 2f * eccentEarthOrbit * Mathf.Sin(geometricMeanAnomSun * DegToRad)
		                           + 4f * eccentEarthOrbit * varY * Mathf.Sin(geometricMeanAnomSun * DegToRad) * Mathf.Cos(2f * geometricMeanLongSun * DegToRad)
		                           - 0.5f * varY * varY * Mathf.Sin(4f * geometricMeanLongSun * DegToRad)
		                           - 1.25f * eccentEarthOrbit * eccentEarthOrbit * Mathf.Sin(2f * geometricMeanAnomSun * DegToRad)) * RadToDeg;
		
		// True solar time
		var trueSolarTime = (hours * 60f + equationOfTime + 4f * longitude) % 1440f;
		
		// Hour angle (in degrees)
		var hourAngle = trueSolarTime / 4f - 180f;
		if (hourAngle < -180f) hourAngle += 360f;
		
		// Convert to radians
		var latRad = latitude    * DegToRad;
		var decRad = declination * DegToRad;
		var haRad  = hourAngle   * DegToRad;
		
		// Calculate solar altitude (elevation)
		var sinAltitude = Mathf.Sin(latRad) * Mathf.Sin(decRad) + Mathf.Cos(latRad) * Mathf.Cos(decRad) * Mathf.Cos(haRad);
		var altitude    = Mathf.Asin(Mathf.Clamp(sinAltitude, -1f, 1f)) * RadToDeg;
		
		// Calculate solar azimuth
		var cosAzimuth = (Mathf.Sin(decRad) - Mathf.Sin(latRad) * sinAltitude) / (Mathf.Cos(latRad) * Mathf.Cos(altitude * DegToRad));
		cosAzimuth = Mathf.Clamp(cosAzimuth, -1f, 1f);
		var azimuth = Mathf.Acos(cosAzimuth) * RadToDeg;
		
		// Adjust azimuth based on hour angle (correct orientation)
		if (hourAngle > 0f) azimuth = 360f - azimuth;
		
		// Create rotation for Unity Directional Light
		// X = elevation (altitude), Y = azimuth direction
		return Quaternion.Euler(altitude, azimuth, 0f);
	}
	}
}