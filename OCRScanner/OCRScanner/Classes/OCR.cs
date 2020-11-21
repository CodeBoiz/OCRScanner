using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;

namespace OCRScanner.Classes
{
    public static class OCR
    {
        //Declare a new Tesseract OCR engine
        private static Tesseract _ocr;

        /// <summary>
        /// Set the dictionary and whitelist for the Tesseract detection object
        /// </summary>
        /// <param name="dataPath">The path to the tessdata file</param>
        public static void SetTesseractObjects(string dataPath)
        {
            //create OCR engine
            _ocr = new Tesseract(dataPath, "eng", OcrEngineMode.TesseractLstmCombined, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-+.():'';1234567890/ ");
        }

        /// <summary>
        /// Pass the image through multiple filters and sort contours
        /// </summary>
        /// <param name="img">The image that will be proccessed</param>
        /// <returns>A list of Mat ROIs</returns>
        private static Mat ImageProccessing(Mat img)
        {
            //Resize the image for better uniformitty throughout the code
            CvInvoke.Resize(img, img, new System.Drawing.Size(600, 800));

            Mat imgClone = img.Clone();

            //Convert the image to grayscale
            CvInvoke.CvtColor(img, img, ColorConversion.Bgr2Gray);

            //Blur the image
            CvInvoke.GaussianBlur(img, img, new System.Drawing.Size(3, 3), 4, 4);

            CvInvoke.Imshow("GaussianBlur", img);
            CvInvoke.WaitKey(0);

            //Threshold the image
            CvInvoke.AdaptiveThreshold(img, img, 100, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, 5, 6);

            CvInvoke.Imshow("Thereshold", img);
            CvInvoke.WaitKey(0);

/*            //Canny the image
            CvInvoke.Canny(img, img, 75, 100);

            CvInvoke.Imshow("Canny", img);
            CvInvoke.WaitKey(0);*/

            /*            //Dilate the canny image
                        CvInvoke.Dilate(img, img, null, new System.Drawing.Point(-1, -1), 8, BorderType.Constant, new MCvScalar(0, 255, 255));

                        CvInvoke.Imshow("Dilate", img);
                        CvInvoke.WaitKey(0);*/

            //Filter the contours to only find relevent ones
            /*            List<Mat> foundOutput = FindandFilterContours(imgClone, img);

                        for (int i = 0; i < foundOutput.Count; i++)
                        {
                            CvInvoke.Imshow("Found Output", foundOutput[i]);
                            CvInvoke.WaitKey(0);
                        }*/

            return img;
        }

        /// <summary>
        /// Find and sort contours found on the filtered image
        /// </summary>
        /// <param name="originalImage">The original unaltered image</param>
        /// <param name="filteredImage">The filtered image</param>
        /// <returns>A list of ROI mat objects</returns>
        private static List<Mat> FindandFilterContours(Mat originalImage, Mat filteredImage)
        {
            //Clone the input image
            Image<Bgr, byte> originalImageClone = originalImage.Clone().ToImage<Bgr, byte>();
            Mat filteredImage2 = filteredImage;

            //Declare a new vector that will store contours
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            //Find and draw the contours on the blank image
            CvInvoke.FindContours(filteredImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
            CvInvoke.DrawContours(filteredImage2, contours, -1, new MCvScalar(255, 0, 0));

            CvInvoke.Imshow("filteredImage2", filteredImage2);
            CvInvoke.WaitKey(0);

            //Create two copys of the cloned image of the input image
            Image<Bgr, byte> allContoursDrawn = originalImageClone.Copy();
            Image<Bgr, byte> finalCopy = originalImageClone.Copy();

            //Create two lists that will be used elsewhere in the algorithm
            List<Rectangle> listRectangles = new List<Rectangle>();
            List<int> listXValues = new List<int>();

            //Loop over all contours
            for (int i = 0; i < contours.Size; i++)
            {
                //Create a bounding rectangle around each contour
                Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                originalImageClone.ROI = rect;

                //Add the bounding rectangle and its x value to their corresponding lists
                listRectangles.Add(rect);
                listXValues.Add(rect.X);

                //Draw the bounding rectangle on the image
                allContoursDrawn.Draw(rect, new Bgr(255, 0, 0), 5);
            }

            //Create two new lists that will hold data in the algorithms later on
            List<int> indexList = new List<int>();
            List<int> smallerXValues = new List<int>();

            //Loop over all relevent information
            for (int i = 0; i < listRectangles.Count; i++)
            {
                //If a bounding rectangle fits certain dementions, add it's x value to another list
                if ((listRectangles[i].Width < 400) && (listRectangles[i].Height < 400)
                    && (listRectangles[i].Y > 200) && (listRectangles[i].Y < 300) &&
                    (listRectangles[i].Width > 50) && (listRectangles[i].Height > 40))
                {
                    originalImageClone.ROI = listRectangles[i];

                    finalCopy.Draw(listRectangles[i], new Bgr(255, 0, 0), 5);

                    smallerXValues.Add(listRectangles[i].X);
                }
            }

            //Sort the smaller list into asending order
            smallerXValues.Sort();

            //Loop over each value in the sorted list, and check if the same value is in the original list
            //If it is, add the index of the that value in the original list to a new list
            for (int i = 0; i < smallerXValues.Count; i++)
            {
                for (int j = 0; j < listXValues.Count; j++)
                {
                    if (smallerXValues[i] == listXValues[j])
                    {
                        indexList.Add(j);
                    }
                }
            }

            //A list to hold the final ROIs
            List<Mat> outputImages = new List<Mat>();

            //Loop over the sorted indexes, and add them to the final list
            for (int i = 0; i < indexList.Count; i++)
            {
                originalImageClone.ROI = listRectangles[indexList[i]];

                outputImages.Add(originalImageClone.Mat.Clone());
            }

            //Return the list of relevent images
            return outputImages;
        }

        /// <summary>
        /// Detects text on an image
        /// </summary>
        /// <param name="img">The image where text will be extracted from</param>
        /// <returns>A string of detected text</returns>
        public static List<string> RecognizeText(Mat img)
        {
            List<string> outputList = new List<string>();

            //Change this file path to the path where the images you want to stich are located
            string filePath = Directory.GetParent(Directory.GetParent
                (Environment.CurrentDirectory).ToString()) + @"\Tessdata\";

            //Declare the use of the dictonary
            SetTesseractObjects(filePath);

            //Get all cropped regions
            Mat croppedRegions = ImageProccessing(img);

            //String that will hold the output of the detected text
            string output = "";

            Tesseract.Character[] words;

            try
            {
                StringBuilder strBuilder = new StringBuilder();

                //Set and detect text on the image
                _ocr.SetImage(croppedRegions);
                _ocr.Recognize();

                //Get the charactors of the detected words
                words = _ocr.GetCharacters();

                //Pass each instance of the detected words to the string builder
                for (int j = 0; j < words.Length; j++)
                {
                    strBuilder.Append(words[j].Text);
                }

                //Pass the stringbuilder into a string variable
                output += strBuilder.ToString() + " ";

                outputList.Add(output);
            }
            catch (AccessViolationException)
            {
                MessageBox.Show("There was a problem with the input image, please retake the image");
            }

            //Return a string
            return outputList;
        }
    }
}