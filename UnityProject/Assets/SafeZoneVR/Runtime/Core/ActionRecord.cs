using System;

namespace SafeZoneVR
{
    public enum ActionKind
    {
        Acerto = 0,
        ErroLeve = 1,
        ErroGrave = 2
    }

    [Serializable]
    public class ActionRecord
    {
        public string id;
        public string label;
        public string advice;
        public float time;
        public ActionKind kind;
        public bool fatal;

        public bool isMistake => kind != ActionKind.Acerto;

        public string KindLabel()
        {
            switch (kind)
            {
                case ActionKind.ErroGrave: return "Erro grave";
                case ActionKind.ErroLeve: return "Erro leve";
                default: return "Acerto";
            }
        }
    }
}
