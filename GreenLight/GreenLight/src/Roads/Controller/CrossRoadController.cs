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
    public class CrossRoadController : EntityController
    {

        // This class contains methods to build CrossRoads, and immediately open a settingsscreen in which the user can determine which paths a driver can take over the crossroad.
        // Connectionpoints are part of the crossroad, and can be enabled or disabled on the settingscreen, linked, or selected.
        // This class also manages clicking on crossroads.
        // It contains a method to calculate drivinglanes out of links, and crosslanes out of those. 

        public bool TempDraw;

        public PictureBox Screen;

        public Form settingScreen;
        private PictureBox settingScreenImage;

        public List<AbstractRoad> roads = new List<AbstractRoad>();

        public CrossRoad selectedRoad;

        private CurvedButtons selectButton, linkButton, disableButton, errorButton, saveButton, deleteButton;

        private Label error;

        string Button = "Select";


        public CrossRoadController(PictureBox _screen)
        {
            this.Screen = _screen;

            initSettingScreen();
        }

        public AbstractRoad newCrossRoad(Point _point1, int _lanes, string _dir)
        {
            CrossRoad _temp = new CrossRoad(_point1, _point1, _lanes, "Cross", false, false, null, null);
            this.selectedRoad = _temp;

            

            return _temp;
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        private void initSettingScreen()
        {
            Dictionary<string, int> menu = Roads.Config.settingsScreen;

            this.settingScreen = new PopUpForm(new Size(menu["width"], menu["length"] + menu["crossextralength"]));
            this.settingScreen.Hide();

            this.settingScreen.Size = new Size(menu["width"], menu["length"] + menu["crossextralength"]);
            this.settingScreen.BackColor = Color.FromArgb(255, 255, 255);
            this.settingScreen.FormBorderStyle = FormBorderStyle.None;

            this.settingScreenImage = new PictureBox();
            this.settingScreenImage.Paint += SettingBoxDraw;
            this.settingScreenImage.MouseClick += SettingBoxClick;
            this.settingScreenImage.Size = new Size(menu["width"] - 2 * menu["offset"], menu["width"] - 2 * menu["offset"]);
            this.settingScreenImage.Location = new Point(menu["offset"], menu["offset"]);
            this.settingScreenImage.BackColor = Color.Black;

            selectButton = new CurvedButtons(new Size(menu["buttonWidth"], menu["buttonHeight"]), new Point(menu["offset"], menu["width"]), menu["buttonCurve"], "../../src/User Interface Recources/Custom_Small_Button.png", "Select", DrawData.Dosis_font_family, this.settingScreen, this.settingScreen.BackColor);
            selectButton.Click += (object o, EventArgs ea) => { this.Button = "Select";  };

            linkButton = new CurvedButtons(new Size(menu["buttonWidth"], menu["buttonHeight"]), new Point(menu["offset"] + menu["buttonWidth"] + menu["betweenButtons"], menu["width"]), menu["buttonCurve"], "../../src/User Interface Recources/Custom_Small_Button.png", "Link", DrawData.Dosis_font_family, this.settingScreen, this.settingScreen.BackColor);
            linkButton.Click += (object o, EventArgs ea) => 
            {
                if(selectedRoad.selectedPoint == null)
                {
                    this.Button = "Select";
                }
                this.Button = "Link";
            };

            disableButton = new CurvedButtons(new Size(menu["buttonWidth"]+10, menu["buttonHeight"]), new Point(menu["offset"] + 2 * menu["buttonWidth"] + 2 * menu["betweenButtons"], menu["width"]), menu["buttonCurve"], "../../src/User Interface Recources/Custom_Small_Button.png", "Disable", DrawData.Dosis_font_family, this.settingScreen, this.settingScreen.BackColor);
            disableButton.Click += (object o, EventArgs ea) => 
            {
                this.Button = "Disable";
                this.selectedRoad.SwitchSelectedPoint(null);
            };

            saveButton = new CurvedButtons(new Size(menu["buttonWidth"], menu["buttonHeight"]), new Point(menu["offset"], menu["width"] + menu["buttonHeight"] + menu["betweenButtons"]), menu["buttonCurve"], "../../src/User Interface Recources/Custom_Small_Button.png", "Save", DrawData.Dosis_font_family, this.settingScreen, this.settingScreen.BackColor);
            saveButton.Click += (object o, EventArgs ea) => {
                selectedRoad.hitbox.Visible = true;
                if (selectedRoad.connectLinks.Count() != 0) CreateDrivingLanes(); else DeleteCrossroad(this.selectedRoad); this.settingScreenImage.Invalidate(); };

            deleteButton = new CurvedButtons(new Size(menu["buttonWidth"]+10, menu["buttonHeight"]), new Point(menu["offset"] + menu["buttonWidth"] + menu["betweenButtons"], menu["width"] + menu["buttonHeight"] + menu["betweenButtons"]), menu["buttonCurve"], "../../src/User Interface Recources/Custom_Small_Button.png", "Delete", DrawData.Dosis_font_family, this.settingScreen, this.settingScreen.BackColor);
            deleteButton.Click += (object o, EventArgs ea) => { selectedRoad.hitbox.Visible = true; DeleteCrossroad(this.selectedRoad); };

            error = new Label();
            error.Text = "";
            error.ForeColor = Color.Red;
            error.Location = new Point(menu["offset"], menu["width"] - 2 * menu["offset"] + 2 * menu["buttonHeight"] + 2 * menu["betweenButtons"]);
            error.Size = new Size(menu["errorwidth"], menu["errorheight"]);
            error.Hide();

            errorButton = new CurvedButtons(new Size(menu["buttonWidth"], menu["buttonHeight"]), new Point(menu["errorwidth"] + menu["offset"], menu["width"] - 2 * menu["offset"] + 2 * menu["buttonHeight"] + 2 * menu["betweenButtons"]), menu["buttonCurve"], "../../src/User Interface Recources/Custom_Small_Button.png", "Oke!", DrawData.Dosis_font_family, this.settingScreen, this.settingScreen.BackColor);
            errorButton.Click += HideError;
            errorButton.Hide();

            MovePanel Move = new MovePanel(settingScreen);
            Move.Size = new Size(400, 100);
            Move.Location = new Point(menu["offset"] + (int)(3.3* menu["buttonWidth"]+10) + menu["betweenButtons"], menu["width"]);

            this.settingScreen.Controls.Add(Move);
            this.settingScreen.Controls.Add(error);
            this.settingScreen.Controls.Add(errorButton);

            this.settingScreen.Controls.Add(selectButton);
            this.settingScreen.Controls.Add(linkButton);
            this.settingScreen.Controls.Add(disableButton);

            this.settingScreen.Controls.Add(saveButton);
            this.settingScreen.Controls.Add(deleteButton);

            this.settingScreen.Controls.Add(this.settingScreenImage);
        }

        // Shows the pop-up menu for CrossRoads
        public void ShowSettingScreen(CrossRoad _road)
        {
            if (General_Form.Main != null)
            {
                General_Form.Main.BuildScreen.builder.roadBuilder.roadType = "Cross";
                TempDraw = General_Form.Main.BuildScreen.Toggle;
                General_Form.Main.BuildScreen.Toggle = true;
                _road.hitbox.Visible = false;
            }            

            this.selectedRoad = _road;
            this.settingScreen.ShowDialog();
            this.settingScreen.BringToFront();
            this.settingScreenImage.Invalidate();
        }
          
        // Handles mouseclicks in the pop-up menu
        private void SettingBoxClick(object o, MouseEventArgs mea)
        {
            ConnectionPoint _conpoint = this.selectedRoad.connectPoints.Find(x => x.Hitbox.Contains(mea.Location));

            if(_conpoint == null)
            {
                return;
            }

            if (this.Button == "Disable")
            {
                if (_conpoint.Active)
                {
                    _conpoint.setActive(false);
                    _conpoint.Hitbox.color = Color.Red;

                    this.selectedRoad.connectLinks.RemoveAll(x => x.begin == _conpoint || x.end == _conpoint);
                }
                else
                {
                    _conpoint.setActive(true);
                    _conpoint.Hitbox.color = Color.Green;
                }
            }
            else if (this.Button == "Select") 
            {
                selectedRoad.SwitchSelectedPoint(_conpoint);
            }
            else if (this.Button == "Link") 
            {
                MakeLink(selectedRoad.selectedPoint, _conpoint);
            }

            this.settingScreenImage.Invalidate();
        }

        // Draws the image inside the pop-up menu
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

            this.selectedRoad.connectPoints.ForEach(x => x.Draw(g));



            foreach(ConnectionLink _link in this.selectedRoad.connectLinks)
            {
                if(selectedRoad.selectedPoint == null)
                {
                    g.DrawLine(Pens.Orange, _link.begin.Location, _link.end.Location);
                }
                else
                {
                    if(_link.begin == selectedRoad.selectedPoint)
                    {
                        g.DrawLine(Pens.Orange, _link.begin.Location, _link.end.Location);
                    }
                }
            }
        }

        // Creates a ConnectionLink between two ConnectionPoints
        private void MakeLink(ConnectionPoint _begin, ConnectionPoint _end)
        {
            List<ConnectionLink> _links = selectedRoad.connectLinks;

            if (_begin.Side == _end.Side)
            {
                DisplayError("Road Cannot connect to the same side!");
                return;
            }

            if(_links.Any(x => x.begin == _end))
            {
                DisplayError("There is already a link in the opposite direction!");
                return;
            }

            if(_links.Any(x => x.begin == _begin && x.end == _end))
            {
                _links.RemoveAll(x => x.begin == _begin && x.end == _end);
                return;
            }

            if (_begin.end)
            {
                DisplayError("This Roadpoint is marked as an end");
                return;
            }

            _links.Add(new ConnectionLink(_begin, _end));
        }

        private void DisplayError(string _text)
        {
            error.Text = _text;
            error.Show();
            errorButton.Show();

            this.settingScreen.Invalidate();
        }

        private void HideError(object o, EventArgs ea)
        {
            error.Text = "";
            error.Hide();
            errorButton.Hide();

            this.settingScreen.Invalidate();
        }

        // Deletes a CrossRoad from the screen
        public void DeleteCrossroad(AbstractRoad _deletedroad)
        {
            foreach(List<CrossArrow> _list in General_Form.Main.BuildScreen.builder.roadBuilder.AllCrossArrows)
            {
                if(_list[0].crossroad == _deletedroad)
                {
                    General_Form.Main.BuildScreen.builder.roadBuilder.AllCrossArrows.Remove(_list);
                    break;
                }
            }
            
            if (General_Form.Main != null)
            {
                General_Form.Main.BuildScreen.builder.roadBuilder.DeleteRoad(_deletedroad);
            }

            if (_deletedroad == this.selectedRoad)
            {
                this.selectedRoad = null;
                DisableSettingScreen();
            }
        }

        private void DisableSettingScreen()
        {
            if (General_Form.Main != null)
            {
                General_Form.Main.BuildScreen.builder.roadBuilder.roadType = "Cross";
                General_Form.Main.BuildScreen.Toggle = TempDraw;
                if(selectedRoad !=  null)
                {
                    selectedRoad.hitbox.Visible = true;
                }
            }

            this.settingScreen.Hide();

            this.Screen.Invalidate();
        }

        // Creates the CrossLanes based on the ConnectionLinks
        public void CreateDrivingLanes()
        {
            List<LanePoints> _temp = null;
            selectedRoad.Drivinglanes.Clear();
            Point _end, _begin;
            foreach (ConnectionPoint _point in selectedRoad.connectPoints)
            {
                _point.setActive(false);
            }

            foreach (ConnectionLink _link in selectedRoad.connectLinks)
            {
                _link.end.setActive(true);
                _link.begin.setActive(true);
            }
            
            Point p = new Point(-1000, -1000);
            Point _firstTop = p, _firstRight = p, _firstLeft = p, _firstBottom = p;
            Point _tempTop = p, _tempRight = p, _tempLeft = p, _tempBottom = p;
            int _top = 0, _right = 0, _left = 0, _bottom = 0;
            
            foreach (ConnectionPoint _point in selectedRoad.connectPoints)
            {
                if (_point.Active)
                {
                    switch (_point.Side)
                    {
                        case ("Top"):
                            if (_tempTop == _point.Location)
                                break;
                            else
                            {
                                _top++;
                                _tempTop = _point.Location;
                                if (_firstTop == p)
                                    _firstTop = _point.Location;
                            }
                            break;
                        case ("Right"):
                            if (_tempRight == _point.Location)
                                break;
                            else
                            {
                                _right++;
                                _tempRight = _point.Location;
                                if (_firstRight == p)
                                    _firstRight = _point.Location;
                            }
                            break;
                        case ("Left"):
                            if (_tempLeft == _point.Location)
                                break;
                            else
                            {
                                _left++;
                                _tempLeft = _point.Location;
                                if (_firstLeft == p)
                                    _firstLeft = _point.Location;
                            }
                            break;
                        case ("Bottom"):
                            if (_tempBottom == _point.Location)
                                break;
                            else
                            {
                                _tempBottom = _point.Location;
                                _bottom++;
                                if (_firstBottom == p)
                                    _firstBottom = _point.Location;
                            }
                            break;
                    }
                }
                
            }

            int _index = 0;

            foreach (ConnectionLink _link in selectedRoad.connectLinks)
            {
                _end = _link.end.Location;
                _begin = _link.begin.Location;

                _index++;
                
                TranslatePoints(ref _begin, ref _end, selectedRoad);

                ConnectionPoint _cpoint;
                Point _point;
                
                for (int t = 0; t < 2; t++)
                {
                    if (t == 0)
                    {
                        _point = _begin;
                        _cpoint = _link.begin;
                    }

                    else
                    { 
                        _point = _end;
                        _cpoint = _link.end;
                    }

                    ConnectionPoint cp = new ConnectionPoint(_point, _cpoint.Side, 0, _cpoint.Place);
                    bool _exists = false;
                    foreach(ConnectionPoint c in selectedRoad.translatedconnectPoints)
                    {
                        if (c.Side == _cpoint.Side && c.Place == _cpoint.Place)
                            _exists = true;
                    }
                    if (!_exists)
                    {
                        selectedRoad.translatedconnectPoints.Add(cp);
                    }
                }


                if (((_link.end.Side == "Top" || _link.end.Side == "Bottom") && (_link.begin.Side == "Top" || _link.begin.Side == "Bottom"))
                    || ((_link.end.Side == "Left" || _link.end.Side == "Right") && (_link.begin.Side == "Left" || _link.begin.Side == "Right")))
                {
                    _temp = LanePoints.CalculateDiagonalLane(_begin, _end);
                }
                else
                {   
                    if (_link.begin.Side == "Left" && _link.end.Side == "Top")
                    {
                        _temp = LanePoints.CalculateCurveLane(_end, _begin, "NW");
                        _temp.Reverse();
                        _temp.ForEach(x => x.Flip());

                    }
                    else if (_link.begin.Side == "Top" && _link.end.Side == "Left")
                    {
                        _temp = LanePoints.CalculateCurveLane(_begin, _end, "NW");

                    }
                    else if (_link.begin.Side == "Right" && _link.end.Side == "Bottom")
                    {
                        _temp = LanePoints.CalculateCurveLane(_end, _begin, "SE");
                        _temp.Reverse();
                        _temp.ForEach(x => x.Flip());

                    }
                    else if (_link.begin.Side == "Bottom" && _link.end.Side == "Right")
                    {
                        _temp = LanePoints.CalculateCurveLane(_begin, _end, "SE");

                    }
                    else if (_link.begin.Side == "Bottom" && _link.end.Side == "Left")
                    {
                        _temp = LanePoints.CalculateCurveLane(_end, _begin, "SW");
                        _temp.Reverse();
                        _temp.ForEach(x => x.Flip());

                    }
                    else if (_link.begin.Side == "Left" && _link.end.Side == "Bottom")
                    {
                        _temp = LanePoints.CalculateCurveLane(_begin, _end, "SW");

                    }
                    else if (_link.begin.Side == "Top" && _link.end.Side == "Right")
                    {
                        _temp = LanePoints.CalculateCurveLane(_end, _begin, "NE");
                        _temp.Reverse();
                        _temp.ForEach(x => x.Flip());

                    }
                    else if (_link.begin.Side == "Right" && _link.end.Side == "Top")
                    {
                        _temp = LanePoints.CalculateCurveLane(_begin, _end, "NE");

                    }
                }

                if (_temp != null)
                {   
                this.selectedRoad.Drivinglanes.Add(new CrossLane(_temp, _link, _index));  
                }
            }

            this.selectedRoad.CreateArrowImages();

            foreach(ConnectionPoint _point in this.selectedRoad.connectPoints)
            {
                Point _transLocation = _point.Location;
                Point _trash = new Point();
                this.TranslatePoints(ref _transLocation,ref _trash, this.selectedRoad);
                _point.transLocation = _transLocation;
            }

            selectedRoad.Top = _top;
            selectedRoad.Right = _right;
            selectedRoad.Left = _left;
            selectedRoad.Bottom = _bottom;

            this.Screen.Invalidate();
            this.DisableSettingScreen();
            General_Form.Main.BuildScreen.builder.roadBuilder.Connection(selectedRoad.point1, selectedRoad.point1, selectedRoad.lanes, selectedRoad.Dir, selectedRoad, selectedRoad.beginconnection, selectedRoad.endconnection);
        }

        // Translates the ConnectionPoints to their actual on-screen location 
        private void TranslatePoints(ref Point _begin, ref Point _end, CrossRoad _road)
        {
            double _lanes = 0.5 * Roads.Config.crossroadExtra;

            double _beginX = _begin.X / _road.Scale + _road.hitbox.Topcord.X - _lanes;
            double _beginY = _begin.Y / _road.Scale + _road.hitbox.Topcord.Y - _lanes;

            double _endX = _end.X / _road.Scale + _road.hitbox.Topcord.X - _lanes;
            double _endY = _end.Y / _road.Scale + _road.hitbox.Topcord.Y - _lanes;

            _begin = new Point((int)Math.Ceiling(_beginX), (int)Math.Ceiling(_beginY));
            _end = new Point((int)Math.Ceiling(_endX), (int)Math.Ceiling(_endY));
        }

        
    }
}
