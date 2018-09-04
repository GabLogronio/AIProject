using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDTNode
{
    DTAction Walk();
}

public delegate object DTCall(object bundle);

public class DTDecision : IDTNode
{
    private DTCall Selector;

    private Dictionary<object, IDTNode> links;

    public DTDecision(DTCall selector)
    {
        Selector = selector;
        links = new Dictionary<object, IDTNode>();
    }

    public void AddLink(object value, IDTNode next)
    {
        links.Add(value, next);
    }

    public DTAction Walk()
    {
        object o = Selector(null);
        return links.ContainsKey(o) ? links[o].Walk() : null;
    }
}

public class DTAction : IDTNode
{
    public DTCall Action;

    public DTAction(DTCall callee)
    {
        Action = callee;
    }

    public DTAction Walk()
    {
        return this;
    }
}

public class DecisionTree
{

    private IDTNode root;

    public DecisionTree(IDTNode start)
    {
        root = start;
    }

    /*public object Update () {
        Debug.Log("update");
        IDTNode n = root;
        while (n.GetNext() != null)
        {
            n = n.GetNext();
        }
        if (n.GetType().ToString().Equals("DTAction"))
        {
            Debug.Log("ok");
            return n;
        }
        else
        {
            Debug.Log(n.GetType().ToString());
        }
        return null;        
    }*/

    public object walk()
    {
        DTAction result = root.Walk();
        if (result != null) return result.Action(null);
        return null;
    }
}