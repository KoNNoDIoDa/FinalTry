using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepByStep : MonoBehaviour
{
    
    List <Node> nodes = new List <Node>();
    public Material mat;
    public Sprite sprite;
    const int MIN = 7;
    int iteration = 5;
    Node parent;
    public bool showRooms = true;
    RectInt safeRoom = new RectInt();

    int exception = 0;
    // Start is called before the first frame update
    void Start()
    {
        parent = new Node();
        parent.SetParam(new RectInt(0, 0, 90, 90), false);
        //parent.SetParam
        Split(iteration, parent);
        CreateCorridors(parent, true);
        for(int i = 0; i < nodes.Count; i++)
        {
            GameObject plate = new GameObject();
            Vector3 position = plate.transform.position;
            position.x = nodes[i].GetParam(showRooms).x; 
            position.y = nodes[i].GetParam(showRooms).y;
            position.z = -10;
            
            plate.transform.position = position;

            Vector2 scale = plate.transform.localScale;
            scale.x = nodes[i].GetParam(showRooms).width;
            scale.y = nodes[i].GetParam(showRooms).height;
            plate.transform.localScale = scale;

            SpriteRenderer rend = plate.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            rend.material = mat;
            rend.sprite = sprite;
            List<Node.Hallways> halls = nodes[i].GetHallways();
            for (int x = 0; x < halls.Count; x++)
            {
                List<RectInt> hall = halls[x].Get();
                for (int y = 0; y < hall.Count; y++)
                {
                    GameObject cor = new GameObject();
                    Vector2 pos = cor.transform.position;
                    pos.x = hall[y].x;
                    pos.y = hall[y].y;
                    cor.transform.localPosition = pos;

                    Vector2 sca = cor.transform.localScale;
                    sca.x = hall[y].width;
                    sca.y = hall[y].height;
                    cor.transform.localScale = sca;

                    SpriteRenderer render = cor.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
                    render.material = mat;
                    render.sprite = sprite;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
                    if (sR)
                    {
                        safeRoom.y = sizesL.y + sizesL.height / 2;
                        Node theRoom = new Node();
                        theRoom.SetParam(safeRoom, true);
                        nodes.Add(theRoom);
                    }

                    left.AddHall(hallway, nodes[hallway.LastIndex()].SetConnection(FindAllRooms(hallway)));

                }
                else if (sizesL.y != sizesR.y)
                {
                    Node.Hallways hallway = new Node.Hallways();
                    hallway.Add(new RectInt(sizesL.x + sizesL.width / 2, sizesL.y + sizesL.height / 2, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)), nodes.IndexOf(left));
                    if (sR)
                    {
                        safeRoom.x = sizesL.x + sizesL.width / 2;
                        Node theRoom = new Node();
                        theRoom.SetParam(safeRoom, true);
                        nodes.Add(theRoom);
                    }

                    left.AddHall(hallway, nodes[hallway.LastIndex()].SetConnection(FindAllRooms(hallway)));

                }
            }
            else if ((sizesL.x + sizesL.width >= sizesR.x || sizesR.x + sizesR.width >= sizesL.x) || Random.value > 0.5 && (sizesL.y + sizesL.height < sizesR.y || sizesR.y + sizesR.height < sizesL.y))
            {
                Node.Hallways hallway = new Node.Hallways();
                hallway.Add(new RectInt(sizesL.width / 2 + sizesL.x, sizesL.height / 2 + sizesL.y, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)), nodes.IndexOf(left));

                left.AddHall(hallway, nodes[hallway.LastIndex()].SetConnection(FindAllRooms(hallway)));

                List<RectInt> halls = hallway.Get();
                hallway.Add(new RectInt(halls[0].x, halls[0].y + halls[0].height - 1, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2) + 1), 1));

                nodes[hallway.LastIndex()].AddHall(hallway, nodes[hallway.LastIndex()].SetConnection(FindAllRooms(hallway)));

                if (sR)
                {
                    if (safeRoom.x == 100500) safeRoom.x = sizesL.width / 2 + sizesL.x;
                    else if (safeRoom.y == 100500) safeRoom.y = halls[0].y + halls[0].height;
                    Node theRoom = new Node();
                    theRoom.SetParam(safeRoom, true);
                    nodes.Add(theRoom);
                }
            }
            else
            {
                Node.Hallways hallway = new Node.Hallways();
                hallway.Add(new RectInt(sizesL.x + sizesL.width / 2, sizesL.y + sizesL.height / 2, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2) + 1), 1), nodes.IndexOf(left));

                left.AddHall(hallway, nodes[hallway.LastIndex()].SetConnection(FindAllRooms(hallway)));

                List<RectInt> halls = hallway.Get();
                hallway.Add(new RectInt(halls[0].x + halls[0].width - 1, halls[0].y, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)));

                nodes[hallway.LastIndex()].AddHall(hallway, nodes[hallway.LastIndex()].SetConnection(FindAllRooms(hallway)));
                if (sR)
                {
                    if (safeRoom.x == 100500) safeRoom.x = halls[0].x + halls[0].width;
                    else if (safeRoom.y == 100500) safeRoom.y = halls[0].y;
                    Node theRoom = new Node();
                    theRoom.SetParam(safeRoom, true);
                    nodes.Add(theRoom);
                }
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
                    exception++;
                    next = FindLeaf(parent, new Vector2((size + (absSize * 32) + theHall.x), theHall.y));
                    size += 32;
                }
                if (next.CrossesRoom(theHall))
                {
                    RectInt nextSize = next.GetParam(false);
                    int indexOfHall = hallway.LastIndex();
                    next.AddHall(hallway, next.SetConnection(hallway.LastIndex()));
                    hallway.Add(nodes.IndexOf(next));
                    DeleteUselessHallways(next, hallway, indexOfHall);
                    if (Mathf.Abs(size + (absSize * nextSize.width)) < Mathf.Abs(theHall.width))
                    {
                        int ind = FindAllRooms(hallway, size + (absSize * nextSize.width));
                        if (ind != hallway.LastIndex())
                        {
                            next.AddHall(hallway, next.SetConnection(ind));
                            DeleteUselessHallways(next, hallway, ind);
                        }
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
                    exception++;
                    next = FindLeaf(parent, new Vector2(theHall.x, (size + (absSize * 32) + theHall.y)));
                    size += 32;
                }

                if (next.CrossesRoom(theHall))
                {
                    RectInt nextSize = next.GetParam(false);
                    int indexOfHall = hallway.LastIndex();
                    next.AddHall(hallway, next.SetConnection(hallway.LastIndex()));
                    hallway.Add(nodes.IndexOf(next));
                    DeleteUselessHallways(next, hallway, indexOfHall);
                    if (Mathf.Abs(size + (absSize * nextSize.height)) < Mathf.Abs(theHall.height))
                    {
                        int ind = FindAllRooms(hallway, size + (absSize * nextSize.height));
                        if(ind != hallway.LastIndex())
                        {
                            next.AddHall(hallway, next.SetConnection(ind));
                            DeleteUselessHallways(next, hallway, ind);
                        }
                        
                    }
                    return nodes.IndexOf(next);
                }
                else return hallway.LastIndex();
            }
            else return hallway.LastIndex();
        }
    }
    void DeleteUselessHallways(Node node, Node.Hallways hallway, int index)
    {
        List<int> indexes = node.GetConnections();
        List<Node.Hallways> hallways = node.GetHallways();

        int sameConnection = 100500;
        int theIndex = indexes.IndexOf(index);

        if (indexes.Count - 1 > theIndex)
        {
            if (indexes[theIndex] == indexes[theIndex + 1])
            {
                sameConnection = indexes[theIndex + 1];
                if (hallways[theIndex].LengthOfIndexes() == 2)
                {
                    hallways.Remove(hallways[theIndex]);
                    indexes.Remove(indexes[theIndex]);
                }
                else if (hallways[theIndex + 1].LengthOfIndexes() == 2)
                {
                    hallways.Remove(hallways[theIndex + 1]);
                    indexes.Remove(indexes[theIndex + 1]);
                }
                else if (hallways[theIndex].LengthOfIndexes() > hallways[theIndex + 1].LengthOfIndexes())
                {
                    hallways.Remove(hallways[theIndex + 1]);
                    indexes.Remove(indexes[theIndex + 1]);
                }
                else
                {
                    hallways.Remove(hallways[theIndex]);
                    indexes.Remove(indexes[theIndex]);
                }
            }
        }
        else if (theIndex > 0)
        {
            if(indexes[theIndex] == indexes[theIndex - 1])
            {
                sameConnection = indexes[theIndex - 1];
                if (hallways[theIndex].LengthOfIndexes() == 2)
                {
                    hallways.Remove(hallways[theIndex]);
                    indexes.Remove(indexes[theIndex]);
                }
                else if (hallways[theIndex - 1].LengthOfIndexes() == 2)
                {
                    hallways.Remove(hallways[theIndex - 1]);
                    indexes.Remove(indexes[theIndex - 1]);
                }
                else if (hallways[theIndex].LengthOfIndexes() > hallways[theIndex - 1].LengthOfIndexes())
                {
                    hallways.Remove(hallways[theIndex - 1]);
                    indexes.Remove(indexes[theIndex - 1]);
                }
                else
                {
                    hallways.Remove(hallways[theIndex]);
                    indexes.Remove(indexes[theIndex]);
                }
            }
        }
        //if(sameConnection)
    }

}
class Node
{
    
    bool isSplit = false;
    RectInt transform;
    RectInt transformRoom;
    List<Hallways> hallways = new List<Hallways>();
    List<int> connected = new List<int>();

    public Node lChild { get; private set; } //= new Node();
    public Node rChild { get; private set; } //= new Node();

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
    public List<int> GetConnections()
    {
        return connected;
    }
    public int SetConnection(int index)
    {
        connected.Add(index);
        connected.Sort();
        return connected.IndexOf(index);
    }
    public List<Hallways> GetHallways()
    {
        //if(hallways == null) hallways = new List<Hallways>();
        return hallways;
    }
    public RectInt GetParam(bool room = false)
    {
        if (room)
        {
            return transformRoom;
        }
        else
        {
            return transform;
        }
    }
    public void SetParam(RectInt transf, bool room = false)
    {
        if (room)
        {
            transformRoom = transf;
            //transformRoom.x = x;
            //transformRoom.y = y;
            //transformRoom.width = width;
            //transformRoom.height = height;
        }
        else
        {
            transform = transf;
            //transform.x = x;
            //transform.y = y;
            //transform.width = width;
            //transform.height = height;
        }

    }
    public void SetParam(bool split)
    {
        isSplit = split;
        lChild = new Node();
        rChild = new Node();
    }
    public void AddHall(Hallways hallway, int index)
    {
        hallways.Insert(index, hallway);
    }
    public struct Hallways
    {
        List<RectInt> halls;
        List<int> indexes;

        public int LengthOfIndexes()
        {
            return indexes.Count;
        }
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
        public List<RectInt> Get()
        {
            return halls;
        }
        public int LastIndex()
        {
            return indexes[indexes.Count - 1];
        }
        public List<int> GetIndexes()
        {
            return indexes;
        }
    }
    public static bool operator ==(Vector2 hallway, Node room)
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