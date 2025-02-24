using System;
using UnityEditor;
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
                var loadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (loadedMaterial != null)
                {
                    throw new Exception("MaterialOnDemand: aucun matériau défini !");

                }
                return loadedMaterial;
            }

            throw new Exception();
        }
    }
}