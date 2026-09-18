using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DefesaCivil.NPC.EditorTools
{
    public static class DCNpcAnimator
    {
        [MenuItem("SafeZone VR/NPC/Criar Animator Controller")]
        public static void Criar()
        {
            var fbx = AssetDatabase.FindAssets("Carlos t:Model")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => p.Replace('\\', '/').Contains("/NPC/"));
            if (fbx == null)
            {
                Debug.LogError("[NPC] Carlos.fbx nao encontrado dentro de uma pasta NPC/.");
                return;
            }
            var clipes = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToDictionary(c => c.name);
            foreach (var n in new[] { "Idle", "Apontar", "Andar" })
                if (!clipes.ContainsKey(n))
                {
                    Debug.LogError($"[NPC] clipe {n} nao encontrado em {fbx}. Reimporte o FBX.");
                    return;
                }

            var destino = Path.GetDirectoryName(fbx).Replace('\\', '/') + "/DC_NPC.controller";
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(destino);
            ctrl.AddParameter("Andando", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Apontar", AnimatorControllerParameterType.Trigger);
            var sm = ctrl.layers[0].stateMachine;
            var idle = sm.AddState("Idle");
            idle.motion = clipes["Idle"];
            var andar = sm.AddState("Andar");
            andar.motion = clipes["Andar"];
            var apontar = sm.AddState("Apontar");
            apontar.motion = clipes["Apontar"];
            sm.defaultState = idle;

            var t = idle.AddTransition(andar);
            t.AddCondition(AnimatorConditionMode.If, 0, "Andando");
            t.hasExitTime = false; t.duration = 0.2f;
            t = andar.AddTransition(idle);
            t.AddCondition(AnimatorConditionMode.IfNot, 0, "Andando");
            t.hasExitTime = false; t.duration = 0.2f;
            var qualquer = sm.AddAnyStateTransition(apontar);
            qualquer.AddCondition(AnimatorConditionMode.If, 0, "Apontar");
            qualquer.hasExitTime = false; qualquer.duration = 0.15f; qualquer.canTransitionToSelf = false;
            t = apontar.AddTransition(idle);
            t.hasExitTime = true; t.exitTime = 0.95f; t.duration = 0.2f;

            AssetDatabase.SaveAssets();
            Debug.Log($"[NPC] controller criado: {destino}");
        }
    }
}
