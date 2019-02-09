using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * Sharp Clean: clean/toolbox.cs
 * Author: Austin Herman
 * Edited: Jair Ramirez 1/4/2017
 */

namespace sharpclean
{
    public class toolbox
    {
        public toolbox(pixel[] p, int width, int height, int total)
        {
            pixels = p;
            imageWidth = width;
            totalPixels = total;
            imageHeight = height;
            brushMin = - (constants.BRUSH_SIZE / 2 - 1);
            brushMax = constants.BRUSH_SIZE / 2;
        }

        //the big boy, iterates through the pixels and drives algorithms
        public void run(ProgressBar progressBar1)
        {
            if (pixels == null)
            {
                MessageBox.Show("No Pixels Loaded", "no pixels", 0);
                return;
            }

            selection s = new selection(pixels, imageWidth, totalPixels);
            for (int i = 0; i < totalPixels; i++)
            {
                progressBar1.Value = i;

                if (s.get(i))
                {
                    buffer = s.Buffer;
                    perimeter = s.Perimeter;
                    objectData dat = new objectData(getAverageValue(), buffer.Count, buffer.Count / s.getedges());
                    conf c = confidence.getconfidence(dat);
                    dat.objconf = c;
                    objdat.Add(dat);

                    if (!c.isStructure)
                        colorbuffer(constants.COLOR_CLEAR);

                }
                s.clearbuffer();
                buffer.Clear();
            }
        }

        //colors a selection of pixels
        private void colorbuffer(int color)
        {
            for (int i = 0; i < buffer.Count; i++)
                pixels[buffer[i]].value = Convert.ToByte(color);
        }

        //colors the edges of a selection of pixels
        private void coloredges(int color)
        {
            for (int i = 0; i < perimeter.Count; i++)
                pixels[perimeter[i]].value = Convert.ToByte(color);
        }

        // debugging funciton to look at the walk path
        public void printWalkPath(fileOps mapCleanup)
        {
            for (int i = 0; i < mapCleanup.walkPath.Count(); ++i)
                Console.WriteLine("x: " +  mapCleanup.walkPath[i].x.ToString() + " y: " + mapCleanup.walkPath[i].y.ToString()); 
        }

        // removes the path that was walked by using the the walk path genertated from the trajectory file in fileops
        public void removeDebris(fileOps mapCleanup)
        {
            for (int i = 0; i < mapCleanup.walkPath.Count(); i++)
                brush(((imageHeight - mapCleanup.walkPath[i].y - 1) * imageWidth) + mapCleanup.walkPath[i].x);
        }

        // changes the color of the pixels and sets the touch value
        private void brush(int trajectoryLocation)
        {
            for (int j = brushMin; j < brushMax; j++)
            {
                for (int k = brushMin; k < brushMax; k++)
                {
                    int pixelLocation = trajectoryLocation + ((imageWidth * j) + k);
                    if (!pixels[pixelLocation].selected) {
                        pixels[pixelLocation].value = Convert.ToByte(constants.COLOR_CLEAR);
                        pixels[pixelLocation].selected = true;
                    }
                }
            }
        }

        private double getAverageValue()
        {
            double avg = 0;
            for (int i = 0; i < buffer.Count; i++)
                avg += pixels[buffer[i]].value;
            return avg / buffer.Count;
        }

        public List<objectData> getObjectData()
        {
            return objdat;
        }

        private pixel[] pixels = null;
        private readonly int imageWidth, totalPixels, imageHeight, brushMin, brushMax;
        private List<int> buffer = new List<int>();
        private List<int> perimeter = new List<int>();
        private List<objectData> objdat = new List<objectData>();
    }
}
