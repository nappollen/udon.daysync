using UnityEditor;
using UnityEngine;

namespace Nappollen.Udon.DaySync.Editor {
	[CustomEditor(typeof(DaySync))]
	public class DaySyncEditor : UnityEditor.Editor {
		private DaySync Module
			=> target as DaySync;

		public override void OnInspectorGUI() {
			GUILayout.Label("Day Sync", EditorStyles.boldLabel);
			GUILayout.Label("A simple day-night cycle controller based on local or network time.", EditorStyles.wordWrappedLabel);

			GUILayout.Space(10f);
			var time = Module.GetTime();
			EditorGUILayout.HelpBox($"Current Time: {time:HH:mm:ss} (UTC{Module.timeZone:+0;-0;0})", MessageType.Info);

			GUILayout.Space(10f);

			Module.sun = EditorGUILayout.ObjectField("Sun", Module.sun, typeof(Transform), true) as Transform;
			Module.lerp = EditorGUILayout.Toggle(
				new GUIContent("Lerp Movement", "Smoothly interpolate the sun's movement."),
				Module.lerp
			);

			if (Module.lerp) {
				EditorGUI.indentLevel++;
				Module.lerpSpeed = EditorGUILayout.Slider(
					new GUIContent("Lerp Speed", "Speed of the sun movement interpolation."),
					Module.lerpSpeed,
					0.1f,
					10f
				);
				EditorGUI.indentLevel--;
			}

			GUILayout.Space(10f);
			GUILayout.Label("Geographic Position", EditorStyles.boldLabel);
			
			Module.latitude = EditorGUILayout.Slider(
				new GUIContent("Latitude", "Latitude in degrees. Positive = Northern hemisphere, Negative = Southern hemisphere."),
				Module.latitude,
				-90f,
				90f
			);
			
			Module.longitude = EditorGUILayout.Slider(
				new GUIContent("Longitude", "Longitude in degrees. Positive = East, Negative = West."),
				Module.longitude,
				-180f,
				180f
			);
			
			EditorGUILayout.HelpBox("Geographic position affects the realistic sun trajectory (maximum height, sunrise/sunset).", MessageType.None);

			GUILayout.Space(10f);
			GUILayout.Label("Time Configuration", EditorStyles.boldLabel);
			
			Module.based = (TimeBased)EditorGUILayout.EnumPopup(
				new GUIContent("Time Based", "Choose whether to base the time on local system time or network time."),
				Module.based
			);

			if (Module.based == TimeBased.Reference) {
				EditorGUILayout.HelpBox("Using network time ensures consistency across different users' experiences.", MessageType.Info);
				Module.timeZone = EditorGUILayout.IntSlider(
					new GUIContent("Time Zone", "Set the time zone offset from UTC when using network time."),
					Module.timeZone,
					-12,
					12
				);
			}

			if (Module.based == TimeBased.Local) {
				EditorGUILayout.HelpBox("Using local time may lead to inconsistencies across different users' experiences.", MessageType.Info);
			}

			if (GUI.changed)
				EditorUtility.SetDirty(Module);
			
			Repaint();
		}
	}
}