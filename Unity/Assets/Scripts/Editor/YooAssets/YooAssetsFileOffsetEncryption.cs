using System;
using System.IO;
using YooAsset;

namespace Chaos
{
    public sealed class YooAssetsFileOffsetEncryption : IEncryptionServices
    {
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            const int offset = 32;
            var src = File.ReadAllBytes(fileInfo.FileLoadPath);
            var dst = new byte[src.Length + offset];
            Buffer.BlockCopy(src, 0, dst, offset, src.Length);

            var result = new EncryptResult
            {
                Encrypted = true,
                EncryptedData = dst
            };
            return result;
        }
    }
}