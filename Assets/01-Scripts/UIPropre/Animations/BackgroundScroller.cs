using UnityEngine;
using UnityEngine.UI;

public class BackgroundMover : MonoBehaviour
{
    [SerializeField] private RawImage _img;
    [SerializeField] private float _xSpeed = 0.1f, _ySpeed = 0.1f;

    void Update()
    {
        // Modification de l'offset pour déplacer la texture
        _img.uvRect = new Rect(
            _img.uvRect.x + _xSpeed * Time.deltaTime,
            _img.uvRect.y + _ySpeed * Time.deltaTime,
            _img.uvRect.width,
            _img.uvRect.height
        );
    }
}
