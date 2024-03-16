using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InterceptMissileScanner))]
public class ScannerEditor : Editor {

	private void OnSceneGUI()
	{
		InterceptMissileScanner scanner = (InterceptMissileScanner)target;
		//Handles.color = Color.cyan;
		//Handles.DrawWireArc(scanner.transform.position, Vector3.up, Vector3.forward, 360, scanner.ScanRange);
		
		//Vector3 ViewAngleA = scanner.GetViewAngle(scanner.ViewAngle / 2);
		//Vector3 ViewAngleB = scanner.GetViewAngle(-scanner.ViewAngle / 2);
		//Handles.DrawLine(scanner.transform.position, scanner.transform.position + ViewAngleA * scanner.ScanRange);
		//Handles.DrawLine(scanner.transform.position, scanner.transform.position + ViewAngleB * scanner.ScanRange);
		
		if(scanner.ShowGizmos)
		{	
			Handles.color = new Color(1,1,1,0.3f);
			Handles.DrawSolidArc(scanner.transform.position, Vector3.up, scanner.transform.forward, scanner.ViewAngle / 2, scanner.ScanRadius);
			Handles.DrawSolidArc(scanner.transform.position, Vector3.up, scanner.transform.forward, -scanner.ViewAngle / 2, scanner.ScanRadius);


			Handles.color = new Color(96,142,0,0.3f);
			Handles.DrawSolidArc(scanner.transform.position, scanner.transform.forward, scanner.GetViewAngle(scanner.ViewAngle / 2), scanner.ViewAngle, scanner.ScanRadius);
			Handles.DrawSolidArc(scanner.transform.position, -scanner.transform.forward, scanner.GetViewAngle(-scanner.ViewAngle / 2), scanner.ViewAngle, scanner.ScanRadius);
		}
	}	
}