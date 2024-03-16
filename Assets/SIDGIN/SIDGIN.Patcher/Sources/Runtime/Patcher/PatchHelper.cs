using SIDGIN.Patcher.Internal.Delta;
using SIDGIN.Patcher.Internal.Hash;
using SIDGIN.Patcher.Internal.Libraries;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SIDGIN.Patcher.Delta
{
    public class PatchHelper
    {
        public const int BLOCK_SIZE = 1024;
        public int zipLevel;
        public static void MakeDifferenceForFile(string filename1, string filename2, string outputFile)
        {
            HashBlock[] hashBlocksFromReceiver;
            using (FileStream sourceStream = File.Open(filename1, FileMode.Open))
            {
                hashBlocksFromReceiver = new HashBlockGenerator(new RollingChecksum(),
                                                                new HashAlgorithmWrapper<MD5>(MD5.Create()),
                                                                BLOCK_SIZE).ProcessStream(sourceStream).ToArray();
            }

            using (FileStream fileStream = File.Open(filename2, FileMode.Open))
            {
                using (FileStream outputStream = File.OpenWrite(outputFile))
                {
                    DeltaGenerator deltaGen = new DeltaGenerator(new RollingChecksum(), new HashAlgorithmWrapper<MD5>(MD5.Create()));
                    deltaGen.Initialize(BLOCK_SIZE, hashBlocksFromReceiver);
                    IEnumerable<IDelta> deltas = deltaGen.GetDeltas(fileStream);
                    deltaGen.Statistics.Dump();
                    fileStream.Seek(0, SeekOrigin.Begin);
                    DeltaStreamer streamer = new DeltaStreamer();
                    streamer.Send(deltas, fileStream, outputStream);
                }
            }
        }
    }
}
