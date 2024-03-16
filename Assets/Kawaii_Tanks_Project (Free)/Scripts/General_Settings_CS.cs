using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ChobiAssets.KTP
{

    public class General_Settings_CS
    {
        // Frame Rate settings.
        public static bool fixFrameRate = true;
        public static int targetFrameRate = 60;
        public static int targetFrameRateMobile = 60;

        // Physics settings.
        public static float fixedTimestep = 0.02f;
        public static float fixedTimestepMobile = 0.02f;

        // Camera Rotation settings.
        public static float cameraRotationSensibility = 2.5f;
        public static float cameraRotationSensibilityMobile = 720.0f;

        // Camera Zoom settings.
        public static float cameraZoomSensibility = 2.5f;
        public static float cameraZoomSensibilityMobile = 30.0f;

        // Gun Camera Zoom settings.
        public static float gunCameraZoomSensibility = 2.0f;
        public static float gunCameraZoomSensibilityMobile = 10.0f;

        // Camera Avoid Obstacle settings.
        public static float cameraAvoidMoveSpeed = 30.0f;
        public static float cameraAvoidMinDist = 2.0f;
        public static float cameraAvoidMaxDist = 30.0f;
        public static float cameraAvoidLag = 0.1f;

        // Aiming settings.
        public static float aimingSensibility = 0.2f;
        public static float aimingSensibilityMobile = 30.0f;

        // Scroll Track Settings.
        public static string mainTextureName = "_MainTex";
    }

}
