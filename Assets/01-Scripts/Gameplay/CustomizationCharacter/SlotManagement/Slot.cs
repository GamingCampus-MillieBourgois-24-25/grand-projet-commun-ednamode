using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharacterCustomization
{
    public class Slot : SlotBase
    {
        private readonly GameObject[] _prefabs;
        private int _selectedIndex = 0;
        private GameObject _currentInstance;

        public override string Name => Type.ToString();
        public override GameObject Preview => _prefabs.Length > 0 ? _prefabs[_selectedIndex] : null;
        public override int SelectedIndex => _selectedIndex;
        public override int VariantsCount => _prefabs.Length;
        public override (SlotType, GameObject)[] Prefabs => _prefabs.Select(p => (Type, p)).ToArray();

        public Slot(SlotType type, GameObject[] prefabs) : base(type)
        {
            Type = type;
            _prefabs = prefabs ?? new GameObject[0];
            IsEnabled = false;
        }

        public override bool HasPrefab()
        {
            return _prefabs.Length > 0;
        }

        public override void SelectNext()
        {
            if (_prefabs.Length == 0) return;
            _selectedIndex = GetNextIndex();
            UpdateInstance();
        }

        public override void SelectPrevious()
        {
            if (_prefabs.Length == 0) return;
            _selectedIndex = GetPreviousIndex();
            UpdateInstance();
        }

        public override void Select(int index)
        {
            if (_prefabs.Length == 0 || index < 0 || index >= _prefabs.Length) return;
            _selectedIndex = index;
            UpdateInstance();
        }

        public override bool TryGetVariantsCountInGroup(GroupType groupType, out int count)
        {
            count = _prefabs.Length;
            return true;
        }

        public override bool TryPickInGroup(GroupType groupType, int index, bool isEnabled)
        {
            if (index < 0 || index >= _prefabs.Length) return false;
            _selectedIndex = index;
            Toggle(isEnabled);
            return true;
        }

        public override List<GameObject> GetAvailablePrefabs()
        {
            return _prefabs.ToList();
        }

        public override void SetPrefab(GameObject newPrefab)
        {
            int index = System.Array.IndexOf(_prefabs, newPrefab);
            if (index != -1)
            {
                _selectedIndex = index;
                UpdateInstance();
            }
        }

        protected override void DrawSlot(Material material, int previewLayer, Camera camera, int submeshIndex)
        {
            if (_prefabs.Length > 0)
            {
                DrawPrefab(_prefabs[_selectedIndex], material, previewLayer, camera, submeshIndex);
            }
        }

        private void UpdateInstance()
        {
            if (_currentInstance != null)
            {
                Object.Destroy(_currentInstance);
                _currentInstance = null;
            }

            if (IsEnabled && _prefabs.Length > 0)
            {
                _currentInstance = Object.Instantiate(_prefabs[_selectedIndex]);
            }
        }
    }
}