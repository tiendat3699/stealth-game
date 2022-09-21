using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class PlayerScanner
{
    public Material materialFieldOfView;
    [SerializeField] private string TargetTag;
    private Mesh mesh;
    private MeshFilter meshFilterFOV;
    private float fov;
    private float ViewDistence;
    public UnityEvent<Transform> OnDetectedTarget;
    public UnityEvent OnLostTarget;
    
    public GameObject CreataFieldOfView(Transform detector, Vector3 pos) {
        //creata field of view
        mesh = new Mesh();
        GameObject FieldOfView = new GameObject("FieldOfView");
        FieldOfView.transform.SetParent(detector);
        FieldOfView.transform.position = pos;
        FieldOfView.transform.rotation = detector.rotation;
        MeshRenderer meshRendererFOV = FieldOfView.AddComponent<MeshRenderer>();
        meshFilterFOV =  FieldOfView.AddComponent<MeshFilter>();
        meshRendererFOV.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRendererFOV.material = materialFieldOfView;
        meshFilterFOV.mesh = mesh;
        return FieldOfView;
    }

    public void Scan(Transform detector) {
        int rayCount = 50;
        float angelIncrease = fov/rayCount;

        //init vetex, uv, triangle
        Vector3[] vertices = new Vector3[rayCount + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        float angel = -fov/2;
        vertices[0] = Vector3.zero;
        int triangleIndex = 0;

        //set vetex and triangle for mesh
        for(int i = 0; i<=rayCount; i++) {
            Vector3 dir = Quaternion.Euler(0, angel, 0) * Vector3.forward;
            Vector3 dirRaycast = Quaternion.Euler(0, angel, 0) * detector.forward.normalized;

            Ray origin = new Ray(detector.position, dirRaycast);
            Vector3 vertex = Vector3.zero +  (dir * ViewDistence);

            // scan object by raycast
            ScanTarget(origin, dir, ref vertex);
            
            vertices[i + 1] = vertex;
            if(i>0) {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = i;
                triangles[triangleIndex + 2] = i + 1;
                triangleIndex += 3;
            }

            angel += angelIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    public void SetFovAngel(float angel) {
        fov = angel;
    }

    public void SetViewDistence(float distance) {
        ViewDistence = distance;
    }


    private void ScanTarget(Ray origin, Vector3 dirFOV, ref Vector3 vertex) {
            RaycastHit[] hits = Physics.RaycastAll(origin, ViewDistence);
            float[] distance = new float[hits.Length];
            float distancePlayer = ViewDistence;
            Transform playerTransform = null;
            if(hits.Length > 0) {
                for(int i = 0; i<hits.Length; i++) {
                    RaycastHit hit = hits[i];
                    if(hit.transform.tag.Equals(TargetTag)) {
                        distance[i] = ViewDistence;
                        distancePlayer =  hit.distance;
                        playerTransform = hit.transform;
                    } else {
                        distance[i] = hit.distance;
                        OnLostTarget?.Invoke();
                    }
                }
                
                float mindistance =  Mathf.Min(distance);
                if(distancePlayer <= mindistance && playerTransform != null) {
                    OnDetectedTarget?.Invoke(playerTransform);
                }
                vertex = Vector3.zero +  (dirFOV * mindistance);
            } else {
                OnLostTarget?.Invoke();
            }
    }


#if UNITY_EDITOR
    public void EditorGizmo(Transform transform, float angle, float radius) {
        Handles.color = new Color32(76, 122, 90, 80);
        Vector3 vectorStart = Quaternion.Euler(0, -angle/2, 0) * transform.forward;
        Handles.DrawSolidArc(transform.position, Vector3.up, vectorStart, angle, radius);
    }
# endif
}