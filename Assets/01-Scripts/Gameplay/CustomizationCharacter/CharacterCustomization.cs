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
        public CharacterCustomization(GameObject characterInstance, SlotLibrary slotLibrary)
        {
            CharacterInstance = characterInstance; // ← ne pas instancier
            CharacterInstance.name = "BaseCharacter";

            _slotLibrary = slotLibrary;
            Slots = CreateSlots(slotLibrary);
        }


        /// <summary>
        /// Sélectionne l'objet précédent pour un slot donné.
        /// </summary>
        public void SelectPrevious(SlotType slotType)
        {
            GetSlotBy(slotType)?.SelectPrevious();
        }

        /// <summary>
        /// Sélectionne l'objet suivant pour un slot donné.
        /// </summary>
        public void SelectNext(SlotType slotType)
        {
            GetSlotBy(slotType)?.SelectNext();
        }

        /// <summary>
        /// Vérifie si un slot est activé.
        /// </summary>
        public bool IsToggled(SlotType slotType)
        {
            return GetSlotBy(slotType)?.IsEnabled ?? false;
        }

        /// <summary>
        /// Active ou désactive un slot donné.
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

            // Limite le nombre de sauvegardes à 4
            while (_savedCombinations.Count > 4)
            {
                _savedCombinations.RemoveAt(0);
            }
        }

        /// <summary>
        /// Charge la dernière combinaison sauvegardée.
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
        /// Réinitialise la personnalisation aux valeurs par défaut.
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
        /// Récupère un slot en fonction de son type.
        /// </summary>
        private SlotBase GetSlotBy(SlotType slotType)
        {
            return Slots.FirstOrDefault(s => s.Type == slotType);
        }

        /// <summary>
        /// Crée les slots de personnalisation à partir de la bibliothèque de slots.
        /// </summary>
        private static SlotBase[] CreateSlots(SlotLibrary slotLibrary)
        {
            var list = new List<SlotBase>();

            list.Add(new FullBodySlot(slotLibrary.FullBodyCostumes));

            foreach (var slotEntry in slotLibrary.Slots)
            {
                var prefabs = slotEntry.Items != null
                    ? slotEntry.Items.Where(item => item != null).Select(item => item.prefab).ToArray()
                    : new GameObject[0];

                list.Add(new Slot(slotEntry.Type, prefabs));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Rafraîchit l'affichage de la personnalisation.
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