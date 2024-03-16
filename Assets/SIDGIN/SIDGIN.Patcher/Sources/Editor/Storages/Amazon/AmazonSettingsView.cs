using UnityEngine;
using UnityEditor;
namespace SIDGIN.Patcher.Editors.Storages
{
    using SIDGIN.Common.Editors;
    using System.Linq;
    using System.Threading.Tasks;

    public class AmazonSettingsView : StorageSettingsView
    {
        public override void OnDraw()
        {
            var amazonSettingsData = EditorSettingsLoader.Get<AmazonSettingsData>();

            if (GUILayout.Button("Import Credentials File"))
            {
                var importString = EditorUtility.OpenFilePanel("Choose Credentials File", "", "csv");
                var importData = System.IO.File.ReadLines(importString).ToList();
                if (importData.Count >= 2)
                {
                    var data = importData[1].Split(',');
                    amazonSettingsData.accessKeyId = data[2];
                    amazonSettingsData.secretAccessKey = data[3];
                }
            }
            amazonSettingsData.bucketName = EditorGUILayout.TextField("Bucket Name:", amazonSettingsData.bucketName);
            amazonSettingsData.accessKeyId = EditorGUILayout.PasswordField("Access Key ID:", amazonSettingsData.accessKeyId);
            amazonSettingsData.secretAccessKey = EditorGUILayout.PasswordField("Secret Access Key ID:", amazonSettingsData.secretAccessKey);
            amazonSettingsData.endpoint = (AmazonEndpoint)EditorGUILayout.EnumPopup("Region Endpoint:", amazonSettingsData.endpoint);

            amazonSettingsData.rootFolder = EditorGUILayout.TextField("Root Folder", amazonSettingsData.rootFolder);
            if (GUILayout.Button("Test"))
            {
                Test();
            }
        }

        public override void OnEnable()
        {
            
        }
        bool connectionError;
        bool downloadError;
        async void Test()
        {
            var amazonSettingsData = EditorSettingsLoader.Get<AmazonSettingsData>();
            connectionError = false;
            downloadError = false;
            var setting = EditorSettingsLoader.Get<SettingsData>();
            var m_def = setting.selectedDefinition;
            setting.selectedDefinition = "SGPatcher_test";
            MainWindow.OnProgressChanged(0, "Test ftp...");
            var storageControl = new StorageControlEditor();
            await Task.Run(async () =>
            {
                var expectedData = "Test connection data";
                bool startConnection = true;
                bool startDownload = false;
                try
                {
                    await storageControl.UploadData(expectedData, "Test_Connection");
                    startConnection = false;
                    startDownload = true;
                    var link = "https://" + amazonSettingsData.bucketName + ".s3." + AmazonApiEditor.ConvertEndpointToString(amazonSettingsData.endpoint) + ".amazonaws.com/" + amazonSettingsData.rootFolder + "/SGPatcher_test/Test_Connection";
                    var actualData = await storageControl.DownloadData(link);
                    startDownload = false;
                    if (actualData == expectedData)
                    {
                        EditorDispatcher.Dispatch(() => EditorUtility.DisplayDialog("The test was successful", "Congratulations! Now you can work with the system ;)", "OK"));
                        connectionError = false;
                        downloadError = false;
                    }
                    else
                    {
                        downloadError = true;
                        EditorDispatcher.Dispatch(() => EditorUtility.DisplayDialog("Error while test", "Error during test execution. Check \"Download base link\". The file with name \"Test_Connection\" must be available through a direct link: \"Download base link\" + /SGPatcher_test/Test_Connection", "OK"));
                    }
                }
                catch (System.Exception ex)
                {
                    connectionError = startConnection;
                    downloadError = startDownload;
                    Debug.LogError(ex);
                }
                finally
                {
                    setting.selectedDefinition = m_def;
                }
            });
            MainWindow.ResetProgress();
        }
    }
}