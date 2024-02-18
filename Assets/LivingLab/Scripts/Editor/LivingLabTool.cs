
using MasterServerToolkit.MasterServer;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class LivingLabTool
{

    [MenuItem("LivingLab" + "/Builds/Room(Headless)")]
    private static void BuildRoomForWindowsHeadless()
    {
        BuildRoomForWindows(true);
    }

    private static void BuildRoomForWindows(bool isHeadless)
    {
        string buildFolder = Path.Combine("Builds", "Room");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] {
                    "Assets/LivingLab/Scenes/Rooms/Zone1.unity",
                    "Assets/LivingLab/Scenes/Rooms/Zone2.unity",
                    "Assets/LivingLab/Scenes/Rooms/Zone3.unity",
                },
            locationPathName = Path.Combine(buildFolder, "Room.exe"),
            target = BuildTarget.StandaloneWindows64,
#if UNITY_2021_1_OR_NEWER
            options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development,
            subtarget = isHeadless ? (int)StandaloneBuildSubtarget.Server : (int)StandaloneBuildSubtarget.Player
#else
                options = isHeadless ? BuildOptions.ShowBuiltPlayer | BuildOptions.EnableHeadlessMode : BuildOptions.ShowBuiltPlayer
#endif
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            string appConfig = Mst.Args.AppConfigFile(buildFolder);

            MstProperties properties = new MstProperties();
            properties.Add(Mst.Args.Names.StartClientConnection, true);
            properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
            properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);
            properties.Add(Mst.Args.Names.RoomIp, Mst.Args.RoomIp);
            properties.Add(Mst.Args.Names.RoomPort, Mst.Args.RoomPort);

            File.WriteAllText(appConfig, properties.ToReadableString("\n", "="));

            Debug.Log("Room build succeeded: " + (summary.totalSize / 1024) + " kb");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Room build failed");
        }
    }

    [MenuItem("LivingLab" + "/Builds/Client")]
    private static void BuildClientForWindows()
    {
        string buildFolder = Path.Combine("Builds", "Client");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] {
                    "Assets/LivingLab/Scenes/Client.unity",
                    "Assets/LivingLab/Scenes/Rooms/Zone1.unity",
                    "Assets/LivingLab/Scenes/Rooms/Zone2.unity",
                    "Assets/LivingLab/Scenes/Rooms/Zone3.unity",
                },
            locationPathName = Path.Combine(buildFolder, "Client.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            string appConfig = Mst.Args.AppConfigFile(buildFolder);

            MstProperties properties = new MstProperties();
            properties.Add(Mst.Args.Names.StartClientConnection, true);
            properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
            properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);

            File.WriteAllText(appConfig, properties.ToReadableString("\n", "="));

            Debug.Log("Client build succeeded: " + (summary.totalSize / 1024) + " kb");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Client build failed");
        }
    }

    [MenuItem("LivingLab" + "/Builds/Master and Spawner")]
    private static void BuildMasterAndSpawnerForWindows()
    {
        string buildFolder = Path.Combine("Builds", "MasterAndSpawner");
        string roomExePath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Room", "Room.exe");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/LivingLab/Scenes/Master.unity" },
            locationPathName = Path.Combine(buildFolder, "MasterAndSpawner.exe"),
            target = BuildTarget.StandaloneWindows64,
#if UNITY_2021_1_OR_NEWER
            options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development,
            subtarget = (int)StandaloneBuildSubtarget.Server
#else
                options = BuildOptions.EnableHeadlessMode | BuildOptions.ShowBuiltPlayer | BuildOptions.Development
#endif
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            MstProperties properties = new MstProperties();
            properties.Add(Mst.Args.Names.StartMaster, true);
            properties.Add(Mst.Args.Names.StartSpawner, true);
            properties.Add(Mst.Args.Names.StartClientConnection, true);
            properties.Add(Mst.Args.Names.MasterIp, Mst.Args.MasterIp);
            properties.Add(Mst.Args.Names.MasterPort, Mst.Args.MasterPort);
            properties.Add(Mst.Args.Names.RoomExecutablePath, roomExePath);
            properties.Add(Mst.Args.Names.RoomIp, Mst.Args.RoomIp);
            properties.Add(Mst.Args.Names.RoomRegion, Mst.Args.RoomRegion);

            File.WriteAllText(Path.Combine(buildFolder, "application.cfg"), properties.ToReadableString("\n", "="));

            Debug.Log("Master Server build succeeded: " + (summary.totalSize / 1024) + " kb");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Master Server build failed");
        }
    }
}
