using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public class AddressablesBundleObfuscator : IPostprocessBuildWithReport
{
    public int callbackOrder => 1;

    public void OnPostprocessBuild(BuildReport report)
    {
        string dataPath = Path.GetDirectoryName(report.summary.outputPath);
        string productName = UnityEditor.PlayerSettings.productName;

        string streamingAssetsAAPath;

#if UNITY_STANDALONE_WIN
        streamingAssetsAAPath = Path.Combine(dataPath, $"{productName}_Data", "StreamingAssets", "aa");
#elif UNITY_STANDALONE_OSX
        streamingAssetsAAPath = Path.Combine(dataPath, $"{productName}.app", "Contents", "Resources", "Data", "StreamingAssets", "aa");
#else
        streamingAssetsAAPath = Path.Combine(dataPath, "Data", "StreamingAssets", "aa");
#endif

        if (!Directory.Exists(streamingAssetsAAPath))
        {
            UnityEngine.Debug.LogWarning($"StreamingAssets/aa not found at: {streamingAssetsAAPath}");
            return;
        }

        ObfuscateAllBundlesInDirectory(streamingAssetsAAPath);
    }

    void ObfuscateAllBundlesInDirectory(string rootPath)
    {
        string[] allFiles = Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories);
        int count = 0;

        foreach (string filePath in allFiles)
        {
            string ext = Path.GetExtension(filePath);

            // Only obfuscate .bundle files
            if (ext == ".bundle")
            {
                ObfuscateFile(filePath);
                count++;
            }
        }

        UnityEngine.Debug.Log($"Obfuscated {count} addressable bundles in {rootPath}");
    }

    void ObfuscateFile(string path)
    {
        byte[] data = File.ReadAllBytes(path);

        if (data.Length < 8) return;

        // Generate dynamic key from bundle name
        string bundleName = Path.GetFileNameWithoutExtension(path);
        byte xorKey = GenerateKeyFromBundleName(bundleName);

        // Change signature
        byte[] obfuscatedSig = System.Text.Encoding.ASCII.GetBytes("ObfusctB");
        for (int i = 0; i < 8; i++)
        {
            data[i] = obfuscatedSig[i];
        }

        // XOR header with dynamic key
        int headerEnd = System.Math.Min(256, data.Length);
        for (int i = 8; i < headerEnd; i++)
        {
            data[i] ^= xorKey;
        }

        File.WriteAllBytes(path, data);
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