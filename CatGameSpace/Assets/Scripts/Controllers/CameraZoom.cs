using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    private static CameraZoom _instance;
    public static CameraZoom Instance
    {
        get { return _instance; }
    }

    public bool playerInterrupt = false;

    private float desiredZoom;
    private Vector3 desiredCamPos;

    [SerializeField]
    private float zoomStart;
    [SerializeField]
    private float zoomSmooth = 10f;
    [SerializeField]
    private float zoomSpeed = 1f;
    [SerializeField]
    private float minZoom = 2f;
    [SerializeField]
    private float maxZoom = 10f;

    [SerializeField]
    private float panSpeed = 0.5f;
    private Vector2 lastMousePosition;
    private bool isPanning = false;

    private float defaultSize;
    private Vector3 defaultCamPos;

    [SerializeField]
    private float minXPos;
    [SerializeField]
    private float minYPos;
    [SerializeField]
    private float maxXPos;
    [SerializeField]
    private float maxYPos;


    private float point0;
    private float point1;
    private float point2;
    private float point3;

    private float point0Zoom;
    private float point1Zoom;
    private float point2Zoom;
    private float point3Zoom;

    public LayerMask lmask0_1;
    public LayerMask lmask0_2;
    public LayerMask lmask0_3;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    private void Start()
    {
        defaultSize = maxZoom;
        defaultCamPos = new Vector3(0, 0, -10);

        desiredZoom = zoomStart;

        StartCoroutine(work());
    }

    IEnumerator work()
    {
        yield return null;

        TextMeshPro[] allTMPs = FindObjectsOfType<TextMeshPro>(true); // true: inactive objeler de dahil
        if (allTMPs.Length == 0)
        {
            Debug.LogWarning("Sahnede hiç TextMeshPro bulunamadý.");
            yield break;
        }

        float minSize = float.MaxValue;
        float maxSize = float.MinValue;

        foreach (var tmp in allTMPs)
        {
            float size = tmp.fontSize;

            if (size < minSize) minSize = size;
            if (size > maxSize) maxSize = size;
        }

        point0 = minSize;
        point1 = minSize + ((maxSize - minSize) / 12);
        point2 = minSize + ((maxSize - minSize) / 6);
        point3 = maxSize;


        foreach (var tmp in allTMPs)
        {
            if(tmp.fontSize < point1)
            {
                tmp.gameObject.layer = 6;
            }
            else if (tmp.fontSize < point2)
            {
                tmp.gameObject.layer = 7;
            }
            else if (tmp.fontSize <= point3)
            {
                tmp.gameObject.layer = 8;
            }
        }

        point0Zoom = minZoom;
        point1Zoom = minZoom + ((maxZoom - minZoom) / 12);
        point2Zoom = minZoom + ((maxZoom - minZoom) / 6);
        point3Zoom = maxZoom;
    }

    void LateUpdate()
    {

        if (playerInterrupt) return;

        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, desiredZoom, Time.deltaTime * zoomSmooth);



        float vertExtent = Camera.main.orthographicSize;
        float horzExtent = vertExtent * Camera.main.aspect;

        // Kamera pozisyonunu al
        Vector3 pos = transform.position;

        // Kameranýn ortasý öyle olmalý ki kenarlar sýnýrlarýn dýþýna çýkmasýn
        pos.x = Mathf.Clamp(pos.x, minXPos + horzExtent, maxXPos - horzExtent);
        pos.y = Mathf.Clamp(pos.y, minYPos + vertExtent, maxYPos - vertExtent);
        pos.z = -10f;
        transform.position = pos;

        TxtDisplayOption();
    }
    public void zoomCam(float zoomValue)
    {
        if (playerInterrupt) return;

        float scroll = zoomValue; // Fare tekerleði okuma
        desiredZoom -= scroll * zoomSpeed * Time.deltaTime;

        desiredZoom = Mathf.Clamp(desiredZoom, minZoom, maxZoom);
        panSpeed = Mathf.Lerp(0.5f, 2.5f, (desiredZoom - minZoom) / (maxZoom - minZoom));
    }


    public void zoomCamForce(float value)
    {
        desiredZoom = value;
        panSpeed = Mathf.Lerp(0.5f, 2.5f, (desiredZoom - minZoom) / (maxZoom - minZoom));
    }

    public void MoveCam(Vector2 movement)
    {
        if (playerInterrupt) return;

        Vector2 move = movement * Time.deltaTime * panSpeed;

        transform.position += new Vector3(move.x, move.y, 0f);
    }

    public void panCam(Vector2 cursorPOS)
    {
        if (playerInterrupt) return;

        if (!isPanning)
        {
            lastMousePosition = cursorPOS;
            isPanning = true;
        }
        else
        {
            Vector2 delta = cursorPOS - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime;
            transform.position += move; // Kamerayý hareket ettir
            lastMousePosition = cursorPOS;


            if (transform.position.x < minXPos)
            {
                transform.position = new Vector3(minXPos, transform.position.y, -10);
            }
            else if (transform.position.x > maxXPos)
            {
                transform.position = new Vector3(maxXPos, transform.position.y, -10);

            }

            if (transform.position.y < minYPos)
            {

                transform.position = new Vector3(transform.position.x, minYPos, -10);
            }
            else if (transform.position.y > maxYPos)
            {

                transform.position = new Vector3(transform.position.x, maxYPos, -10);
            }

        }
    }
    public void stopPanCam()
    {
        if (playerInterrupt) return;

        isPanning = false;
    }

    public void TxtDisplayOption()
    {
        if (Camera.main.orthographicSize < point1Zoom)
        {
            Camera.main.cullingMask = lmask0_1;
        }
        else if (Camera.main.orthographicSize < point2Zoom)
        {
            Camera.main.cullingMask = lmask0_2;
        }
        else if (Camera.main.orthographicSize <= point3Zoom)
        {
            Camera.main.cullingMask = lmask0_3;
        }
    }
    Coroutine hintRoutine = null;
    public void Hint(Vector2 ItemPos)
    {
        if(hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }

        hintRoutine = StartCoroutine(HintCamera(new Vector3(ItemPos.x, ItemPos.y, -10)));

    }
    IEnumerator HintCamera(Vector3 pos)
    {
        playerInterrupt = true;
        while (true)
        {
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 5f);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize,minZoom,Time.deltaTime * zoomSmooth);

            TxtDisplayOption();

            if (minZoom - Camera.main.orthographicSize < .0025f && Vector2.Distance(transform.position, pos) < .0025f) break;
            yield return null;
        }
        transform.position = pos;
        Camera.main.orthographicSize = minZoom;
        desiredZoom = Camera.main.orthographicSize;
        playerInterrupt = false;

    }
    public void defaultCam()
    {
        if(hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
            playerInterrupt = false;
        }

        desiredZoom = defaultSize;
        Camera.main.transform.position = defaultCamPos;
    }
}
