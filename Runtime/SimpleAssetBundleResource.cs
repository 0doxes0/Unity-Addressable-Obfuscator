using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Obfuscation.Addressables
{
    /// <summary>
    /// Simple AssetBundle wrapper£¬used to load AB into memory
    /// </summary>
    public class SimpleAssetBundleResource : IAssetBundleResource
    {
        private AssetBundle m_AssetBundle;

        public SimpleAssetBundleResource(AssetBundle bundle)
        {
            m_AssetBundle = bundle;
            Debug.Log($"[SimpleAssetBundleResource] Created wrapper for bundle: {(bundle != null ? bundle.name : "null")}");
        }

        public AssetBundle GetAssetBundle()
        {
            return m_AssetBundle;
        }

        public void Unload()
        {
            if (m_AssetBundle != null)
            {
                m_AssetBundle.Unload(true);
                m_AssetBundle = null;
            }
        }
    }
}
