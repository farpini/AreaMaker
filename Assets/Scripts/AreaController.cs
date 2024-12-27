using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AreaController : MonoBehaviour
{
    [SerializeField] private Transform groundMap;
    [SerializeField] private MapSO mapSettings;
    [SerializeField] private Transform verticePointPrefab;
    [SerializeField] private Button createAreaButton;
    [SerializeField] private Button unionAreaButton;
    [SerializeField] private Button subtractAreaButton;
    [SerializeField] private GameObject areaOperationPanel;
    [SerializeField] private Area areaPrefab;
    [SerializeField] private TextMeshProUGUI createAreaText;
    [SerializeField] private StateController state;
    [SerializeField] private ColorSelection fillColorSelection;
    [SerializeField] private WidthSelection fillStripesWidthSelection;
    [SerializeField] private SpeedSelection fillStripesSpeedSelection;
    [SerializeField] private ColorSelection contourColorSelection;
    [SerializeField] private WidthSelection contourWidthSelection;

    [SerializeField] private Vector2 worldPosition;
    [SerializeField] private Vector2 worldPositionRounded;

    private Transform verticesHolder;
    private Plane groundPlane;
    private List<Transform> areaVertices;
    private List<Vector2Int> fixedVertices;
    private Area area;

    private Vector2 INVALID_FLOAT2;

    private const int PRECISION = 100;
    private const float MAGNET_RANGE = 1f;
    private const float ANGLE_NARROW_LIMIT = 30f;


    void Awake ()
    {
        groundMap.localScale = new Vector3(mapSettings.mapDimension.x, 0.1f, mapSettings.mapDimension.y);
        groundMap.position = new Vector3(mapSettings.mapDimension.x * 0.5f, 0.1f, mapSettings.mapDimension.y * 0.5f);

        groundPlane = new Plane(Vector3.up, Vector3.zero);
        worldPosition = Vector2.zero;
        worldPositionRounded = Vector2.zero;

        INVALID_FLOAT2 = new Vector2(-1f, -1f);

        var verticesGameObject = new GameObject("Vertices");
        verticesHolder = verticesGameObject.transform;

        areaVertices = new();
        fixedVertices = new();

        area = null;

        createAreaButton.onClick.AddListener(() => OnCreateAreaButtonClicked());
        unionAreaButton.onClick.AddListener(() => OnUnionAreaButtonClicked());
        subtractAreaButton.onClick.AddListener(() => OnSubtractAreaButtonClicked());
    }

    void OnDestroy ()
    {
        createAreaButton.onClick.RemoveAllListeners();
        unionAreaButton.onClick.RemoveAllListeners();
        subtractAreaButton.onClick.RemoveAllListeners();
    }

    void Start()
    {
        state = StateController.None;

        areaOperationPanel.SetActive(false);

        if (area == null)
        {
            area = Instantiate(areaPrefab, new Vector3(0f, 0.21f, 0f), Quaternion.identity);
            fillColorSelection.Initialize(area.FillRenderer);
            fillStripesWidthSelection.Initialize(area.FillRenderer);
            fillStripesSpeedSelection.Initialize(area.FillRenderer);
            contourColorSelection.Initialize(area.ContourRenderer);
            contourWidthSelection.Initialize(area.ContourRenderer);
        }
    }

    void Update()
    {
        GetWorldPosition();
        CheckKeyboardEvents();
        CheckMouseEvents();
        DrawAreaVertices();
        DrawAreaCountour();
    }

    private void GetWorldPosition ()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var intersectPoint = math.up();

        if (groundPlane.Raycast(ray, out float distance))
        {
            intersectPoint = ray.GetPoint(distance);
        }

        if (intersectPoint.y < 1f && intersectPoint.x >= 0f && intersectPoint.x < mapSettings.mapDimension.x &&
            intersectPoint.z >= 0f && intersectPoint.z < mapSettings.mapDimension.y)
        {
            worldPosition = new Vector2(intersectPoint.x, intersectPoint.z);
            worldPositionRounded = WorldPositionRounded(worldPosition);
        }
        else
        {
            worldPosition = INVALID_FLOAT2;
        }
    }

    private void CheckKeyboardEvents ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state == StateController.CreatingArea || state == StateController.AreaCreated)
            {
                ClearCurrentArea();
            }
        }
    }

    private void CheckMouseEvents ()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            CheckVerticePlacement();
        }
    }

    private void OnCreateAreaButtonClicked ()
    {
        if (state == StateController.CreatingArea || state == StateController.AreaCreated)
        {
            ClearCurrentArea();
            return;
        }

        if (state != StateController.None)
        {
            return;
        }

        SetupNewArea();
    }

    private void OnUnionAreaButtonClicked ()
    {
        if (state != StateController.AreaCreated)
        {
            return;
        }

        if (area != null)
        {
            area.Union(fixedVertices, PRECISION);
        }

        ClearCurrentArea();
    }

    private void OnSubtractAreaButtonClicked ()
    {
        if (state != StateController.AreaCreated)
        {
            return;
        }

        if (area != null)
        {
            area.Subtract(fixedVertices, PRECISION);
        }

        ClearCurrentArea();
    }

    private bool CheckVerticePlacement ()
    {
        if (state != StateController.CreatingArea)
        {
            return false;
        }

        if (areaVertices.Count <= 0)
        {
            return false;
        }

        if (worldPosition == INVALID_FLOAT2)
        {
            return false;
        }

        CreateNewVertice();

        //var lastVertice = areaVertices[areaVertices.Count - 1];

        return true;
    }

    private void ClearCurrentArea ()
    {
        foreach (var vertice in areaVertices)
        {
            Object.Destroy(vertice.gameObject);
        }

        createAreaText.text = "Create an Area";

        areaOperationPanel.SetActive(false);

        areaVertices.Clear();
        fixedVertices.Clear();

        state = StateController.None;
    }

    private void CreateArea ()
    {
        areaOperationPanel.SetActive(true);

        state = StateController.AreaCreated;
    }

    private void CreateNewVertice ()
    {
        if (state != StateController.CreatingArea)
        {
            return;
        }

        if (areaVertices.Count > 0)
        {
            var lastVertice = areaVertices[areaVertices.Count - 1];
            var roundVertice = RoundVertice(lastVertice.position);

            if (areaVertices.Count > 2)
            {
                var penulVertice = RoundVertice(areaVertices[areaVertices.Count - 2].position);
                var anpenulVertice = RoundVertice(areaVertices[areaVertices.Count - 3].position);

                if (Vector2.Angle(penulVertice - roundVertice, penulVertice - anpenulVertice) < ANGLE_NARROW_LIMIT)
                {
                    Debug.LogWarning("NARROW ANGLE");
                    return;
                }
            }

            if (areaVertices.Count > 1)
            {
                var penulVertice = RoundVertice(areaVertices[areaVertices.Count - 2].position);

                if (CheckIntersection(roundVertice, penulVertice))
                {
                    if (roundVertice == RoundVertice(areaVertices[0].position))
                    {
                        CreateArea();
                        return;
                    }

                    Debug.LogWarning("SELF-INTERSECTION");
                    return;
                }
            }
            
            fixedVertices.Add(roundVertice);
        }

        areaVertices.Add(Instantiate(verticePointPrefab, verticesHolder));
    }

    private void SetupNewArea ()
    {
        ClearCurrentArea();

        state = StateController.CreatingArea;
        createAreaText.text = "Cancel Area creation";

        CreateNewVertice();
    }

    private void DrawAreaVertices ()
    {
        if (state != StateController.CreatingArea)
        {
            return;
        }

        if (areaVertices.Count <= 0)
        {
            return;
        }

        var lastVertice = areaVertices[areaVertices.Count - 1];
        var firstVerticePosition = areaVertices[0].position;

        if (Mathf.Abs(worldPositionRounded.x - firstVerticePosition.x) < MAGNET_RANGE &&
            Mathf.Abs(worldPositionRounded.y - firstVerticePosition.z) < MAGNET_RANGE)
        {
            lastVertice.position = firstVerticePosition;
        }
        else
        {
            lastVertice.position = new Vector3(worldPositionRounded.x, 0.21f, worldPositionRounded.y);
        }
    }

    private void DrawAreaCountour ()
    {
        if (state == StateController.CreatingArea || state == StateController.AreaCreated)
        {
            for (int i = 1; i < areaVertices.Count; i++)
            {
                var linePosition = areaVertices[i].position;

                var linePositionPrevious = areaVertices[i - 1].position;

                Debug.DrawLine(linePosition, linePositionPrevious, Color.white);
            }
        }
    }

    private Vector2 WorldPositionRounded (Vector2 point)
    {
        return new Vector2(Mathf.Round(point.x * PRECISION) / PRECISION, Mathf.Round(point.y * PRECISION) / PRECISION);
    }

    private Vector2Int RoundVertice (Vector3 vertice)
    {
        return new Vector2Int(Mathf.RoundToInt(vertice.x * PRECISION), Mathf.RoundToInt(vertice.z * PRECISION));
    }

    private bool CheckIntersection (Vector2Int p1, Vector2Int p2)
    {
        for (int i = 1; i < areaVertices.Count - 1; i++)
        {
            var pA = (Vector2)RoundVertice(areaVertices[i - 1].position);
            var pB = (Vector2)RoundVertice(areaVertices[i].position);
            if (MathAux.IntersectLineSegments2D((Vector2)p1, (Vector2)p2, pA, pB, out var intersection))
            {
                if ((Vector2)intersection == (Vector2)p2)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}

public enum StateController
{
    None = 0,
    CreatingArea = 1,
    AreaCreated = 2
}