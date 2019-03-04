using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ImageMagick;
using System.Drawing;

/*
 * Sharp Clean: fileOps.cs
 * Author: Joey Harrison
 */

namespace sharpclean
{
    // Handles file operations for the program, including opening and saving images, locating paths, and user interfacing
    public class fileOps
    {
        // Initialize the values for paths
        private string dirPath;
		private string imgPath;
		private string trajPath;
		private string offsetPath;
		private string tempPath;
		private string[] dirFiles;

        public struct coords
        {
            public decimal x, y;
            public bool usable;
        }

        public struct intCoords
        {
            public int x, y;
        }

        public coords park, dock, globaloffset;
        public intCoords[] walkPath;

        #region Public member functions

        // Constructor
        public fileOps()
        {
            dirPath = "";
            imgPath = "";
            trajPath = "";
            offsetPath = "";
        }

        #region Get initial files

        public bool getUserPath()
        {
            // Open the file dialog and save the image and directory paths
            using (OpenFileDialog openFD = new OpenFileDialog())
            {
                // File dialog settings
                openFD.Title = "Select an image file";
                openFD.InitialDirectory = "c:\\Users\\";
                openFD.Filter = "png files (*.png)|*.png";
                openFD.RestoreDirectory = true;

                DialogResult result = openFD.ShowDialog();

                // Only save the file and directory path if the user selects "OK"
                if (result == DialogResult.OK) {
                    imgPath = openFD.FileName;
					return true;
                }
                else
                    return false;
            }
        }

        public string getDir()
        {
            // Gets and sets the directory name
            dirPath = Path.GetDirectoryName(imgPath);

			// Save all the files in this directory
			dirFiles = Directory.GetFiles(dirPath, "*.*", SearchOption.TopDirectoryOnly);

			return dirPath;
        }

        public string getTraj() // Gets the full trajectory file path
        {
            trajPath = getFilebyType(".ply");
			if (trajPath == "") {
				park.usable = false;
				dock.usable = false;
			}
            return trajPath;
        }

        public string getOffset() // Gets the full info file path
        {
            offsetPath = getFilebyType(".info");
			if (offsetPath == "") {
				park.usable = false;
				dock.usable = false;
			}
			return offsetPath;
        }

		public string getImagePath()
		{
			return imgPath;
		}

        #endregion

        public void generateParkandDock()
        {
            // open offset file, read to the 5th line and save the offset
            StreamReader offsetfile = new StreamReader(offsetPath);

            for (int i = 0; i < 5; i++)
                offsetfile.ReadLine();

            string[] ss;
            string streamline = offsetfile.ReadLine();

            if (streamline.Contains(":"))
            {
                ss = streamline.Split();
                globaloffset.x = Convert.ToDecimal(ss[1]);
                globaloffset.y = Convert.ToDecimal(ss[2]);
            }

            // open trajectory file, read to 'end_header' line while checking for 'element', if found then save vertex count
            int vertexes = 0;

            StreamReader trajfile = new StreamReader(trajPath);

            while ((streamline = trajfile.ReadLine()) != "end_header")
            {
                if ((ss = streamline.Split())[0] == "element")
                    vertexes = Convert.ToInt16(ss[2]);
            }
            walkPath = new intCoords[vertexes - 1];


            // store first two columns for park
            ss = trajfile.ReadLine().Split();
            park.x = Convert.ToDecimal(ss[0]);
            park.y = Convert.ToDecimal(ss[1]);

            // store walk path with conversions
            walkPath[0].x = Convert.ToInt16((Convert.ToDecimal(ss[0]) * 20)) + Convert.ToInt16(globaloffset.x);
            walkPath[0].y = Convert.ToInt16((Convert.ToDecimal(ss[1]) * 20)) + Convert.ToInt16(globaloffset.y);

            // read to the second to last vertex element
            for (int i = 1; i < vertexes - 1; i++)
            {
                ss = trajfile.ReadLine().Split();
                walkPath[i].x = Convert.ToInt32((Convert.ToDecimal(ss[0]) * 20)) + Convert.ToInt32(globaloffset.x);
                walkPath[i].y = Convert.ToInt32((Convert.ToDecimal(ss[1]) * 20)) + Convert.ToInt32(globaloffset.y);
            }

            // store as dock
            ss = trajfile.ReadLine().Split();
            dock.x = Convert.ToDecimal(ss[0]);
            dock.y = Convert.ToDecimal(ss[1]);

            // convert offset to meters, convert park/dock to global meter coordinates
            globaloffset.x = globaloffset.x / 20;
            globaloffset.y = globaloffset.y / 20;

            park.x += globaloffset.x;
            park.y += globaloffset.y;
            dock.x += globaloffset.x;
            dock.y += globaloffset.y;

            // Set the park and dock locations to be usable
            park.usable = true;
            dock.usable = true;

        }

        public string getTempPath()
        {
            return this.tempPath;
        }

        public string generatePGM() // Generates 2 .pgm files from originally selected image and returns the path to a new pgm with the same name as the png
        {
            // Set the temporary pgm file path
            tempPath = dirPath + "\\" + "temp.pgm";

            // Using ImageMagick.NET, convert the .png image to .pgm
            using (MagickImage pngMap = new MagickImage(imgPath))
            {
                pngMap.Write(tempPath);
                return tempPath;
            }
        }

        public string getStoreInfo(string info)
        {
            // Get the directory name
            string mapFolder = Path.GetFileName(this.dirPath);

            // Initialize return variable
            string storeInfo = "";

            // Get either the store name or the store number
            if (info == "name")
                storeInfo = getStoreName(mapFolder);
            else if (info == "number")
                storeInfo = getStoreNumber(mapFolder);

            return storeInfo;
        }

        public string getSaveFile()
        {
            SaveFileDialog saveFD = new SaveFileDialog();

            saveFD.Title = "Save the image file";
            saveFD.Filter = "pgm files (*.pgm)|*.pgm";

            // Build the string for park and dock locations
            string parkNdock = "_P_" + Math.Round(park.x, 2) + "_" + Math.Round(park.y, 2) + "_D_" + Math.Round(dock.x, 2) + "_" + Math.Round(dock.y, 2);

            // If the park and dock locations have been calculated, add them to the file name, otherwise keep the same name as the original .png
            if (park.usable && dock.usable)
            {
                saveFD.FileName = Path.GetFileName(this.dirPath) + parkNdock;
            }
            else
            {
                saveFD.FileName = Path.GetFileNameWithoutExtension(this.imgPath);
            }

            saveFD.InitialDirectory = this.dirPath;

            DialogResult result = saveFD.ShowDialog();

            if (result == DialogResult.OK)
            {
                return saveFD.FileName;
            }
            else
            {
                return "err::no_map_selected";
            }
        }

        #endregion

        #region Private member functions

        private string getStoreName(string folder)
        {
            string storeName;

            // Assign the store name based on their acronym
            if (folder.Substring(0, 2).ToUpper() == "GM")
            {
                storeName = "Giant Martin";
            }
            else if (folder.Substring(0, 2).ToUpper() == "SS")
            {
                storeName = "Stop & Shop";
            }
            else if (folder.Length > 3)
            {
                if (folder.Substring(0, 3).ToUpper() == "SNS")
                    storeName = "Stop & Shop";
                else
                {
                    storeName = "Unknown Store";
                }
            }
            else
            {
                storeName = "Unknown Store";
            }
            return storeName;
        }

        private string getStoreNumber(string folder)
        {
            string storeNumber = "";

            // The only number in the directory name should be the store number
            for (int i = 0; i < folder.Length; i++)
            {
                if (Char.IsDigit(folder[i]))
                {
                    storeNumber += folder[i];
                }
            }
            return storeNumber;
        }

        private string getFilebyType(string fileExt)
        {
            // Iterate through the directory files to find either the trajectory or offset file
            for (int i = 0; i < this.dirFiles.Length; i++)
            {
                if (fileExt == Path.GetExtension(this.dirFiles[i]))
                {
                    return this.dirFiles[i];
                }
            }
            // Should not be reached unless the file is missing
            return "";
        }

        #endregion
    }
}