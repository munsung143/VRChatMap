using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MazeGenerator : EditorWindow
{
    static Texture2D texture;
    static int textureLength = 128;
    static int xLength = 100;
    static int yLength = 100;
    static string localPath = "Assets/MazeTexture.png";
    MazeNode[,] nodes;
    Stack<MazeNode> stk;

    static Transform maze;
    static GameObject cubePrefab;
    static float cubeLength = 1f;

    [MenuItem("Tools/미로 생성기")]
    public static void ShowWindow()
    {
        GetWindow<MazeGenerator>("미로 생성기");
    }

    private void OnGUI()
    {

        GUILayout.Label("미로 텍스처 생성", EditorStyles.boldLabel);
        textureLength = EditorGUILayout.IntPopup("텍스처 한 변의 픽셀 수", textureLength,
        new string[]{"8", "16", "32", "64", "128", "256", "1024", "2048", "4096", "8192", "16384"},
        new int[]{8, 16, 32, 64, 128, 256, 1024, 2048, 4096, 8192, 16384});
        xLength = EditorGUILayout.IntField("미로의 가로 픽셀 수", xLength);
        yLength = EditorGUILayout.IntField("미로의 세로 픽셀 수", yLength);
        localPath = EditorGUILayout.TextField("경로", localPath);

        if (GUILayout.Button("텍스처 생성"))
        {
            GenerateMazeTexture();
        }

        GUILayout.Label("미로 생성", EditorStyles.boldLabel);

        texture = (Texture2D)EditorGUILayout.ObjectField("텍스처", texture, typeof(Texture2D), false);
        maze = (Transform)EditorGUILayout.ObjectField("부모 트렌스폼", maze, typeof(Transform), true);
        cubePrefab = (GameObject)EditorGUILayout.ObjectField("큐브 프리팹", cubePrefab, typeof(GameObject), false);
        cubeLength = EditorGUILayout.FloatField("큐브 한 변의 길이", cubeLength);

        if (GUILayout.Button("생성"))
        {
            GenerateMaze();
        }
    }

    public void GenerateMazeTexture()
    {
        int xNodeLength = xLength / 2;
        int yNodeLength = yLength / 2;
        texture = new Texture2D(textureLength, textureLength, TextureFormat.RGBA32, false);
        nodes = new MazeNode[yNodeLength, xNodeLength];
        stk = new Stack<MazeNode>();

        // 미로 영역 검은색 초기화
        for (int y = 0; y < xNodeLength * 2 + 1; y++)
            for (int x = 0; x < yNodeLength * 2 + 1; x++)
                texture.SetPixel(x, y, Color.black);

        // 노드 생성 및 각 노드를 초록색으로 표시
        for (int y = 0; y < xNodeLength; y++)
            for (int x = 0; x < yNodeLength; x++)
            {
                nodes[y, x] = new MazeNode(x, y);
                texture.SetPixel(nodes[y, x].x * 2 + 1, nodes[y, x].y * 2 + 1, Color.green);
            }

        // 첫 노드 위치 랜덤으로 정하고 트랙 시작
        int firstX = Random.Range(0, xNodeLength);
        int firstY = Random.Range(0, yNodeLength);
        MazeNode firstNode = nodes[firstY, firstX];
        firstNode.cameFrom = Direction.None;

        stk.Push(firstNode);
        while (stk.Count != 0)
        {
            MazeNode node = stk.Peek();
            int pixelPosX = node.x * 2 + 1;
            int pixelPosY = node.y * 2 + 1;
            node.isVisited = true;
            List<Direction> directions = node.directions;
            bool find = false;
            while (directions.Count != 0)
            {
                int num = Random.Range(0, directions.Count);
                Direction dir = directions[num];
                directions.RemoveAt(num);

                MazeNode next = null;
                if (dir == Direction.East && node.x + 1 < xNodeLength) next = nodes[node.y, node.x + 1];
                else if (dir == Direction.West && node.x - 1 > -1) next = nodes[node.y, node.x - 1];
                else if (dir == Direction.South && node.y - 1 > -1) next = nodes[node.y - 1, node.x];
                else if (dir == Direction.North && node.y + 1 < yNodeLength) next = nodes[node.y + 1, node.x];

                if (next != null && !next.isVisited)
                {
                    find = true;
                    next.cameFrom = dir;
                    stk.Push(next);
                }
            }
            if (find)
            {
                continue;
            }
            stk.Pop();
            texture.SetPixel(pixelPosX, pixelPosY, Color.white);
            switch (node.cameFrom)
            {
                case Direction.East:
                    texture.SetPixel(pixelPosX - 1, pixelPosY, Color.white);
                    break;
                case Direction.West:
                    texture.SetPixel(pixelPosX + 1, pixelPosY, Color.white);
                    break;
                case Direction.South:
                    texture.SetPixel(pixelPosX, pixelPosY + 1, Color.white);
                    break;
                case Direction.North:
                    texture.SetPixel(pixelPosX, pixelPosY - 1, Color.white);
                    break;
            }
        }
        string fullPath = Path.Combine(Application.dataPath, "../" + localPath);
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);
        AssetDatabase.Refresh();
        //DestroyImmediate(texture);
    }
    private void GenerateMaze()
    {
        if (maze == null || cubePrefab == null || texture == null)
        {
            Debug.LogError("[MazeGenerator] 모든 필드(Maze Parent, Cube Prefab, Source Image)를 할당해주세요.");
            return;
        }

        // 텍스처 읽기/쓰기 설정 확인 안내
        if (!texture.isReadable)
        {
            Debug.LogError($"[MazeGenerator] 텍스처의 'Read/Write Enabled'를 체크해주세요.");
            return;
        }
        // 뒤에서부터 삭제해야 인덱스 오염 방지
        for (int i = maze.childCount - 1; i >= 0; i--)
        {
            GameObject child = maze.GetChild(i).gameObject;
            Undo.DestroyObjectImmediate(child);
        }

        int imgWidth = texture.width;
        int imgHeight = texture.height;

        // 2. 이미지 픽셀 탐색 및 큐브 생성
        for (int y = 0; y < imgHeight; y++)
        {
            for (int x = 0; x < imgWidth; x++)
            {
                Color c = texture.GetPixel(x, y);

                // RGB 평균값이 임계값 이하이면 검은색으로 판단
                if (c.r <= 0.1f && c.g <= 0.1f && c.b <= 0.1f)
                {
                    Vector3 position = new Vector3(x * cubeLength, 0f, y * cubeLength);
                    //Debug.Log($"{x / verticalOffset * length} {y / horizontalOffset * length} pixel: {x} {y - 2047}");

                    // 에디터 환경에서 Undo 지원하며 인스턴스화
                    GameObject spawnedCube = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab, maze);

                    // 프리팹이 아닌 일반 씬 오브젝트인 경우 처리
                    if (spawnedCube == null)
                    {
                        spawnedCube = Instantiate(cubePrefab, maze);
                    }

                    spawnedCube.transform.localPosition = position;
                    //spawnedCube.transform.localScale = Vector3.one * length;

                    Undo.RegisterCreatedObjectUndo(spawnedCube, "Generate Maze - Spawn Cube");
                }
            }
        }

        Debug.Log("[MazeGenerator] 미로 생성이 완료되었습니다.");
    }

    private void ClearChildren(Transform parent)
    {

    }
}
