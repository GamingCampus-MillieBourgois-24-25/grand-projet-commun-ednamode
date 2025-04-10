using System.Collections.Generic;
using UnityEngine;

namespace CharacterCustomization
{
    public abstract class SlotBase
    {

        public abstract string Name { get; }
        public abstract GameObject Preview { get; } 
        public abstract int SelectedIndex { get; }
        public abstract int VariantsCount { get; }
        public abstract (SlotType, GameObject)[] Prefabs { get; } 

        public SlotType Type { get; }
        public bool IsEnabled { get; private set; } = true;

        protected SlotBase(SlotType type)
        {
            Type = type;
        }

        public abstract void SelectNext();
        public abstract void SelectPrevious();
        public abstract void Select(int index);
        public abstract bool TryGetVariantsCountInGroup(GroupType groupType, out int count);
        public abstract bool TryPickInGroup(GroupType groupType, int index, bool isEnabled);

        public abstract List<GameObject> GetAvailablePrefabs(); 
        public abstract void SetPrefab(GameObject newPrefab); 


        public void Draw(Material material, int previewLayer, Camera camera, int submeshIndex)
        {
            if (IsEnabled)
            {
                DrawSlot(material, previewLayer, camera, submeshIndex);
            }
        }

        public bool IsOfType(SlotType type)
        {
            return Type == type;
        }

        public void Toggle(bool isToggled)
        {
            IsEnabled = isToggled;
        }

        protected abstract void DrawSlot(Material material, int previewLayer, Camera camera, int submeshIndex);

        protected int GetNextIndex()
        {
            var targetIndex = SelectedIndex + 1;
            if (targetIndex >= VariantsCount)
            {
                targetIndex = 0;
            }

            return targetIndex;
        }

        protected int GetPreviousIndex()
        {
            var targetIndex = SelectedIndex - 1;
            if (targetIndex < 0)
            {
                targetIndex = VariantsCount - 1;
            }
            return targetIndex;
        }

        protected static void DrawPrefab(GameObject prefab, Material material, int previewLayer, Camera camera, int submeshIndex)
        {
            // Instancier le prefab pour la prévisualisation
            GameObject previewObject = Object.Instantiate(prefab);
            previewObject.transform.position = new Vector3(0, -.01f, 0);
            previewObject.transform.rotation = Quaternion.identity;

            // Appliquer le matériau au prefab
            var renderers = previewObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material = material;
            }

            previewObject.layer = previewLayer;
            foreach (var renderer in renderers)
            {
                renderer.gameObject.layer = previewLayer;
            }

            Object.Destroy(previewObject, 0.1f); 
        }

        public virtual bool HasPrefab()
        {
            return false; 
        }
    }
}