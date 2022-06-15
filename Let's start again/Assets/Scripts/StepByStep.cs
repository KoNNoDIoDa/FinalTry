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
    // Start is called before the first frame update
    void Start()
    {
        parent = new Node();
        parent.SetParam(new RectInt(0, 0, 90, 90), false);
        //parent.SetParam
        Split(iteration, parent, nodes);
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
            List<Node.Hallways> halls = new List<Node.Hallways>();
            nodes[i].GetParam(ref halls);
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
    void Split(int iterations, Node node, List <Node> nodes)
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
                int split = (sizes.height > 2 * MIN ? (int) Random.Range(sizes.height * 0.4f, sizes.height * 0.5f) : MIN);
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

            Split(iterations - 1, node.lChild, nodes);
            Split(iterations - 1, node.rChild, nodes);
            if (iterations == iteration)
            {
                CreateCorridors(node, nodes, true);
            }
            else CreateCorridors(node, nodes, false);
        }
        else nodes.Add(node);
        CreateRooms(node);
    }
    void CreateRooms(Node node)
    {
        RectInt sizes = node.GetParam(false);
        int randX = Random.Range(2, sizes.x / 10);
        int randY = Random.Range(2, sizes.y / 10);
        int randW = (int) (randX * Random.Range(1f, 2f));
        //randW = randW < sizes.width / 10 ? sizes.width - 2 : randW;
        int randH = (int) (randY * Random.Range(1f, 2f));
        //randH = randH < sizes.height / 10 ? sizes.height - 2 : randH;
        node.SetParam(new RectInt(sizes.x + randX, sizes.y + randY, (sizes.width - 4 < 3 ? 3 : (sizes.width - 4 > randW ? sizes.width - randW : sizes.width - 4)), (sizes.height - 4 < 3 ? 3 : (sizes.height - 4 > randH ? sizes.height - randH : sizes.height - 4))), true);
    }
    void CreateCorridors(Node node, List<Node> nodes, bool sR)
    {
        Node left = FindLeaf(node.lChild);
        Node right = FindLeaf(node.rChild);

        RectInt sizesL = left.GetParam(true);
        RectInt sizesR = right.GetParam(true);

        if(sizesL.x == sizesR.x || sizesL.y == sizesR.y)
        {
            if(sizesL.x != sizesR.x)
            {
                Node.Hallways hallway = new Node.Hallways();
                hallway.Add(new RectInt(sizesL.x + sizesL.width/ 2, sizesL.y + sizesL.height / 2, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2) + 1), 1), nodes.IndexOf(left));
                if (sR)
                {
                    safeRoom.y = sizesL.y + sizesL.height / 2;
                    Node theRoom = new Node();
                    theRoom.SetParam(safeRoom, true);
                    nodes.Add(theRoom);
                }

                left.AddHall(hallway);
                FindAllRooms(node);
            }
            else if(sizesL.y != sizesR.y)
            {
                Node.Hallways hallway = new Node.Hallways();
                hallway.Add(new RectInt(sizesL.x + sizesL.width / 2, sizesL.y + sizesL.height / 2, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y  + sizesR.height/ 2) + 1)), nodes.IndexOf(left));
                if (sR)
                {
                    safeRoom.x = sizesL.x + sizesL.width / 2;
                    Node theRoom = new Node();
                    theRoom.SetParam(safeRoom, true);
                    nodes.Add(theRoom);
                }

                left.AddHall(hallway);
                FindAllRooms(node);
            }
        }
        else if ((sizesL.x + sizesL.width >= sizesR.x || sizesR.x + sizesR.width >= sizesL.x) || Random.value > 0.5 && (sizesL.y + sizesL.height < sizesR.y || sizesR.y + sizesR.height < sizesL.y))
        {
            Node.Hallways hallway = new Node.Hallways();
            hallway.Add(new RectInt(sizesL.width / 2 + sizesL.x, sizesL.height / 2 + sizesL.y, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)), nodes.IndexOf(left));
            List <RectInt> halls = hallway.Get();
            hallway.Add(new RectInt(halls[0].x, halls[0].y + halls[0].height, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2) + 1), 1));
            left.AddHall(hallway);
            FindAllRooms(node);
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
            hallway.Add(new RectInt(sizesL.x + sizesL.width/2, sizesL.y+sizesL.height /2, -((sizesL.x + sizesL.width/2) - (sizesR.x + sizesR.width / 2) + 1), 1), nodes.IndexOf(left));
            List<RectInt> halls = hallway.Get();
            hallway.Add(new RectInt(halls[0].x + halls[0].width, halls[0].y, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2) + 1)));
            left.AddHall(hallway);
            FindAllRooms(node);
            if (sR)
            {
                if (safeRoom.x == 100500) safeRoom.x = halls[0].x + halls[0].width;
                else if (safeRoom.y == 100500) safeRoom.y = halls[0].y;
                Node theRoom = new Node();
                theRoom.SetParam(safeRoom, true);
                nodes.Add(theRoom);
            }
        }
    }
    Node FindLeaf(Node node)
    {
        Node leaf;
        if (node.GetParam())
        {
            if (Random.value > 0.5) leaf = FindLeaf(node.lChild);
            else leaf = FindLeaf(node.rChild);
        }
        else leaf = node;

        return leaf;
    }
    void FindAllRooms(Node node)
    {

    }
}
class Node
{
    
    bool isSplit = false;
    RectInt transform;
    RectInt transformRoom;
    List<Hallways> hallways = new List<Hallways>();

    public Node lChild { get; private set; } //= new Node();
    public Node rChild { get; private set; } //= new Node();

    public Node GetAny()
    {
        return lChild;
    }
    public void GetParam(ref List<Hallways> halls)
    {
        //if(hallways == null) hallways = new List<Hallways>();
        halls = hallways;
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
    public bool GetParam()
    {
        return isSplit;
    }
    public void SetParam(bool split)
    {
        isSplit = split;
        lChild = new Node();
        rChild = new Node();
    }
    public void AddHall(Hallways hallway)
    {
        //if(hallways == null) hallways = new List<Hallways>();
        hallways.Add(hallway);
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
        public List<RectInt> Get()
        {
            return halls;
        }
    }
}