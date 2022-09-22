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
                    RenderHallway(hallwaysList[x][z]);
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
            node.SetParam();
            if (splitH)
            {
                int split = (sizes.height > 2 * MIN ? (int) Random.Range(sizes.height * 0.4f, sizes.height * 0.6f) : MIN);
                node.LChild.SetParam(new RectInt(sizes.x, sizes.y, sizes.width, split), false);

                node.RChild.SetParam(new RectInt(sizes.x, sizes.y + (iterations == iteration ? split + 32 : split), sizes.width, sizes.height - split), false);

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
                node.LChild.SetParam(new RectInt(sizes.x, sizes.y, split, sizes.height), false);

                node.RChild.SetParam( new RectInt(sizes.x + (iterations == iteration ? split + 32 : split), sizes.y, sizes.width - split, sizes.height), false);

                if (iterations == iteration)
                {
                    safeRoom.x = split + 2;
                    safeRoom.width = 28;
                    safeRoom.height = 15;
                    safeRoom.y = 100500;
                }
            }

            Split(iterations - 1, node.LChild);
            Split(iterations - 1, node.RChild);
            
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
        //RenderRoom(node);
        
    }
    void CreateCorridors(Node node, bool sR = false)
    {
        if (node.LChild != null)
        {

            Node left = FindLeaf(node.LChild);
            Node right = FindLeaf(node.RChild);

            RectInt sizesL = left.GetParam(true);
            RectInt sizesR = right.GetParam(true);

            if (sizesL.x == sizesR.x || sizesL.y == sizesR.y)
            {
                if (sizesL.x != sizesR.x)
                {
                    Node.Hallways hallway = new Node.Hallways();
                    hallway.Add(new RectInt(sizesL.x + sizesL.width / 2, sizesL.y + sizesL.height / 2, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2) + 1), 1));
                    if (sR)
                    {
                        safeRoom.y = sizesL.y + sizesL.height / 2;
                        nodes[32].SetParam(safeRoom, false);
                        nodes[32].SetParam(safeRoom, true);
                        RenderRoom(nodes[32]);
                    }

                    FindAllRooms(ref hallway, parent);
                }
                else if (sizesL.y != sizesR.y)
                {
                    Node.Hallways hallway = new Node.Hallways();
                    hallway.Add(new RectInt(sizesL.x + sizesL.width / 2, sizesL.y + sizesL.height / 2, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)));
                    if (sR)
                    {
                        safeRoom.x = sizesL.x + sizesL.width / 2;
                        nodes[32].SetParam(safeRoom, false);
                        nodes[32].SetParam(safeRoom, true);
                        RenderRoom(nodes[32]);
                    }

                    FindAllRooms(ref hallway, parent);
                }
            }
            else if ((sizesL.x + sizesL.width >= sizesR.x || sizesR.x + sizesR.width >= sizesL.x) || Random.value > 0.5 && (sizesL.y + sizesL.height < sizesR.y || sizesR.y + sizesR.height < sizesL.y))
            {

                Node.Hallways hallway = new Node.Hallways();
                hallway.Add(new RectInt(sizesL.width / 2 + sizesL.x, sizesL.height / 2 + sizesL.y, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)));
                FindAllRooms(ref hallway, parent);
                List<RectInt> halls = hallway.Get();
                if (sR)
                {
                    if (safeRoom.x == 100500) safeRoom.x = sizesL.width / 2 + sizesL.x;
                    else if (safeRoom.y == 100500) safeRoom.y = halls[0].y + halls[0].height;
                    nodes[32].SetParam(safeRoom, false);
                    nodes[32].SetParam(safeRoom, true);
                    RenderRoom(nodes[32]);
                }

                hallway.Add(new RectInt(halls[0].x, halls[0].y + halls[0].height - 1, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2) + 1), 1));

                FindAllRooms(ref hallway, parent);
            }
            else
            {
                Node.Hallways hallway = new Node.Hallways();
                hallway.Add(new RectInt(sizesL.x + sizesL.width / 2, sizesL.y + sizesL.height / 2, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2) + 1), 1));
                FindAllRooms(ref hallway, parent);
                List<RectInt> halls = hallway.Get();
                if (sR)
                {
                    if (safeRoom.x == 100500) safeRoom.x = halls[0].x + halls[0].width;
                    else if (safeRoom.y == 100500) safeRoom.y = halls[0].y;
                    nodes[32].SetParam(safeRoom, false);
                    nodes[32].SetParam(safeRoom, true);
                    RenderRoom(nodes[32]);
                }
                hallway.Add(new RectInt(halls[0].x + halls[0].width - 1, halls[0].y, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)));
                FindAllRooms(ref hallway, parent);
            }
            CreateCorridors(node.LChild);
            CreateCorridors(node.RChild);
        }

    }
    Node FindLeaf(Node node)
    {
        Node leaf;
        if (node.LChild != null)
        {
            if (Random.value > 0.5) leaf = FindLeaf(node.LChild);
            else leaf = FindLeaf(node.RChild);
        }
        else leaf = node;

        return leaf;
    }
    Node FindLeaf(Node node, Vector2 pos)
    {
        Node leaf = null;
        if (node.LChild != null)
        {
            if (pos == node.LChild)
            {
                leaf = FindLeaf(node.LChild, pos);
            }
            else if (pos == node.RChild)
            {
                leaf = FindLeaf(node.RChild, pos);
            }
        }
        else
        {
            leaf = node;
        }
        return leaf;
    }
    
    void FindAllRooms(ref Node.Hallways hallway, Node node)
    {
        List<RectInt> hallwayAsRects = hallway.Get();
        RectInt hallAsRect = hallwayAsRects[hallwayAsRects.Count-1];
        if (node.LChild != null)
        {
            RectInt left = node.LChild.GetParam(false);
            RectInt right = node.RChild.GetParam(false);
            bool leftIsCrossed = HallwayCrossesRect(left, hallAsRect);
            bool rightIsCrossed = HallwayCrossesRect(right, hallAsRect);

            if (leftIsCrossed) FindAllRooms(ref hallway, node.LChild);
            if (node == parent && leftIsCrossed && rightIsCrossed) FindAllRooms(ref hallway, nodes[32]);
            if (rightIsCrossed) FindAllRooms(ref hallway, node.RChild);
        }
        else
        {
            RectInt roomRect = node.GetParam(true);
            if (HallwayCrossesRect(roomRect, hallAsRect)) hallway.Add(nodes.IndexOf(node));
        }


        if (node == parent)
        {
            List<int> indexesOfCrossedRooms = hallway.GetIndexes();
            for(int i = 0; i < indexesOfCrossedRooms.Count; i++)
            {
                if(i < indexesOfCrossedRooms.Count - 1)nodes[indexesOfCrossedRooms[i]].SetConnection(indexesOfCrossedRooms[i+1], hallway, nodes, false);
            }
        }
    }
    bool HallwayCrossesRect(RectInt rect, RectInt hallAsRect)
    {
        int hallScalar;
        Vector2Int longHallSide;
        Vector2Int perpendiculToHallRoomSide;
        Vector2Int parallelToHallRoomSide;
        if (hallAsRect.height != 1)
        {
            hallScalar = hallAsRect.x;
            longHallSide = new Vector2Int(hallAsRect.y, hallAsRect.height);
            perpendiculToHallRoomSide = new Vector2Int(rect.x, rect.width);
            parallelToHallRoomSide = new Vector2Int(rect.y, rect.height);
        }
        else
        {
            hallScalar = hallAsRect.y;
            longHallSide = new Vector2Int(hallAsRect.x, hallAsRect.width);
            perpendiculToHallRoomSide = new Vector2Int(rect.y, rect.height);
            parallelToHallRoomSide = new Vector2Int(rect.x, rect.width);
        }
        bool positiveHallLength = longHallSide[1] > 0;
        if (hallScalar >= perpendiculToHallRoomSide[0] && hallScalar <= perpendiculToHallRoomSide[1] + perpendiculToHallRoomSide[0])
        {
            return positiveHallLength ? (longHallSide[0] <= parallelToHallRoomSide[0] + parallelToHallRoomSide[1] && longHallSide[0] + longHallSide[1] >= parallelToHallRoomSide[0] + parallelToHallRoomSide[1])
                || (longHallSide[0] <= parallelToHallRoomSide[0] && longHallSide[0] + longHallSide[1] >= parallelToHallRoomSide[0])
                || (longHallSide[0] >= parallelToHallRoomSide[0] && longHallSide[0] + longHallSide[1] <= parallelToHallRoomSide[0] + parallelToHallRoomSide[1]) ? true : false
                : (longHallSide[0] >= parallelToHallRoomSide[0] + parallelToHallRoomSide[1] && longHallSide[0] + longHallSide[1] <= parallelToHallRoomSide[0] + parallelToHallRoomSide[1])
                || (longHallSide[0] >= parallelToHallRoomSide[0] && longHallSide[0] + longHallSide[1] <= parallelToHallRoomSide[0])
                || (longHallSide[0] <= parallelToHallRoomSide[0] + parallelToHallRoomSide[1] && longHallSide[0] + longHallSide[1] >= parallelToHallRoomSide[0]) ? true : false; 
        }
        else return false;
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
                //map.SetTile(new Vector3Int(x, y, 0), CheckTilesAround(new Vector3Int(x, y, 0)));
                Vector3Int currentTile = new Vector3Int(x, y, 0);
                if ((x == roomPosition.x || x == roomPosition.x + roomPosition.width - 1) && y == roomPosition.y) map.SetTile(currentTile, x == roomPosition.x ? bottomLeftTile : bottomRightTile);
                else if (x == roomPosition.x || x == roomPosition.x + roomPosition.width - 1)
                {
                    if (y == roomPosition.y + roomPosition.height - 1)
                    {
                        map.SetTile(currentTile, x == roomPosition.x ? topLeftTile : topRightTile);
                    }
                    else
                    {
                        map.SetTile(currentTile, x == roomPosition.x ? middleLeftTile : middleRightTile);
                    }
                }
                else if (y == roomPosition.y || y == roomPosition.y + roomPosition.height - 1)
                {
                    if (x == roomPosition.x + roomPosition.width - 1)
                    {
                        map.SetTile(currentTile, y == roomPosition.y ? bottomRightTile : topRightTile);
                    }
                    else
                    {
                        map.SetTile(currentTile, y == roomPosition.y ? bottomMiddleTile : topMiddleTile);
                    }
                }
                else map.SetTile(currentTile, middleTile);
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
            for(int x = halls[i].x; halls[i].width > 0 ? x < halls[i].width + halls[i].x : x > halls[i].width + halls[i].x ; x += halls[i].width < 0 ? -1 : 1 )
            {
                for(int y = halls[i].y; halls[i].height > 0 ? y < halls[i].y + halls[i].height : y > halls[i].y + halls[i].height; y += halls[i].height < 0 ? -1 : 1)
                {
                    map.SetTile(new Vector3Int(x, y, 0), middleTile);
                }
            }
        }
        for (int i = 0; i < halls.Count; i++)
        {
            for (int x = halls[i].x; halls[i].width > 0 ? x < halls[i].width + halls[i].x : x > halls[i].width + halls[i].x; x += halls[i].width < 0 ? -1 : 1)
            {
                for (int y = halls[i].y; halls[i].height > 0 ? y < halls[i].y + halls[i].height : y > halls[i].y + halls[i].height; y += halls[i].height < 0 ? -1 : 1)
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
    //bool isSplit = false;
    RectInt transform;
    RectInt transformRoom;
    readonly List<Vector2Int> doors = new List<Vector2Int>();

    public Node LChild { get; private set; }
    public Node RChild { get; private set; }

    public Hashtable GetConnections() => connections;
    public void SetConnection(int index, Hallways hall, List<Node> nodes = null, bool upper = false)    {
        //if (nodes != null && index != nodes.IndexOf(this))
        //{
            if (nodes != null)
            {
                RectInt neighborRoom = nodes[index].GetParam(true);
                upper = neighborRoom.x < transformRoom.x + transformRoom.width && neighborRoom.y < transformRoom.y + transformRoom.height;
            }
            else upper = !upper;
            if (!connections.ContainsKey(index))
            {
                Hallways[] halls = { hall };
                connections.Add(index, halls);
                AddDoor(hall, upper);

                if (nodes != null) nodes[index].SetConnection(nodes.IndexOf(this), hall, null, upper);
            }
            else
            {
                Hallways[] hallways = (Hallways[])connections[index];
                if (hallways.Length > 2)
                {
                    List<int> indexes = hall.GetIndexes();
                    //if (indexes.Count > 2)
                    //{
                        Hallways[] halls = new Hallways[hallways.Length + 1];
                        hallways.CopyTo(halls, 0);
                        halls[halls.Length - 1] = hall;
                        connections.Remove(index);
                        connections.Add(index, halls);
                        AddDoor(hall, upper);
                    //}
                }

            //}
       }
        
    }
    void AddDoor(Hallways hallway, bool upper) //Данный метод добавляет в список дверей данной комнаты новую дверь, ведущую в корридор
    {
        int x, y;
        List<RectInt> hallAsRects = hallway.Get();
        RectInt rectCrossesTheRoom = hallAsRects[hallAsRects.Count - 1]; //Учитывается только последний прямоугольник корридора, тк данный метод вызывается из метода SetConnection, который работает только с последним добавленным прямоугольником
        bool crossesByX = rectCrossesTheRoom.width == 1;

        int roomScalar = (crossesByX?
            upper ? transformRoom.y : transformRoom.y + transformRoom.height - 1 
            : upper ? transformRoom.x : transformRoom.x + transformRoom.width -1);

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
    public void SetParam()
    {
        //isSplit = split;
        LChild = new Node();
        RChild = new Node();
    }
    public struct Hallways
    {
        List<RectInt> halls;
        List<int> indexes;

        public void Add(RectInt hall)
        {
            if (halls == null) halls = new List<RectInt>();
            halls.Add(hall);
        }
        public void Add(int index)
        {
            if (indexes == null) indexes = new List<int>();
            if ((indexes.Count != 0 && indexes[indexes.Count - 1] != index) || indexes.Count == 0 ) indexes.Add(index);
        }
        public List<RectInt> Get() => halls;
        public int LastIndex() => indexes[indexes.Count - 1];
        public List<int> GetIndexes() => indexes;
        public RectInt GetLastHallRect() => halls[halls.Count - 1];
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