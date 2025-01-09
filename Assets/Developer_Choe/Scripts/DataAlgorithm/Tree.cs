using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeNode<T>
{
    public T Number { get; set; }
    public T Height { get; set; }
    public int Data { get; set; }
    public List<TreeNode<T>> Children { get; set; } = new List<TreeNode<T>>();

    public TreeNode<T> FindNode(T data)
    {
        if (EqualityComparer<T>.Default.Equals(Number, data))//이건 첨보는데 신기하군.
        {
            return this;
        }

        foreach (var child in Children)
        {
            var foundNode = child.FindNode(data);
            if (foundNode != null)
            {
                return foundNode;
            }
        }
        return null;
    }
    public TreeNode<T> FindWithHeight(T height)
    {
        if (EqualityComparer<T>.Default.Equals(Height, height))//이건 첨보는데 신기하군.
        {
            return this;
        }

        foreach (var child in Children)
        {
            var foundNode = child.FindWithHeight(height);
            if (foundNode != null)
            {
                return foundNode;
            }
        }
        return null;
    }
    public int GetHeight(TreeNode<int> root)
    {
        int height = 0;

        foreach (TreeNode<int> child in root.Children)
        {
            int newHeight = GetHeight(child) + 1;

            height = Math.Max(height, newHeight);
        }
        return height;
    }
    public int GetMaxInChildren()
    {
        int maxInChildren = 0;

        for (int i = 0; i < Children.Count; i++)
        {
            int data = Children[i].Data;
            if (maxInChildren < data) maxInChildren = data;
        }
        return maxInChildren;
    }
}
