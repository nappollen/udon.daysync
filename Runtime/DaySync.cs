using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Nappollen.Udon.DaySync {
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
		/// </summary>
		private Quaternion CalculateSunPosition(DateTime dateTime) {
			// Day of year (1-366)
			var dayOfYear = dateTime.DayOfYear;

			// Decimal hour
			var hours = dateTime.Hour + dateTime.Minute / 60f + dateTime.Second / 3600f;

			// Solar declination (Earth's axial tilt ~23.45°)
			// Simplified formula: δ = -23.45° * cos(360/365 * (d + 10))
			var declination = -23.45f * Mathf.Cos(2f * Mathf.PI / 365f * (dayOfYear + 10));

			// Equation of time (correction for elliptical orbit)
			var b              = 2f * Mathf.PI / 365f * (dayOfYear - 81);
			var equationOfTime = 9.87f         * Mathf.Sin(2f * b) - 7.53f * Mathf.Cos(b) - 1.5f * Mathf.Sin(b);

			// Local solar time (in hours)
			var solarTime = hours + (4f * longitude + equationOfTime) / 60f;

			// Hour angle (15° per hour, 0° at solar noon)
			var hourAngle = (solarTime - 12f) * 15f;

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

			// Adjust azimuth based on time (morning vs afternoon)
			if (hourAngle > 0) azimuth = 360f - azimuth;

			// Create rotation for Unity Directional Light
			// For Directional Light: positive X = shines downward
			// altitude: -90 (below horizon) to +90 (zenith)
			// Mapping: altitude -90 -> X = -90 (below map), altitude +90 -> X = 90 (zenith)
			// Amplify so sun goes further below map for darker night
			var sunAngle = altitude * 1.5f; // Amplification for darker night
			
			Debug.Log($"[DaySync] Raw altitude: {altitude:F1}°, Sun angle: {sunAngle:F1}°");
			
			return Quaternion.Euler(sunAngle, azimuth, 0f);
		}
	}
}