using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharacterCustomization
{
    public class Slot : SlotBase
    {
        private readonly SlotGroup[] _groups;
        private readonly List<SlotVariant> _variants;

        private SlotVariant _selected;

        public override string Name => Type.ToString();
        public override GameObject Preview => _selected.PreviewObject;
        public override int SelectedIndex => _variants.FindIndex(v => v.Name == _selected.Name);
        public override int VariantsCount => _variants.Count;

        public override (SlotType, Mesh)[] Meshes => new[]
        {
            (Type, _selected.Mesh),
        };

        public Slot(SlotType type, SlotGroupEntry[] slotGroupEntries) : base(type)
        {
            _groups = slotGroupEntries.Select(TranslateGroup).ToArray();
            _variants = FlattenVariants(_groups);
            if (_variants.Count == 0)
            {
                throw new Exception($"Slot {type} n'a pas de variantes disponibles !");
            }

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

        public override bool TryGetVariantsCountInGroup(GroupType stepGroupType, out int count)
        {
            var group = _groups.FirstOrDefault(g => g.Type == stepGroupType);
            if (group != null)
            {
                count = group.Variants.Length;
                return true;
            }

            count = 0;
            return false;
        }

        public override bool TryPickInGroup(GroupType groupType, int index, bool isEnabled)
        {
            if (!isEnabled || _groups.All(g => g.Type != groupType))
            {
                return false;
            }

            var mesh = _groups.First(g => g.Type == groupType).Variants[index].Mesh;
            _selected = new SlotVariant(mesh);
            Toggle(true);

            return true;
        }

        protected override void DrawSlot(Material material, int previewLayer, Camera camera, int submeshIndex)
        {
            DrawMesh(_selected.Mesh, material, previewLayer, camera, submeshIndex);
        }

        private static SlotGroup TranslateGroup(SlotGroupEntry entry)
        {
            var variants = entry.Variants
                .Select(v => new SlotVariant(v.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh))
                .ToArray();

            return new SlotGroup(entry.Type, variants);
        }

        private static List<SlotVariant> FlattenVariants(SlotGroup[] groups)
        {
            var variants = new List<SlotVariant>();
            foreach (var group in groups)
            {
                variants.AddRange(group.Variants);
            }

            return variants;
        }


        public override void SetMesh(Mesh newMesh)
        {
            var existingVariant = _variants.FirstOrDefault(v => v.Mesh == newMesh);
            if (existingVariant != null)
            {
                _selected = existingVariant;
            }
            else
            {
                _selected = new SlotVariant(newMesh);
                _variants.Add(_selected);
            }

            GameObject targetObject = _selected.PreviewObject;
            if (targetObject == null)
            {
                Debug.LogWarning("targetObject est null !");
                return;
            }

            // Réinitialiser la position, rotation et échelle
            targetObject.transform.localPosition = Vector3.zero;
            targetObject.transform.localRotation = Quaternion.identity;
            targetObject.transform.localScale = Vector3.one;

            // Appliquer le mesh
            var skinnedRenderer = targetObject.GetComponentInChildren<SkinnedMeshRenderer>(true);
            var meshRenderer = targetObject.GetComponentInChildren<MeshRenderer>(true);

            if (skinnedRenderer != null)
            {
                skinnedRenderer.sharedMesh = newMesh;
                Debug.Log($"SkinnedMeshRenderer trouvé et mesh mis à jour pour {targetObject.name}");
            }
            else if (meshRenderer != null)
            {
                var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    meshFilter.sharedMesh = newMesh;
                    Debug.Log($"MeshRenderer trouvé sur {meshRenderer.gameObject.name}, mise à jour du mesh.");
                }
                else
                {
                    Debug.LogWarning($"MeshFilter non trouvé sur {meshRenderer.gameObject.name}.");
                }
            }
            else
            {
                Debug.LogWarning($"Aucun SkinnedMeshRenderer ni MeshRenderer trouvé sur {targetObject.name} ou ses enfants.");
            }
        }





        public override bool HasMesh()
        {
            return _selected.Mesh != null;
        }


        public override List<Mesh> GetAvailableMeshes()
        {
            return _variants.Select(v => v.Mesh).ToList();
        }


    }

}