using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Sharp Clean: clean/filler.cs
 * Author: Austin Herman
 */

namespace sharpclean
{
    class filler
    {
        filler()
        {
            topmost = -1;
            bottommost = -1;
            leftmost = -1;
            rightmost = -1;
            pathSize = 0;
            boundsError = false;
            System.Console.WriteLine(filler_warn + "object created with out passing pixels and information.\n");
        }

        public filler(pixel[] pixels, int imageWidth, int totalPixels)
        {
            topmost = totalPixels;
            bottommost = -1;
            leftmost = imageWidth;
            rightmost = -1;
            pathSize = 0;
            boundsError = false;
            p = pixels;
            width = imageWidth;
            total = totalPixels;
        }

        public int run(List<int> buffer, int buffersize, node buff)
        {
            for (int i = 0; i < buffersize; i++)
                feedBounds(buffer[i]);

            int count = 0;
            for (int i = 0; i < buffersize; i++)
            {
                if (buffer[i] + width < total && !p[buffer[i] + width].selected && p[buffer[i] + width].value >= constants.VALUE_THRESHOLD)
                {
                    start(buffer[i] + width);
                    List<path> whitePixels = getPath();
                    if (whitePixels.Count != 0)
                    {
                        for (int j = 0; j < whitePixels.Count; j++)
                        {
                            buffer.Add(whitePixels[j].id);
                            tree.insert(ref buff, whitePixels[j].id);
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        public void feedBounds(int id)
        {
            if ((id % width) < leftmost) leftmost = id % width;

            if ((id % width) > rightmost) rightmost = id % width;

            if (id < topmost) topmost = id;

            if (id > bottommost) bottommost = id;
        }

        private bool inbounds(int id)
        {
            if (id % width <= leftmost
            || id % width >= rightmost
            || id < topmost
            || id > bottommost)
                return false;
            return true;
        }

        public void start(int id)
        {
            curpath.Clear();
            pathSize = 0;
            boundsError = false;

            addtoPath(direction.none, id);
            if (boundsError) {
                clearPath();
                return;
            }

            iteratePath();
        }

        private void addtoPath(direction dir, int id)
        {
            if (!inbounds(id))
                boundsError = true;
            else
            {
                path pd = new path(dir, id);
                curpath.Add(pd);
                pathSize++;
                p[pd.id].selected = true;
            }
        }

        private void iteratePath()
        {
            for (int i = 0; i < pathSize; i++)
            {
                check(curpath[i]);
                if (boundsError)
                {
                    clearPath();
                    return;
                }
            }
        }

        private void check(path pd)
        {
            switch (pd.dir) //0 none, 1 up, 2 down, 3 left, 4 right
            {
                case direction.up:
                    {
                        if (pd.id - width > 0)
                        {
                            if (!p[pd.id - width].selected && !p[pd.id - width].found)
                                addtoPath(direction.up, pd.id - width); //up
                        }
                        if (pd.id % width != 0)
                        {
                            if (!p[pd.id - 1].selected && !p[pd.id - 1].found)
                                addtoPath(direction.left, pd.id - 1);   //left
                        }
                        if ((pd.id + 1) % width != 0)
                        {
                            if (!p[pd.id + 1].selected && !p[pd.id + 1].found)
                                addtoPath(direction.right, pd.id + 1);  //right
                        }
                        break;
                    }
                case direction.down:
                    {
                        if (pd.id + width < total)
                        {
                            if (!p[pd.id + width].selected && !p[pd.id + width].found)
                                addtoPath(direction.down, pd.id + width);   //down
                        }
                        if (pd.id % width != 0)
                        {
                            if (!p[pd.id - 1].selected && !p[pd.id - 1].found)
                                addtoPath(direction.left, pd.id - 1);   //left
                        }
                        if ((pd.id + 1) % width != 0)
                        {
                            if (!p[pd.id + 1].selected && !p[pd.id + 1].found)
                                addtoPath(direction.right, pd.id + 1);  //right
                        }
                        break;
                    }
                case direction.left:
                    {
                        if (pd.id - width > 0)
                        {
                            if (!p[pd.id - width].selected && !p[pd.id - width].found)
                                addtoPath(direction.up, pd.id - width); //up
                        }
                        if (pd.id + width < total)
                        {
                            if (!p[pd.id + width].selected && !p[pd.id + width].found)
                                addtoPath(direction.down, pd.id + width);   //down
                        }
                        if (pd.id % width != 0)
                        {
                            if (!p[pd.id - 1].selected && !p[pd.id - 1].found)
                                addtoPath(direction.left, pd.id - 1);   //left
                        }
                        break;
                    }
                case direction.right:
                    {
                        if (pd.id - width > 0)
                        {
                            if (!p[pd.id - width].selected && !p[pd.id - width].found)
                                addtoPath(direction.up, pd.id - width); //up
                        }
                        if (pd.id + width < total)
                        {
                            if (!p[pd.id + width].selected && !p[pd.id + width].found)
                                addtoPath(direction.down, pd.id + width);   //down
                        }
                        if ((pd.id + 1) % width != 0)
                        {
                            if (!p[pd.id + 1].selected && !p[pd.id + 1].found)
                                addtoPath(direction.right, pd.id + 1);  //right
                        }
                        break;
                    }
                case direction.none:
                    {
                        if (pd.id - width > 0)
                        {
                            if (!p[pd.id - width].selected && !p[pd.id - width].found)
                                addtoPath(direction.up, pd.id - width); //up
                        }
                        if (pd.id + width < total)
                        {
                            if (!p[pd.id + width].selected && !p[pd.id + width].found)
                                addtoPath(direction.down, pd.id + width);   //down
                        }
                        if (pd.id % width != 0)
                        {
                            if (!p[pd.id - 1].selected && !p[pd.id - 1].found)
                                addtoPath(direction.left, pd.id - 1);   //left
                        }
                        if ((pd.id + 1) % width != 0)
                        {
                            if (!p[pd.id + 1].selected && !p[pd.id + 1].found)
                                addtoPath(direction.right, pd.id + 1);  //right
                        }
                        break;
                    }
            }
        }

        private void clearPath()
        {
            curpath.Clear();
            pathSize = 0;
        }

        public void printBounds()
        {
            int x, y;
            x = leftmost % width;
            y = leftmost / width;
            System.Console.WriteLine("\nl: %i(%i, %i),", leftmost, x, y);
            x = topmost % width;
            y = topmost / width;
            System.Console.WriteLine(" u: %i(%i, %i),", topmost, x, y);
            x = rightmost % width;
            y = rightmost / width;
            System.Console.WriteLine(" r: %i(%i, %i),", rightmost, x, y);
            x = bottommost % width;
            y = bottommost / width;
            System.Console.WriteLine(" b: %i(%i, %i)\n", bottommost, x, y);
        }

        public List<path> getPath()
        {
            return curpath;
        }

        private pixel[] p;
        private readonly int width, total;
        private int topmost, bottommost, leftmost, rightmost;
        private List<path> curpath = new List<path>();
        private int pathSize;
        private bool boundsError;
        private readonly string filler_warn = "::FILLER::warning : ";
    }
}