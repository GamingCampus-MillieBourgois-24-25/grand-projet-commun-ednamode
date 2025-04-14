using System;
using UnityEngine;

namespace CharacterCustomization
{
    public class MaterialOnDemand
    {
        private readonly string[] _paths;
        private Material _value;

        public Material Value => _value ? _value : LoadMaterial();

        public MaterialOnDemand(params string[] paths)
        {
            _paths = paths;
        }

        private Material LoadMaterial()
        {
            foreach (var path in _paths)
            {
                Debug.Log($"Tentative de chargement du mat�riau � : {path}");
                string cleanPath = path.EndsWith(".mat") ? path.Substring(0, path.Length - 4) : path;
                var loadedMaterial = Resources.Load<Material>(cleanPath);
                if (loadedMaterial != null)
                {
                    _value = loadedMaterial;
                    Debug.Log($"Mat�riau charg� : {loadedMaterial.name}");
                    return _value;
                }
                else
                {
                    Debug.LogWarning($"�chec du chargement du mat�riau � : {cleanPath}");
                }
            }

            throw new Exception("MaterialOnDemand: aucun mat�riau d�fini !");
        }
    }
}