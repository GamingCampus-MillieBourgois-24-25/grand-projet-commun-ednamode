using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class SmoothWavyTextEffect : MonoBehaviour
{
    [Header("🎯 Comportement")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool enableWavyEffect = true;
    [SerializeField] private bool enableAppearEffect = true;

    [Header("🌀 Effet Wavy")]
    [SerializeField] private float amplitude = 5f;
    [SerializeField] private float frequency = 2f;
    [SerializeField] private float speed = 1f;

    [Header("✨ Apparition")]
    [SerializeField] private float letterFadeDuration = 0.3f;
    [SerializeField] private float letterScaleFactor = 1.2f;
    [SerializeField] private float letterMoveDistance = 10f;
    [SerializeField] private float letterAppearDelay = 0.05f;

    private TMP_Text textMeshPro;
    private TMP_TextInfo textInfo;
    private Vector3[][] originalVertices;
    private bool animationStarted;

    private void Awake()
    {
        textMeshPro = GetComponent<TMP_Text>();
        textMeshPro.ForceMeshUpdate();
    }

    private void OnEnable()
    {
        if (playOnStart)
            Play();
    }

    public void Play()
    {
        StopAllCoroutines();
        animationStarted = false;

        if (enableAppearEffect)
            StartCoroutine(AppearText());
        else if (enableWavyEffect)
            StartCoroutine(AnimateWavyText());
    }

    IEnumerator AppearText()
    {
        textMeshPro.ForceMeshUpdate();
        textInfo = textMeshPro.textInfo;
        textMeshPro.alpha = 0;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertIndex = charInfo.vertexIndex;
            var verts = textInfo.meshInfo[matIndex].vertices;

            Vector3 center = (verts[vertIndex] + verts[vertIndex + 2]) * 0.5f;

            for (int j = 0; j < 4; j++)
            {
                verts[vertIndex + j] += Vector3.up * letterMoveDistance;
                verts[vertIndex + j] = center + (verts[vertIndex + j] - center) * 0.5f;
            }

            textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            textMeshPro.DOFade(1f, letterFadeDuration).SetDelay(i * letterAppearDelay);

            for (int j = 0; j < 4; j++)
            {
                Vector3 start = verts[vertIndex + j];
                Vector3 end = start - Vector3.up * letterMoveDistance;

                DOTween.To(() => start, x =>
                {
                    verts[vertIndex + j] = x;
                    textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                }, end, letterFadeDuration).SetDelay(i * letterAppearDelay);
            }

            DOTween.To(() => 0.5f, scale =>
            {
                for (int j = 0; j < 4; j++)
                    verts[vertIndex + j] = center + (verts[vertIndex + j] - center) * scale;

                textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            }, letterScaleFactor, letterFadeDuration).SetEase(Ease.OutBack).SetDelay(i * letterAppearDelay);

            yield return new WaitForSeconds(letterAppearDelay);
        }

        yield return new WaitForSeconds(0.2f);
        if (enableWavyEffect)
            StartCoroutine(AnimateWavyText());
    }

    IEnumerator AnimateWavyText()
    {
        if (animationStarted) yield break;
        animationStarted = true;

        textMeshPro.ForceMeshUpdate();
        textInfo = textMeshPro.textInfo;
        StoreOriginalVertices();

        while (true)
        {
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int matIndex = charInfo.materialReferenceIndex;
                int vertIndex = charInfo.vertexIndex;

                Vector3[] verts = textInfo.meshInfo[matIndex].vertices;
                Vector3[] orig = originalVertices[matIndex];

                float wave = Mathf.Sin((Time.time * speed) + (i * frequency)) * amplitude;
                Vector3 offset = new Vector3(0, wave, 0);

                for (int j = 0; j < 4; j++)
                    verts[vertIndex + j] = orig[vertIndex + j] + offset;
            }

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
        originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            Vector3[] src = textInfo.meshInfo[i].vertices;
            originalVertices[i] = new Vector3[src.Length];
            src.CopyTo(originalVertices[i], 0);
        }
    }
}
