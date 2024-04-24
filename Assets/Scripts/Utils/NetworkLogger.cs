using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkLogger : MonoBehaviour
{
#if !UNITY_EDITOR
	static Queue<string> queue = new Queue<string>(6);
	void OnEnable() {
		Application.RegisterLogCallback(HandleLog);
	}

	void OnDisable() {
		Application.RegisterLogCallback(null);
	}

	void OnGUI() {
		GUILayout.BeginArea(new Rect(0, Screen.height - 140, Screen.width, 140));
		foreach (string s in queue) {
			GUILayout.Label(s);
		}
		GUILayout.EndArea();
	}

	void HandleLog(string message, string stackTrace, LogType type) {
		queue.Enqueue(Time.time + " - " + message);
		if (queue.Count > 5) {
			queue.Dequeue();
		}
	}
#endif
}
