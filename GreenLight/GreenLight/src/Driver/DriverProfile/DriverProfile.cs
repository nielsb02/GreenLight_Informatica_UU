using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace GreenLight
{

    //Data class for the DriverProfile
    //This is a class that collects some of the data
    //that is sent to the data collector once this car has finished its lifespan

    public class DriverProfile
    {
        public int ticksOnBrake = 0;
        public double fuelUsed = 0;
        public int ticksStationary = 0;

        public bool isBraking;
        public string imageFile;
        public Tuple<int, int> imageIndex;

        public string mood;
        public DriverProfileFace faceType;
        public Bitmap imgFace;

        //To add some interest to the profiles, we decided to give them all faces and personality
        //there are 4 presets of faces that can be selected during in the World creation
        //These are 4 grid of multiple faces, a random face is then choicen from this grid

        public DriverProfile(World _physics)
        {
            Random ran = new Random();

            string _type = _physics.entityTypes;

            faceType = DriverProfileData.faces.Find(x => x.name == _type); 

            if(faceType == null)
            {
                faceType = DriverProfileData.faces.First();
            }

            //this.mood = faceType.speech[1][ran.Next(0, faceType.speech[1].Count)];

            int _indexX = ran.Next(1, faceType.index.Item1 + 1);
            int _indexY = ran.Next(1, faceType.index.Item2 + 1);

            this.imageIndex = new Tuple<int, int>(_indexX, _indexY);

            Bitmap _original = new Bitmap(faceType.fileName);
            Rectangle _src = new Rectangle(faceType.imgSize.Item1 * (_indexX - 1), faceType.imgSize.Item2 * (_indexX - 1), faceType.imgSize.Item1, faceType.imgSize.Item2);
            this.imgFace = (Bitmap)_original.Clone(_src, _original.PixelFormat);
        }

        //Allows the AI to document the amount of braketicks that occurred
        public void AddBrakeTick()
        {
            this.ticksOnBrake++;
        }

        //Calculates the used fuel based on the amount of time driven and at which speed the car drove during that time
        public void CalculateFuel(double _speed)
        {
            ticksStationary = _speed <= 0 ? ticksStationary++ : ticksStationary;
            fuelUsed += _speed * 0.005;
        }
    }
}
