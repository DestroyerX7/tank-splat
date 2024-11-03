using Unity.Netcode;
using UnityEngine;

public class MapManager : NetworkBehaviour
{
    public static MapManager Instance { get; private set; }

    [SerializeField] private MapListSO _mapListSO;
    private readonly NetworkVariable<int> _mapIndex = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _mapIndex.Value = LobbyManager.Instance.LobbyData.MapIndex;
        }

        if (IsClient)
        {
            Instantiate(_mapListSO.Maps[_mapIndex.Value].MapPrefab, transform);
            Camera.main.orthographicSize = _mapListSO.Maps[_mapIndex.Value].CameraFov;
        }
    }

    public Vector2 GetSpawnPos(int index)
    {
        return _mapListSO.Maps[LobbyManager.Instance.LobbyData.MapIndex].SpawnPositions[index];
    }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     { // capture screen shot on left mouse button down

    //         string folderPath = "D:/Unity Projects/Tank Splat/Assets/Screenshots"; // the path of your project folder

    //         if (!System.IO.Directory.Exists(folderPath)) // if this path does not exist yet
    //             System.IO.Directory.CreateDirectory(folderPath);  // it will get created

    //         var screenshotName =
    //                                 "Screenshot_" +
    //                                 System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + // puts the current time right into the screenshot name
    //                                 ".png"; // put youre favorite data format here
    //         ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName), 2); // takes the sceenshot, the "2" is for the scaled resolution, you can put this to 600 but it will take really long to scale the image up
    //         Debug.Log(folderPath + screenshotName); // You get instant feedback in the console
    //     }
    // }
}
