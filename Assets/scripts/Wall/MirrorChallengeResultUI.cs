using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Liga o resultado do MirrorWallDetector a uma UI simples (UnityEngine.UI.Text).
///
/// Suporta os dois cenários mais comuns:
/// 1) Mostrar a percentagem/estado ao vivo, atualizado a cada checkInterval.
/// 2) Fazer uma verificação pontual num momento específico (ex.: quando um
///    cronómetro chega a zero, ou a parede chega ao jogador) e mostrar
///    PASSOU/FALHOU nesse instante.
/// </summary>
public class MirrorChallengeResultUI : MonoBehaviour
{
    [SerializeField] private MirrorWallDetector detector;

    [Header("UI (opcional, para o estado ao vivo)")]
    [Tooltip("Texto atualizado continuamente com a percentagem atual. Deixar vazio se não quiseres feedback ao vivo.")]
    [SerializeField] private Text livePercentageText;

    [Header("UI (para a verificação pontual)")]
    [Tooltip("Texto onde aparece PASSOU / FALHOU quando TriggerFinalCheck() é chamado.")]
    [SerializeField] private Text finalResultText;

    private void OnEnable()
    {
        // Atualiza a UI ao vivo sempre que o detector recalcula a percentagem
        // (a cada checkInterval, não por frame — por isso é barato).
        detector.OnEvaluated += HandleLiveUpdate;
    }

    private void OnDisable()
    {
        detector.OnEvaluated -= HandleLiveUpdate;
    }

    private void HandleLiveUpdate(bool passed, float percentage)
    {
        if (livePercentageText == null) return;
        livePercentageText.text = $"{percentage:F0}% de erro";
        livePercentageText.color = passed ? Color.green : Color.red;
    }

    /// <summary>
    /// Chamar isto no momento exato em que queres o veredito final — por exemplo:
    /// - quando um cronómetro do desafio chega a zero
    /// - quando a parede (a mover-se em direção ao jogador) chega à posição do jogador
    /// - ao carregar num botão de "Tentar"
    ///
    /// Liga isto a esse evento (Invoke de um Timer, OnTriggerEnter de uma zona,
    /// OnClick de um Button, etc.) em vez de chamar isto por frame.
    /// </summary>
    public void TriggerFinalCheck()
    {
        bool passed = detector.CheckResult();

        if (finalResultText != null)
        {
            finalResultText.text = passed ? "PASSOU" : "FALHOU";
            finalResultText.color = passed ? Color.green : Color.red;
        }

        Debug.Log(passed
            ? $"Desafio superado! Erro: {detector.CurrentCollidingPercentage:F1}%"
            : $"Desafio falhado. Erro: {detector.CurrentCollidingPercentage:F1}% (limite: 10%)");
    }
}