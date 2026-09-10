#if UNITY_EDITOR
using UnityEditor;

namespace SafeZoneVR.Editor
{
    public static class SafeZoneBuildAll
    {
        [MenuItem("SafeZone VR/Build/0. Montar tudo (dados + fase + menu + configurações)", false, 0)]
        public static void BuildEverything()
        {
            SafeZoneDataBuilder.Build();
            AlagamentoGameplayBuilder.Build();
            MainMenuBuilder.Build();
            QuestProjectConfigurator.Apply();
            EditorUtility.DisplayDialog("SafeZone VR", "Tudo montado: dados, fase Alagamento, menu principal e configurações do projeto.", "OK");
        }
    }
}
#endif
