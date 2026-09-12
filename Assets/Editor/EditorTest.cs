using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorTest : EditorWindow
{
    private Transform maze;              // 메이즈 오브젝트 (부모 Transform)
    private GameObject cubePrefab;       // 프리팹 또는 큐브 오브젝트
    private Texture2D img;               // 텍스처 이미지
    private float length = 1.0f;         // 큐브 한 변의 길이
    private int horizontalOffset = 1;    // 가로 오프셋 (픽셀 수)
    private int verticalOffset = 1;      // 세로 오프셋 (픽셀 수)
    private float colorThreshold = 0.1f; // 검은색 판정 임계값

    [MenuItem("Tools/Maze Generator")]
    public static void ShowWindow()
    {
        GetWindow<EditorTest>("Maze Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Maze Generation Settings", EditorStyles.boldLabel);

        maze = (Transform)EditorGUILayout.ObjectField("Maze Parent", maze, typeof(Transform), true);
        cubePrefab = (GameObject)EditorGUILayout.ObjectField("Cube Prefab", cubePrefab, typeof(GameObject), false);
        img = (Texture2D)EditorGUILayout.ObjectField("Source Image", img, typeof(Texture2D), false);

        length = EditorGUILayout.FloatField("Cube Length", length);
        horizontalOffset = EditorGUILayout.IntField("Horizontal Offset", horizontalOffset);
        verticalOffset = EditorGUILayout.IntField("Vertical Offset", verticalOffset);
        colorThreshold = EditorGUILayout.Slider("Black Threshold", colorThreshold, 0f, 1f);

        if (GUILayout.Button("Generate Maze"))
        {
            GenerateMaze();
        }
    }

    private void GenerateMaze()
    {
        if (maze == null || cubePrefab == null || img == null)
        {
            Debug.LogError("[MazeGenerator] 모든 필드(Maze Parent, Cube Prefab, Source Image)를 할당해주세요.");
            return;
        }

        // 텍스처 읽기/쓰기 설정 확인 안내
        if (!img.isReadable)
        {
            Debug.LogError($"[MazeGenerator] 이미지 '{img.name}'의 Texture Import Settings에서 'Read/Write Enabled'를 체크해주세요.");
            return;
        }

        // 1. 기존 자식 오브젝트 모두 제거 (Undo 지원)
        ClearChildren(maze);

        int imgWidth = img.width;
        int imgHeight = img.height;

        // 실행 취소(Undo) 등록
        Undo.RegisterCompleteObjectUndo(maze.gameObject, "Generate Maze");

        // 2. 이미지 픽셀 탐색 및 큐브 생성
        for (int y = 0; y < imgHeight; y += verticalOffset)
        {
            for (int x = 0; x < imgWidth; x += horizontalOffset)
            {
                Color c = img.GetPixel(x, y);

                // RGB 평균값이 임계값 이하이면 검은색으로 판단
                if (c.r <= colorThreshold && c.g <= colorThreshold && c.b <= colorThreshold && c.a > 0.5f)
                {
                    // 좌측 맨위 시작점에 맞추기 위해 Y축 좌표는 음수로 배치
                    Vector3 position = new Vector3(-x/verticalOffset * length, 0f, y/horizontalOffset * length);

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
        // 뒤에서부터 삭제해야 인덱스 오염 방지
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            Undo.DestroyObjectImmediate(child);
        }
    }
}
