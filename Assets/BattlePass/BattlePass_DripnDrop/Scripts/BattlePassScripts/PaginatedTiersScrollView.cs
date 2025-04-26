using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EasyBattlePass
{
    public class PaginatedTiersScrollView : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform[] sections;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private float scrollSpeed = 2f;

        private float sectionSize;
        private Coroutine currentCoroutine;
        private bool isScrolling;

        [SerializeField] private bool isHorizontal;
        [SerializeField] private bool isVertical;

        private void Awake()
        {
            InitializeSections();
        }

        private void InitializeSections()
        {
            if (sections.Length > 1 && isHorizontal)
            {
                sectionSize = Mathf.Abs(sections[1].anchoredPosition.x - sections[0].anchoredPosition.x);
            }

            if (sections.Length > 1 && isVertical)
            {
                sectionSize = Mathf.Abs(sections[1].anchoredPosition.y - sections[0].anchoredPosition.y);
            }
        }

        public void ScrollToSection(int index)
        {
            if (index >= 0 && index < sections.Length && !isScrolling)
            {
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }
                scrollRect.enabled = false;  // Disable ScrollRect user interaction
                if (isVertical)
                    StartCoroutine(SmoothScrollTo(index - 7));
                else if (isHorizontal)
                    StartCoroutine(SmoothScrollTo(index));
            }
        }

        private IEnumerator SmoothScrollTo(int index)
        {
            isScrolling = true;
            float targetPosition = index * sectionSize;
            float targetPositionVert = (sections.Length - index - 1) * sectionSize;
            float elapsedTime = 0;
            float durationX = Mathf.Abs(targetPosition + content.anchoredPosition.x) / sectionSize / scrollSpeed;
            float durationY = Mathf.Abs(targetPosition + content.anchoredPosition.y) / sectionSize / scrollSpeed;
            float marginOfError = 0.1f * sectionSize; // Define your own margin of error

            if (isHorizontal)
            {
                while (elapsedTime < durationX)
                {
                    float newX = Mathf.Lerp(content.anchoredPosition.x, -targetPosition, elapsedTime / durationX);
                    content.anchoredPosition = new Vector2(newX, content.anchoredPosition.y);

                    if (Mathf.Abs(content.anchoredPosition.x - (-targetPosition)) < marginOfError)
                    {
                        break;
                    }

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            if (isVertical)
            {
                while (elapsedTime < durationY)
                {
                    {
                        float newY = Mathf.Lerp(content.anchoredPosition.y, targetPosition, elapsedTime / durationY);
                        content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);

                        if (Mathf.Abs(content.anchoredPosition.y - targetPosition) < marginOfError)
                        {
                            break;
                        }

                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }
                }
            }
            

            // Snap content directly to target position
            if(isHorizontal)
                content.anchoredPosition = new Vector2(-targetPosition, content.anchoredPosition.y);

            if (isVertical)
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetPosition);

            isScrolling = false;
            scrollRect.enabled = true;  // Re-enable ScrollRect user interaction
            currentCoroutine = null;
        }

        public void OnUserInteraction()
        {
            if (isScrolling && currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                isScrolling = false;
                currentCoroutine = null;
            }
        }
    }
}



