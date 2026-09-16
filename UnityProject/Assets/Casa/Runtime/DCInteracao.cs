using UnityEngine;

namespace DefesaCivil.Casa
{
    /// <summary>
    /// Marca um objeto da casa como interativo. Adicionado automaticamente
    /// pelo importador (Editor/DCCasaImportador.cs) a partir da propriedade
    /// customizada "dc_interacao" gravada no Blender.
    ///
    /// Valores que existem hoje:
    ///   disjuntor_geral   alavanca vermelha do quadro de luz (corredor)
    ///   porta_quadro_luz  portinhola do quadro de luz, origem na dobradica
    ///   detector_fumaca   detector no teto do corredor
    ///   painel_educativo    painel sob o pergolado da praca
    ///   totem_defesa_civil  totem na esquina da praca
    ///   abrigo_inadequado   arvores da praca (granizo: nao se abrigar)
    ///
    /// O script de gameplay procura por este componente em vez de depender
    /// do nome do objeto.
    /// </summary>
    public class DCInteracao : MonoBehaviour
    {
        [Tooltip("Identificador da interacao, vindo do Blender (dc_interacao).")]
        public string id;

        [Tooltip("Estado inicial, vindo do Blender (dc_estado). Ex.: ligado.")]
        public string estado;
    }
}
