using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepByStep : MonoBehaviour
{
    
    List <Node> nodes = new List <Node>();
    public Material mat;
    public Sprite sprite;
    const int MIN = 6;
    Node parent;
    // Start is called before the first frame update
    void Start()
    {
        parent = new Node();
        parent.SetParam(0, 0, 100, 100, false);
        //parent.SetParam
        Split(5, parent, nodes);
        for(int i = 0; i < nodes.Count; i++)
        {
            GameObject plate = new GameObject();
            Vector2 position = plate.transform.position;
            position.x = nodes[i].GetParam(true).x; 
            position.y = nodes[i].GetParam(true).y;
            plate.transform.position = position;

            Vector2 scale = plate.transform.localScale;
            scale.x = nodes[i].GetParam(true).width;
            scale.y = nodes[i].GetParam(true).height;
            plate.transform.localScale = scale;

            SpriteRenderer rend = plate.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            rend.material = mat;
            rend.sprite = sprite;
            List<Node.Hallways> halls = new List<Node.Hallways>();
            nodes[i].GetParam(ref halls);
            for(int x = 0; x < halls.Count; x++)
            {
                List<Rect> hall = halls[x].Get();
                for(int y = 0; y < hall.Count; y++)
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
        Rect sizes = node.GetParam(false);

        if (iterations > 0   && (sizes.width >= 2 * MIN || sizes.height >= 2 * MIN))
        {
            
            bool splitH = Random.value > 0.5;
            if (sizes.width > sizes.height & sizes.width / sizes.height >= 1.25 & sizes.width >= 2 * MIN) splitH = false;
            else if (sizes.height > sizes.width & sizes.height / sizes.width >= 1.25 & sizes.height >= 2 * MIN) splitH = true;
            node.SetParam(true);
            if (splitH)
            {
                float split = Random.Range(sizes.height * 0.3f, sizes.height * 0.5f);
                node.lChild.SetParam(sizes.x, sizes.y, sizes.width, split, false);

                node.rChild.SetParam(sizes.x, sizes.y + split, sizes.width, sizes.height - split, false);
            }
            else
            {
                float split = Random.Range(sizes.width*0.3f, sizes.width * 0.5f);
                node.lChild.SetParam(sizes.x, sizes.y, split, sizes.height, false);

                node.rChild.SetParam(sizes.x + split, sizes.y, sizes.width - split, sizes.height, false);
            }
            Split(iterations - 1, node.lChild, nodes);
            Split(iterations - 1, node.rChild, nodes);
            CreateCorridors(node, nodes);
        }
        else nodes.Add(node);
        CreateRooms(node);
    }
    void CreateRooms(Node node)
    {
        Rect sizes = node.GetParam(false);
        float randX = Random.Range(2, sizes.x / 10);
        float randY = Random.Range(2, sizes.y / 10);
        float randW = randX * Random.Range(1f, 2f);
        //randW = randW < sizes.width / 10 ? sizes.width - 2 : randW;
        float randH = randY * Random.Range(1f, 2f);
        //randH = randH < sizes.height / 10 ? sizes.height - 2 : randH;
        node.SetParam(sizes.x + randX, sizes.y + randY, (sizes.width - 4 > randW ? sizes.width - randW : sizes.width - 4), (sizes.height - 4 > randH ? sizes.height - randH : sizes.height - 4), true);
    }
    void CreateCorridors(Node node, List<Node> nodes)
    {
        Node left = FindLeaf(node.lChild);
        Node right = FindLeaf(node.rChild);

        Rect sizesL = left.GetParam(true);
        Rect sizesR = right.GetParam(true);

        if(sizesL.x == sizesR.x || sizesL.y == sizesR.y)
        {
            if(sizesL.x == sizesR.x)
            {
                Node.Hallways hallway = new Node.Hallways();
                hallway.Add(new Rect(sizesL.x + sizesL.width/ 2, sizesL.y + sizesL.height / 2, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2)), 1), nodes.IndexOf(left));
                left.AddHall(hallway);
                FindAllRooms(node);
            }
            else
            {
                Node.Hallways hallway = new Node.Hallways();
                hallway.Add(new Rect(sizesL.x + sizesL.width / 2, sizesL.y + sizesL.height / 2, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y  + sizesR.height/ 2))), nodes.IndexOf(left));
                left.AddHall(hallway);
                FindAllRooms(node);
            }
        }
        else if (Random.value > 0.5)
        {
            Node.Hallways hallway = new Node.Hallways();
            hallway.Add(new Rect(sizesL.width / 2 + sizesL.x, sizesL.height / 2 + sizesL.y, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2))), nodes.IndexOf(left));
            List <Rect> halls = hallway.Get();
            hallway.Add(new Rect(halls[0].x, halls[0].y + halls[0].height, -((sizesL.x + sizesL.width / 2) - (sizesR.x + sizesR.width / 2)), 1));
            left.AddHall(hallway);
            FindAllRooms(node);
        }
        else
        {
            Node.Hallways hallway = new Node.Hallways();
            hallway.Add(new Rect(sizesL.x + sizesL.width/2, sizesL.y, -((sizesL.x + sizesL.width/2) - (sizesR.x + sizesR.width / 2)), 1), nodes.IndexOf(left));
            List<Rect> halls = hallway.Get();
            hallway.Add(new Rect(halls[0].x + halls[0].width, halls[0].y, 1, -((sizesL.y + sizesL.height / 2) - (sizesR.y + sizesR.height / 2))));
            left.AddHall(hallway);
            FindAllRooms(node);
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
    Rect transform;
    Rect transformRoom;
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
    public Rect GetParam(bool room = false)
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
    public void SetParam(float x, float y, float width, float height, bool room = false)
    {
        if (room)
        {
            transformRoom.x = x;
            transformRoom.y = y;
            transformRoom.width = width;
            transformRoom.height = height;
        }
        else
        {
            transform.x = x;
            transform.y = y;
            transform.width = width;
            transform.height = height;
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
        List<Rect> halls;
        List<int> indexes;

        public void Add(Rect hall, int index)
        {
            if (halls == null) halls = new List<Rect>();
            if(indexes == null) indexes = new List<int>();
            halls.Add(hall);
            indexes.Add(index);
        }
        public void Add(Rect hall)
        {
            if (halls == null) halls = new List<Rect>();
            halls.Add(hall);
        }
        public void Add(int index)
        {
            if (indexes == null) indexes = new List<int>();
            indexes.Add(index);
        }
        public List<Rect> Get()
        {
            return halls;
        }
    }
}