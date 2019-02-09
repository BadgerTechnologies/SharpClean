using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Sharp Clean: clean/edge.cs
 * Author: Austin Herman
 */

namespace sharpclean
{
    class edge
    {
        edge()
        {
            Console.WriteLine(edge_warn + "edge initialized with no pixels\n");
        }

        public edge(int w, int t)
        {
            sel = null;
            per = null;
            numEdges = 0;
            tolerance = 0;
            perimSize = 0;
            fieldSet = false;
            width = w;
            total = t;
        }

        public void detect(List<int> mselection, node buff)
        {
            sel = buff;
            stack.Add(mselection[0]);
            perimeter.Add(mselection[0]);
            tree.insert(ref per, mselection[0]);
            perimSize++;
            numEdges++;
            iterateEdges();

            per = null;
            sel = null;
        }

        private void iterateEdges()
        {
            while (stack.Count != 0)
            {
                int id = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                checkneighbors(getOctan(id), id);
            }
        }

        private octan getOctan(int id)
        {
            octan oct = new octan();
            if ((id - width) > 0)
            {
                oct.tl = tree.findNode(sel, id - width - 1);
                oct.t = tree.findNode(sel, id - width);
                oct.tr = tree.findNode(sel, id - width + 1);
            }

            if (id % width != 0)
                oct.l = tree.findNode(sel, id - 1);

            if ((id + 1) % width != 0)
                oct.r = tree.findNode(sel, id + 1);

            if ((id + width) < total)
            {
                oct.bl = tree.findNode(sel, id + width - 1);
                oct.b = tree.findNode(sel, id + width);
                oct.br = tree.findNode(sel, id + width + 1);
            }

            return oct;
        }
        private void checkneighbors(octan oct, int id)
        {
            if ((id - width) > 0)
            { //read: "check if top left is an edge"
                check(oct.tl, oct.t, oct.l, field.tl);
                check(oct.t, oct.tl, oct.tr, field.t);
                check(oct.tr, oct.t, oct.r, field.tr);
            }

            if (id % width != 0)
                check(oct.l, oct.tl, oct.bl, field.l);

            if ((id + 1) % width != 0)
                check(oct.r, oct.tr, oct.br, field.r);

            if ((id + width) < total)
            {
                check(oct.bl, oct.l, oct.b, field.bl);
                check(oct.b, oct.bl, oct.br, field.b);
                check(oct.br, oct.b, oct.r, field.br);
            }
        }

        private void check(int centerpixel, int neigh1, int neigh2, field dir)
        {
            if (centerpixel != -1 && !(neigh1 != -1 && neigh2 != -1))
            {
                if (tree.insert(ref per, centerpixel))
                {
                    addperimeterpixel(centerpixel);
                    checkfield(dir);
                }
            }
        }

        private void addperimeterpixel(int p)
        {
            stack.Add(p);
            perimeter.Add(p);
            perimSize++;
        }

        private void checkfield(field dir)
        {
            if (!fieldSet)
            {
                setField(dir);
                numEdges++;
            }
            else
            {
                tolerance += curfield[Convert.ToInt32(dir)];
                if (tolerance < -4 || tolerance > 4)
                {
                    tolerance = 0;
                    numEdges++;
                    setField(dir);
                }
            }
        }

        private void setField(field dir)
        {
            if (dir == field.t || dir == field.b)
            {   //vertical
                curfield = fieldvector.verticalfield;
                fieldSet = true;
            }

            else if (dir == field.l || dir == field.r)
            {   //horizontal
                curfield = fieldvector.horizontalfield;
                fieldSet = true;
            }
            else if (dir == field.tl || dir == field.br)
            {   //leftslant
                curfield = fieldvector.leftslantfield;
                fieldSet = true;
            }
            else
            {   //rightslant
                curfield = fieldvector.rightslantfield;
                fieldSet = true;
            }
        }

        public List<int> getPerimiter()
        {
            return perimeter;
        }

        public int getSizeofPerimeter()
        {
            return perimSize;
        }

        public int getEdges()
        {
            return numEdges;
        }

        private node sel = new node();
        private node per = new node();
        private int[] curfield = new int[8];
        private bool fieldSet;
        private List<int> perimeter = new List<int>();
        private List<int> stack = new List<int>();
        private readonly int width, total;
        private int numEdges, perimSize, tolerance;
        private readonly string edge_warn = "::EDGE::warning : ";
    }
}