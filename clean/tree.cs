using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Sharp Clean: clean/tree.cs
 * Author: Austin Herman
 */

namespace sharpclean
{
    public static class tree
    {
        public static bool insert(ref node n, int id)
        {
            if (n == null)
            {
                n = new node(id);
                return true;
            }
            node r = n;
            while (n != null)
            {
                if (id < n.id)
                {
                    if (n.left != null)
                        n = n.left;
                    else
                    {
                        n.left = new node(id);
                        n = r;
                        return true;
                    }
                }
                else if (id > n.id)
                {
                    if (n.right != null)
                        n = n.right;
                    else
                    {
                        n.right = new node(id);
                        n = r;
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }
            n = r;
            return false;
        }

        public static int findNode(node n, int id)
        {
            if (n == null) return -1;
            node r = n;
            while (n != null)
            {
                if (id == n.id)
                {
                    n = r;
                    return id;
                }
                if (id < n.id) n = n.left;
                else if (id > n.id) n = n.right;
            }
            n = r;
            return -1;
        }

        public static void buildTree(List<int> v, node r)
        {
            List<tup> stack = new List<tup>();
            int m, s, e;
            m = (0 + v.Count - 1) / 2;
            insert(ref r, v[m]);
            tup right = new tup(m + 1, v.Count);
            tup left = new tup(0, m);
            stack.Add(right);
            stack.Add(left);
            while (stack.Count > 0)
            {
                s = stack[stack.Count - 1].s;
                e = stack[stack.Count - 1].e;
                stack.RemoveAt(stack.Count - 1);
                if (s < e)
                {
                    m = (s + e) / 2;
                    insert(ref r, v[m]);
                    right.change(m + 1, v.Count);
                    left.change(0, m);
                    stack.Add(right);
                    stack.Add(left);
                }
            }
        }

        public static void getInOrder(node n, List<int> v)
        {
            node r = n;
            List<node> s = new List<node>();
            while (n != null || s.Count > 0)
            {
                while (n != null)
                {
                    s.Add(n);
                    n = n.left;
                }
                n = s[s.Count - 1];
                s.RemoveAt(s.Count - 1);
                v.Add(n.id);
                n = n.right;
            }
            n = r;
        }
    }
}