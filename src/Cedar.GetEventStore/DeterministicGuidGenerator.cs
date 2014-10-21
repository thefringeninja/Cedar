namespace Cedar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    internal class DeterministicGuidGenerator
    {
        private Guid _nameSpace;
        private readonly byte[] _namespaceBytes;

        public DeterministicGuidGenerator(Guid guidNameSpace)
        {
            _nameSpace = guidNameSpace;
            _namespaceBytes = guidNameSpace.ToByteArray();
            SwapByteOrder(_namespaceBytes);
        }

        public Guid Create(string input)
        {
            return Create(Encoding.UTF8.GetBytes(input));
        }

        public Guid Create(IEnumerable<byte> input)
        {
            byte[] hash;
            byte[] inputBuffer = input.ToArray();
            using (var algorithm = SHA1.Create())
            {
                algorithm.TransformBlock(_namespaceBytes, 0, _namespaceBytes.Length, null, 0);
                algorithm.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
                hash = algorithm.Hash;
            }

            var newGuid = new byte[16];
            Array.Copy(hash, 0, newGuid, 0, 16);

            newGuid[6] = (byte)((newGuid[6] & 0x0F) | (5 << 4));
            newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

            SwapByteOrder(newGuid);
            return new Guid(newGuid);
        }

        private static void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }

        private static void SwapBytes(byte[] guid, int left, int right)
        {
            var temp = guid[left];
            guid[left] = guid[right];
            guid[right] = temp;
        }
    }
}