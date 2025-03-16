using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class SmoothWavyTextEffect : MonoBehaviour
{
    [Header("Références")]
    public TMP_Text textMeshPro;

     [Header("Effet d'Apparition")]
     [SerializeField] private float letterFadeDuration = 0.3f; // Durée du fade-in
     [SerializeField] private float letterScaleFactor = 1.2f; // Grossissement max
     [SerializeField] private float letterMoveDistance = 10f; // Distance de montée
     [SerializeField] private float letterAppearDelay = 0.05f; // Délai entre chaque lettre

    [Header("Effet Wavy")]
    [SerializeField] private float amplitude = 5f; // Hauteur du mouvement ondulatoire
    [SerializeField] private float frequency = 2f; // Fréquence de l'onde
    [SerializeField] private float speed = 1f; // Vitesse de l'animation

    private TMP_TextInfo textInfo;
    private Vector3[][] originalVertices;
    private bool animationStarted = false;

    void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TMP_Text>();

        textMeshPro.ForceMeshUpdate();
        StartCoroutine(AnimateWavyText());
    }

    IEnumerator AppearText()
    {
        textMeshPro.ForceMeshUpdate();
        textInfo = textMeshPro.textInfo;
        textMeshPro.alpha = 0; // Rendre le texte invisible au début

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Récupération de la position initiale de la lettre
            Vector3 letterCenter = (vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2;

            // Initialiser chaque lettre avec une montée et une échelle plus petite
            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] += Vector3.up * letterMoveDistance; // Déplacement initial vers le haut
                vertices[vertexIndex + j] = letterCenter + (vertices[vertexIndex + j] - letterCenter) * 0.5f; // Réduction initiale
            }

            textMeshPro.mesh.vertices = textInfo.meshInfo[materialIndex].vertices;
            textMeshPro.UpdateGeometry(textInfo.meshInfo[materialIndex].mesh, materialIndex);

            // Effet de fade-in
            textMeshPro.DOFade(1, letterFadeDuration).SetDelay(i * letterAppearDelay);

            // Animation de montée et retour à la position d'origine
            for (int j = 0; j < 4; j++)
            {
                Vector3 startPos = vertices[vertexIndex + j];
                Vector3 endPos = startPos - Vector3.up * letterMoveDistance;

                DOTween.To(() => startPos, x =>
                {
                    vertices[vertexIndex + j] = x;
                    textMeshPro.mesh.vertices = textInfo.meshInfo[materialIndex].vertices;
                    textMeshPro.UpdateGeometry(textInfo.meshInfo[materialIndex].mesh, materialIndex);
                }, endPos, letterFadeDuration).SetEase(Ease.OutQuad).SetDelay(i * letterAppearDelay);
            }

            // Animation de scale-up
            DOTween.To(() => 0.5f, scale =>
            {
                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = letterCenter + (vertices[vertexIndex + j] - letterCenter) * scale;
                }
                textMeshPro.mesh.vertices = textInfo.meshInfo[materialIndex].vertices;
                textMeshPro.UpdateGeometry(textInfo.meshInfo[materialIndex].mesh, materialIndex);
            }, letterScaleFactor, letterFadeDuration).SetEase(Ease.OutBack).SetDelay(i * letterAppearDelay);

            yield return new WaitForSeconds(letterAppearDelay);
        }

        yield return new WaitForSeconds(0.5f); // Pause avant l'effet wavy
        StartCoroutine(AnimateWavyText());
    }

    IEnumerator AnimateWavyText()
    {
        if (animationStarted) yield break;
        animationStarted = true;

        while (true)
        {
            textMeshPro.ForceMeshUpdate();
            textInfo = textMeshPro.textInfo;

            if (originalVertices == null || originalVertices.Length < textInfo.meshInfo.Length)
                StoreOriginalVertices();

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                // Récupérer la position de base
                Vector3[] originalVerts = originalVertices[materialIndex];

                // Calcul du mouvement sinusoïdal
                float waveOffset = Mathf.Sin((Time.time * speed) + (i * frequency)) * amplitude;
                Vector3 waveMotion = new Vector3(0, waveOffset, 0);

                // Appliquer le mouvement sur chaque sommet de la lettre
                vertices[vertexIndex] = originalVerts[vertexIndex] + waveMotion;
                vertices[vertexIndex + 1] = originalVerts[vertexIndex + 1] + waveMotion;
                vertices[vertexIndex + 2] = originalVerts[vertexIndex + 2] + waveMotion;
                vertices[vertexIndex + 3] = originalVerts[vertexIndex + 3] + waveMotion;
            }

            // Appliquer les modifications sur le texte
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textMeshPro.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return null;
        }
    }

    private void StoreOriginalVertices()
    {
        textMeshPro.ForceMeshUpdate();
        textInfo = textMeshPro.textInfo;
        originalVertices = new Vector3[textInfo.meshInfo.Length][];

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            Vector3[] vertices = textInfo.meshInfo[i].vertices;
            originalVertices[i] = new Vector3[vertices.Length];
            for (int j = 0; j < vertices.Length; j++)
            {
                originalVertices[i][j] = vertices[j];
            }
        }
    }
}
