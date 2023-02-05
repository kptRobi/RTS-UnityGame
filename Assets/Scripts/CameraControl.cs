using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour {

    static CameraControl cameraControl;

    //zaznaczone jednostki
    List<ISelectable> selectedUnits = new List<ISelectable>();

    //Obsługa kamery
    //przesuwanie i zoom
    public float cameraSpeed, zoomSpeed, groundHeight;
    public Vector2 cameraHeightMinMax;
    public Vector2 cameraRotationMinMax;
    new Camera camera;
    Vector2 mousePos, mousePosScreen, keyboardInput, mouseScroll;
    bool isCursorInGameScreen;

    //parametry
    [Range(0, 1)]
    public float zoomLerp = .1f;
    [Range(0, 0.2f)]
    public float cursorTreshold;

    //zaznaczanie jednostek
    RectTransform selectionBox;
    Rect selectionRect, boxRect;

    BuildingPlacer placer;
    GameObject buildingPrefabToSpawn;

    private void Awake()
    {
        cameraControl = this;
        selectionBox = GetComponentInChildren<Image>(true).transform as RectTransform;
        camera = GetComponent<Camera>();
        selectionBox.gameObject.SetActive(false);
    }
    private void Start()
    {
        placer = GameObject.FindObjectOfType<BuildingPlacer>();
        placer.gameObject.SetActive(false);
    }


    private void Update()
    {
        UpdateMovement();
        UpdateZoom();
        UpdateClicks();
        UpdatePlacer();
    }


    void UpdateMovement()
    {
        keyboardInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        mousePos = Input.mousePosition;
        mousePosScreen = camera.ScreenToViewportPoint(mousePos);
        isCursorInGameScreen = mousePosScreen.x >= 0 && mousePosScreen.x <= 1 &&
            mousePosScreen.y >= 0 && mousePosScreen.y <= 1;

        Vector2 movementDirection = keyboardInput;

        //przesuwanie po krawedzi ekranu
        if (isCursorInGameScreen)
        {
            //lewo
            if (mousePosScreen.x < cursorTreshold) movementDirection.x -= 1 - mousePosScreen.x / cursorTreshold;
            //prawo
            if (mousePosScreen.x > 1 - cursorTreshold) movementDirection.x += 1 - (1 - mousePosScreen.x) / (cursorTreshold);
            //góra
            if (mousePosScreen.y < cursorTreshold) movementDirection.y -= 1 - mousePosScreen.y / cursorTreshold;
            //dół
            if (mousePosScreen.y > 1 - cursorTreshold) movementDirection.y += 1 - (1 - mousePosScreen.y) / (cursorTreshold);
        }
        var deltaPosition = new Vector3(movementDirection.x, 0, movementDirection.y);
        deltaPosition *= cameraSpeed * Time.deltaTime;
        transform.position += deltaPosition;
    }


    void UpdateZoom()
    {
        mouseScroll = Input.mouseScrollDelta;
        float zoomDelta = mouseScroll.y * zoomSpeed * Time.deltaTime;
        zoomLerp = Mathf.Clamp01(zoomLerp + zoomDelta);

        //przybliżanie
        var position = transform.position;
        position.y = Mathf.Lerp(cameraHeightMinMax.y, cameraHeightMinMax.x, zoomLerp) + groundHeight;
        transform.position = position;

        //rotowanie wokół X
        var rotation = transform.localEulerAngles;
        rotation.x = Mathf.Lerp(cameraRotationMinMax.y, cameraRotationMinMax.x, zoomLerp);
        transform.localEulerAngles = rotation;
    }


    void UpdateClicks()
    {
        //zaznaczanie
        if (Input.GetMouseButtonDown(0))
        {
            selectionBox.gameObject.SetActive(true);
            selectionRect.position = mousePos;
            TryBuild();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            selectionBox.gameObject.SetActive(false);

        }
        if (Input.GetMouseButton(0))
        {
            selectionRect.size = mousePos - selectionRect.position;
            boxRect = AbsRect(selectionRect);
            selectionBox.anchoredPosition = boxRect.position;
            selectionBox.sizeDelta = boxRect.size;
            if(boxRect.size.x != 0 || boxRect.size.y != 0)
            UpdateSelecting();
        }
        if (Input.GetMouseButtonDown(1))
        {
            GiveCommands();
            buildingPrefabToSpawn = null;
        }

    }



    //przekształcenie prostokątu zaznaczenia
    Rect AbsRect(Rect rect)
    {
        if(rect.width < 0)
        {
            rect.x += rect.width;
            rect.width *= -1;
        }
        if (rect.height < 0)
        {
            rect.y += rect.height;
            rect.height *= -1;
        }
        return rect;
    }

    //zaznaczanie jednostek
    void UpdateSelecting()
    {
        selectedUnits.Clear();
        foreach (ISelectable selectable in Unit.SelectableUnits)
        {
            if (selectable == null) continue;
            MonoBehaviour monoBehaviour = selectable as MonoBehaviour;
            var pos = monoBehaviour.transform.position;
            var posScreen = camera.WorldToScreenPoint(pos);
            bool inRect = IsPointInRect(boxRect, posScreen);
            (selectable as ISelectable).setSelected(inRect);
            if (inRect)
            {
                selectedUnits.Add(selectable);
            }
        }
    }

    bool IsPointInRect(Rect rect, Vector2 point)
    {
        return point.x >= rect.position.x && point.x <= (rect.position.x + rect.size.x) &&
            point.y >= rect.position.y && point.y <= (rect.position.y + rect.size.y);
    }

    Ray ray;
    RaycastHit rayHit;
    [SerializeField]
    LayerMask commandLayerMask = -1, buildingLayerMask = 0;

    //dawanie poleceń
    void GiveCommands()
    {
        ray = camera.ViewportPointToRay(mousePosScreen);
        if (Physics.Raycast(ray, out rayHit, 1000, commandLayerMask))
        {
            object commandData = null;
            if (rayHit.collider is TerrainCollider)
            {
                commandData = rayHit.point;
            }
            else
            {
                commandData = rayHit.collider.gameObject.GetComponent<Unit>();
            }
            GiveCommands(commandData, "Command");
        }
    }

    void GiveCommands(object dataCommand, string commandName)
    {
        foreach (ISelectable selectable in selectedUnits)
        {
            (selectable as MonoBehaviour).SendMessage(commandName, dataCommand, SendMessageOptions.DontRequireReceiver);
        }
    }

    public static void SpawnUnits(GameObject prefab)
    {
        cameraControl.GiveCommands(prefab,"Spawn");
    }
    public static void SpawnBuilding(GameObject prefab)
    {
        cameraControl.buildingPrefabToSpawn = prefab;
        //selec
    }

    void UpdatePlacer()
    {
        placer.gameObject.SetActive(buildingPrefabToSpawn);
        if (placer.gameObject.activeInHierarchy)
        {
            ray = camera.ViewportPointToRay(mousePosScreen);
            if (Physics.Raycast(ray, out rayHit, 1000, buildingLayerMask))
            {
                placer.SetPosition(rayHit.point);
            }
        }
    }

    void TryBuild()
    {
        if(buildingPrefabToSpawn && placer && placer.isActiveAndEnabled && placer.CanBuildHere())
        {
            var buyable = buildingPrefabToSpawn.GetComponent<Buyable>();
            if (!buyable || !Money.TrySpendMoney(buyable.cost)) return;

            var unit = Instantiate(buildingPrefabToSpawn, placer.transform.position, placer.transform.rotation);

        }
    }
}
