using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharacterCustomization
{
    /// <summary>
    /// Gestion de la personnalisation du personnage.
    /// </summary>
    public class CharacterCustomization
    {
        private static readonly SlotType[] AlwaysEnabledParts = { SlotType.Body, SlotType.Faces };
        private readonly List<List<SavedSlot>> _savedCombinations = new();
        private readonly SlotLibrary _slotLibrary;
        public SlotEntry[] SlotEntries => _slotLibrary.Slots;

        public GameObject CharacterInstance { get; private set; }
        public SlotBase[] Slots { get; private set; }
        public int SavedCombinationsCount => _savedCombinations.Count;

        /// <summary>
        /// Constructeur de la personnalisation du personnage.
        /// </summary>
        public CharacterCustomization(GameObject characterPrefab, SlotLibrary slotLibrary)
        {
            Vector3 spawnPosition = new Vector3(9.64f, 5.03f, -4f);
            Quaternion spawnRotation = Quaternion.Euler(0f, 150f, 0f);
            CharacterInstance = Object.Instantiate(characterPrefab, spawnPosition, spawnRotation);
            CharacterInstance.name = "BaseCharacter";

            _slotLibrary = slotLibrary;
            Slots = CreateSlots(slotLibrary);
        }

        /// <summary>
        /// S�lectionne l'objet pr�c�dent pour un slot donn�.
        /// </summary>
        public void SelectPrevious(SlotType slotType)
        {
            GetSlotBy(slotType)?.SelectPrevious();
        }

        /// <summary>
        /// S�lectionne l'objet suivant pour un slot donn�.
        /// </summary>
        public void SelectNext(SlotType slotType)
        {
            GetSlotBy(slotType)?.SelectNext();
        }

        /// <summary>
        /// V�rifie si un slot est activ�.
        /// </summary>
        public bool IsToggled(SlotType slotType)
        {
            return GetSlotBy(slotType)?.IsEnabled ?? false;
        }

        /// <summary>
        /// Active ou d�sactive un slot donn�.
        /// </summary>
        public void Toggle(SlotType type, bool isToggled)
        {
            GetSlotBy(type)?.Toggle(isToggled);
        }

        /// <summary>
        /// Sauvegarde la combinaison actuelle de personnalisation.
        /// </summary>
        public void SaveCombination()
        {
            var savedCombinations = Slots.Select(slot => new SavedSlot(slot.Type, slot.IsEnabled, slot.SelectedIndex)).ToList();
            _savedCombinations.Add(savedCombinations);

            // Limite le nombre de sauvegardes � 4
            while (_savedCombinations.Count > 4)
            {
                _savedCombinations.RemoveAt(0);
            }
        }

        /// <summary>
        /// Charge la derni�re combinaison sauvegard�e.
        /// </summary>
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

        /// <summary>
        /// R�initialise la personnalisation aux valeurs par d�faut.
        /// </summary>
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

        /// <summary>
        /// R�cup�re un slot en fonction de son type.
        /// </summary>
        private SlotBase GetSlotBy(SlotType slotType)
        {
            return Slots.FirstOrDefault(s => s.Type == slotType);
        }

        /// <summary>
        /// Cr�e les slots de personnalisation � partir de la biblioth�que de slots.
        /// </summary>
        private static SlotBase[] CreateSlots(SlotLibrary slotLibrary)
        {
            var list = new List<SlotBase>();

            list.Add(new FullBodySlot(slotLibrary.FullBodyCostumes));

            foreach (var slotEntry in slotLibrary.Slots)
            {
                var prefabs = slotEntry.Items
                    .Where(item => item != null)
                    .Select(item => item.prefab)
                    .ToArray();
                list.Add(new Slot(slotEntry.Type, prefabs));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Rafra�chit l'affichage de la personnalisation.
        /// </summary>
        public void RefreshCustomization()
        {
            foreach (var slot in Slots)
            {
                slot.Toggle(slot.HasPrefab());
            }
        }
    }
}