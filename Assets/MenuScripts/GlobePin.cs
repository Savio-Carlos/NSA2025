using UnityEngine;

public class PinFaceCamera : MonoBehaviour
{
    public Transform cameraTransform;  // arraste a câmera aqui no inspetor

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Faz o pin olhar pra câmera
        transform.LookAt(cameraTransform);
        transform.Rotate(0, 180, 0); // caso o sprite fique "de costas"
    }
}
