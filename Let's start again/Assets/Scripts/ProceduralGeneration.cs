using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    public GameObject space;
    public int width;
    public int height;
    const int MIN = 10;
    const int MAX = 100;

    // Start is called before the first frame update
    void Start()
    {
        bool[,] spaceAr = new bool[width, height];
        TreeNode parentNode = new TreeNode();
        List <TreeNode> leafsNode = new List <TreeNode>();
        parentNode.coords.width = width;
        parentNode.coords.height = height;
        Split(parentNode, ref leafsNode);
        DrawAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Split(TreeNode node, ref List <TreeNode> nodeAr)
    {
        if (!node.isSplit)
        {
            if(node.coords.width > MIN || node.coords.height > MIN)
            {
                node.lChild = new TreeNode();
                node.rChild = new TreeNode();

                node.isSplit = true;
                if(node.coords.width >= MIN || node.coords.height >= MIN && node.coords.width >= 2 * MIN || node.coords.height >= 2 * MIN) //Разберись с вычитанием высот
                {
                    bool horiz = Random.value < 0.5;
                    if (node.coords.width >= 2 * MIN && (node.coords.height <= 2 * MIN || horiz)) //Поправь генерацию случайного числа
                    {
                        int splitV = Random.Range(MIN, node.coords.width - MIN);
                        node.lChild.coords.x = node.coords.x;
                        node.lChild.coords.width = splitV;
                        node.lChild.coords.y = node.coords.y;
                        node.lChild.coords.height = node.coords.height;

                        node.rChild.coords.x = splitV;
                        node.rChild.coords.width = node.coords.width - splitV;
                        node.rChild.coords.y = node.coords.y;
                        node.lChild.coords.height = node.coords.height;
                    }
                    else if (node.coords.height >= 2 * MIN && (node.coords.width <= 2 * MIN || !horiz))
                    {
                        int splitH = Random.Range(MIN, node.coords.height - MIN);
                        node.lChild.coords.x = node.coords.x;
                        node.lChild.coords.width = node.coords.width;
                        node.lChild.coords.y = node.coords.y;
                        node.lChild.coords.height = splitH;

                        node.rChild.coords.x = node.coords.x;
                        node.rChild.coords.width = node.coords.width;
                        node.rChild.coords.y = splitH;
                        node.lChild.coords.height = node.coords.height - splitH;
                    }
                }
                Split(node.lChild, ref nodeAr);
                Split(node.rChild, ref nodeAr);
                if(node.lChild != null) CreateCorridors(node, nodeAr);
            }
            else
            {
                nodeAr.Add(node);
                CreateRooms(node);
            }
            
        }
    }
    void CreateRooms(TreeNode node)
    {
        node.roomCoords.width = Random.Range(3, node.coords.width - 2);
        node.roomCoords.height = Random.Range(3, node.coords.height - 2);
        node.coords.x = Random.Range(1, node.coords.width - node.roomCoords.width);
        node.coords.y = Random.Range(1, node.coords.height - node.roomCoords.height);
        
        
    }
    void CreateCorridors(TreeNode node, List<TreeNode> nodeAr) //Из списка нужно будет высчитывать индекс соединяемых комнат
    {
        TreeNode left = FindLeaf(node.lChild, false, new Vector2(0, 0));
        TreeNode right = FindLeaf(node.rChild, false, new Vector2(0, 0));

        //int co ? (left.hallwayList.Count)

        if(left.coords.x == right.coords.x || left.coords.y == right.coords.y)
        {
            if(left.coords.x == right.coords.x)
            {
                left.hallwayList.Add(new TreeNode.Hallway());
                left.hallwayList[left.hallwayList.Count - 1].indexOfRooms.Add(nodeAr.IndexOf(left)); //nullReferenceExpectation СХУЯЛИИИИИИИИ БЛЯТЬ ЗДЕСЬ ОН СУКА

                left.hallwayList[left.hallwayList.Count - 1].halls.Add(new RectInt(left.roomCoords.width / 2, right.roomCoords.height / 2, 1, (left.roomCoords.height / 2) - (right.roomCoords.height / 2)));
                FindAllRooms(ref left.hallwayList, nodeAr[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms.Count - 1]], nodeAr);
            }
            if(left.coords.y == right.coords.y)
            {
                left.hallwayList.Add(new TreeNode.Hallway());
                left.hallwayList[left.hallwayList.Count - 1].indexOfRooms.Add(nodeAr.IndexOf(left));

                left.hallwayList[left.hallwayList.Count - 1].halls.Add(new RectInt(left.roomCoords.width / 2, right.roomCoords.height / 2, (left.roomCoords.width / 2) - (right.roomCoords.width / 2), 1));
                FindAllRooms(ref left.hallwayList, nodeAr[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms.Count - 1]], nodeAr);
            }
        }
        else if(Random.value < 0.5) //Добавь генерацию корридоров при одинаковой координате (коридор без угла)
        {
            left.hallwayList.Add(new TreeNode.Hallway());
            left.hallwayList[left.hallwayList.Count - 1].indexOfRooms.Add(nodeAr.IndexOf(left));

            left.hallwayList[left.hallwayList.Count - 1].halls.Add(new RectInt(left.roomCoords.width / 2, right.roomCoords.height / 2, 1, (left.roomCoords.height / 2) - (right.roomCoords.height / 2)));
            FindAllRooms(ref left.hallwayList, nodeAr[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms.Count - 1]], nodeAr);

            left.hallwayList[left.hallwayList.Count - 1].halls.Add(new RectInt(left.roomCoords.width / 2, right.roomCoords.height / 2, (left.roomCoords.width / 2) - (right.roomCoords.width / 2), 1));
            FindAllRooms(ref left.hallwayList, nodeAr[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms.Count - 1]], nodeAr);
        }
        else
        {
            left.hallwayList.Add(new TreeNode.Hallway());
            left.hallwayList[left.hallwayList.Count - 1].indexOfRooms.Add(nodeAr.IndexOf(left));

            left.hallwayList[left.hallwayList.Count - 1].halls.Add(new RectInt(right.roomCoords.width / 2, right.roomCoords.height / 2, 1, (right.roomCoords.height / 2) - (left.roomCoords.height / 2)));
            FindAllRooms(ref left.hallwayList, nodeAr[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms.Count - 1]], nodeAr);

            left.hallwayList[left.hallwayList.Count - 1].halls.Add(new RectInt(right.roomCoords.width / 2, left.roomCoords.height / 2, (right.roomCoords.width / 2) - (left.roomCoords.width / 2), 1));
            FindAllRooms(ref left.hallwayList, nodeAr[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms[left.hallwayList[left.hallwayList.Count - 1].indexOfRooms.Count - 1]], nodeAr);
        }
    }
    void FindAllRooms(ref List<TreeNode.Hallway> hallways, TreeNode node, List<TreeNode> nodeAr) //Ты остановился на том, что нужно добавлять найденную методом комнату в список + вычитать размер этой комнаты + высчитывать дошёл ли коридор после всего этого до нужной комнаты
    {
        int am = hallways.Count - 1;
        int amm = hallways[am].halls.Count - 1;
        TreeNode next;
        if (hallways[am].halls[amm].height > hallways[am].halls[amm].width)
        {
            int height = hallways[am].halls[amm].height;
            height -= (hallways[am].halls[amm].y >= node.coords.y) ? node.coords.y - node.coords.height : node.coords.y;
            if (height > 0)
            {
                next = FindLeaf(node, true, new Vector2(node.coords.x, node.coords.y + 1));
                hallways[hallways.Count - 1].indexOfRooms.Add(nodeAr.IndexOf(next));
                next.hallwayList.Add(hallways[hallways.Count - 1]);
                FindAllRooms(ref hallways, next, nodeAr);
            }
            else
            {
                node.hallwayList.Add(hallways[hallways.Count - 1]);
                hallways[hallways.Count - 1].indexOfRooms.Add(nodeAr.IndexOf(node));
            }
        }
        else
        {
            int width = hallways[am].halls[amm].width;
            width -= (hallways[am].halls[amm].x >= node.coords.x) ? node.coords.x - node.coords.width : node.coords.x;
            if (width > 0)
            {
                next = FindLeaf(node, true, new Vector2(node.coords.x + 1, node.coords.y));
                hallways[hallways.Count - 1].indexOfRooms.Add(nodeAr.IndexOf(next));
                next.hallwayList.Add(hallways[hallways.Count - 1]);
                FindAllRooms(ref hallways, next, nodeAr);
            }
            else
            {
                node.hallwayList.Add(hallways[hallways.Count - 1]);
                hallways[hallways.Count - 1].indexOfRooms.Add(nodeAr.IndexOf(node));
            }
        }
    }
    TreeNode FindLeaf(TreeNode node, bool bySize, Vector2 pos)
    {
        if (!bySize)
        {
            TreeNode leaf;

            if (node.lChild != null)
            {
                if (Random.value > 0.5) leaf = FindLeaf(node.lChild, false, new Vector2(0, 0));
                else leaf = FindLeaf(node.rChild, false, new Vector2(0, 0));
            }
            else leaf = node;
            return leaf;
        }
        else
        {
            TreeNode leaf;

            if(node.rChild != null)
            {
                if (node.rChild.coords.x > pos.x || node.rChild.coords.x > pos.y) leaf = FindLeaf(node.lChild, true, pos);
                else leaf = FindLeaf(node.rChild, true, pos);
            }
            else leaf = node;
            return leaf;

        }
        
    }
    void DrawAll()
    {

    }
}
class TreeNode
{
    internal bool isSplit = false;
    internal RectInt coords;
    internal RectInt roomCoords;
    internal TreeNode lChild;
    internal TreeNode rChild;

    internal List <Hallway> hallwayList = new List <Hallway>();

    internal struct Hallway
    {
        internal List<RectInt> halls;
        internal List<int> indexOfRooms;
    }
}
