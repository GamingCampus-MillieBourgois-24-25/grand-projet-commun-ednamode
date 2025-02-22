using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharacterCustomization
{
    public class CharacterCustomization 
    {
        private static readonly SlotType[] AlwaysEnabledParts = { SlotType.Body, SlotType.Faces };
        private readonly List<List<SavedSlot>> _savedCombinations = new();
        private readonly GameObject _characterGameObject;

        public SlotBase[] Slots { get; private set; }
        public int SavedCombinationsCount => _savedCombinations.Count;

        public CharacterCustomization(SlotLibrary slotLibrary)
        {
            _characterGameObject = LoadBaseMesh();
            Slots = CreateSlots(slotLibrary);
        }

        public GameObject InstantiateCharacter()
        {
            return Object.Instantiate(_characterGameObject, Vector3.zero, Quaternion.identity);
        }

        public void SelectPrevious(SlotType slotType)
        {
            GetSlotBy(slotType)?.SelectPrevious();
        }

        public void SelectNext(SlotType slotType)
        {
            GetSlotBy(slotType)?.SelectNext();
        }

        public bool IsToggled(SlotType slotType)
        {
            return GetSlotBy(slotType)?.IsEnabled ?? false;
        }

        public void Toggle(SlotType type, bool isToggled)
        {
            GetSlotBy(type)?.Toggle(isToggled);
        }

        public void SaveCombination()
        {
            var savedCombinations = Slots.Select(slot => new SavedSlot(slot.Type, slot.IsEnabled, slot.SelectedIndex)).ToList();
            _savedCombinations.Add(savedCombinations);

            while (_savedCombinations.Count > 4)
            {
                _savedCombinations.RemoveAt(0);
            }
        }

        public void LastCombination()
        {
            if (_savedCombinations.Count == 0) return;

            var lastSavedCombination = _savedCombinations.Last();

            foreach (var slot in Slots)
            {
                SavedSlot? savedCombination = lastSavedCombination.FirstOrDefault(c => c.SlotType == slot.Type);
                if (savedCombination.HasValue) 
                {
                    slot.Toggle(savedCombination.Value.IsEnabled);
                    slot.Select(savedCombination.Value.VariantIndex);
                }

            }
            _savedCombinations.Remove(lastSavedCombination);
        }

        public void ToDefault()
        {
            foreach (var slot in Slots)
            {
                if (!AlwaysEnabledParts.Contains(slot.Type))
                {
                    slot.Toggle(false);
                }
            }
        }

        private SlotBase GetSlotBy(SlotType slotType)
        {
            return Slots.FirstOrDefault(s => s.Type == slotType);
        }

        private static GameObject LoadBaseMesh()
        {
            return new GameObject("BaseCharacter");
        }

        private static SlotBase[] CreateSlots(SlotLibrary slotLibrary)
        {
            var list = new List<SlotBase>();
            list.Add(new FullBodySlot(slotLibrary.FullBodyCostumes));
            list.AddRange(slotLibrary.Slots.Select(s => new Slot(s.Type, s.Groups)));
            return list.ToArray();
        }
    }
}
