using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class starPoint
{
    private readonly float x, y;
    private int connections;
    private GameObject star;
    private readonly int nameNum = -1;

    public starPoint(float xPos, float yPos, int connect)
    {
        x = xPos;
        y = yPos;
        connections = connect;
    }
    public starPoint(float xPos, float yPos, int connect, int newNum)
    {
        x = xPos;
        y = yPos;
        connections = connect;
        nameNum = newNum;
    }

    public void incConnec()
    {
        connections += 1;
    }

    public int getConnec()
    {
        return connections;
    }

    public Vector3 getPos()
    {
        return new Vector3(x,y, 0);
    }

    public void setStar(GameObject obj)
    {
        star = obj;
    }

    public GameObject getStar()
    {
        return star;
    }

    public int getName()
    {
        return nameNum;
    }
}

public class edge
{
    public readonly starPoint p1, p2;

    public edge(starPoint q1, starPoint q2)
    {
        p1 = q1;
        p2 = q2;
    }

    public bool eq(edge obj)
    {
        if (!(obj is null))
        {
            if (p1 == obj.p1 && p2 == obj.p2 ||
                p1 == obj.p2 && p2 == obj.p1)
            {
                return true;
            }
        }
        return false;
    }
}

public class triangle
{
    public readonly starPoint[] points = new starPoint[3];
    public readonly List<edge> edges = new List<edge>();

    public triangle(starPoint a, starPoint b, starPoint c)
    {
        points[0] = a;
        points[1] = b;
        points[2] = c;
        edges.Add(new edge(a, b));
        edges.Add(new edge(a, c));
        edges.Add(new edge(b, c));
    }
}

public class circle
{
    public readonly starPoint center;
    public readonly float diam;

    public circle(starPoint c, float d)
    {
        center = c;
        diam = d;
    }
}


public class ConstellationScript : MonoBehaviour
{
    private readonly List<starPoint> _stars = new List<starPoint>();
    private List<edge> _lines = new List<edge>();
    private float totalLength = 0;

    public GameObject starFab;
    public GameObject lineFab;

    public float getTotalLength()
    {
        return totalLength;
    }

    public float getAvgLength()
    {
        return totalLength / _lines.Count;
    }

    public int getNumStars()
    {
        return _stars.Count;
    }

    public void spawnStar()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        starPoint newStar = new starPoint(mousePos.x, mousePos.y, 0, _stars.Count);
        Vector3 newStarPos = newStar.getPos();
        GameObject star = Instantiate(starFab, new Vector3(newStarPos.x, newStarPos.y, newStarPos.z), Quaternion.identity, gameObject.transform);
        newStar.setStar(star);
        star.GetComponent<AudioSource>().clip = musicManagerScript.M.rndSingleNote();
        star.GetComponent<AudioSource>().Play();
        star.name = "S" + newStar.getName();
        _stars.Add(newStar);
        if (_stars.Count > 1) DrawLines();
    }
    
    private static float starDistance(starPoint a, starPoint b)
    {
        return (float)Math.Sqrt(Math.Pow(a.getPos().x - b.getPos().x, 2) + Math.Pow(a.getPos().y - b.getPos().y, 2));
    }

    private void DrawLines()
    {
        if (_stars.Count < 3) _lines.Add(new edge(_stars[0], _stars[1]));
        else
        {
            Delaunay();
            relativeNeighborhood(); 
        }
        foreach (var t in _lines)
        {
            GameObject newLine = Instantiate(lineFab, gameObject.transform);
            newLine.name = "L" + t.p1.getName() + "->" + t.p2.getName();
            Vector3[] points =
            {
                new Vector3(t.p1.getPos().x,t.p1.getPos().y,0),
                new Vector3(t.p2.getPos().x,t.p2.getPos().y,0)
            };
            newLine.GetComponent<LineRenderer>().SetPositions(points);
            t.p1.incConnec();
            t.p2.incConnec();
            totalLength += starDistance(t.p1, t.p2);
        }
    }

    private void Delaunay()
    {
        //Clear previous lines
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("L")) Destroy(child.gameObject);
        }
        _lines.Clear();

        //create our triangular mesh
        //create a super-triangle to start mesh - big enough that all stars are within it
        List<triangle> triangles = new List<triangle>();
        starPoint sa = new starPoint(-10, -5, 0, 100);
        starPoint sb = new starPoint(0, 10, 0, 101);
        starPoint sc = new starPoint(10, -5, 0, 102);
        var superTriangle = new triangle(sa, sb, sc);
        triangles.Add(superTriangle);

        //for each point we need to add to our mesh
        foreach (var node in _stars)
        {
            //find all the triangles that are now 'bad'
            //per Delaunay, this means any triangle who's circumcircle contains our node
            List<triangle> badTriangles = new List<triangle>();
            List<edge> badEdges = new List<edge>();
            List<edge> dupEdges = new List<edge>();
            
            //go through each triangle in our mesh
            foreach (var tri in triangles)
            {
                //if our node is inside this triangle's circumcircle
                if (insideCircle(node, circleFromTriangle(tri)))
                {
                    //it is a bad triangle
                    badTriangles.Add(tri);

                    //find the boundary of the polygonal hole
                    //ie: get the edges in our mesh that 'bound' the bad triangles
                    //by removing any edges shared by more than one bad triangle
                    foreach (var e in tri.edges)
                    {
                        if (containsEdge(badEdges, e)) dupEdges.Add(e);
                        else badEdges.Add(e);
                    }
                }
            }

            //remove the duplicates (shared edges) from badEdges
            //this will *actually* get the boundary of our polygonal hole
            List<edge> indexes = new List<edge>();
            foreach (edge bad in badEdges)
            {
                foreach (edge dupe in dupEdges)
                {
                    if (bad.eq(dupe)) indexes.Add(bad);
                }
            }
            foreach (edge i in indexes)
            {
                badEdges.Remove(i);
            }

            //update our triangulation list to remove bad triangles
            var newTriangles = triangles.Except(badTriangles).ToList();
            triangles = newTriangles;
            
            //make new triangles using our node
            //each triangle will include the node and the points of each edge of our polygonal hole
            foreach (var edge in badEdges.ToList())
            {
                triangles.Add(new  triangle(edge.p1, edge.p2, node));
            }
            //repeat for each node in our list!
        }

        //we should now have all of the triangles we want in our final mesh!
        //we just need to remove any edges that connect to our initial super triangle
        List<edge> finalEdges = new List<edge>();
        
        //go through our triangles
        for (int i = 0; i < triangles.Count; i++)
        {
            bool isInSuperTriangle = false;
            //see if any of the points in this triangle are points in our super triangle
            foreach (var p in triangles[i].points)
            {
                if (Array.IndexOf(superTriangle.points, p) != -1) isInSuperTriangle = true;
            }
            //if this triangle does not connect to our super triangle
            //and if it isn't already in our list
            //add it to our list of final edges!
            if (!isInSuperTriangle)
            {
                foreach (edge e in triangles[i].edges)
                {
                    // if (!containsEdge(finalEdges, e))
                    finalEdges.Add(e);
                }
            }
        }
        
        //the lines we want to draw in DrawLines() are these final edges
        _lines = finalEdges;
    }

    private void relativeNeighborhood()
    {
        if (_lines.Count == 1) return;
        List<edge> relEdges = new List<edge>();
        foreach (edge e in _lines)
        {
            bool anyBlocking = false;
            float length = starDistance(e.p1, e.p2);
            foreach (starPoint star in _stars)
            {
                if (e.p1 == star || e.p2 == star) continue;
                if (starDistance(star, e.p1) < length && starDistance(star, e.p2) < length)
                {
                    anyBlocking = true;
                    break;
                }
            }
            if (!anyBlocking) relEdges.Add(e);
        }
        _lines = relEdges;
    }

    private bool containsEdge(List<edge> edges, edge e)
    {
        if (edges.Count == 0 || e == null) return false;
        foreach (var edge in edges)
        {
            if (edge.eq(e)) return true;
        }
        return false;
    }

    private bool insideCircle(starPoint node, circle c)
    {
        if (c == null) return false;
        var dist = starDistance(node, c.center);
        return dist < c.diam / 2;
    }

    private circle circleFromTriangle(triangle t)
    {
        (float, float, float) bi1 = perpendicularBisector(t.points[0], t.points[1]);
        (float, float, float) bi2 = perpendicularBisector(t.points[1], t.points[2]);
        starPoint center = findIntersection(bi1, bi2);
        circle c = new circle(center, starDistance(center, t.points[0]) * 2);
        return center != null ? c : null;
    }
    
    private (float,float,float) perpendicularBisector(starPoint a, starPoint b)
    {
        starPoint midpoint = new starPoint((a.getPos().x + b.getPos().x) / 2, (a.getPos().y + b.getPos().y) / 2, 0);
        float p = a.getPos().x - b.getPos().x;
        float q = b.getPos().y - a.getPos().y;
        float r = - p*midpoint.getPos().x + q*midpoint.getPos().y;
        return (-p, q, r);
    }

    private starPoint findIntersection((float, float, float) l1, (float, float, float) l2)
    {
        var determinant = l1.Item1 * l2.Item2 - l2.Item1 * l1.Item2;
        if (determinant == 0) return null;
        return new starPoint((l2.Item2*l1.Item3-l1.Item2*l2.Item3)/determinant,
                         (l1.Item1*l2.Item3-l2.Item1*l1.Item3)/determinant, 0);
    }
}