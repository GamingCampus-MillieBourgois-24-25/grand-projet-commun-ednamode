using UnityEngine;

namespace CharacterCustomization
{
    public static class PreviewCreator
    {
        private static readonly MaterialProvider MaterialProvider = new();

        public static GameObject CreateVariantPreview(GameObject prefab)
        {
            // Instancier le prefab
            GameObject variant = Object.Instantiate(prefab);

            // Configurer la pr�visualisation
            variant.name = prefab.name; // Utiliser le nom du prefab
            variant.transform.position = Vector3.one * int.MaxValue; // Placer l'objet hors de la vue
            variant.hideFlags = HideFlags.HideAndDontSave; // Masquer l'objet dans la hi�rarchie

            // Appliquer le mat�riau principal � tous les renderers du prefab
            var renderers = variant.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.sharedMaterial = MaterialProvider.MainColor;
            }

            return variant;
        }
    }
}