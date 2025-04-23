using UnityEngine;

namespace CloudFine
{
    public class Orbit : MonoBehaviour
    {
        [Header("Reference cible")]
        [Tooltip("Cible de l'orbite")]
        public Transform orbitTarget;

        [Header("Paramètres de l'orbite")]
        [Tooltip("Distance de l'orbite")]
        public float distance = 10f;
        [Tooltip("Vitesse de rotation")]
        public float speed = 30f; // en degrés par seconde
        [Tooltip("Angle de départ")]
        [Range(0f, 360f)]
        public float startAngle = 0f; // Angle de départ horizontal en degrés
        [Tooltip("Inclinaison verticale (en degrés)")]
        [Range(-89f, 89f)]
        public float verticalAngle = 45f; // Inclinaison verticale

        private float currentAngle; // Angle courant en degrés

        void Start()
        {
            // Initialisation de l'angle de départ
            currentAngle = startAngle;
            UpdatePosition();
        }

        void Update()
        {
            // On incrémente l'angle en fonction du temps et de la vitesse
            currentAngle += speed * Time.deltaTime;
            currentAngle %= 360f; // Pour rester entre 0 et 360
            UpdatePosition();
        }

        void UpdatePosition()
        {
            if (!orbitTarget) return;

            Vector3 target = orbitTarget.position;

            // Calcul de la position orbitale
            float radians = currentAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(radians) * distance;
            float z = Mathf.Sin(radians) * distance;

            // Calcul de la hauteur en fonction de l'angle vertical
            float y = Mathf.Tan(verticalAngle * Mathf.Deg2Rad) * distance;

            transform.position = target + new Vector3(x, y, z);
            transform.LookAt(target);
        }
    }
}
