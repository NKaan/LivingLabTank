using SIDGIN.Zip;
using System;
using System.IO;

namespace SIDGIN.Patcher.Delta
{
    using Common;
    using Internal.Delta;
    using SIDGIN.Patcher.Client;
    using System.Threading.Tasks;

    internal class PatchApplier
    {
        public event Action<float, string> onProgress;
        float progress;
        void ApplyPatchForFile(FileEntry entry, ZipFile zf, string outputDirectory)
        {
            string entryFilename = Path.Combine(outputDirectory, entry.Filename);
            if (entry.Type == EntryType.Removed)
            {
                if (File.Exists(entryFilename))
                    File.Delete(entryFilename);
            }

            if (entry.Type == EntryType.Added)
            {
                ArchiveHelper.ExtractFile(zf, entryFilename, entry.Filename);
            }

            if (entry.Type == EntryType.Modified)
            {
                string hash_old = HashHelper.GetHashFromFile(entryFilename);

                if (entry.HashOld != hash_old)
                    throw new FileHashesDontMatch(Localize.Get("Error_during_update"), 
                        new Exception(Localize.Format("File_hashes_dont_match",hash_old,entry.HashOld)));

                string patched_file = Path.Combine(outputDirectory, Guid.NewGuid().ToString() + ".tmpmod");

                if (File.Exists(patched_file))
                    File.Delete(patched_file);

                using (FileStream sourceStream = File.Open(entryFilename, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (FileStream outStream = File.Open(patched_file, FileMode.Create))
                    {
                        DeltaStreamer streamer = new DeltaStreamer();
                        streamer.Receive(ArchiveHelper.ExtractStream(zf, entry.Filename), sourceStream, outStream);
                        outStream.Close();
                    }
                    sourceStream.Close();
                }
                File.Copy(patched_file, entryFilename, true);
                File.Delete(patched_file);
            }

            if (entry.Type == EntryType.Added || entry.Type == EntryType.Modified)
            {
                string hash_new = HashHelper.GetHashFromFile(entryFilename);

                if (entry.HashNew != hash_new)
                    throw new FileHashesDontMatch(Localize.Get("File_doesnt_match_hash_server"));
            }

        }
        public Task ApplyPatch(string filepath, string outputDirectory)
        {
            return Task.Run(() =>
            {
                using (FileStream fs = File.OpenRead(filepath))
                {
                    using (ZipFile zf = new ZipFile(fs))
                    {
                        onProgress?.Invoke(0, Localize.Get("Reading"));

                        var patchEntryString = ArchiveHelper.ExtractData(zf, "configuration.json");
                        var patchEntry = patchEntryString.ToPatchEntry();
                        var entryes = patchEntry.FileEntries;
                        for (int i = 0; i < entryes.Count; i++)
                        {
                            progress = (float)i / (float)entryes.Count;
                            onProgress?.Invoke(progress, Localize.Format("Process_file", entryes[i].Filename));
                            ApplyPatchForFile(entryes[i], zf, outputDirectory);
                        }
                        zf.Close();
                    }
                    fs.Close();
                }
                onProgress?.Invoke(1, Localize.Get("Patch_done"));
            });
        }
        public Task ApplyFull(string filepath, string outputDirectory)
        {
            return Task.Run(() =>
            {
                ArchiveHelper.ExtractZip(filepath, outputDirectory, onProgress);
            });
        }
        void OnProgressChange(float progress, string status)
        {
            if (onProgress != null)
            {
                SGDispatcher.Register(() => onProgress.Invoke(progress, status));
            }
        }
    }
}
