using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public class TextureDownloader : AssetDownloader
    {
        protected override Type assetType
        {
            get { return typeof(Texture); }
        }

        protected override Type[] componentTypes
        {
            get { return new Type[] { typeof(RawImage), typeof(MeshRenderer), typeof(SkinnedMeshRenderer) }; }
        }

        public Texture texture
        {
            get { return asset as Texture; }
        }

        [SerializeField]
        [Tooltip("Material name or index to change texture\n(default is first material)")]
        private string materialName;
        public string MaterialName
        {
            get { return materialName; }
            set { if (materialName != value) { materialName = value; } }
        }

        [SerializeField]
        [Tooltip("Property name of texture in shader\n(default is main texture")]
        private string propertyName;
        public string PropertyName
        {
            get { return propertyName; }
            set { if (propertyName != value) { propertyName = value; } }
        }

        public Material GetMaterial()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Material[] materials;
                if (Application.isPlaying)
                {
                    materials = renderer.materials;
                }
                else
                {
                    materials = renderer.sharedMaterials;
                }
                if (!string.IsNullOrEmpty(materialName))
                {
                    try
                    {
                        int materialIndex = int.Parse(materialName);
                        if (materialIndex >= 0 && materialIndex < materials.Length)
                        {
                            return materials[materialIndex];
                        }
                    }
                    catch (Exception)
                    {
                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (materials[i] != null &&
                                (string.Compare(materials[i].name, materialName, true) == 0 ||
                                    string.Compare(materials[i].name, materialName + " (Instance)", true) == 0))
                            {
                                return materials[i];
                            }
                        }
                    }
                }
                else
                {
                    if (Application.isPlaying)
                    {
                        return renderer.material;
                    }
                    else
                    {
                        return renderer.sharedMaterial;
                    }
                }
            }
            return null;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            const string assetExtension = ".png";
            RawImage rawImage = GetComponent<RawImage>();
            if (rawImage != null)
            {
                FindPathFromAssets(rawImage.texture, assetExtension);
            }
            else
            {
                Material material = GetMaterial();
                if (material != null)
                {
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        FindPathFromAssets(material.GetTexture(propertyName), assetExtension);
                    }
                    else
                    {
                        FindPathFromAssets(material.mainTexture, assetExtension);
                    }
                }
            }
            Load();
        }
#endif

        protected override void ApplyAsset()
        {
            if (texture != null)
            {
                RawImage rawImage = GetComponent<RawImage>();
                if (rawImage != null)
                {
                    rawImage.texture = texture;
                }
                else
                {
                    Material material = GetMaterial();
                    if (material != null)
                    {
                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            material.SetTexture(propertyName, texture);
                        }
                        else
                        {
                            material.mainTexture = texture;
                        }
                    }
                }
            }
        }
    }
}
