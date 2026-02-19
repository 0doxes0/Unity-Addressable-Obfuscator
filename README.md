# Unity Addressable Obfuscator

This is a very basic tool to obfuscate Unity's Addressable AssetBundles to prevent extraction by tools like AssetStudio2.
 
## What it does

- It Modifies bundle signatures and headers with dynamic keys during build, so common tools like AssetStudio can't parse the bundle.
- Uses a Custom Addressables provider to load obfuscated bundles smoothly at runtime.

## Limitations
⚠️This tool is NOT cryptographically secure and should not be relied upon for protecting sensitive assets.

Limitations include:
- Only first few bytes are obfuscated; rest of bundle is unencrypted
- Uses XOR encryption which is fast at runtime but easily reversible
- Does not protect against runtime memory inspection
- Does not protect non-Addressable AssetBundles
- Only tested on Addressable 2.8.0, might not be compatible with other versions

## Setup
1. Add the files to your project:
   - `Editor/AddressablesBundleObfuscator.cs`
   - `Runtime/ObfuscatedAssetBundleProvider.cs`  
   - `Runtime/SimpleAssetBundleResource.cs`
   
   You can put them anywhere you want but `AddressablesBundleObfuscator.cs` need to be under an `Editor` folder

2. In Addressables Settings (Window → Asset Management → Addressables → Settings), change "Asset Bundle Provider" to "Obfuscated AssetBundle Provider"

3. Build the game! - obfuscation happens automatically

4. This is only a bootstraping baseline, you can modify the encryption method to get better security.

## Use Responsibly
This tool is for legitimate DRM and IP protection purposes. 

Don't use it for:
   
 - Hiding malicious content in game assets
 - Preventing legitimate modding
 - Resale or providing false sense of security

## License
MIT
