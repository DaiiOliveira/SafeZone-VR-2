using UnityEngine;

namespace DefesaCivil.Casa
{
    public class DCInteracao : MonoBehaviour
    {
        [Tooltip("Identificador da interacao, vindo do Blender (dc_interacao).")]
        public string id;

        [Tooltip("Estado inicial, vindo do Blender (dc_estado). Ex.: ligado.")]
        public string estado;
    }
}
