using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class RegionMarkerBillboard : MonoBehaviour
{
    [SerializeField] private bool lockToWorldUp = true;

    private Camera targetCamera;
    private RegionLoader regionLoader;

    public void Initialize(RegionLoader loader)
    {
        regionLoader = loader;
        AcquireCamera();
    }

    private void Awake()
    {
        AcquireCamera();
    }

    private void OnEnable()
    {
        AcquireCamera();
    }

    private void LateUpdate()
    {
        if (!targetCamera || !targetCamera.isActiveAndEnabled)
        {
            AcquireCamera();
        }

        if (!targetCamera)
        {
            return;
        }

        var cameraPosition = targetCamera.transform.position;
        var direction = cameraPosition - transform.position;

        if (lockToWorldUp)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 1e-6f)
        {
            return;
        }

        if (lockToWorldUp)
        {
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
        else
        {
            transform.forward = direction.normalized;
        }
    }

    private void AcquireCamera()
    {
        if (!regionLoader)
        {
            regionLoader = GetComponentInParent<RegionLoader>();
        }

        var camera = Camera.main;
        if (!camera || !camera.isActiveAndEnabled)
        {
            camera = Camera.current;
        }

        if ((!camera || !camera.isActiveAndEnabled) && regionLoader && regionLoader.Terrain)
        {
            camera = regionLoader.Terrain.GetComponentInChildren<Camera>();
        }

        if ((!camera || !camera.isActiveAndEnabled) && Camera.allCamerasCount > 0)
        {
            camera = Camera.allCameras[0];
        }

#if UNITY_EDITOR
        if ((!camera || !camera.isActiveAndEnabled) && !Application.isPlaying)
        {
            if (SceneView.lastActiveSceneView != null)
            {
                camera = SceneView.lastActiveSceneView.camera;
            }
        }
#endif

        targetCamera = camera;
    }
}
