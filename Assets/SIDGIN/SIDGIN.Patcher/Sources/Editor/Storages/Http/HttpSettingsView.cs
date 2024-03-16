using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
namespace SIDGIN.Patcher.Editors.Storages
{
    using SIDGIN.Common.Editors;
    using static SIDGIN.Patcher.Editors.Storages.HttpSettingsData;

    public class HttpSettingsView : StorageSettingsView
    {
        bool ftpConnectionError = false;
        bool fileDownloadError = false;
        public override void OnEnable()
        {
            ftpConnectionError = false;
            fileDownloadError = false;
        }
        public override void OnDraw()
        {
            var httpSettingsData = EditorSettingsLoader.Get<HttpSettingsData>();
            httpSettingsData.ftpProtocolType = (FTPType)EditorGUILayout.EnumPopup("FTP Protocol:", httpSettingsData.ftpProtocolType);
            httpSettingsData.ftpDomain = EditorGUILayout.TextField("FTP Domain:", httpSettingsData.ftpDomain);
            httpSettingsData.ftpPort = EditorGUILayout.TextField("FTP Port:", httpSettingsData.ftpPort);
            httpSettingsData.ftpUser = EditorGUILayout.TextField("FTP User:", httpSettingsData.ftpUser);
            httpSettingsData.ftpPassword = EditorGUILayout.PasswordField("FTP Password:", httpSettingsData.ftpPassword);
            httpSettingsData.enableSSL = EditorGUILayout.Toggle("Enable SSL:", httpSettingsData.enableSSL);
            httpSettingsData.ftpHomeDirectory = EditorGUILayout.TextField("FTP Home Directory:", httpSettingsData.ftpHomeDirectory);
            if (httpSettingsData.ftpProtocolType == FTPType.SFTP)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Open SSH key:", GUILayout.Width(150));
                if (string.IsNullOrEmpty(httpSettingsData.privateKeyFilePath))
                {
                    EditorGUILayout.LabelField("Please select key file");
                }
                else
                {
                    EditorGUILayout.LabelField(httpSettingsData.privateKeyFilePath);
                }

                if (GUILayout.Button("Change"))
                {
                    httpSettingsData.privateKeyFilePath = EditorUtility.OpenFilePanel("Select private key", "", "key");
                }
                EditorGUILayout.EndHorizontal();
                httpSettingsData.privateKeyPassphrase = EditorGUILayout.TextField("Key file passphrase:", httpSettingsData.privateKeyPassphrase);
            }
            httpSettingsData.rootLink = EditorGUILayout.TextField("Download base link:", httpSettingsData.rootLink);
            if (ftpConnectionError)
            {
                EditorGUILayout.HelpBox("FTP server settings are incorrect. Check console.", MessageType.Error);
            }
            if (fileDownloadError)
            {
                EditorGUILayout.HelpBox("Unable to download test file from server.", MessageType.Error);
            }
            if (GUILayout.Button("Test"))
            {
                Test();
            }
        }
        public string ftpDomain;
        public string ftpUser;
        public string ftpPassword;
        public string ftpPort;
        public string privateKeyFilePath;
        public string privateKeyPassword;
        public string rootLink;
       

        public async void Test()
        {
            var httpSettingsData = EditorSettingsLoader.Get<HttpSettingsData>();
            ftpConnectionError = false;
            fileDownloadError = false;
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
                    await storageControl.UploadData(expectedData, "Test_Connection.json");
                    startConnection = false;
                    startDownload = true;
                    var actualData = await storageControl.DownloadData("Test_Connection.json");
                    startDownload = false;
                    if (actualData == expectedData)
                    {
                        EditorDispatcher.Dispatch(() => EditorUtility.DisplayDialog("The test was successful", "Congratulations! Now you can work with the system ;)", "OK"));
                        ftpConnectionError = false;
                        fileDownloadError = false;
                    }
                    else
                    {
                        fileDownloadError = true;
                        EditorDispatcher.Dispatch(() => EditorUtility.DisplayDialog("Error while test", "Error during test execution. Check \"Download base link\". The file with name \"Test_Connection\" must be available through a direct link: \"Download base link\" + /SGPatcher_test/Test_Connection", "OK"));
                    }
                }
                catch (System.Exception ex)
                {
                    ftpConnectionError = startConnection;
                    fileDownloadError = startDownload;
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