using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ImageMagick;

/*
 * Sharp Clean: Program.cs
 * Author: Joey Harrison
 */

namespace sharpclean
{
    public partial class Form1 : Form
    {
        fileOps mapCleanup = new fileOps();
        image img = new image();
        Image mapImage = null;
        toolbox tBox = null;
        string mapPath = "";
        string csvPath = "";
        string dirPath;
        string trajPath;
        string offsetPath;
        string tempPGMPath = "";
        string fileSaveName = "";

        public Form1()
        {
            // Initializes form components
            InitializeComponent();

            // Adds the form closing event (the red x in the top right corner)
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e) // Handle any unfinished things in the program
        {
            #region Try to delete the temporary files if they are in the directory
            try
            {
                File.Delete(mapCleanup.getTempPath());
            }
            catch (Exception ee)
            {
                Console.WriteLine("No file (temp.pgm) found to delete. Error: " + ee);
            }
            try
            {
                File.Delete(tempPGMPath);
            }
            catch (Exception ee)
            {
                Console.WriteLine("No file (temp2.pgm) found to delete. Error: " + ee);
            }
            #endregion
        }

        // This is the button click to save the file paths and load the image into the picture box
        private void button1_Click(object sender, EventArgs e)
        {
            // Assign map path by bringing up file dialog
            this.mapPath = this.mapCleanup.getImage();

            // check for pre-existing temp files - delete any if found
            string[] files = Directory.GetFiles(Path.GetDirectoryName(mapPath), "*.pgm", SearchOption.AllDirectories);
            for (int i = 0; i < files.Count(); i++) {
                Console.WriteLine(files[i]);
                if (Path.GetFileName(files[i]) == "temp.pgm" || Path.GetFileName(files[i]) == "temp2.pgm")
                    File.Delete(files[i]);
            }

            // Only continue if a valid .png file was selected
            if (this.mapPath != "err::no_map_selected")
            {
                // Reset the progress bar
                progressBar1.Visible = false;
                label1.Visible = false;

                // Disable the Save Button
                button3.Enabled = false;

                #region Try to dispose of the previous map image and delete the temporary file(s)
                try
                {
                    File.Delete(mapCleanup.getTempPath());
                    this.mapImage.Dispose();
                }
                catch (Exception ee)
                {
                    Console.WriteLine("Exception thrown: " + ee);
                }
                try
                {
                    File.Delete(tempPGMPath);
                }
                catch (Exception ee)
                {
                    Console.WriteLine("No file (temp2.pgm) found to delete. Error: " + ee);
                }
                #endregion

                // Assign the image and clear the picturebox
                this.mapImage = Image.FromFile(this.mapPath);
                pictureBox1.Image = null;

                // Assign the image to the picture box
                pictureBox1.Image = this.mapImage;

                // Assign the directory path and load the trajectory and offset files
                this.dirPath = this.mapCleanup.getDir();

                // Assign the paths for the offset and trajectory files
                this.offsetPath = mapCleanup.getOffset();
                this.trajPath = mapCleanup.getTraj();

                // If the paths for both the offset and trajectory files aren't found, do not generate Park and Dock coordinates
                if (this.offsetPath != "" && this.trajPath != "")
                {
                    mapCleanup.generateParkandDock(true);
                }
                else
                {
                    mapCleanup.generateParkandDock(false);
                }

                // Generate a .pgm file 
                string pgmPath = mapCleanup.generatePGM();

                // Hide the generated .pgm file
                File.SetAttributes(pgmPath, FileAttributes.Hidden);

                // Make the store info headers visible
                label2.Visible = true;
                label3.Visible = true;

                // Get the store name and number
                string storeName = mapCleanup.getStoreInfo("name");
                string storeNumber = mapCleanup.getStoreInfo("number");

                // Populate the labels with the store name and the store number
                label4.Text = storeName;
                label5.Text = storeNumber;

                // Create data path as "[directory]\[storename][storenumber]data.csv"
                csvPath = mapCleanup.getDir() + "\\" + storeName + storeNumber + "data.csv";

                // Load the image
                if (img.load(pgmPath))
                    tBox = new toolbox(img.getpixels(), img.getImageData().width, img.getImageData().height, img.getImageData().totalpixels);

                // Make the Clean Map button clickable
                button2.Enabled = true; // Clean Map Button

                // Make the progress bar and bar label visible to the user
                label1.Visible = true;
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                progressBar1.Maximum = img.getImageData().totalpixels;

                // Allow Eagle Eye to be turned on
                button7.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e) // Cleans the map
        {
            if (tBox != null)
            {
                if (trajPath != "")
                    tBox.removeDebris(mapCleanup);

                // Update the progress bar as the cleaning is performed
                tBox.run(progressBar1);

                // Create a path to temporary cleaned .pgm file
                this.tempPGMPath = mapCleanup.getDir() + "\\" + "temp2.pgm";

                // Create a temporary cleaned .pgm file to hold the cleaned map
                img.write(tempPGMPath);

                byte[] pngData;

                // Create a new temporary cleaned .png file to hold the cleaned map to be used by the picturebox
                using (MagickImage newPNG = new MagickImage(this.tempPGMPath))
                {
                    newPNG.Format = MagickFormat.Png;
                    pngData = newPNG.ToByteArray();
                }

                MemoryStream mStream = new MemoryStream();
                mStream.Write(pngData, 0, Convert.ToInt32(pngData.Length));
                Bitmap tempPNG = new Bitmap(mStream, false);
                mStream.Dispose();

                // Hide the temporary .pgm files so the user can't select or delete them accidentally
                File.SetAttributes(this.tempPGMPath, FileAttributes.Hidden);

                // Set the picture box image to temp.png (This image is a cleaned version of the map, used for display purposes only)
                pictureBox1.Image.Dispose();
                pictureBox1.Image = tempPNG;

                // Display a message and enable saving upon success
                MessageBox.Show("Cleaning is done!", "Clean done", 0);
                button3.Enabled = true; // Save Map Button
                button4.Enabled = true; // Save Data button

                // Disable the Clean Map button
                button2.Enabled = false;
            }
            else
                MessageBox.Show("Toolbox was not loaded!", "Toolbox not loaded", 0);
        }

        private void button3_Click(object sender, EventArgs e) // Saves the file
        {
            // Get the file's save name
            this.fileSaveName = mapCleanup.getSaveFile();

            try
            {
                // Write the file, notify the user, and allow the user to open the original file in GIMP
                img.write(fileSaveName);
                MessageBox.Show("File successfully saved!", "File Saved", 0);
                button6.Enabled = true;
            }
            catch
            {
                MessageBox.Show("File was not saved.", "File Not Saved", 0);
            }
        }

        private void button4_Click(object sender, EventArgs e) // Save Data Button
        {
            // Open/create csvfile, get hte object data from the toolbox
            StreamWriter csvfile = new StreamWriter(csvPath);
            List<objectData> data = tBox.getObjectData();

            // Write the file header
            csvfile.WriteLine("val, size, edge, dust, struc, res, type, c avg, c edge, c size");

            // Iterate through the list and print out the data
            for (int i = 0; i < data.Count; i++)
            {
                csvfile.Write(data[i].avgval + "," + data[i].size + "," + data[i].edgeratio + "," + data[i].objconf.dust + "," + data[i].objconf.structure + ",");

                if (data[i].objconf.isStructure)
                    csvfile.Write((data[i].objconf.structure - data[i].objconf.dust) + ",struc," + (data[i].objconf.s_val - data[i].objconf.d_val) + "," + (data[i].objconf.s_edge - data[i].objconf.d_edge) + "," + (data[i].objconf.s_size - data[i].objconf.d_size) + "\n");
                else
                    csvfile.Write((data[i].objconf.dust - data[i].objconf.structure) + ",dust," + (data[i].objconf.d_val - data[i].objconf.s_val) + "," + (data[i].objconf.d_edge - data[i].objconf.s_edge) + "," + (data[i].objconf.d_size - data[i].objconf.s_size) + "\n");
            }

            csvfile.Close();

            MessageBox.Show("Data Saved!", "Data Saved", 0);
        }

        private void button5_Click(object sender, EventArgs e) // Help Button
        {
            // Display the Help Form
            helpForm newHelpForm = new helpForm();
            newHelpForm.Show();
        }

        private void button6_Click(object sender, EventArgs e) // This button opens the original map in GIMP -- Needs to be made more robust
        {
            // Open GIMP with the original image and the new image - may need to search for .exe based on different version of GIMP
            System.Diagnostics.Process.Start("C:\\Program Files\\GIMP 2\\bin\\gimp-2.10.exe", "\"" + this.mapPath + "\" \"" + this.fileSaveName + "\"");

            // Don't allow the user to open GIMP on the same file multiple times
            button6.Enabled = false;
        }

        private void button7_Click(object sender, EventArgs e) // Eagle Eye Button
        {
            button7.Enabled = false;
            MessageBox.Show("Eagle Eye not yet implemented!");
            button7.Enabled = true;
        }
    }
}
