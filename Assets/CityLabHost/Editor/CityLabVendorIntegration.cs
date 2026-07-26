using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Victoria.CityMode;

namespace Victoria.CityLab.Editor
{
    public static class CityLabVendorIntegration
    {
        const string AdaptedRoot = "Assets/CityLabHost/Adapted";
        const string PrefabRoot = AdaptedRoot + "/Prefabs";
        const string MaterialRoot = AdaptedRoot + "/Materials";
        const string ControllerPath = AdaptedRoot + "/CityLabVillager.controller";
        const string LibraryPath = "Assets/CityLabHost/Resources/CityLabVisualLibrary.asset";

        static readonly string[] VendorRoots =
        {
            "Assets/DoubleL",
            "Assets/EmaceArt",
            "Assets/Kevin Iglesias",
            "Assets/Polytope Studio",
            "Assets/URP GanzSe Free Modular Character Pack"
        };

        static readonly string[] HouseSources =
        {
            "Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_01a_PRE.prefab",
            "Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_02a_PRE.prefab",
            "Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03a_PRE.prefab"
        };

        const string TownCentreSource = "Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03b_PRE.prefab";
        const string StockSource = "Assets/EmaceArt/Slavic World Free/Prefabs/Items/House/EA03_Items_House_Firewood_01a_PRE.prefab";
        const string CharacterSource = "Assets/URP GanzSe Free Modular Character Pack/Prefabs/Modular Character/GanzSe Free Modular Character Update 1_1.prefab";
        const string IdleSource = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Idles/HumanM@Idle01.fbx";
        const string WalkSource = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Walk/HumanM@Walk01_Forward.fbx";

        static readonly string[] TreeSources =
        {
            "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Fruit_Tree_01_green.prefab",
            "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_green.prefab"
        };

        [MenuItem("Victoria/CityLab/Integrate Vendor Assets")]
        public static void Integrate()
        {
            EnsureFolder("Assets/CityLabHost", "Adapted");
            EnsureFolder(AdaptedRoot, "Prefabs");
            EnsureFolder(AdaptedRoot, "Materials");

            var houses = HouseSources.Select((path, index) =>
                CreateNormalizedPrefab(path, $"CityLab House {index + 1}", 6.2f, 9.5f, 0.2f, true)).ToArray();
            var centre = CreateNormalizedPrefab(TownCentreSource, "CityLab Town Centre", 7.2f, 12f, 0.2f, true);
            var stock = CreateNormalizedPrefab(StockSource, "CityLab Wood Stock", 1.5f, 5f, 1f, true);
            var character = CreateNormalizedPrefab(CharacterSource, "CityLab Villager", 1.75f, 1.2f, 1f, true);
            var trees = TreeSources.Select((path, index) =>
                CreateNormalizedPrefab(path, $"CityLab Tree {index + 1}", index == 0 ? 8f : 11f, 8f, 1f, false)).ToArray();

            var controller = CreateAnimatorController();
            var library = AssetDatabase.LoadAssetAtPath<CityVisualLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<CityVisualLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }
            library.townCentrePrefab = centre;
            library.stockpilePrefab = stock;
            library.housePrefabs = houses;
            library.villagerPrefab = character;
            library.villagerAnimatorController = controller;
            library.treePrefabs = trees;
            library.rockPrefabs = Array.Empty<GameObject>();
            EditorUtility.SetDirty(library);

            WriteAuditReport(library);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"CITYLAB_VENDOR_OK houses={houses.Length} trees={trees.Length} character={character.name}");
        }

        [MenuItem("Victoria/CityLab/Audit Vendor Assets")]
        public static void Audit()
        {
            WriteAuditReport(AssetDatabase.LoadAssetAtPath<CityVisualLibrary>(LibraryPath));
            AssetDatabase.Refresh();
            Debug.Log("CITYLAB_VENDOR_AUDIT_OK report=Docs/VENDOR_AUDIT.md");
        }

        static GameObject CreateNormalizedPrefab(string sourcePath, string label, float targetHeight,
            float targetFootprint, float postScale, bool stripScripts)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null)
                throw new InvalidOperationException("Vendor prefab missing: " + sourcePath);

            var root = new GameObject(label);
            try
            {
                var child = (GameObject)PrefabUtility.InstantiatePrefab(source);
                child.name = "Visual";
                child.transform.SetParent(root.transform, true);
                PrefabUtility.UnpackPrefabInstance(child, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                if (stripScripts)
                {
                    foreach (var behaviour in child.GetComponentsInChildren<MonoBehaviour>(true))
                        UnityEngine.Object.DestroyImmediate(behaviour);
                }
                foreach (var collider in child.GetComponentsInChildren<Collider>(true))
                    UnityEngine.Object.DestroyImmediate(collider);
                ConvertMaterialsToUrp(child);

                var bounds = CalculateBounds(child);
                var heightScale = bounds.size.y > 0.001f ? targetHeight / bounds.size.y : 1f;
                var horizontal = Mathf.Max(bounds.size.x, bounds.size.z);
                var footprintScale = horizontal > 0.001f ? targetFootprint / horizontal : 1f;
                child.transform.localScale *= Mathf.Min(heightScale, footprintScale);
                bounds = CalculateBounds(child);
                child.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                child.transform.localScale *= postScale;
                child.transform.localPosition *= postScale;

                var path = PrefabRoot + "/" + label.Replace(' ', '_') + ".prefab";
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        static void ConvertMaterialsToUrp(GameObject root)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new InvalidOperationException("URP Lit shader missing during Vendor conversion.");

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var source = materials[i];
                    if (source == null)
                        continue;
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out var guid, out long localId);
                    var assetPath = $"{MaterialRoot}/{guid}_{localId}.mat";
                    var adapted = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                    if (adapted == null)
                    {
                        adapted = new Material(shader) { name = "CityLab URP " + source.name, enableInstancing = true };
                        var texture = source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") :
                            source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
                        var color = source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") :
                            source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white;
                        adapted.SetTexture("_BaseMap", texture);
                        adapted.SetColor("_BaseColor", color);
                        if (source.HasProperty("_BumpMap"))
                        {
                            var normal = source.GetTexture("_BumpMap");
                            if (normal != null)
                            {
                                adapted.SetTexture("_BumpMap", normal);
                                adapted.EnableKeyword("_NORMALMAP");
                            }
                        }
                        AssetDatabase.CreateAsset(adapted, assetPath);
                    }
                    materials[i] = adapted;
                }
                renderer.sharedMaterials = materials;
            }
        }

        static Bounds CalculateBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(root.transform.position, Vector3.one);
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        static AnimatorController CreateAnimatorController()
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Working", AnimatorControllerParameterType.Bool);
            var machine = controller.layers[0].stateMachine;
            var idle = machine.AddState("Idle");
            var walk = machine.AddState("Walk");
            var work = machine.AddState("Work");
            idle.motion = LoadClip(IdleSource);
            walk.motion = LoadClip(WalkSource);
            work.motion = idle.motion;
            machine.defaultState = idle;

            AddTransition(idle, walk, "Speed", AnimatorConditionMode.Greater, 0.1f);
            AddTransition(walk, idle, "Speed", AnimatorConditionMode.Less, 0.1f);
            var toWork = machine.AddAnyStateTransition(work);
            toWork.hasExitTime = false;
            toWork.duration = 0.12f;
            toWork.AddCondition(AnimatorConditionMode.If, 0f, "Working");
            AddTransition(work, idle, "Working", AnimatorConditionMode.IfNot, 0f);
            return controller;
        }

        static void AddTransition(AnimatorState from, AnimatorState to, string parameter,
            AnimatorConditionMode mode, float threshold)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.12f;
            transition.AddCondition(mode, threshold, parameter);
        }

        static AnimationClip LoadClip(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                .FirstOrDefault(clip => !clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase));
        }

        static void WriteAuditReport(CityVisualLibrary library)
        {
            var report = new StringBuilder();
            report.AppendLine("# Audit des assets Vendor — CityLab");
            report.AppendLine();
            report.AppendLine("Généré par l'outil d'admission CityLab. Les sources restent intactes dans leurs dossiers Unity Store ; seuls des prefabs adaptés sont utilisés par le prototype.");
            report.AppendLine();
            report.AppendLine("| Pack | Fichiers | Prefabs | Modèles | Scripts | Shaders | Décision |");
            report.AppendLine("|---|---:|---:|---:|---:|---:|---|");
            foreach (var root in VendorRoots)
            {
                var guids = AssetDatabase.FindAssets(string.Empty, new[] { root });
                var paths = guids.Select(AssetDatabase.GUIDToAssetPath).Distinct().ToArray();
                var scripts = paths.Count(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
                var shaders = paths.Count(path => path.EndsWith(".shader", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase));
                var prefabs = paths.Count(path => path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase));
                var models = paths.Count(path => path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase));
                var decision = root.EndsWith("DoubleL") ? "Réserve animation (non activé dans le slice)" : "Admis via variante CityLab";
                report.AppendLine($"| `{root}` | {paths.Length} | {prefabs} | {models} | {scripts} | {shaders} | {decision} |");
            }
            report.AppendLine();
            report.AppendLine("## Sélection active");
            report.AppendLine();
            report.AppendLine($"- Catalogue runtime valide : **{(library != null && library.HasDurableSlice ? "oui" : "non")}**.");
            report.AppendLine("- EmaceArt : trois maisons composites, un bâtiment central et un tas de bois.");
            report.AppendLine("- GanzSe : personnage complet copié, normalisé à 1,75 m et débarrassé des scripts de démonstration.");
            report.AppendLine("- Kevin Iglesias : idle et marche Humanoid sans root motion pilotés par CityLab.");
            report.AppendLine("- Polytope : deux arbres normalisés, distribués de façon déterministe en périphérie.");
            report.AppendLine("- DoubleL : pack conservé pour une future action de chantier ; aucun asset DoubleL n'est requis par le slice actuel.");
            report.AppendLine();
            report.AppendLine("## Risques et garde-fous");
            report.AppendLine();
            report.AppendLine("- Les shaders Vendor ne sont jamais chargés par le code métier ; leur rendu URP est validé dans le build CityLab.");
            report.AppendLine("- Les scripts de démo ne sont pas utilisés. Le contrôleur GanzSe est isolé Editor-only car son fichier importe `UnityEditor`.");
            report.AppendLine("- Les colliders Vendor sont supprimés des variantes visuelles afin de ne pas perturber les routes, le NavMesh ou la sélection.");
            report.AppendLine("- Toute publication du dépôt contenant les sources Unity Store doit rester privée et respecter l'EULA Unity Asset Store.");
            File.WriteAllText("Docs/VENDOR_AUDIT.md", report.ToString(), Encoding.UTF8);
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
