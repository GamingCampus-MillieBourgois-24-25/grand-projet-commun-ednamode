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
        public override (SlotType, GameObject)[] Prefabs => _selected.Elements.Select(e => (e.Type, e.Prefab)).ToArray();

        public FullBodySlot(FullBodyEntry[] fullBodyEntries) : base(SlotType.FullBody)
        {
            _variants = fullBodyEntries.Select(e => new FullBodyVariant(e)).ToList();
            _selected = _variants.Any() ? _variants.First() : null;
        }

        public override void SelectNext()
        {
            if (_variants.Count == 0) return;
            _selected = _variants[GetNextIndex()];
        }

        public override void SelectPrevious()
        {
            if (_variants.Count == 0) return;
            _selected = _variants[GetPreviousIndex()];
        }

        public override void Select(int index)
        {
            if (_variants.Count == 0 || index < 0 || index >= _variants.Count) return;
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
            if (!isEnabled || groupType != GroupType.Costumes || index < 0 || index >= _variants.Count)
            {
                return false;
            }

            _selected = _variants[index];
            Toggle(true);
            return true;
        }

        protected override void DrawSlot(Material material, int previewLayer, Camera camera, int submeshIndex)
        {
            if (_selected == null) return;
            foreach (var element in _selected.Elements)
            {
                DrawPrefab(element.Prefab, material, previewLayer, camera, submeshIndex);
            }
        }

        public override List<GameObject> GetAvailablePrefabs()
        {
            return _variants
                .SelectMany(v => v.Elements.Select(e => e.Prefab))
                .Distinct()
                .ToList();
        }

        public override void SetPrefab(GameObject prefab)
        {
            var foundVariant = _variants.FirstOrDefault(v => v.Elements.Any(e => e.Prefab == prefab));
            if (foundVariant != null)
            {
                _selected = foundVariant;
            }
         
        }
    }
}