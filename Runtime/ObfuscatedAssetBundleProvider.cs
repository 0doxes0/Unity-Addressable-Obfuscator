using System;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Obfuscation.Addressables
{
    [DisplayName("Obfuscated AssetBundle Provider")]
    public class ObfuscatedAssetBundleProvider : AssetBundleProvider
    {
        private const string OBFUSCATED_SIGNATURE = "ObfusctB";
        private const string UNITY_SIGNATURE = "UnityFS";
        private const int HEADER_XOR_LENGTH = 256;


        public override void Provide(ProvideHandle provideHandle)
        {
            // get bundle path
            string path = provideHandle.ResourceManager.TransformInternalId(provideHandle.Location);

            // Check file existence
            if (!File.Exists(path))

            {
                Debug.LogError($"[ObfuscatedProvider] File not found: {path}");
                provideHandle.Complete<IAssetBundleResource>(null, false,
                    new FileNotFoundException($"Bundle file not found: {path}"));
                return;
            }

            // Read signature (8 bytes)
            byte[] signatureBytes = new byte[8];
            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    fs.Read(signatureBytes, 0, 8);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ObfuscatedProvider] Failed to read signature from {path}: {e.Message}");
                provideHandle.Complete<IAssetBundleResource>(null, false, e);
                return;
            }

            string signature = System.Text.Encoding.ASCII.GetString(signatureBytes);

            if (signature == OBFUSCATED_SIGNATURE)
            {
                StartDeobfuscatedLoad(provideHandle, path);
            }
            else
            {
                base.Provide(provideHandle);
            }

        }

        private void StartDeobfuscatedLoad(ProvideHandle handle, string path)
        {
            byte[] obfuscatedData = null;

            try
            {
                // Read obfuscated bundle
                obfuscatedData = File.ReadAllBytes(path);
            }

            catch (Exception e)
            {
                Debug.LogError($"[ObfuscatedProvider] Failed to read file: {e.Message}");
                handle.Complete<IAssetBundleResource>(null, false, e);
                return;
            }

            try
            {
                // Generate dynamic key from bundle name
                string bundleName = Path.GetFileNameWithoutExtension(path);
                byte xorKey = GenerateKeyFromBundleName(bundleName);

                // Restore signature: ObfusctB -> UnityFS
                byte[] correctSig = System.Text.Encoding.ASCII.GetBytes(UNITY_SIGNATURE);
                for (int i = 0; i < 8; i++)
                {
                    obfuscatedData[i] = correctSig[i];
                }

                // XOR restore header with dynamic key
                int headerEnd = Math.Min(HEADER_XOR_LENGTH, obfuscatedData.Length);
                for (int i = 8; i < headerEnd; i++)
                {
                    obfuscatedData[i] ^= xorKey;
                }
            }

            catch (Exception e)
            {
                Debug.LogError($"[ObfuscatedProvider] Deobfuscation failed: {e.Message}");
                handle.Complete<IAssetBundleResource>(null, false, e);
                return;
            }

            // Use LoadFromMemoryAsync instead of file
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(obfuscatedData);

            request.completed += (op) =>
            {
                AssetBundle bundle = request.assetBundle;

                if (bundle != null)
                {
                    var wrapper = new SimpleAssetBundleResource(bundle);
                    handle.Complete(wrapper, true, null);
                }

                else
                {
                    Debug.LogError($"[ObfuscatedProvider] Failed to load bundle from memory");
                    handle.Complete<IAssetBundleResource>(null, false,
                        new Exception("AssetBundle.LoadFromMemoryAsync returned null"));
                }
            };
        }

        public override void Release(IResourceLocation location, object asset)
        {
            if (asset is SimpleAssetBundleResource wrapper)

            {
                wrapper.Unload();
            }
            else
            {
                base.Release(location, asset);
            }
        }

        private static byte GenerateKeyFromBundleName(string bundleName)
        {
            // Generate XOR key from bundle name to make each bundle unique
            int sum = 0;
            foreach (char c in bundleName)
            {
                sum += c;
            }
            return (byte)((sum % 255) + 1); // Avoid 0 as key
        }

    }
}
