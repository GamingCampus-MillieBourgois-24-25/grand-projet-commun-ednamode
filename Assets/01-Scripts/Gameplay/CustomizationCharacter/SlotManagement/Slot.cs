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
        private readonly GameObject[] _prefabs; // Tableau de prefabs
        private int _selectedIndex;

        public override string Name => Type.ToString();
        public override GameObject Preview => _prefabs[_selectedIndex]; // Utilisez le prefab comme preview
        public override int SelectedIndex => _selectedIndex;
        public override int VariantsCount => _prefabs.Length;


        public override (SlotType, GameObject)[] Prefabs => new[]
        {
        (Type, _prefabs[_selectedIndex]), // Retourne le prefab actuel
        };


        public Slot(SlotType type, GameObject[] prefabs) : base(type)
        {
            _prefabs = prefabs;
            if (_prefabs.Length == 0)
            {
                throw new Exception($"Slot {type} n'a pas de variantes disponibles !");
            }

            _selectedIndex = 0;
        }

        public override void SelectNext()
        {
            _selectedIndex = GetNextIndex();
        }

        public override void SelectPrevious()
        {
            _selectedIndex = GetPreviousIndex();
        }
        public override void Select(int index)
        {
            if (index >= 0 && index < _prefabs.Length)
            {
                _selectedIndex = index;
            }
        }

        public override bool TryGetVariantsCountInGroup(GroupType stepGroupType, out int count)
        {
            // Cette méthode n'est plus nécessaire si vous ne gérez pas de groupes
            count = 0;
            return false;
        }

        public override bool TryPickInGroup(GroupType groupType, int index, bool isEnabled)
        {
            // Cette méthode n'est plus nécessaire si vous ne gérez pas de groupes
            return false;
        }

        protected override void DrawSlot(Material material, int previewLayer, Camera camera, int submeshIndex)
        {
            // Cette méthode n'est plus nécessaire si vous utilisez des prefabs
            Debug.LogWarning("DrawSlot n'est pas implémenté pour les prefabs.");
        }

        private static SlotGroup TranslateGroup(SlotGroupEntry entry)
        {
            var variants = entry.Variants
                .Select(v => new SlotVariant(v)) // Utilisez le GameObject directement
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

        public override void SetPrefab(GameObject newPrefab)
        {
            var index = Array.IndexOf(_prefabs, newPrefab);
            if (index >= 0)
            {
                _selectedIndex = index;
            }
            else
            {
                Debug.LogWarning($"Le prefab {newPrefab.name} n'existe pas dans ce slot.");
            }
        }

        public override bool HasPrefab()
        {
            return _prefabs.Length > 0 && _prefabs[_selectedIndex] != null;
        }
        public override List<GameObject> GetAvailablePrefabs()
        {
            return _prefabs.ToList();
        }
        private int GetNextIndex()
        {
            return (_selectedIndex + 1) % _prefabs.Length;
        }

        private int GetPreviousIndex()
        {
            return (_selectedIndex - 1 + _prefabs.Length) % _prefabs.Length;
        }
        public class SlotVariant
        {
            public GameObject Prefab { get; }

            public SlotVariant(GameObject prefab)
            {
                Prefab = prefab;
            }
        }

        public class SlotGroup
        {
            public GroupType Type { get; }
            public SlotVariant[] Variants { get; }

            public SlotGroup(GroupType type, SlotVariant[] variants)
            {
                Type = type;
                Variants = variants;
            }
        }
    }
}