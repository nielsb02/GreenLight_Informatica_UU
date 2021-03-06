using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Text;

namespace GreenLight
{
    // This class contains methods to build a straight (horizontal, vertical or diagonal) road or a curved road.
    // It also contains a method to build a crossroad, by redirecting to the newcrossroad method in the crossroadcontroller.
    // This class manages clicking on roads. 
    // When a road is clicked, a window opens, in which you can change the direction of the driving lanes of that particular road, 
    // by clicking on a lane in a Image of the road, shown on the settingsscreen.
    // Because a hitbox is re-calculated for every driving lane of the road on the Image on the settingscreen.
    // You can also delete the road you've selected, with the delete Button.
    // A method to calculate the direction of the road is here as well. 
    // There is also a method that manages connections between roads.

    public class RoadController : EntityController
    {
        public List<AbstractRoad> roads = new List<AbstractRoad>();
        public PictureBox Screen;
        public string roadType = "D";
        public string lastType = "X";

        private Form settingScreen;
        private PictureBox settingScreenImage;
        private CurvedButtons doneButton, deleteButton;

        private AbstractRoad selectedRoad;

        public CrossRoadController crossRoadController;
        public Bitmap ArrowBitmap = new Bitmap(Image.FromFile("../../src/User Interface Recources/Arrow.png"));
        public List<List<CrossArrow>> AllCrossArrows = new List<List<CrossArrow>>();

        public bool visualizeLanePoints = true;

        public RoadController(PictureBox _screen)
        {
            this.Screen = _screen;
            this.Screen.MouseClick += RoadClick;
            crossRoadController = new CrossRoadController(this.Screen);
            initSettingScreen();
        }

        private void initSettingScreen()
        {
            Dictionary<string, int> menu = Roads.Config.settingsScreen;

            this.settingScreen = new PopUpForm(new Size(menu["width"], menu["length"]));
            this.settingScreen.Hide();

            this.settingScreen.Size = new Size(menu["width"], menu["length"]);
            this.settingScreen.BackColor = Color.FromArgb(255, 255, 255);
            this.settingScreen.FormBorderStyle = FormBorderStyle.None;

            settingScreenImage = new PictureBox();

            settingScreenImage.Paint += SettingBoxDraw;
            settingScreenImage.MouseClick += SettingBoxClick;

            settingScreenImage.Size = new Size(menu["width"] - 2 * menu["offset"], menu["width"] - 2 * menu["offset"]);
            settingScreenImage.Location = new Point(menu["offset"], menu["offset"]);
            settingScreenImage.BackColor = Color.Black;

            doneButton = new CurvedButtons(new Size(menu["buttonWidth"], menu["buttonHeight"]), new Point(menu["offset"], menu["width"]), menu["buttonCurve"], "../../src/User Interface Recources/Custom_Small_Button.png", "Save", DrawData.Dosis_font_family, this.settingScreen, this.settingScreen.BackColor);
            doneButton.Click += (object o, EventArgs ea) => { DoneSettingScreen(); };

            deleteButton = new CurvedButtons(new Size(menu["buttonWidth"] + 10, menu["buttonHeight"]), new Point(menu["offset"] + menu["buttonWidth"] + menu["betweenButtons"], menu["width"]), menu["buttonCurve"], "../../src/User Interface Recources/Custom_Small_Button.png", "Delete", DrawData.Dosis_font_family, this.settingScreen, this.settingScreen.BackColor);
            deleteButton.Click += (object o, EventArgs ea) => { DeleteRoad(this.selectedRoad); };

            MovePanel move_panel = new MovePanel(settingScreen);

            move_panel.Location = new Point(menu["mpX"], menu["mpY"]);
            move_panel.Size = new Size(menu["mpWidth"], menu["mpHeight"]);
            settingScreen.Controls.Add(move_panel);


            settingScreen.Controls.Add(doneButton);
            settingScreen.Controls.Add(deleteButton);

            this.settingScreen.Controls.Add(settingScreenImage);
        }

        // The following three methods create the different types of roads. 
        public void BuildDiagonalRoad(Point _point1, Point _point2, int _lanes, bool _beginconnection, bool _endconnection, AbstractRoad _beginConnectedTo, AbstractRoad _endConnectedTo)
        {
            string _dir = RoadMath.Direction(_point1, _point2, "DiagonalRoad");
            AbstractRoad _road = new DiagonalRoad(_point1, _point2, _lanes, _dir, "Diagonal", _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
            roads.Add(_road);

            Connection(_point1, _point2, _lanes, _dir, _road, _beginconnection, _endconnection);


            CheckLaneDirections(_road, 0);
            General_Form.Main.BuildScreen.builder.gridController.FlipGridpointsToTrue(_road.hitbox);
            General_Form.Main.BuildScreen.builder.gridController.ConnectionGridpointsToFalse();
        }

        public void BuildCrossRoad(Point _point1, int _lanes, bool _beginconnection, bool _endconnection)
        {
            if (_lanes % 2 == 0)
            {
                _point1.X -= 10;
                _point1.Y += 10;
            }
            AbstractRoad _temp = crossRoadController.newCrossRoad(_point1, _lanes, "CrossRoad");
            this.roads.Add(_temp);

            this.Screen.Invalidate();
            crossRoadController.ShowSettingScreen((CrossRoad)_temp);
        }

        public void BuildCurvedRoad(Point _point1, Point _point2, int _lanes, string _type, bool _beginconnection, bool _endconnection, AbstractRoad _beginConnectedTo, AbstractRoad _endConnectedTo)
        {
            string _dir = RoadMath.Direction(_point1, _point2, _type);
            Point _temp1 = _point1;
            Point _temp2 = _point2;

            if (_type == "Curved")
            {
                if (_dir == "NW")
                {
                    _dir = "SE";
                    _point1 = _temp2;
                    _point2 = _temp1;
                }
                else if (_dir == "NE")
                {
                    _dir = "SW";
                    _point1 = _temp2;
                    _point2 = _temp1;
                }
            }
            else if (_type == "Curved2")
            {
                if (_dir == "SE")
                {
                    _dir = "NW";
                    _point1 = _temp2;
                    _point2 = _temp1;
                }
                else if (_dir == "SW")
                {
                    _dir = "NE";
                    _point1 = _temp2;
                    _point2 = _temp1;
                }
            }

            AbstractRoad _road = new CurvedRoad(_point1, _point2, _lanes, _dir, _type, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
            roads.Add(_road);
            Connection(_point1, _point2, _lanes, _dir, _road, _beginconnection, _endconnection);
            CheckLaneDirections(_road, 0);
            General_Form.Main.BuildScreen.builder.gridController.FlipGridpointsToTrue(_road.hitbox);
            General_Form.Main.BuildScreen.builder.gridController.ConnectionGridpointsToFalse();
        }


        // This method checks if a new road is close enough to another road to make a connection, if it is the (Cross)Connection class is called to make it happen
        public void Connection(Point _point1, Point _point2, int _lanes, string _dir, AbstractRoad _road, bool _beginconnection, bool _endconnection)
        {
            try
            {
                foreach (AbstractRoad x in roads)
                {
                    // if neither of the two roads are CrossRoads
                    if (x != _road && (_road.Type != "Cross" && x.Type != "Cross"))
                    {
                        Point _temp1, _temp2, _temp3 = new Point(-100, -100), _temp4 = new Point(-100, -100);
                        _temp1 = x.getPoint1();
                        _temp2 = x.getPoint2();

                        if (x.getLanes() == _lanes)
                        {
                            if (_lanes % 2 == 0 && _lanes > 2)
                            {
                                _temp3 = x.Drivinglanes[_lanes / 2 + 1].points.First().cord;
                                _temp4 = x.Drivinglanes[_lanes / 2 + 1].points.Last().cord;
                            }
                            else if (_lanes % 2 == 0)
                            {
                                _temp3 = x.Drivinglanes[0].points.First().cord;
                                _temp4 = x.Drivinglanes[0].points.Last().cord;
                            }

                            if ((_point1 == _temp1 || Math.Sqrt(Math.Pow(_point1.X - _temp1.X, 2) + Math.Pow(_point1.Y - _temp1.Y, 2)) <= Grid.Config.SpacingWidth + 1) && (_point1 != _temp3 && _point1 != _temp4))
                            {
                                if (_beginconnection == false)
                                {

                                    Connection _connection = new Connection(_point1, _temp1, _lanes, _dir, x.Dir, _road, x);

                                }
                                else
                                {
                                    x.beginconnection = true;
                                    x.beginConnectedTo = _road;
                                    _road.beginConnectedTo = x;
                                }
                            }
                            else if ((_point1 == _temp2 || Math.Sqrt(Math.Pow(_point1.X - _temp2.X, 2) + Math.Pow(_point1.Y - _temp2.Y, 2)) <= Grid.Config.SpacingWidth + 1) && (_point1 != _temp3 && _point1 != _temp4))
                            {
                                if (_beginconnection == false)
                                {
                                    Connection _connection = new Connection(_point1, _temp2, _lanes, _dir, x.Drivinglanes[0].dir, _road, x);
                                }
                                else
                                {
                                    x.endconnection = true;
                                    x.endConnectedTo = _road;
                                    _road.beginConnectedTo = x;
                                }
                            }
                            else if ((_point2 == _temp1 || Math.Sqrt(Math.Pow(_point2.X - _temp1.X, 2) + Math.Pow(_point2.Y - _temp1.Y, 2)) <= Grid.Config.SpacingWidth + 1) && (_point2 != _temp3 && _point2 != _temp4))
                            {
                                if (_endconnection == false)
                                {
                                    Connection connection = new Connection(_point2, _temp1, _lanes, _dir, x.Drivinglanes[0].dir, _road, x);
                                }
                                else
                                {
                                    x.beginconnection = true;
                                    x.beginConnectedTo = _road;
                                    _road.endConnectedTo = x;

                                }
                            }
                            else if ((_point2 == _temp2 || Math.Sqrt(Math.Pow(_point2.X - _temp2.X, 2) + Math.Pow(_point2.Y - _temp2.Y, 2)) <= Grid.Config.SpacingWidth + 1) && (_point1 != _temp3 && _point1 != _temp4))
                            {
                                if (_endconnection == false)
                                {
                                    Connection _connection = new Connection(_point2, _temp2, _lanes, _dir, x.Drivinglanes[0].dir, _road, x);
                                }
                                else
                                {
                                    x.endconnection = true;
                                    x.endConnectedTo = _road;
                                    _road.endConnectedTo = x;
                                }
                            }
                        }
                    }
                    // if one or both of the roads are CrossRoads
                    else if (x != _road)
                    {
                        Point _temp1;
                        Point _temp2;

                        _temp1 = x.getPoint1();
                        _temp2 = x.getPoint2();

                        if (x.Type == "Cross" && _road.Type != "Cross")
                        {
                            foreach (ConnectionPoint _cp in x.translatedconnectPoints)
                            {
                                if (!(_road.beginConnectedTo == x || _road.endConnectedTo == x))
                                {
                                    if (_cp.Side == "Top" || _cp.Side == "Bottom")
                                    {
                                        if (_point1 == _cp.Location || (Math.Abs(_point1.Y - _cp.Location.Y) <= 25 && _point1.X == _cp.Location.X))
                                        {
                                            CrossConnection _connection = new CrossConnection(_point1, _cp.Location, _dir, x.Dir, _road, x);
                                        }
                                        else if (_point2 == _cp.Location || (Math.Abs(_point2.Y - _cp.Location.Y) <= 25 && _point2.X == _cp.Location.X))
                                        {
                                            CrossConnection _connection = new CrossConnection(_point2, _cp.Location, _dir, x.Dir, _road, x);
                                        }
                                    }
                                    else
                                    {
                                        if (_point1 == _cp.Location || (Math.Abs(_point1.X - _cp.Location.X) <= 25 && _point1.Y == _cp.Location.Y))
                                        {
                                            CrossConnection _connection = new CrossConnection(_point1, _cp.Location, _dir, x.Dir, _road, x);
                                        }
                                        else if (_point2 == _cp.Location || (Math.Abs(_point2.X - _cp.Location.X) <= 25 && _point2.Y == _cp.Location.Y))
                                        {
                                            CrossConnection _connection = new CrossConnection(_point2, _cp.Location, _dir, x.Dir, _road, x);
                                        }
                                    }
                                }
                            }
                        }

                        else if (x.Type != "Cross" && _road.Type == "Cross")
                        {
                            foreach (ConnectionPoint _cp in _road.translatedconnectPoints)
                            {
                                if (!(x.beginConnectedTo == _road || x.endConnectedTo == _road))
                                {
                                    if (_cp.Side == "Top" || _cp.Side == "Bottom")
                                    {
                                        if (_temp1 == _cp.Location || (Math.Abs(_temp1.Y - _cp.Location.Y) <= 25 && _temp1.X == _cp.Location.X))
                                        {
                                            CrossConnection _connection = new CrossConnection(_cp.Location, _temp1, _dir, x.Dir, _road, x);
                                        }
                                        else if (_temp2 == _cp.Location || (Math.Abs(_temp2.Y - _cp.Location.Y) <= 25 && _temp2.X == _cp.Location.X))
                                        {
                                            CrossConnection _connection = new CrossConnection(_cp.Location, _temp2, _dir, x.Dir, _road, x);
                                        }
                                    }
                                    else
                                    {
                                        if (_temp1 == _cp.Location || (Math.Abs(_temp1.X - _cp.Location.X) <= 25 && _temp1.Y == _cp.Location.Y))
                                        {
                                            CrossConnection _connection = new CrossConnection(_cp.Location, _temp1, _dir, x.Dir, _road, x);
                                        }
                                        else if (_temp2 == _cp.Location || (Math.Abs(_temp2.X - _cp.Location.X) <= 25 && _temp2.Y == _cp.Location.Y))
                                        {
                                            CrossConnection _connection = new CrossConnection(_cp.Location, _temp2, _dir, x.Dir, _road, x);
                                        }
                                    }
                                }
                            }
                        }

                        else
                        {
                            foreach (ConnectionPoint _cp in _road.translatedconnectPoints)
                            {
                                foreach (ConnectionPoint _cp2 in x.translatedconnectPoints)
                                {
                                    if ((_cp.Side == "Top" && _cp2.Side == "Bottom") || (_cp2.Side == "Top" && _cp.Side == "Bottom"))
                                    {
                                        if (_cp.Location == _cp2.Location || (Math.Abs(_cp.Location.Y - _cp2.Location.Y) <= 25 && _cp.Location.X == _cp2.Location.X))
                                        {
                                            CrossConnection _connection = new CrossConnection(_cp.Location, _cp2.Location, _dir, x.Dir, _road, x);
                                        }
                                    }
                                    else if ((_cp.Side == "Left" || _cp.Side == "Right") && (_cp2.Side == "Left" || _cp2.Side == "Right"))
                                    {
                                        if (_cp.Location == _cp2.Location || Math.Abs(_cp.Location.X - _cp2.Location.X) <= 25 && _cp.Location.Y == _cp2.Location.Y)
                                        {
                                            CrossConnection _connection = new CrossConnection(_cp.Location, _cp2.Location, _dir, x.Dir, _road, x);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { };
        }

        // Removes a road when the undo button is clicked
        public void UndoRoad()
        {
            if (roads.Count != 0)
            {
                if (roads[roads.Count - 1].Type == "Cross")
                {
                    crossRoadController.DeleteCrossroad(roads[roads.Count - 1]);
                    General_Form.Main.BuildScreen.Screen.Invalidate();
                }
                else
                {
                    General_Form.Main.BuildScreen.builder.gridController.UndoGridpoints(roads[roads.Count - 1]);
                    DeleteRoad(roads[roads.Count - 1]);
                    if (roads.Count == 0)
                    {
                        General_Form.Main.BuildScreen.builder.gridController.ResetAllGridpoints();
                    }
                    General_Form.Main.BuildScreen.Screen.Invalidate();
                }
            }
        }

        public override void Initialize()
        {

        }

        // Handles what happens when a road is clicked
        public void RoadClick(object o, MouseEventArgs mea)
        {
            if (this.roadType == "D" || settingScreen.Visible == true)
            {
                return;
            }

            try
            {
                this.selectedRoad = roads.Find(x => x.hitbox.Contains(mea.Location));
                if (this.selectedRoad == null)
                {
                    return;
                }

                if (this.roadType == "X")
                {
                    EnableSettingScreen();
                }
            }
            catch (Exception e)
            {

            }
        }

        private void EnableSettingScreen()
        {

            if (selectedRoad.Type == "Cross")
            {
                crossRoadController.ShowSettingScreen((CrossRoad)selectedRoad);
                return;
            }

            selectedRoad.hitbox.color = Color.Pink;

            if (selectedRoad.Drivinglanes.All(x => x.offsetHitbox == null))
            {
                DrivingLaneHitbox();
            }

            settingScreen.ShowDialog();
            settingScreen.BringToFront();
            settingScreen.Invalidate();
        }

        private void DisableSettingScreen()
        {
            settingScreen.Hide();
            Screen.Invalidate();
        }

        private void DoneSettingScreen()
        {
            selectedRoad.hitbox.color = Color.Yellow;
            DisableSettingScreen();
        }

        // Deletes a road when the delete button in the road pop-up-screen is pressed
        public void DeleteRoad(AbstractRoad _deletedroad)
        {
            if (_deletedroad == this.selectedRoad)
            {
                this.selectedRoad = null;
                DisableSettingScreen();
            }
            General_Form.Main.BuildScreen.builder.gridController.UndoGridpoints(_deletedroad);
            roads.Remove(_deletedroad);

            if (roads.Count == 0)
            {
                General_Form.Main.BuildScreen.builder.gridController.ResetAllGridpoints();
            }
        }

        private void SettingBoxClick(object o, MouseEventArgs mea)
        {
            if (selectedRoad.Drivinglanes.All(x => x.offsetHitbox == null))
            {
                return;
            }

            DrivingLane _lane = (DrivingLane)selectedRoad.Drivinglanes.Find(x => x.offsetHitbox.Contains(mea.Location));

            if (_lane == null)
            {
                return;
            }

            _lane.FlipPoints();

            CheckLaneDirections(selectedRoad, 0);
            Screen.Invalidate();
            settingScreen.Invalidate();
            settingScreenImage.Invalidate();
        }

        // Draws the image in a road's pop-up-screen
        private void SettingBoxDraw(object o, PaintEventArgs pea)
        {
            Graphics g = pea.Graphics;

            Bitmap b = new Bitmap(Screen.Width, Screen.Height);
            Screen.DrawToBitmap(b, new Rectangle(new Point(0, 0), Screen.Size));

            Hitbox _hitbox = selectedRoad.hitbox;

            int _maxSize = Math.Max(_hitbox.Size.Width, _hitbox.Size.Height) + Roads.Config.scaleOffset * 2;
            int _diff = Math.Abs(_hitbox.Size.Width - _hitbox.Size.Height) / 2;

            Rectangle _rec;

            if (_hitbox.Size.Width > _hitbox.Size.Height)
            {
                _rec = new Rectangle(_hitbox.Topcord.X - Roads.Config.scaleOffset, _hitbox.Topcord.Y - Roads.Config.scaleOffset - _diff, _maxSize, _maxSize);
            }
            else if (_hitbox.Size.Width == _hitbox.Size.Height)
            {
                _rec = new Rectangle(_hitbox.Topcord.X - Roads.Config.scaleOffset, _hitbox.Topcord.Y - Roads.Config.scaleOffset, _maxSize, _maxSize);
            }
            else
            {
                _rec = new Rectangle(_hitbox.Topcord.X - Roads.Config.scaleOffset - _diff, _hitbox.Topcord.Y - Roads.Config.scaleOffset, _maxSize, _maxSize);
            }



            Rectangle _des = new Rectangle(0, 0, this.settingScreenImage.Width, this.settingScreenImage.Height);

            g.DrawImage(b, _des, _rec, GraphicsUnit.Pixel);

            selectedRoad.Drivinglanes.ForEach(x => x.DrawoffsetHitbox(g));
        }

        // Creates a hitbox for each DrivingLane
        private void DrivingLaneHitbox()
        {
            Point _diff = selectedRoad.hitbox.Topcord;

            foreach (DrivingLane _drivinglane in selectedRoad.Drivinglanes)
            {
                Point _one, _two;
                if (_drivinglane.flipped)
                {
                    _one = _drivinglane.points.First().cord;
                    _two = _drivinglane.points.Last().cord;
                }
                else
                {
                    _one = _drivinglane.points.Last().cord;
                    _two = _drivinglane.points.First().cord;
                }


                Point _oneoffset = new Point(_one.X - _diff.X, _one.Y - _diff.Y);
                Point _twooffset = new Point(_two.X - _diff.X, _two.Y - _diff.Y);

                double _scale;

                double diff = Math.Max(selectedRoad.hitbox.Size.Width, selectedRoad.hitbox.Size.Height) + Roads.Config.scaleOffset * 2;

                if (selectedRoad.hitbox.Size.Width > selectedRoad.hitbox.Size.Height)
                {
                    _scale = (double)(this.settingScreenImage.Width) / diff;
                }
                else
                {
                    _scale = (double)(this.settingScreenImage.Height) / diff;
                }

                double? offset;



                float Degree = RoadMath.OldCalculateAngle(_one, _two);

                if (selectedRoad.hitbox.Size.Width >= selectedRoad.hitbox.Size.Height)
                {
                    if ((Degree >= 315 && Degree < 360) || (Degree >= 0 && Degree < 45) || (Degree >= 135 && Degree < 225))
                    {

                        if (selectedRoad.Type == "Curved" || selectedRoad.Type == "Curved2")
                        {
                            offset = (double)this.settingScreenImage.Height / 2 - selectedRoad.hitbox.Size.Height / 2 * _scale;
                        }
                        else
                        {
                            int temp = Math.Abs(_oneoffset.Y - _twooffset.Y);
                            offset = (double)this.settingScreenImage.Height / 2 - temp / 2 * _scale;
                        }

                        _oneoffset = new Point((int)((_oneoffset.X + Roads.Config.scaleOffset) * _scale), (int)(((_oneoffset.Y) * _scale) + offset));
                        _twooffset = new Point((int)((_twooffset.X + Roads.Config.scaleOffset) * _scale), (int)(((_twooffset.Y) * _scale) + offset));
                    }
                    else
                    {
                        if (selectedRoad.Type == "Curved" || selectedRoad.Type == "Curved2")
                        {
                            offset = (double)this.settingScreenImage.Height / 2 - selectedRoad.hitbox.Size.Height / 2 * _scale;
                        }
                        else
                        {
                            int temp = Math.Abs(_oneoffset.Y - _twooffset.Y);
                            offset = (double)this.settingScreenImage.Height / 2 - ((((double)selectedRoad.getLanes() * Roads.Config.laneWidth) / 2) * _scale) - temp / 2 * _scale;
                        }

                        _oneoffset = new Point((int)((_oneoffset.X + Roads.Config.scaleOffset) * _scale), (int)(((_oneoffset.Y) * _scale) + offset));
                        _twooffset = new Point((int)((_twooffset.X + Roads.Config.scaleOffset) * _scale), (int)(((_twooffset.Y) * _scale) + offset));
                    }
                }
                else if (selectedRoad.hitbox.Size.Width < selectedRoad.hitbox.Size.Height)
                {
                    if ((Degree >= 315 && Degree < 360) || (Degree >= 0 && Degree < 45) || (Degree >= 135 && Degree < 225))
                    {

                        if (selectedRoad.Type == "Curved" || selectedRoad.Type == "Curved2")
                        {
                            offset = (double)this.settingScreenImage.Width / 2 - selectedRoad.hitbox.Size.Width / 2 * _scale;
                        }
                        else
                        {
                            int temp = Math.Abs(_oneoffset.X - _twooffset.X);
                            offset = (double)this.settingScreenImage.Width / 2 - ((((double)selectedRoad.getLanes() * Roads.Config.laneWidth) / 2) * _scale) - temp / 2 * _scale;
                        }


                        _oneoffset = new Point((int)(((_oneoffset.X) * _scale) + offset), (int)((_oneoffset.Y + Roads.Config.scaleOffset) * _scale));
                        _twooffset = new Point((int)(((_twooffset.X) * _scale) + offset), (int)((_twooffset.Y + Roads.Config.scaleOffset) * _scale));
                    }
                    else
                    {

                        if (selectedRoad.Type == "Curved" || selectedRoad.Type == "Curved2")
                        {
                            offset = (double)this.settingScreenImage.Height / 2 - selectedRoad.hitbox.Size.Height / 2 * _scale;
                        }
                        else
                        {
                            int temp = Math.Abs(_oneoffset.X - _twooffset.X);
                            offset = (double)this.settingScreenImage.Width / 2 - temp / 2 * _scale;
                        }


                        _oneoffset = new Point((int)(((_oneoffset.X) * _scale) + offset), (int)((_oneoffset.Y + Roads.Config.scaleOffset) * _scale));
                        _twooffset = new Point((int)(((_twooffset.X) * _scale) + offset), (int)((_twooffset.Y + Roads.Config.scaleOffset) * _scale));
                    }
                }

                Point[] _points = new Point[4];
                if (selectedRoad.Type == "Curved" || selectedRoad.Type == "Curved2")
                {
                    _points = RoadMath.hitBoxPointsCurved(_oneoffset, _twooffset, 1, (int)(Roads.Config.laneWidth * _scale), false, RoadMath.Direction(_oneoffset, _twooffset, selectedRoad.Type));

                }
                else if (selectedRoad.Type == "Diagonal")
                {
                    _points = RoadMath.hitBoxPointsDiagonal(_oneoffset, _twooffset, 1, (int)(Roads.Config.laneWidth * _scale), false, RoadMath.calculateSlope(_oneoffset, _twooffset), false);

                }
                Hitbox _hitbox = selectedRoad.CreateHitbox(_points);


                _drivinglane.offsetHitbox = _hitbox;
            }
        }

        private void CheckLaneDirections(AbstractRoad _thisroad, int _count)
        {
            if (_thisroad.Type == "Cross")
            {
                return;
            }
            if (_count < roads.Count())
            {
                if (_thisroad.Type == "Cross")
                {
                    return;
                }
                CheckConnectionDirections(_thisroad, _thisroad.beginConnectedTo, "Begin", _count);
                CheckConnectionDirections(_thisroad, _thisroad.endConnectedTo, "End", _count);
                General_Form.Main.BuildScreen.Screen.Invalidate();
            }
        }

        // Makes sure connected lanes go in the same direction
        public void CheckConnectionDirections(AbstractRoad _thisroad, AbstractRoad _connectionroad, string _connectionpoint, int _count)
        {

            if (_connectionroad != null && _connectionroad.Type != "Cross")
            {
                bool _flipped = false;
                foreach (Lane _thislane in _thisroad.Drivinglanes)
                {
                    double _mindistance = 50;
                    bool needsflipped = false;
                    int _indexmatchinglane = -1;

                    foreach (Lane _beginlane in _connectionroad.Drivinglanes)
                    {
                        double _distanceFirst = RoadMath.Distance(_thislane.points.First().cord, _beginlane.points.First().cord);
                        double _distanceLast = RoadMath.Distance(_thislane.points.Last().cord, _beginlane.points.Last().cord);
                        double _distanceDiff1 = RoadMath.Distance(_thislane.points.First().cord, _beginlane.points.Last().cord);
                        double _distanceDiff2 = RoadMath.Distance(_thislane.points.Last().cord, _beginlane.points.First().cord);

                        if (_distanceFirst < _mindistance && _distanceFirst < _distanceDiff1 && _distanceFirst < _distanceDiff2 && _distanceFirst < _distanceLast)
                        {
                            _indexmatchinglane = _connectionroad.Drivinglanes.IndexOf(_beginlane);
                            needsflipped = true;
                            _mindistance = _distanceFirst;
                        }
                        else if (_distanceLast < _mindistance && _distanceLast < _distanceDiff1 && _distanceLast < _distanceDiff2 && _distanceLast < _distanceFirst)
                        {
                            _indexmatchinglane = _connectionroad.Drivinglanes.IndexOf(_beginlane);
                            needsflipped = true;
                            _mindistance = _distanceLast;
                        }
                        else if (_distanceDiff1 < _mindistance && _distanceDiff1 < _distanceFirst && _distanceDiff1 < _distanceLast && _distanceDiff1 < _distanceDiff2)
                        {
                            _indexmatchinglane = _connectionroad.Drivinglanes.IndexOf(_beginlane);
                            needsflipped = false;
                            _mindistance = _distanceDiff1;
                        }
                        else if (_distanceDiff2 < _mindistance && _distanceDiff2 < _distanceFirst && _distanceDiff2 < _distanceLast && _distanceDiff2 < _distanceDiff1)
                        {
                            _indexmatchinglane = _connectionroad.Drivinglanes.IndexOf(_beginlane);
                            needsflipped = false;
                            _mindistance = _distanceDiff2;
                        }

                    }
                    if (needsflipped && _mindistance <= 2 && _indexmatchinglane >= 0 && _indexmatchinglane < _connectionroad.Drivinglanes.Count)
                    {
                        _connectionroad.Drivinglanes[_indexmatchinglane].FlipPoints();
                        _flipped = true;

                    }
                }
                if (_flipped)
                {
                    CheckLaneDirections(_connectionroad, _count + 1);
                }
            }
        }

        // Rebuilds roads when a project is loaded
        public void loadRoads(string[] _roadWords)
        {
            if (_roadWords[1] == "Diagonal")
            {
                BuildDiagonalRoad(new Point(int.Parse(_roadWords[2]), int.Parse(_roadWords[3])), new Point(int.Parse(_roadWords[4]), int.Parse(_roadWords[5])), int.Parse(_roadWords[6]), bool.Parse(_roadWords[7]), bool.Parse(_roadWords[8]), null, null);
            }
            else if (_roadWords[1] == "Curved" || _roadWords[1] == "Curved2")
            {
                BuildCurvedRoad(new Point(int.Parse(_roadWords[2]), int.Parse(_roadWords[3])), new Point(int.Parse(_roadWords[4]), int.Parse(_roadWords[5])), int.Parse(_roadWords[6]), _roadWords[1], bool.Parse(_roadWords[7]), bool.Parse(_roadWords[8]), null, null);
            }
            else if (_roadWords[1] == "Cross")
            {
                CrossRoad _new = new CrossRoad(new Point(int.Parse(_roadWords[2]), int.Parse(_roadWords[3])), new Point(int.Parse(_roadWords[2]), int.Parse(_roadWords[3])), int.Parse(_roadWords[6]), _roadWords[1], bool.Parse(_roadWords[7]), bool.Parse(_roadWords[8]), null, null);

                for (int t = 0; t < int.Parse(_roadWords[9]); t++)
                {
                    foreach (ConnectionPoint x in _new.connectPoints)
                    {
                        if (x.Location.X == int.Parse(_roadWords[10 + t * 4]) && x.Location.Y == int.Parse(_roadWords[11 + t * 4]))
                            foreach (ConnectionPoint y in _new.connectPoints)
                            {
                                if (y.Location.X == int.Parse(_roadWords[12 + t * 4]) && y.Location.Y == int.Parse(_roadWords[13 + t * 4]))
                                    _new.connectLinks.Add(new ConnectionLink(x, y));
                            }
                    }
                }
                roads.Add(_new);
                crossRoadController.roads.Add(_new);
                crossRoadController.selectedRoad = _new;
                crossRoadController.CreateDrivingLanes();
            }
            General_Form.Main.BuildScreen.Screen.Invalidate();
        }
    }
}
