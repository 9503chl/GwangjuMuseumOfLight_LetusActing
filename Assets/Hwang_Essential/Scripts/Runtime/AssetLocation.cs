using System;

namespace UnityEngine
{
    [Serializable]
    public enum AssetLocation
    {
        /// <summary>
        /// Relative path to <see cref="Application.persistentDataPath"/>
        /// </summary>
        PersistentData,
        /// <summary>
        /// Relative path to <see cref="Application.streamingAssetsPath"/>
        /// </summary>
        StreamingAssets,
        /// <summary>
        /// Custom path or URL
        /// </summary>
        PathOrUrl
    }
}
