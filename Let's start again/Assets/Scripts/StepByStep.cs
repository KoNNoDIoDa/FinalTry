using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StepByStep : MonoBehaviour
{
    public Tile tile;
    readonly List<Node> nodes = new List<Node>();
    public Material mat;
    public Sprite sprite;
    const int MIN = 7;
    const int iteration = 5;
    Node parent;
    public bool showRooms = true;
    RectInt safeRoom = new RectInt();
    public Tilemap map;

    public Tile topLeftTile;
    public Tile topMiddleTile;
    public Tile topRightTile;

    public Tile middleLeftTile;
    public Tile middleTile;
    public Tile middleRightTile;

    public Tile bottomLeftTile;
    public Tile bottomMiddleTile;
    public Tile bottomRightTile;

    public Tile innerTopRightTile;
    public Tile innerTopLeftTile;
    public Tile innerBottomLeftTile;
    public Tile innerBottomRightTile;

    public Tile topDoor;
    public Tile leftDoor;
    public Tile rightDoor;
    public Tile bottomDoor;

    // Start is called before the first frame update
    void Start()
    {
        parent = new Node();
        parent.SetParam(new RectInt(0, 0, 90, 90), false);
        //parent.SetParam
        Split(iteration, parent);
        Node safe = new Node();
        safe.SetParam(new RectInt(100500, 100500, 1, 1), false);
        nodes.Add(safe);
        CreateCorridors(parent, true);
        for(int i = 0; i < nodes.Count; i++)
        {
            List<Vector2Int> doors = nodes[i].GetDoors();
            RenderRoom(nodes[i]);
            Hashtable halls = nodes[i].GetConnections();
            Node.Hallways[][] hallwaysList = new Node.Hallways[halls.Values.Count][];
            halls.Values.CopyTo(hallwaysList, 0);
            for (int x = 0; x < hallwaysList.Length; x++)
            {
                if(x < doors.Count)
                {
                    GameObject door = new GameObject();
                    Vector3 position = door.transform.position;
                    position.x = doors[x].x;
                    position.y = doors[x].y;
                    position.z = -10;
                    door.transform.localPosition = position;

                    Vector2 scale = door.transform.localScale;
                    scale.x = 1;
                    scale.y = 1;
                    door.transform.localScale = scale;

                    SpriteRenderer renderD = door.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
                    renderD.material = mat;
                    renderD.sprite = sprite;
                }
                for (int z = 0; z < hallwaysList[x].Length; z++)
                {
                    List<RectInt> hallwayAsRects = hallwaysList[x][z].Get();

                    for (int y = 0; y < hallwayAsRects.Count; y++)
                    {

                        GameObject cor = new GameObject();
                        Vector3 pos = cor.transform.position;
                        pos.x = hallwayAsRects[y].x;
                        pos.y = hallwayAsRects[y].y;
                        pos.z = 10;
                        cor.transform.localPosition = pos;

                        Vector2 sca = cor.transform.localScale;
                        sca.x = hallwayAsRects[y].width;
                        sca.y = hallwayAsRects[y].height;
                        cor.transform.localScale = sca;

                        SpriteRenderer render = cor.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
                        render.material = mat;
                        render.sprite = sprite;
                    }
                }

            }
        }
    }

    void Split(int iterations, Node node)
    {
        RectInt sizes = node.GetParam(false);

        if (iterations > 0 && (sizes.width >= 2 * MIN || sizes.height >= 2 * MIN))
        {
            
            bool splitH = Random.value > 0.5;
            if ((sizes.width > sizes.height && sizes.width / sizes.height >= 1) && sizes.width >= 2 * MIN) splitH = false;
            else if ((sizes.height > sizes.width && sizes.height / sizes.width >= 1) && sizes.height >= 2 * MIN) splitH = true;
            node.SetParam(true);
            if (splitH)
            {
                int split = (sizes.height > 2 * MIN ? (int) Random.Range(sizes.height * 0.4f, sizes.height * 0.6f) : MIN);
                node.lChild.SetParam(new RectInt(sizes.x, sizes.y, sizes.width, split), false);

                node.rChild.SetParam(new RectInt(sizes.x, sizes.y + (iterations == iteration ? split + 32 : split), sizes.width, sizes.height - split), false);

                if(iterations == iteration)
                {
                    safeRoom.y = split + 2;
                    safeRoom.height = 28;
                    safeRoom.width = 15;
                    safeRoom.x = 100500;
                }
            }
            else
            {
                int split = (sizes.width > 2 * MIN ? (int) Random.Range(sizes.width*0.4f, sizes.width * 0.6f) : MIN);
                node.lChild.SetParam(new RectInt(sizes.x, sizes.y, split, sizes.height), false);

                node.rChild.SetParam( new RectInt(sizes.x + (iterations == iteration ? split + 32 : split), sizes.y, sizes.width - split, sizes.height), false);

                if (iterations == iteration)
                {
                    safeRoom.x = split + 2;
                    safeRoom.width = 28;
                    safeRoom.height = 15;
                    safeRoom.y = 100500;
                }
            }

            Split(iterations - 1, node.lChild);
            Split(iterations - 1, node.rChild);
            
        }
        else
        {
            nodes.Add(node);
            CreateRooms(node);
        }
    }
    void CreateRooms(Node node)
    {
        RectInt sizes = node.GetParam(false);
        int randX = Random.Range(2, sizes.width / 4);
        int randY = Random.Range(2, sizes.height / 4);
        int randW = (int) (randX * Random.Range(1f, 2f));
        //randW = randW < sizes.width / 10 ? sizes.width - 2 : randW;
        int randH = (int) (randY * Random.Range(1f, 2f));
        //randH = randH < sizes.height / 10 ? sizes.height - 2 : randH;
        node.SetParam(new RectInt(sizes.x + randX, sizes.y + randY, (sizes.width - 4 < 4 ? 4 : (sizes.width - 4 > randW ? sizes.width - randW : sizes.width - 4)), (sizes.height - 4 < 4 ? 4 : (sizes.height - 4 > randH ? sizes.height - randH : sizes.height - 4))), true);
        
    }
    void CreateCorridors(Node node, bool sR = false)
    {
        if (node.lChild != null)
        {

            Node left = FindLeaf(node.lChild);
            Node right = FindLeaf(node.rChild);

            RectInt sizesL = left.GetParam(true);
            RectInt sizesR = right.GetParam(true);

            if (sizesL.x == sizesR.x || sizesL.y == sizesR.y)
            {
                if (sizesL.x != sizesR.x)
                {
                    Node.Hallways hallway = new Node.Hallways();
                    hallway.Add(new RectInt(sizesL.x + sizesL.width / 2, sizesL.y + sizesL.height / 2, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2) + 1), 1), nodes.IndexOf(left));
                    RenderHallway(hallway);
                    if (sR)
                    {
                        safeRoom.y = sizesL.y + sizesL.height / 2;
                        nodes[32].SetParam(safeRoom, false);
                        nodes[32].SetParam(safeRoom, true);
                        RenderRoom(nodes[32]);
                    }

                    int index = FindAllRooms(hallway);
                    left.SetConnection(index, hallway, nodes);

                }
                else if (sizesL.y != sizesR.y)
                {
                    Node.Hallways hallway = new Node.Hallways();
                    hallway.Add(new RectInt(sizesL.x + sizesL.width / 2, sizesL.y + sizesL.height / 2, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)), nodes.IndexOf(left));
                    RenderHallway(hallway);
                    if (sR)
                    {
                        safeRoom.x = sizesL.x + sizesL.width / 2;
                        nodes[32].SetParam(safeRoom, false);
                        nodes[32].SetParam(safeRoom, true);
                        RenderRoom(nodes[32]);
                    }

                    int index = FindAllRooms(hallway);
                    left.SetConnection(index, hallway, nodes);

                }
            }
            else if ((sizesL.x + sizesL.width >= sizesR.x || sizesR.x + sizesR.width >= sizesL.x) || Random.value > 0.5 && (sizesL.y + sizesL.height < sizesR.y || sizesR.y + sizesR.height < sizesL.y))
            {

                Node.Hallways hallway = new Node.Hallways();
                hallway.Add(new RectInt(sizesL.width / 2 + sizesL.x, sizesL.height / 2 + sizesL.y, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)), nodes.IndexOf(left));

                int index = FindAllRooms(hallway);
                if (index != nodes.IndexOf(left)) left.SetConnection(index, hallway, nodes);

                List<RectInt> halls = hallway.Get();
                hallway.Add(new RectInt(halls[0].x, halls[0].y + halls[0].height - 1, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2) + 1), 1));
                RenderHallway(hallway);

                if (sR)
                {
                    if (safeRoom.x == 100500) safeRoom.x = sizesL.width / 2 + sizesL.x;
                    else if (safeRoom.y == 100500) safeRoom.y = halls[0].y + halls[0].height;
                    nodes[32].SetParam(safeRoom, false);
                    nodes[32].SetParam(safeRoom, true);
                    RenderRoom(nodes[32]);
                }

                int connection = FindAllRooms(hallway);
                if (connection != hallway.LastIndex()) nodes[hallway.LastIndex()].SetConnection(connection, hallway, nodes);

                
            }
            else
            {
                Node.Hallways hallway = new Node.Hallways();
                hallway.Add(new RectInt(sizesL.x + sizesL.width / 2, sizesL.y + sizesL.height / 2, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2) + 1), 1), nodes.IndexOf(left));

                int index = FindAllRooms(hallway);
                if (index != nodes.IndexOf(left)) left.SetConnection(index, hallway, nodes);

                List<RectInt> halls = hallway.Get();
                hallway.Add(new RectInt(halls[0].x + halls[0].width - 1, halls[0].y, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)));
                RenderHallway(hallway);

                if (sR)
                {
                    if (safeRoom.x == 100500) safeRoom.x = halls[0].x + halls[0].width;
                    else if (safeRoom.y == 100500) safeRoom.y = halls[0].y;
                    nodes[32].SetParam(safeRoom, false);
                    nodes[32].SetParam(safeRoom, true);
                    RenderRoom(nodes[32]);
                }

                int connection = FindAllRooms(hallway);
                if (connection != hallway.LastIndex()) nodes[hallway.LastIndex()].SetConnection(connection, hallway, nodes);

                
            }
            CreateCorridors(node.lChild);
            CreateCorridors(node.rChild);
        }

    }
    Node FindLeaf(Node node)
    {
        Node leaf;
        if (node.lChild != null)
        {
            if (Random.value > 0.5) leaf = FindLeaf(node.lChild);
            else leaf = FindLeaf(node.rChild);
        }
        else leaf = node;

        return leaf;
    }
    Node FindLeaf(Node node, Vector2 pos)
    {
        Node leaf = null;
        if (node.lChild != null)
        {
            if (pos == node.lChild)
            {
                leaf = FindLeaf(node.lChild, pos);
            }
            else if (pos == node.rChild)
            {
                leaf = FindLeaf(node.rChild, pos);
            }
        }
        else
        {
            leaf = node;
        }
        return leaf;
    }
    int FindAllRooms(Node.Hallways hallway,  int size = -100500)
    {

        List<RectInt> halls = hallway.Get();
        RectInt theHall = halls[halls.Count - 1];
        RectInt sizes = nodes[hallway.LastIndex()].GetParam(false);
        int absSize;
        if (Mathf.Abs(theHall.width) > Mathf.Abs(theHall.height))
        {
            absSize = Mathf.Abs(theHall.width) / theHall.width;
            if (size == -100500) size = (theHall.width > 0 ? (sizes.width + sizes.x - theHall.x) + 1 : sizes.x - theHall.x - 1);
            if(Mathf.Abs(size) <= Mathf.Abs(theHall.width))
            {
                Node next = FindLeaf(parent, new Vector2(theHall.x + size, theHall.y));
                if (next == null)
                {
                    next = FindLeaf(parent, new Vector2((size + (absSize * 32) + theHall.x), theHall.y));
                    size += 32;
                    nodes[hallway.LastIndex()].SetConnection(32, hallway, nodes);
                    hallway.Add(32);
                    RectInt nextSize = next.GetParam(false);
                    if (Mathf.Abs(size + (absSize * nextSize.width)) < Mathf.Abs(theHall.width))
                    {
                        FindAllRooms(hallway, size + (absSize * nextSize.width));
                    }
                    return nodes.IndexOf(next);
                }
                else if (next.CrossesRoom(theHall))
                {
                    RectInt nextSize = next.GetParam(false);
                    nodes[hallway.LastIndex()].SetConnection(nodes.IndexOf(next), hallway, nodes);
                    hallway.Add(nodes.IndexOf(next));
                    if (Mathf.Abs(size + (absSize * nextSize.width)) < Mathf.Abs(theHall.width))
                    {
                        FindAllRooms(hallway, size + (absSize * nextSize.width));
                    }
                    return nodes.IndexOf(next);
                }
                else return hallway.LastIndex();
            }
            else return hallway.LastIndex();
        }
        else
        {
            absSize = Mathf.Abs(theHall.height) / theHall.height;
            if (size == -100500) size = (theHall.height > 0 ? (sizes.height + sizes.y - theHall.y) + 1: sizes.y - theHall.y - 1);
            if(Mathf.Abs(size) < Mathf.Abs(theHall.height))
            {
                Node next = FindLeaf(parent, new Vector2(theHall.x, theHall.y + size));
                if (next == null)
                {
                    next = FindLeaf(parent, new Vector2(theHall.x, (size + (absSize * 32) + theHall.y)));
                    size += 32;
                    nodes[hallway.LastIndex()].SetConnection(32, hallway, nodes);
                    hallway.Add(32);
                    RectInt nextSize = next.GetParam(false);
                    if (Mathf.Abs(size + (absSize * nextSize.width)) < Mathf.Abs(theHall.width))
                    {
                        FindAllRooms(hallway, size + (absSize * nextSize.width));
                    }
                    return nodes.IndexOf(next);
                }

                else if (next.CrossesRoom(theHall))
                {
                    RectInt nextSize = next.GetParam(false);
                    nodes[hallway.LastIndex()].SetConnection(nodes.IndexOf(next), hallway, nodes);
                    hallway.Add(nodes.IndexOf(next));
                    if (Mathf.Abs(size + (absSize * nextSize.height)) < Mathf.Abs(theHall.height))
                    {
                        FindAllRooms(hallway, size + (absSize * nextSize.height));
                    }
                    return nodes.IndexOf(next);
                }
                else return hallway.LastIndex();
            }
            else return hallway.LastIndex();
        }
    }
    void RenderRoom(Node node)
    {
        RectInt roomPosition = node.GetParam(true);
        for(int x = roomPosition.x; x < roomPosition.width + roomPosition.x; x++)
        {
            for(int y = roomPosition.y; y < roomPosition.height + roomPosition.y; y++)
            {
                map.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
        SetWalls(node);
    }
    void SetWalls(Node node)
    {
        RectInt roomPosition = node.GetParam(true);
        for (int x = roomPosition.x; x < roomPosition.width + roomPosition.x; x++)
        {
            for (int y = roomPosition.y; y < roomPosition.height + roomPosition.y; y++)
            {
                map.SetTile(new Vector3Int(x, y, 0), CheckTilesAround(new Vector3Int(x, y, 0)));
            }
        }
    }
    Tile CheckTilesAround(Vector3Int middleTransform, int side = 0)
    {
        var middle = map.GetTile(middleTransform);
        if (middle == null) return null;

        var bottomLeft = map.GetTile(new Vector3Int(middleTransform.x - 1, middleTransform.y-1, 0));
        var bottomMiddle = map.GetTile(new Vector3Int(middleTransform.x, middleTransform.y - 1, 0));
        var bottomRight = map.GetTile(new Vector3Int(middleTransform.x + 1, middleTransform.y - 1, 0));

        var middleLeft = map.GetTile(new Vector3Int(middleTransform.x - 1, middleTransform.y, 0));
        var middleRight = map.GetTile(new Vector3Int(middleTransform.x + 1, middleTransform.y, 0));

        var topLeft = map.GetTile(new Vector3Int(middleTransform.x - 1, middleTransform.y + 1, 0));
        var topMiddle = map.GetTile(new Vector3Int(middleTransform.x, middleTransform.y + 1, 0));
        var topRight = map.GetTile(new Vector3Int(middleTransform.x + 1, middleTransform.y + 1, 0));

        if (side == 0 || side == -1)
        {
            if (middleLeft == null && bottomMiddle == null) return bottomLeftTile;
            if (bottomMiddle == null && middleLeft != null && middleRight != null) return bottomMiddleTile;
            if (bottomMiddle == null && middleRight == null) return bottomRightTile;

            if (middleLeft == null && topMiddle != null && bottomMiddle != null) return middleLeftTile;
        }
        if(side == 0 || side == 1)
        {
            if (middleRight == null && topMiddle != null && bottomMiddle != null) return middleRightTile;

            if (middleLeft == null && topMiddle == null) return topLeftTile;
            if (topMiddle == null && middleRight != null && middleRight != null) return topMiddleTile;
            if (topMiddle == null && middleRight == null) return topRightTile;
        }
        if(side == 0)
        {
            if (topLeft == null && topMiddle != null && middleLeft != null) return innerTopLeftTile;
            if (topRight == null && topMiddle != null && middleRight != null) return innerTopRightTile;
            if (bottomLeft == null && bottomMiddle != null && middleLeft != null) return innerBottomLeftTile;
            if (bottomRight == null && bottomMiddle != null && middleRight != null) return innerBottomRightTile;
        }
        return middleTile;
    }

    void RenderHallway(Node.Hallways hallway)
    {
        List<RectInt> halls = hallway.Get();
        for(int i = 0; i < halls.Count; i++)
        {
            for(int x = halls[i].x; x < halls[i].width + halls[i].x; x++)
            {
                for(int y = halls[i].y; y < halls[i].y + halls[i].height; y++)
                {
                    map.SetTile(new Vector3Int(x, y, 0), middleTile);
                }
            }
        }
        for (int i = 0; i < halls.Count; i++)
        {
            for (int x = halls[i].x; x < halls[i].width + halls[i].x; x++)
            {
                for (int y = halls[i].y; y < halls[i].y + halls[i].height; y++)
                {
                    Tile leftTile = CheckTilesAround(new Vector3Int(x, y, 0), -1);
                    Tile rightTile = CheckTilesAround(new Vector3Int(x, y, 0), 1);

                    if(leftTile != null)
                    {
                        if (leftTile == bottomLeftTile)
                        {
                            map.SetTile(new Vector3Int(x - 1, y - 1, 0), bottomLeftTile);
                            map.SetTile(new Vector3Int(x, y - 1, 0), bottomMiddleTile);
                            map.SetTile(new Vector3Int(x - 1, y, 0), middleLeftTile);
                        }
                        else if (leftTile == bottomRightTile)
                        {
                            map.SetTile(new Vector3Int(x + 1, y - 1, 0), bottomRightTile);
                            map.SetTile(new Vector3Int(x, y - 1, 0), bottomMiddleTile);
                            map.SetTile(new Vector3Int(x + 1, y, 0), middleRightTile);
                        }
                        else if (leftTile == bottomMiddleTile)
                        {
                            map.SetTile(new Vector3Int(x, y - 1, 0), bottomMiddleTile);
                        }
                        else if (leftTile == middleLeftTile)
                        {
                            map.SetTile(new Vector3Int(x - 1, y, 0), middleLeftTile);
                        }
                    }
                    if(rightTile != null)
                    {
                        if(rightTile == topLeftTile)
                        {
                            map.SetTile(new Vector3Int(x - 1, y + 1, 0), topLeftTile);
                            map.SetTile(new Vector3Int(x, y + 1, 0), topMiddleTile);
                            map.SetTile(new Vector3Int(x - 1, y, 0), middleLeftTile);
                        }
                        if (rightTile == topRightTile)
                        {
                            map.SetTile(new Vector3Int(x + 1, y + 1, 0), topRightTile);
                            map.SetTile(new Vector3Int(x, y + 1, 0), topMiddleTile);
                            map.SetTile(new Vector3Int(x + 1, y, 0), middleRightTile);
                        }
                        if(rightTile == topMiddleTile)
                        {
                            map.SetTile(new Vector3Int(x, y + 1, 0), topMiddleTile);
                        }
                        if (rightTile == middleRightTile)
                        {
                            map.SetTile(new Vector3Int(x + 1, y, 0), middleRightTile);
                        }
                    }
                }
            }
        }
    }
}
class Node
{
    readonly Hashtable connections = new Hashtable();
    bool isSplit = false;
    RectInt transform;
    RectInt transformRoom;
    List<Vector2Int> doors = new List<Vector2Int>();

    public Node lChild { get; private set; }
    public Node rChild { get; private set; }

    public bool CrossesRoom(RectInt hall)
    {
        if (hall.y >= transformRoom.y - 1 && hall.height == 1 && hall.y <= transformRoom.height + transformRoom.y + 1)
        {
            if (hall.x >= transformRoom.x - 1 && hall.x < transformRoom.x + transformRoom.width + 1) return true;
            else
            {
                int difference = (hall.x >= transformRoom.x - 1) ? hall.x - transformRoom.x + transformRoom.width + 1 : hall.x - transformRoom.x - 1;
                if (hall.width > 0 ? hall.width - difference >= 0 : hall.width - difference <= 0) return true;
                else return false;
            }
        }
        else if (hall.x >= transformRoom.x - 1 && hall.width == 1 && hall.x <= transformRoom.width + transformRoom.x + 1)
        {
            if (hall.y >= transformRoom.y - 1 && hall.y <= transformRoom.y + transformRoom.height + 1) return true;
            else
            {
                int difference = (hall.y >= transformRoom.y - 1) ? hall.y - transformRoom.y + transformRoom.height + 1 : hall.y - transformRoom.y - 1;
                if (hall.height > 0 ? hall.height - difference >= 0 : hall.height - difference <= 0) return true;
                else return false;
            }
        }
        else return false;
    }
    public Hashtable GetConnections() => connections;
    public void SetConnection(int index, Hallways hall, List<Node> nodes = null)
    {
        if (!connections.ContainsKey(index))
        {
            Hallways[] halls = { hall };
            connections.Add(index, halls);
            AddDoor(hall);

            if (nodes != null) nodes[index].SetConnection(nodes.IndexOf(this), hall);
        }
        else
        {
            Hallways[] hallways = (Hallways[])connections[index];
            if(hallways.Length > 2)
            {
                List<int> indexes = hall.GetIndexes();
                if(indexes.Count < 2)
                {
                    Hallways[] halls = new Hallways[hallways.Length + 1];
                    hallways.CopyTo(halls, 0);
                    halls[halls.Length - 1] = hall;
                    connections.Remove(index);
                    connections.Add(index, halls);
                    AddDoor(hall);
                }
            }
            
        }
    }
    void AddDoor(Hallways hallway) //Данный метод добавляет в список дверей данной комнаты новую дверь, ведущую в корридор
    {
        int x = 0, y = 0;
        List<RectInt> hallAsRects = hallway.Get();
        RectInt rectCrossesTheRoom = hallAsRects[hallAsRects.Count - 1]; //Учитывается только последний прямоугольник корридора, тк данный метод вызывается из метода SetConnection, который работает только с последним добавленным прямоугольником
        bool crossesByX = (rectCrossesTheRoom.width == 1 ? true : false);

        int roomScalar = (crossesByX?
            rectCrossesTheRoom.y < transformRoom.y ? transformRoom.y : transformRoom.y + transformRoom.height 
            : rectCrossesTheRoom.x < transformRoom.x ? transformRoom.x : transformRoom.x + transformRoom.width);

        int hallwayScalar = (crossesByX? rectCrossesTheRoom.x : rectCrossesTheRoom.y);
        x = (crossesByX ? hallwayScalar : roomScalar);
        y = (crossesByX ? roomScalar : hallwayScalar);

        doors.Add(new Vector2Int(x, y));
    }
    public List<Vector2Int> GetDoors() => doors;
    public RectInt GetParam(bool room = false) => (room ? transformRoom : transform);
    public void SetParam(RectInt transf, bool room = false)
    {
        if (room)
        {
            transformRoom = transf;
        }
        else
        {
            transform = transf;
        }

    }
    public void SetParam(bool split)
    {
        isSplit = split;
        lChild = new Node();
        rChild = new Node();
    }
    public struct Hallways
    {
        List<RectInt> halls;
        List<int> indexes;

        public void Add(RectInt hall, int index)
        {
            if (halls == null) halls = new List<RectInt>();
            if(indexes == null) indexes = new List<int>();
            halls.Add(hall);
            indexes.Add(index);
        }
        public void Add(RectInt hall)
        {
            if (halls == null) halls = new List<RectInt>();
            halls.Add(hall);
        }
        public void Add(int index)
        {
            if (indexes == null) indexes = new List<int>();
            indexes.Add(index);
        }
        public List<RectInt> Get() => halls;
        public int LastIndex() => indexes[indexes.Count - 1];
        public List<int> GetIndexes() => indexes;
    }

    public static bool operator ==(Vector2 hallway, Node room) //Переделай в обычный метод
    {
        RectInt size = room.GetParam(false);
        return ((hallway.x >= size.x && hallway.x <= size.x + size.width) && (hallway.y >= size.y && hallway.y <= size.y + size.height));
    }
    public static bool operator !=(Vector2 hallway, Node room)
    {
        RectInt size = room.GetParam(false);
        return ((hallway.x >= size.x && hallway.x <= size.x + size.width) && (hallway.y >= size.y && hallway.y <= size.y + size.height));
    }
}