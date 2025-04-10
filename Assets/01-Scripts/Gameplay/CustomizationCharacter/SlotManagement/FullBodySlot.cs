using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharacterCustomization
{
    public class FullBodySlot : SlotBase
    {
        private readonly List<FullBodyVariant> _variants;

        private FullBodyVariant _selected;

        public override string Name => "Full Body";
        public override GameObject Preview => _selected.PreviewObject;
        public override int SelectedIndex => _variants.FindIndex(v => v.Name == _selected.Name);
        public override int VariantsCount => _variants.Count;
        public override (SlotType, GameObject)[] Prefabs => _selected.Elements.Select(e => (e.Type, e.Prefab)).ToArray(); // Utilisez des prefabs

        public FullBodySlot(FullBodyEntry[] fullBodyEntries) : base(SlotType.FullBody)
        {
            _variants = fullBodyEntries.Select(e => new FullBodyVariant(e)).ToList();
            _selected = _variants.First();
        }

        public override void SelectNext()
        {
            _selected = _variants[GetNextIndex()];
        }

        public override void SelectPrevious()
        {
            _selected = _variants[GetPreviousIndex()];
        }

        public override void Select(int index)
        {
            _selected = _variants[index];
        }

        public override bool TryGetVariantsCountInGroup(GroupType groupType, out int count)
        {
            if (groupType == GroupType.Costumes)
            {
                count = _variants.Count;
                return true;
            }

            count = 0;
            return false;
        }

        public override bool TryPickInGroup(GroupType groupType, int index, bool isEnabled)
        {
            if (!isEnabled || groupType != GroupType.Costumes)
            {
                return false;
            }

            _selected = _variants[index];
            Toggle(true);

            return true;
        }

        protected override void DrawSlot(Material material, int previewLayer, Camera camera, int submeshIndex)
        {
            foreach (var element in _selected.Elements)
            {
                DrawPrefab(element.Prefab, material, previewLayer, camera, submeshIndex); // Utilisez DrawPrefab au lieu de DrawMesh
            }
        }

        public override List<GameObject> GetAvailablePrefabs()
        {
            return _variants
                .SelectMany(v => v.Elements.Select(e => e.Prefab))
                .Distinct()
                .ToList(); // Retourne une liste de prefabs
        }

        public override void SetPrefab(GameObject prefab)
        {
            var foundVariant = _variants.FirstOrDefault(v => v.Elements.Any(e => e.Prefab == prefab));
            if (foundVariant != null)
            {
                _selected = foundVariant;
            }
            else
            {
                Debug.LogWarning($"Prefab {prefab.name} non trouvé dans FullBodySlot.");
            }
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

            // Configurer le layer et la caméra pour la prévisualisation
            previewObject.layer = previewLayer;
            foreach (var renderer in renderers)
            {
                renderer.gameObject.layer = previewLayer;
            }

            // Détruire l'objet après la prévisualisation (si nécessaire)
            Object.Destroy(previewObject, 0.1f); // Ajustez le délai selon vos besoins
        }
    }
}