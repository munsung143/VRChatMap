using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework.Internal.Execution;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class MazeGenerate : EditorWindow
{
    static Texture2D texture;

    static int textureLength = 256;
    static int xLength = 10;
    static int yLength = 10;
    static MazeNode[,] nodes;
    static int count;

    [MenuItem("Tools/Create and Save Texture")]
    public static void Create()
    {
        count = 0;
        texture = new Texture2D(textureLength, textureLength, TextureFormat.RGBA32, false);
        nodes = new MazeNode[yLength, xLength];

        // 미로 영역 검은색 초기화
        for (int y = 0; y < xLength * 2 + 1; y++)
            for (int x = 0; x < yLength * 2 + 1; x++)
                texture.SetPixel(x, y, Color.black);

        // 노드 생성 및 각 노드를 초록색으로 표시
        for (int y = 0; y < xLength; y++)
            for (int x = 0; x < yLength; x++)
            {
                nodes[y, x] = new MazeNode(x, y);
                texture.SetPixel(nodes[y,x].x *2 + 1, nodes[y,x].y *2 + 1, Color.green);
            }

        // 첫 노드 위치 랜덤으로 정하고 트랙 시작
        int firstX = Random.Range(0, xLength);
        int firstY = Random.Range(0, yLength);
        MazeNode firstNode = nodes[firstY, firstX];
        firstNode.cameFrom = Direction.None;
        Track(firstNode);

        Save();
    }
    public static void Save()
    {
        string localPath = "Assets/GeneratedTexture.png";
        string fullPath = Path.Combine(Application.dataPath, "../" + localPath);
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);
        AssetDatabase.Refresh();
        DestroyImmediate(texture);
    }

    public static void Track(MazeNode node)
    {
        Debug.Log($"cur : {node.x} {node.y}");
        int pixelPosX = node.x * 2 + 1;
        int pixelPosY = node.y * 2 + 1;
        node.isVisited = true;
        count++;
        if (count > 700) return;

        // 현위치 및 건너온 경로를 붉은색으로 표시
        texture.SetPixel(pixelPosX, pixelPosY, Color.red);
        switch (node.cameFrom)
        {
            case Direction.East:
                texture.SetPixel(pixelPosX - 1, pixelPosY, Color.red);
                break;
            case Direction.West:
                texture.SetPixel(pixelPosX + 1, pixelPosY, Color.red);
                break;
            case Direction.South:
                texture.SetPixel(pixelPosX, pixelPosY + 1, Color.red);
                break;
            case Direction.North:
                texture.SetPixel(pixelPosX, pixelPosY - 1, Color.red);
                break;
        }

        List<Direction> directions = new List<Direction>() { Direction.East, Direction.West, Direction.South, Direction.North };
        while (directions.Count != 0)
        {
            int num = Random.Range(0, directions.Count);
            Direction dir = directions[num];
            directions.RemoveAt(num);

            MazeNode next = null;
            if (dir == Direction.East && node.x + 1 < xLength) next = nodes[node.y, node.x + 1];
            else if (dir == Direction.West && node.x - 1 > -1) next = nodes[node.y, node.x - 1];
            else if (dir == Direction.South && node.y - 1 > -1) next = nodes[node.y - 1, node.x];
            else if (dir == Direction.North && node.y + 1 < yLength) next = nodes[node.y + 1, node.x];

            if (next != null && !next.isVisited)
            {
                next.cameFrom = dir;
                Debug.Log($"{node.x} {node.y} 에서 {dir} 방향 진행");
                Track(next);
            }
        }
       if (count >700) return;
        Debug.Log($"고립됨 : {node.x} {node.y}");
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
}

public enum Direction
{
    East = 0,
    West,
    South,
    North,
    None

}


public class MazeNode
{
    public int x;
    public int y;
    public bool isVisited;
    public Direction cameFrom;
    public MazeNode(int x, int y)
    {
        this.x = x;
        this.y = y;
        isVisited = false;

    }
}
