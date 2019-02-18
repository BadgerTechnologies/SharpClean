using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

/*
 * Sharp Clean: clean/image.cs
 * Author: Austin Herman
 */

namespace sharpclean
{
    class image
    {
        public bool load(string filename)
        {
            StreamReader infile = null;
            try {
                infile = new StreamReader(filename, Encoding.UTF7);
            }
            catch (FileNotFoundException) {
                Console.WriteLine(image_err + "could not open file: " + filename + "\n");
                return false;
            }

            //first line is always version
            mdata.filetype = infile.ReadLine().Substring(0, 2);
            if (!(mdata.filetype != "P5" || mdata.filetype != "P2")) {
                Console.WriteLine(image_err + "invalid file type: " + mdata.filetype + "\n");
                return false;
            }

            //ignore comments
            string comments = "";
            while (true) {
                comments = infile.ReadLine();
                if (comments[0] != '#') break;
            }

            //get width, height, and total
            string[] ss = comments.Split();
            mdata.width = int.Parse(ss[0]);
            mdata.height = int.Parse(ss[1]);
            mdata.totalpixels = mdata.width * mdata.height;
            
            //get maximum grey value in file
            mdata.maxgreyval = Convert.ToInt16(infile.ReadLine());

            //get image data
            if (mdata.filetype == "P2") loadP2(infile);
            else loadP5(infile);

            Console.WriteLine("successfully loaded...\n");
            dataLoaded = true;

            infile.Close();

            return true;
        }

        public void write(string filename)
        {
            if (!dataLoaded) {
                Console.WriteLine(image_err + "image data not loaded\n");
                return;
            }

            if (mdata.filetype == "P2")
            {
                StreamWriter p2write = new StreamWriter(filename, false);

                p2write.Write(mdata.filetype + "\n" + "# Created by Sharp Clean Software\n" + mdata.width + " " + mdata.height + "\n" + mdata.maxgreyval + "\n");

                for (int i = 0; i < mdata.totalpixels; i++)
                    p2write.WriteLine(Convert.ToString(pixels[i].value));

                p2write.Close();
            }
            else
            {
                StreamWriter p5write = new StreamWriter(filename, false, Encoding.Default);

                p5write.Write(mdata.filetype + "\n# Created by Sharp Clean Software\n" + mdata.width + " " + mdata.height + "\n" + mdata.maxgreyval + "\n");

                for (int i = 0; i < mdata.totalpixels; i++)
                    p5write.Write(Convert.ToChar(pixels[i].value));

                p5write.Flush();
                p5write.Close();
            }

        }

        private void loadP2(StreamReader f)
        {
            pixels = new pixel[mdata.totalpixels];
            string line;
            int i = 0;

            while ((line = f.ReadLine()) != null)
            {
                pixels[i].value = Convert.ToByte(line);
                pixels[i].id = i;
                pixels[i].selected = false;
                i++;
            }
        }

        private void loadP5(StreamReader f)
        {
            pixels = new pixel[mdata.totalpixels];
            char[] buffer = new char[mdata.totalpixels];

            f.ReadBlock(buffer, 0, mdata.totalpixels);

            for (int i = 0; i < mdata.totalpixels; i++)
            {
                pixels[i].value = Convert.ToByte(buffer[i]);
                pixels[i].id = i;
                pixels[i].selected = false;
            }
        }

        public pixel[] getpixels()
        {
            return pixels;
        }

        public ref data getImageData()
        {
            return ref mdata;
        }

        public bool getDataLoaded()
        {
            return dataLoaded;
        }

        private bool dataLoaded = false;
        private data mdata;
        private pixel[] pixels;
        private readonly string image_err = "::IMAGE::error : ";
    }
}