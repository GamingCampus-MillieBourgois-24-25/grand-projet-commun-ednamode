using System.Linq;

namespace CharacterCustomization
{
    public static class MaterialPaths
    {
        private static readonly string[] MainColorNames = { "Material", "Color" };
        private static readonly string GlassName = "Glass";
        private static readonly string EmissionName = "Emission";

        // Préfixe relatif à Resources
        private static readonly string MaterialsFolder = "Materials/";

        public static readonly string[] MainColorPaths = MainColorNames.Select(GetMaterialPath).ToArray();
        public static readonly string GlassPath = GetMaterialPath(GlassName);
        public static readonly string EmissionPath = GetMaterialPath(EmissionName);

        private static string GetMaterialPath(string materialName)
        {
            return $"{MaterialsFolder}{materialName}"; // Ex. "Materials/Glass"
        }
    }
}