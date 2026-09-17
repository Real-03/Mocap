using UnityEngine;
using TMPro;

/// <summary>
/// Mostra no ecrã a percentagem de erro (corpo em colisão com o obstáculo) do
/// MirrorWallDetector atualmente ativo.
///
/// Como as paredes são geradas dinamicamente, o detector associado pode mudar ao
/// longo do jogo — por isso a leitura é feita a cada frame (é só ler uma
/// propriedade já calculada, não recalcula nada), e o detector pode ser trocado
/// a qualquer momento com SetDetector(), por exemplo quando uma parede nova nasce.
/// </summary>
public class MirrorChallengeResultUI : MonoBehaviour
{
    [Header("Detector")]
    [Tooltip("Pode ficar vazio no início e ser atribuído em runtime via SetDetector() quando a primeira parede for criada.")]
    [SerializeField] private MirrorWallDetector detector;

    [Header("UI")]
    [Tooltip("Texto atualizado continuamente com a percentagem de erro atual.")]
    [SerializeField] private TMP_Text percentageText;
    [Tooltip("Formato do texto. {0} = percentagem. Ex.: \"{0:F0}% de erro\".")]
    [SerializeField] private string percentageFormat = "{0:F0}% de erro";

    [Tooltip("Texto opcional onde aparece PASSOU / FALHOU quando TriggerFinalCheck() é chamado.")]
    [SerializeField] private TMP_Text finalResultText;

    [Header("Cores")]
    [SerializeField] private Color passColor = Color.green;
    [SerializeField] private Color failColor = Color.red;

    /// <summary>
    /// Troca o detector cuja percentagem está a ser mostrada — chamar isto sempre
    /// que uma nova parede/obstáculo se torna o desafio atual do jogador.
    /// </summary>
    public void SetDetector(MirrorWallDetector newDetector)
    {
        detector = newDetector;

        if (finalResultText != null)
        {
            finalResultText.text = string.Empty;
        }
    }

    private void Update()
    {
        if (detector == null || percentageText == null) return;

        percentageText.text = string.Format(percentageFormat, detector.CurrentCollidingPercentage);
        percentageText.color = detector.LastCheckPassed ? passColor : failColor;
    }

    /// <summary>
    /// Chamar isto no momento exato em que queres o veredito final — por exemplo:
    /// - quando um cronómetro do desafio chega a zero
    /// - quando a parede (a mover-se em direção ao jogador) chega à posição do jogador
    /// - ao carregar num botão de "Tentar"
    /// </summary>
    public void TriggerFinalCheck()
    {
        if (detector == null) return;

        bool passed = detector.CheckResult();

        if (finalResultText != null)
        {
            finalResultText.text = passed ? "PASSOU" : "FALHOU";
            finalResultText.color = passed ? passColor : failColor;
        }
    }
}