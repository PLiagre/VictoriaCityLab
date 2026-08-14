using System;
using System.Collections.Generic;
using UnityEngine;

namespace Victoria.CityMode.AssetHost
{
    /// <summary>
    /// Creates transient URP materials from serialized catalogue references.
    /// Materials die with their partition scene and never become a lookup or a
    /// second content catalogue.
    /// </summary>
    public sealed class AssetPartitionVisualBinder : MonoBehaviour
    {
        [SerializeField] Texture2D meadow;
        [SerializeField] Texture2D road;
        [SerializeField] Renderer[] meadowRenderers = Array.Empty<Renderer>();
        [SerializeField] Renderer[] roadRenderers = Array.Empty<Renderer>();
        [SerializeField] Renderer[] cityRenderers = Array.Empty<Renderer>();

        readonly List<Material> ownedMaterials = new List<Material>();

        public void ConfigureBiome(
            Texture2D meadowTexture,
            Texture2D roadTexture,
            Renderer[] meadowTargets,
            Renderer[] roadTargets)
        {
            meadow = meadowTexture;
            road = roadTexture;
            meadowRenderers = meadowTargets ?? Array.Empty<Renderer>();
            roadRenderers = roadTargets ?? Array.Empty<Renderer>();
            cityRenderers = Array.Empty<Renderer>();
        }

        public void ConfigureCity(Renderer[] renderers)
        {
            meadow = null;
            road = null;
            meadowRenderers = Array.Empty<Renderer>();
            roadRenderers = Array.Empty<Renderer>();
            cityRenderers = renderers ?? Array.Empty<Renderer>();
        }

        void Awake()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new InvalidOperationException("URP Lit shader is required by the asset host.");

            if (meadow != null)
            {
                var material = CreateMaterial(shader, "City Mode Meadow", new Color(0.47f, 0.55f, 0.26f));
                material.SetTexture("_BaseMap", meadow);
                material.SetFloat("_Smoothness", 0.05f);
                Assign(meadowRenderers, material);
            }
            if (road != null)
            {
                var material = CreateMaterial(shader, "City Mode Road", new Color(0.48f, 0.35f, 0.21f));
                material.SetTexture("_BaseMap", road);
                material.SetFloat("_Smoothness", 0.03f);
                Assign(roadRenderers, material);
            }

            var palette = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            foreach (var renderer in cityRenderers)
            {
                if (renderer == null)
                    continue;
                var sourceMaterials = renderer.sharedMaterials;
                var replacements = new Material[sourceMaterials.Length];
                for (var index = 0; index < replacements.Length; index++)
                {
                    var sourceName = sourceMaterials[index] == null
                        ? "timber"
                        : sourceMaterials[index].name;
                    if (!palette.TryGetValue(sourceName, out var replacement))
                    {
                        replacement = CreateCityMaterial(shader, sourceName);
                        palette.Add(sourceName, replacement);
                    }
                    replacements[index] = replacement;
                }
                renderer.sharedMaterials = replacements;
            }
        }

        Material CreateCityMaterial(Shader shader, string sourceName)
        {
            var lower = sourceName.ToLowerInvariant();
            var color = new Color(0.22f, 0.09f, 0.025f, 1f);
            var metallic = 0f;
            var smoothness = 0.13f;
            if (lower.Contains("stone")) color = new Color(0.30f, 0.31f, 0.29f, 1f);
            else if (lower.Contains("plaster")) color = new Color(0.46f, 0.34f, 0.19f, 1f);
            else if (lower.Contains("fresh") || lower.Contains("cut")) color = new Color(0.58f, 0.30f, 0.09f, 1f);
            else if (lower.Contains("bark")) color = new Color(0.10f, 0.035f, 0.012f, 1f);
            else if (lower.Contains("roof_accent_a")) color = Html("#7A2515");
            else if (lower.Contains("roof_accent_b")) color = Html("#46515B");
            else if (lower.Contains("roof_accent_c")) color = Html("#667246");
            else if (lower.Contains("roof_a")) color = Html("#3B120A");
            else if (lower.Contains("roof_b")) color = Html("#252B31");
            else if (lower.Contains("roof_c")) color = Html("#414A25");
            else if (lower.Contains("iron") || lower.Contains("steel"))
            {
                color = new Color(0.18f, 0.20f, 0.21f, 1f);
                metallic = 0.85f;
                smoothness = 0.45f;
            }
            else if (lower.Contains("bronze"))
            {
                color = new Color(0.43f, 0.20f, 0.055f, 1f);
                metallic = 0.72f;
                smoothness = 0.38f;
            }
            var material = CreateMaterial(shader, "Ported " + sourceName, color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (lower.Contains("ember"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(2.5f, 0.18f, 0.015f));
            }
            return material;
        }

        Material CreateMaterial(Shader shader, string name, Color color)
        {
            var material = new Material(shader)
            {
                name = name,
                enableInstancing = true
            };
            material.SetColor("_BaseColor", color);
            ownedMaterials.Add(material);
            return material;
        }

        static void Assign(Renderer[] renderers, Material material)
        {
            foreach (var renderer in renderers)
                if (renderer != null)
                    renderer.sharedMaterial = material;
        }

        static Color Html(string value)
        {
            if (ColorUtility.TryParseHtmlString(value, out var color))
                return color;
            throw new InvalidOperationException("Invalid asset host colour: " + value);
        }

        void OnDestroy()
        {
            foreach (var material in ownedMaterials)
                if (material != null)
                    Destroy(material);
            ownedMaterials.Clear();
        }
    }
}
