﻿using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System;

namespace GreenLight
{
    // This is the Connection class, which makes it possible to connect roads that end close to one another. (one gridpoint)
    // For now it can only connect two diagonal roads, but since there are a lot of ways you can draw two diagonal roads, 
    // we needed a lot of different ways to connect them, hence the 2000 lines of if-statements.
    // There also are quite a lot of "//Console.WriteLine" this makes it easier for us to track down bugs or glitches.   

    class Connection
    {
        private int _shift = 20;

        // The constructor method sees what kind of roads have to be connected and calls another method accordingly. 
        public Connection(Point _point1, Point _point2, int _lanes, string _dir, string _dir2, AbstractRoad _roadOne, AbstractRoad _roadTwo, int _count)
        {
            
            //Console.WriteLine("-- Connection --");

            Point _temp1 = _roadOne.getPoint1();
            Point _temp2 = _roadOne.getPoint2();
            Point _temp3 = _roadTwo.getPoint1();
            Point _temp4 = _roadTwo.getPoint2();

            if (_lanes > 3)
            {
                _shift = 60;
            }
            else if(_lanes > 1)
            {
                _shift = 40;
            }

            //Console.WriteLine("RoadOne " + _roadOne.Type + " ------- RoadTwo " + _roadTwo.Type);

            if (_roadOne.Type == "Diagonal" && _roadTwo.Type == "Diagonal")
            {
                // Here we check wether te connecting part of the road is horizontal or vertical,
                // so not wether the road itself is horizontal or vertical but just the end of the road.
                // This makes it a little bit more clear and easier. 

                char _roadOneEnds;
                char _roadTwoEnds;
                if ((_roadOne.slp > -1 && _roadOne.slp < 1 && _roadOne.slp != 0) || (_roadOne.slp == 0 && _temp1.Y == _temp2.Y))
                    _roadOneEnds = 'v';
                else
                    _roadOneEnds = 'h';
                if ((_roadTwo.slp > -1 && _roadTwo.slp < 1 && _roadTwo.slp != 0) || (_roadTwo.slp == 0 && _temp3.Y == _temp4.Y))
                    _roadTwoEnds = 'v';
                else
                    _roadTwoEnds = 'h';

                //Console.WriteLine(_roadOneEnds + "--------" + _roadTwoEnds);
                if (_roadOne.slp == _roadTwo.slp)
                {
                    StraightandStraight(_point1, _point2, _lanes, _temp1, _temp2, _temp3, _temp4, _roadOne, _roadTwo, _roadOneEnds, _roadTwoEnds);
                }
                else //(_roadOne.slp != _roadTwo.slp)
                {

                    if ((_roadOne.slp == 0 && _roadTwo.slp != 0) || (_roadOne.slp != 0 && _roadTwo.slp == 0))
                    {
                        StraightandDiagonal(_point1, _point2, _lanes, _temp1, _temp2, _temp3, _temp4, _roadOne, _roadTwo, _roadOneEnds, _roadTwoEnds);
                    }
                    else //(_roadOne.slp !=0 && _roadTwo.slp !=0)
                    {
                        DiagonalandDiagonal(_point1, _point2, _lanes, _temp1, _temp2, _temp3, _temp4, _roadOne, _roadTwo, _roadOneEnds, _roadTwoEnds);
                    }
                }

            }

            else if ((_roadOne.Type == "Curved" || _roadOne.Type == "Curved2" ) && (_roadTwo.Type == "Curved" || _roadTwo.Type == "Curved2" ))
            {
                char _roadOneEnds;
                char _roadTwoEnds;

                if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                {
                    _roadOneEnds = 'h';
                }
                else
                {
                    _roadOneEnds = 'v';
                }
                if (Math.Abs(_temp3.X - _point2.X) < Math.Abs(_temp4.X - _point2.X))
                {
                    _roadTwoEnds = 'h';
                }
                else
                {
                    _roadTwoEnds = 'v';
                }

                //Console.WriteLine("RoadOne " +  _dir + " -------  RoadTwo " + _dir2);
                //Console.WriteLine("RoadOne " + _roadOneEnds.ToString() + " -------  RoadTwo " + _roadTwoEnds.ToString());

                CurvedandCurved(_point1, _point2, _lanes, _temp1, _temp2, _temp3, _temp4, _roadOne, _roadTwo, _roadOneEnds, _roadTwoEnds, _dir, _dir2);
            }
            else if ((_roadOne.Type == "Curved" || _roadOne.Type == "Curved2" || _roadOne.Type == "Diagonal") && (_roadTwo.Type == "Curved" || _roadTwo.Type == "Curved2" || _roadTwo.Type == "Diagonal"))
            {
                char _roadOneEnds;
                char _roadTwoEnds;

                if (_roadOne.Type == "Diagonal" && (_roadTwo.Type == "Curved" || _roadTwo.Type == "Curved2"))
                {
                    if ((_roadOne.slp > -1 && _roadOne.slp < 1 && _roadOne.slp != 0) || (_roadOne.slp == 0 && _temp1.Y == _temp2.Y))
                        _roadOneEnds = 'v';
                    else
                        _roadOneEnds = 'h';

                    if (Math.Abs(_temp3.X - _point2.X) < Math.Abs(_temp4.X - _point2.X))
                        _roadTwoEnds = 'h';
                    else
                        _roadTwoEnds = 'v';
                }
                else
                {
                    if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                        _roadOneEnds = 'h';
                    else
                        _roadOneEnds = 'v';

                    if ((_roadTwo.slp > -1 && _roadTwo.slp < 1 && _roadTwo.slp != 0) || (_roadTwo.slp == 0 && _temp3.Y == _temp4.Y))
                        _roadTwoEnds = 'v';
                    else
                        _roadTwoEnds = 'h';
                }

                DiagonalandCurved(_point1, _point2, _lanes, _temp1, _temp2, _temp3, _temp4, _roadOne, _roadTwo, _roadOneEnds, _roadTwoEnds, _dir, _dir2);
            }
        }

        // This method connects two straight roads, either both horizontal, both vertical or one of each.
        // It also takes care of connections between two diagonal roads with the same slope (that is not 0).
        public void StraightandStraight(Point _point1, Point _point2, int _lanes, Point _temp1, Point _temp2, Point _temp3, Point _temp4, AbstractRoad _roadOne, AbstractRoad _roadTwo, char _roadOneEnds, char _roadTwoEnds)
        {
            RoadController _controller = General_Form.Main.BuildScreen.builder.roadBuilder;
            _controller.roads.Remove(_roadOne);
            _controller.roads.Remove(_roadTwo);

            string _curvedType = "Curved";
            if (_roadOne.slp == _roadTwo.slp && _roadOne.slp == 0)
            {
                // If both roads are Horizontal or vertical it will create one new road.
                if (_roadOneEnds == 'v' && _roadTwoEnds == 'v')
                {
                    _controller.BuildDiagonalRoad(new Point(Math.Min(Math.Min(_temp1.X, _temp2.X), Math.Min(_temp3.X, _temp4.X)), _temp1.Y), new Point(Math.Max(Math.Max(_temp1.X, _temp2.X), Math.Max(_temp3.X, _temp4.X)), _temp1.Y), _lanes, _roadTwo.beginconnection, _roadTwo.endconnection, _roadTwo.beginConnectedTo, _roadTwo.endConnectedTo);
                }

                else if (_roadOneEnds == 'h' && _roadTwoEnds == 'h')
                {
                    _controller.BuildDiagonalRoad(new Point(_temp1.X, Math.Min(Math.Min(_temp1.Y, _temp2.Y), Math.Min(_temp3.Y, _temp4.Y))), new Point(_temp1.X, Math.Max(Math.Max(_temp1.Y, _temp2.Y), Math.Max(_temp3.Y, _temp4.Y))), _lanes, _roadTwo.beginconnection, _roadTwo.endconnection, _roadTwo.beginConnectedTo, _roadTwo.endConnectedTo);
                }

                // If one is horizontal and the other one is vertical it will modify the old roads and create a curved road to connect them.
                else if (_roadOneEnds == 'h' && _roadTwoEnds == 'v')
                {
                    Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);
                    if (Math.Abs(_temp1.Y - _point1.Y) < Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            _curvedType = "Curved";
                            _curvedstart = new Point(_temp1.X, _temp1.Y + _shift);
                            _controller.BuildDiagonalRoad(new Point(_temp1.X, _temp1.Y + _shift - 2), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            _curvedType = "Curved2";
                            _curvedstart = new Point(_temp1.X, _temp1.Y - _shift);
                            _controller.BuildDiagonalRoad(new Point(_temp1.X, _temp1.Y - (_shift - 2)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp1.Y - _point1.Y) > Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            _curvedType = "Curved2";
                            _curvedstart = new Point(_temp2.X, _temp2.Y - _shift);
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X, _temp2.Y - (_shift - 2)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedType = "Curved";
                            _curvedstart = new Point(_temp2.X, _temp2.Y + _shift);
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X, _temp2.Y + _shift - 2), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }

                    if (Math.Abs(_temp3.X - _point2.X) < Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            _curvedend = new Point(_temp3.X + _shift, _temp3.Y);
                            _controller.BuildDiagonalRoad(new Point(_temp3.X + _shift - 2, _temp3.Y), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            _curvedend = new Point(_temp3.X - _shift, _temp3.Y);
                            _controller.BuildDiagonalRoad(new Point(_temp3.X - (_shift - 2), _temp3.Y), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp3.X - _point2.X) > Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp4.X < _temp3.X)
                        {
                            _curvedend = new Point(_temp4.X + _shift, _temp4.Y);
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X + _shift - 2, _temp4.Y), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, _roadOne);
                        }
                        else
                        {
                            _curvedend = new Point(_temp4.X - _shift, _temp4.Y);
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X - (_shift - 2), _temp4.Y), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, _roadOne);
                        }
                    }
                    _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curvedType, true, true, _controller.roads[_controller.roads.Count - 2], _controller.roads[_controller.roads.Count - 1]);
                }

                else if (_roadOneEnds == 'v' && _roadTwoEnds == 'h')
                {
                    Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);
                    if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            _curvedend = new Point(_temp1.X + _shift, _temp1.Y);
                            _controller.BuildDiagonalRoad(new Point(_temp1.X + _shift - 2, _temp1.Y), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            _curvedend = new Point(_temp1.X - _shift, _temp1.Y);
                            _controller.BuildDiagonalRoad(new Point(_temp1.X - (_shift - 2), _temp1.Y), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp1.X - _point1.X) > Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            _curvedend = new Point(_temp2.X - _shift, _temp2.Y);
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X - _shift + 2, _temp2.Y), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedend = new Point(_temp2.X + _shift, _temp2.Y);
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X + (_shift - 2), _temp2.Y), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }

                    if (Math.Abs(_temp3.Y - _point2.Y) < Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.Y < _temp4.Y)
                        {
                            _curvedType = "Curved";
                            _curvedstart = new Point(_temp3.X, _temp3.Y + _shift);
                            _controller.BuildDiagonalRoad(new Point(_temp3.X, _temp3.Y + _shift - 2), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            _curvedType = "Curved2";
                            _curvedstart = new Point(_temp3.X, _temp3.Y - _shift);
                            _controller.BuildDiagonalRoad(new Point(_temp3.X, _temp3.Y - (_shift - 2)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp3.Y - _point2.Y) > Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp4.Y < _temp3.Y)
                        {
                            _curvedType = "Curved";
                            _curvedstart = new Point(_temp4.X, _temp4.Y + _shift);
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X, _temp4.Y + _shift - 2), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedType = "Curved2";
                            _curvedstart = new Point(_temp4.X, _temp4.Y - _shift);
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X, _temp4.Y - (_shift - 2)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }

                    _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curvedType, true, true, _controller.roads[_controller.roads.Count - 1], _controller.roads[_controller.roads.Count - 2]);
                }
                //Console.WriteLine(_controller.roads.Count);

            }

            // Two non horizontal or vertical roads with the same slope.

            else if (_roadOne.slp == _roadTwo.slp)
            {
                ////Console.WriteLine(_controller.roads.Count);
                if (_roadOne.slp > 0)
                {
                    _controller.BuildDiagonalRoad(new Point(Math.Min(Math.Min(_temp1.X, _temp2.X), Math.Min(_temp3.X, _temp4.X)), Math.Min(Math.Min(_temp1.Y, _temp2.Y), Math.Min(_temp3.Y, _temp4.Y))),
                        new Point(Math.Max(Math.Max(_temp1.X, _temp2.X), Math.Max(_temp3.X, _temp4.X)), Math.Max(Math.Max(_temp1.Y, _temp2.Y), Math.Max(_temp3.Y, _temp4.Y))), _lanes, _roadTwo.beginconnection, _roadTwo.endconnection, _roadTwo.beginConnectedTo, _roadTwo.endConnectedTo);
                }
                else
                {
                    _controller.BuildDiagonalRoad(new Point(Math.Min(Math.Min(_temp1.X, _temp2.X), Math.Min(_temp3.X, _temp4.X)), Math.Max(Math.Max(_temp1.Y, _temp2.Y), Math.Max(_temp3.Y, _temp4.Y))),
                        new Point(Math.Max(Math.Max(_temp1.X, _temp2.X), Math.Max(_temp3.X, _temp4.X)), Math.Min(Math.Min(_temp1.Y, _temp2.Y), Math.Min(_temp3.Y, _temp4.Y))), _lanes, _roadTwo.beginconnection, _roadTwo.endconnection, _roadTwo.beginConnectedTo, _roadTwo.endConnectedTo);
                }
            }
        }

        // This method connects a straight road with a diagonal, so a horizontal or vertical road and a road with a slope.
        public void StraightandDiagonal(Point _point1, Point _point2, int _lanes, Point _temp1, Point _temp2, Point _temp3, Point _temp4, AbstractRoad _roadOne, AbstractRoad _roadTwo, char _roadOneEnds, char _roadTwoEnds)
        {
            int _direction;
            bool _beginconnection, _endconnection;
            AbstractRoad _beginConnectedTo, _endConnectedTo;
            RoadController _controller = General_Form.Main.BuildScreen.builder.roadBuilder;
            string _curvedType = "Curved";

            // If the roads end the same, we can simply just enlarge the horizontal or vertical road. 

            if (_roadOneEnds == 'v' && _roadTwoEnds == 'v')
            {
                int _distance = Math.Abs(_point1.X - _point2.X) + 1;
                //Console.WriteLine(_distance);

                if (_roadOne.slp == 0 && Math.Sqrt(Math.Pow(_point1.X - _temp1.X, 2) + Math.Pow(_point1.Y - _temp1.Y, 2)) < Math.Sqrt(Math.Pow(_point1.X - _temp2.X, 2) + Math.Pow(_point1.Y - _temp2.Y, 2)))
                {
                    if (_temp1.X < _temp2.X)
                    {
                        _direction = -1;
                        _beginconnection = true;
                        _endconnection = _roadOne.endconnection;
                    }

                    else
                    {
                        _direction = 1;
                        _beginconnection = true;
                        _endconnection = _roadOne.endconnection;
                    }

                    _beginConnectedTo = _roadTwo;
                    _endConnectedTo = _roadOne.endConnectedTo;
                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                    _controller.roads.Remove(_roadOne);
                    _controller.BuildDiagonalRoad(new Point(_temp1.X + _distance * _direction, _temp1.Y), _temp2, _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);

                }
                else if (_roadOne.slp == 0 && Math.Sqrt(Math.Pow(_point1.X - _temp1.X, 2) + Math.Pow(_point1.Y - _temp1.Y, 2)) > Math.Sqrt(Math.Pow(_point1.X - _temp2.X, 2) + Math.Pow(_point1.Y - _temp2.Y, 2)))
                {
                    if (_temp2.X < _temp1.X)
                    {
                        _direction = -1;
                        _beginconnection = _roadOne.beginconnection; ;
                        _endconnection = true;
                    }
                    else
                    {
                        _direction = 1;
                        _beginconnection = _roadOne.beginconnection;
                        _endconnection = true;
                    }

                    _beginConnectedTo = _roadOne.beginConnectedTo;
                    _endConnectedTo = _roadTwo;
                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                    _controller.roads.Remove(_roadOne);
                    _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X + _distance * _direction, _temp2.Y), _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);

                }
                else if (_roadTwo.slp == 0 && Math.Sqrt(Math.Pow(_point2.X - _temp3.X, 2) + Math.Pow(_point2.Y - _temp3.Y, 2)) < Math.Sqrt(Math.Pow(_point2.X - _temp4.X, 2) + Math.Pow(_point2.Y - _temp4.Y, 2)))
                {
                    if (_temp3.X < _temp4.X)
                    {
                        _direction = -1;
                        _beginconnection = true;
                        _endconnection = _roadTwo.endconnection;
                    }
                    else
                    {
                        _direction = 1;
                        _beginconnection = true;
                        _endconnection = _roadTwo.endconnection;
                    }

                    _beginConnectedTo = _roadOne;
                    _endConnectedTo = _roadTwo.endConnectedTo;
                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                    _controller.roads.Remove(_roadTwo);
                    _controller.BuildDiagonalRoad(new Point(_temp3.X + _distance * _direction, _temp3.Y), _temp4, _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                }
                else
                {
                    if (_temp4.X < _temp3.X)
                    {
                        _direction = -1;
                        _beginconnection = _roadTwo.beginconnection;
                        _endconnection = true;
                    }
                    else
                    {
                        _direction = 1;
                        _beginconnection = _roadTwo.beginconnection;
                        _endconnection = true;
                    }

                    _beginConnectedTo = _roadTwo.beginConnectedTo;
                    _endConnectedTo = _roadOne;
                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                    _controller.roads.Remove(_roadTwo);
                    _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X + _distance * _direction, _temp4.Y), _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                }
            }

            else if (_roadOneEnds == 'h' && _roadTwoEnds == 'h')
            {
                int _distance = Math.Abs(_point1.Y - _point2.Y) + 1;
                //Console.WriteLine(_distance);

                if (_roadOne.slp == 0 && Math.Sqrt(Math.Pow(_point1.X - _temp1.X, 2) + Math.Pow(_point1.Y - _temp1.Y, 2)) < Math.Sqrt(Math.Pow(_point1.X - _temp2.X, 2) + Math.Pow(_point1.Y - _temp2.Y, 2)))
                {
                    if (_temp1.Y < _temp2.Y)
                    {
                        _direction = -1;
                        _beginconnection = true;
                        _endconnection = _roadOne.endconnection;
                    }

                    else
                    {
                        _direction = 1;
                        _beginconnection = true;
                        _endconnection = _roadOne.endconnection;
                    }

                    _beginConnectedTo = _roadTwo;
                    _endConnectedTo = _roadOne.endConnectedTo;
                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                    _controller.roads.Remove(_roadOne);
                    _controller.BuildDiagonalRoad(new Point(_temp1.X, _temp1.Y + _distance * _direction), _temp2, _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                }
                else if (_roadOne.slp == 0 && Math.Sqrt(Math.Pow(_point1.X - _temp1.X, 2) + Math.Pow(_point1.Y - _temp1.Y, 2)) > Math.Sqrt(Math.Pow(_point1.X - _temp2.X, 2) + Math.Pow(_point1.Y - _temp2.Y, 2)))
                {
                    if (_temp2.Y < _temp1.Y)
                    {
                        _direction = -1;
                        _beginconnection = _roadOne.beginconnection;
                        _endconnection = true;
                    }
                    else
                    {
                        _direction = 1;
                        _beginconnection = _roadOne.beginconnection;
                        _endconnection = true;
                    }

                    _beginConnectedTo = _roadOne.beginConnectedTo;
                    _endConnectedTo = _roadTwo;
                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                    _controller.roads.Remove(_roadOne);
                    _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X, _temp2.Y + _distance * _direction), _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                }
                else if (_roadTwo.slp == 0 && Math.Sqrt(Math.Pow(_point2.X - _temp3.X, 2) + Math.Pow(_point2.Y - _temp3.Y, 2)) < Math.Sqrt(Math.Pow(_point2.X - _temp4.X, 2) + Math.Pow(_point2.Y - _temp4.Y, 2)))
                {
                    if (_temp3.Y < _temp4.Y)
                    {
                        _direction = -1;
                        _beginconnection = true;
                        _endconnection = _roadTwo.endconnection;
                    }
                    else
                    {
                        _direction = 1;
                        _beginconnection = true;
                        _endconnection = _roadTwo.endconnection;
                    }

                    _beginConnectedTo = _roadOne;
                    _endConnectedTo = _roadTwo.endConnectedTo;
                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                    _controller.roads.Remove(_roadTwo);
                    _controller.BuildDiagonalRoad(new Point(_temp3.X, _temp3.Y + _distance * _direction), _temp4, _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                }
                else
                {
                    if (_temp4.Y < _temp3.Y)
                    {
                        _direction = -1;
                        _beginconnection = _roadTwo.beginconnection;
                        _endconnection = true;
                    }
                    else
                    {
                        _direction = 1;
                        _beginconnection = _roadTwo.beginconnection;
                        _endconnection = true;
                    }

                    _beginConnectedTo = _roadTwo.beginConnectedTo;
                    _endConnectedTo = _roadOne;
                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                    _controller.roads.Remove(_roadTwo);
                    _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X, _temp4.Y + _distance * _direction), _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                }
            }

            // If they don't have the same ending then we once again have to draw a curved road to connect them, we also have to adjust the original roads a little bit.

            else if (_roadOneEnds == 'h' && _roadTwoEnds == 'v')
            {
                _controller.roads.Remove(_roadOne);
                _controller.roads.Remove(_roadTwo);
                Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);

                if (_roadOne.slp == 0)
                {
                    if (Math.Abs(_temp1.Y - _point1.Y) < Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            //Console.WriteLine(1.1);
                            _curvedType = "Curved";
                            _curvedstart = new Point(_temp1.X, _temp1.Y + _shift);
                            _controller.BuildDiagonalRoad(new Point(_temp1.X, _temp1.Y + _shift - 2), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.beginConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(1.2);
                            _curvedType = "Curved2";
                            _curvedstart = new Point(_temp1.X, _temp1.Y - _shift);
                            _controller.BuildDiagonalRoad(new Point(_temp1.X, _temp1.Y - (_shift - 2)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.beginConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp1.Y - _point1.Y) > Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            //Console.WriteLine(1.3);
                            _curvedType = "Curved2";
                            _curvedstart = new Point(_temp2.X, _temp2.Y + _shift);
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X, _temp2.Y + _shift - 2), _lanes, _roadOne.beginconnection, true, _roadOne.endConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(1.4);
                            _curvedType = "Curved";
                            _curvedstart = new Point(_temp2.X, _temp2.Y - _shift);
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X, _temp2.Y - (_shift - 2)), _lanes, _roadOne.beginconnection, true, _roadOne.endConnectedTo, null);
                        }
                    }

                    if (Math.Abs(_temp3.Y - _point2.Y) < Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            //Console.WriteLine(1.5);
                            _curvedend = new Point(_temp3.X + _shift, (int)(_temp3.Y + _shift * _roadTwo.slp));
                            _controller.BuildDiagonalRoad(new Point(_temp3.X + (_shift - 1), (int)(_temp3.Y + (_shift - 1) * _roadTwo.slp)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(1.6);
                            _curvedend = new Point(_temp3.X - _shift, (int)(_temp3.Y - _shift * _roadTwo.slp));
                            _controller.BuildDiagonalRoad(new Point(_temp3.X - (_shift - 1), (int)(_temp3.Y - (_shift - 1) * _roadTwo.slp)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp3.Y - _point2.Y) > Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            //Console.WriteLine(1.7);
                            _curvedend = new Point(_temp4.X - _shift, (int)(_temp4.Y - _shift * _roadTwo.slp));
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X - (_shift - 1), (int)(_temp4.Y - (_shift - 1) * _roadTwo.slp)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(1.8);
                            _curvedend = new Point(_temp4.X + _shift, (int)(_temp4.Y + _shift * _roadTwo.slp));
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X + (_shift - 1), (int)(_temp4.Y + (_shift - 1) * _roadTwo.slp)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                }
                
                else // if(_roadTwo.slp == 0)
                {
                    if (Math.Abs(_temp1.Y - _point1.Y) < Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            //Console.WriteLine(2.1);
                            _curvedType = "Curved";
                            _curvedstart = new Point((int)(_temp1.X + _shift / _roadOne.slp), _temp1.Y + _shift);
                            _controller.BuildDiagonalRoad(new Point((int)(_temp1.X + (_shift - 1) / _roadOne.slp), _temp1.Y + (_shift - 1)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(2.2);
                            _curvedType = "Curved2";
                            _curvedstart = new Point((int)(_temp1.X - _shift / _roadOne.slp), _temp1.Y - _shift);
                            _controller.BuildDiagonalRoad(new Point((int)(_temp1.X - (_shift - 1) / _roadOne.slp), _temp1.Y - (_shift - 1)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp1.Y - _point1.Y) > Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            //Console.WriteLine(2.3);
                            _curvedType = "Curved2";
                            _curvedstart = new Point((int)(_temp2.X - _shift / _roadOne.slp), _temp2.Y - _shift);
                            _controller.BuildDiagonalRoad(_temp1, new Point((int)(_temp2.X - (_shift - 1) / _roadOne.slp), _temp2.Y - (_shift - 1)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(2.4);
                            _curvedType = "Curved";
                            _curvedstart = new Point((int)(_temp2.X + _shift / _roadOne.slp), _temp2.Y + _shift);
                            _controller.BuildDiagonalRoad(_temp1, new Point((int)(_temp2.X + (_shift - 1) / _roadOne.slp), _temp2.Y + (_shift - 1)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }

                    if (Math.Abs(_temp3.X - _point2.X) < Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            //Console.WriteLine(2.5);
                            _curvedend = new Point(_temp3.X + _shift, _temp3.Y);
                            _controller.BuildDiagonalRoad(new Point(_temp3.X + (_shift - 2), _temp3.Y), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(2.6);
                            _curvedend = new Point(_temp3.X - _shift, _temp3.Y);
                            _controller.BuildDiagonalRoad(new Point(_temp3.X - (_shift - 2), _temp3.Y), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp3.X - _point2.X) > Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            //Console.WriteLine(2.7);
                            _curvedend = new Point(_temp4.X - _shift, _temp4.Y);
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X - (_shift - 2), _temp4.Y), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(2.8);
                            _curvedend = new Point(_temp4.X + _shift, _temp4.Y);
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X + (_shift - 2), _temp4.Y), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                }
                _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curvedType, true, true, _controller.roads[_controller.roads.Count - 2], _controller.roads[_controller.roads.Count - 1]);
            }

            else if (_roadOneEnds == 'v' && _roadTwoEnds == 'h')
            {
                _controller.roads.Remove(_roadOne);
                _controller.roads.Remove(_roadTwo);
                Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);

                if (_roadTwo.slp == 0)
                {
                    if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            //Console.WriteLine(3.1);
                            _curvedend = new Point(_temp1.X + _shift, (int)(_temp1.Y + _shift * _roadOne.slp));
                            _controller.BuildDiagonalRoad(new Point(_temp1.X + (_shift - 1), (int)(_temp1.Y + (_shift - 1) * _roadOne.slp)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(3.2);
                            _curvedend = new Point(_temp1.X - _shift, (int)(_temp1.Y - _shift * _roadOne.slp));
                            _controller.BuildDiagonalRoad(new Point(_temp1.X - (_shift - 1), (int)(_temp1.Y - (_shift - 1) * _roadOne.slp)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp1.X - _point1.X) > Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            //Console.WriteLine(3.3);
                            _curvedend = new Point(_temp2.X - _shift, (int)(_temp2.Y - _shift * _roadOne.slp));
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X - (_shift - 1), (int)(_temp2.Y - (_shift - 1) * _roadOne.slp)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(3.4);
                            _curvedend = new Point(_temp2.X + _shift, (int)(_temp2.Y + _shift * _roadOne.slp));
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp2.X + (_shift - 1), (int)(_temp2.Y + (_shift - 1) * _roadOne.slp)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }

                    if (Math.Abs(_temp3.Y - _point2.Y) < Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.Y < _temp4.Y)
                        {
                            //Console.WriteLine(3.5);
                            _curvedType = "Curved";
                            _curvedstart = new Point(_temp3.X, _temp3.Y + _shift);
                            _controller.BuildDiagonalRoad(new Point(_temp3.X, _temp3.Y + (_shift - 2)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(3.6);
                            _curvedType = "Curved2";
                            _curvedstart = new Point(_temp3.X, _temp3.Y - _shift);
                            _controller.BuildDiagonalRoad(new Point(_temp3.X, _temp3.Y - (_shift - 2)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp3.Y - _point2.Y) > Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.Y < _temp4.Y)
                        {
                            //Console.WriteLine(3.7);
                            _curvedType = "Curved2";
                            _curvedstart = new Point(_temp4.X, _temp4.Y - _shift);
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X, _temp4.Y - (_shift - 2)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(3.8);
                            _curvedType = "Curved";
                            _curvedstart = new Point(_temp4.X, _temp4.Y + _shift);
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X, _temp4.Y + (_shift - 2)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                }

                else // if(_roadOne.slp == 0)
                {
                    if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            //Console.WriteLine(4.1);
                            _curvedend = new Point(_temp1.X + _shift, _temp1.Y);
                            _controller.BuildDiagonalRoad(new Point(_temp1.X + (_shift - 2), _temp1.Y), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(4.2);
                            _curvedend = new Point(_temp1.X - _shift, _temp1.Y);
                            _controller.BuildDiagonalRoad(new Point(_temp1.X - (_shift - 2), _temp1.Y), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp1.X - _point1.X) > Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            //Console.WriteLine(4.3);
                            _curvedend = new Point(_temp2.X - _shift, _temp2.Y);
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X - (_shift - 2), _temp2.Y), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(4.4);
                            _curvedend = new Point(_temp2.X + _shift, _temp2.Y);
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X + (_shift - 2), _temp2.Y), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }

                    if (Math.Abs(_temp3.Y - _point2.Y) < Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.Y < _temp4.Y)
                        {
                            //Console.WriteLine(4.5);
                            _curvedType = "Curved";
                            _curvedstart = new Point((int)(_temp3.X + _shift / _roadTwo.slp), _temp3.Y + _shift);
                            _controller.BuildDiagonalRoad(new Point((int)(_temp3.X + (_shift - 1) / _roadTwo.slp), _temp3.Y + (_shift - 1)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(4.6);
                            _curvedType = "Curved2";
                            _curvedstart = new Point((int)(_temp3.X - _shift / _roadTwo.slp), _temp3.Y - _shift);
                            _controller.BuildDiagonalRoad(new Point((int)(_temp3.X - (_shift - 1) / _roadTwo.slp), _temp3.Y - (_shift - 1)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp3.Y - _point2.Y) > Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.Y < _temp4.Y)
                        {
                            //Console.WriteLine(4.7);
                            _curvedType = "Curved2";
                            _curvedstart = new Point((int)(_temp4.X - _shift / _roadTwo.slp), _temp4.Y - _shift);
                            _controller.BuildDiagonalRoad(_temp3, new Point((int)(_temp4.X - (_shift - 1) / _roadTwo.slp), _temp4.Y - (_shift - 1)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(4.8);
                            _curvedType = "Curved";
                            _curvedstart = new Point((int)(_temp4.X + _shift / _roadTwo.slp), _temp4.Y + _shift);
                            _controller.BuildDiagonalRoad(_temp3, new Point((int)(_temp4.X + (_shift - 1) / _roadTwo.slp), _temp4.Y + (_shift - 1)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                }
                _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curvedType, true, true, _controller.roads[_controller.roads.Count - 1], _controller.roads[_controller.roads.Count - 2]);
            }
        }

        // This method connects two diagonal roads, so two roads with a slope, but not the same slope.
        public void DiagonalandDiagonal(Point _point1, Point _point2, int _lanes, Point _temp1, Point _temp2, Point _temp3, Point _temp4, AbstractRoad _roadOne, AbstractRoad _roadTwo, char _roadOneEnds, char _roadTwoEnds)
        {
            RoadController _controller = General_Form.Main.BuildScreen.builder.roadBuilder;
            double _distance = Math.Sqrt(Math.Pow(_point1.X - _point2.X, 2) + Math.Pow(_point1.Y - _point2.Y, 2)) + 1;
            int _direction;
            bool _beginconnection, _endconnection;
            AbstractRoad _beginConnectedTo, _endConnectedTo;

            // If the roads end the same, we can simply just enlarge on of the two roads. 

            if (_roadOneEnds == 'v' && _roadTwoEnds == 'v')
            {
                if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                {
                    if (_temp1.X < _temp2.X)
                        _direction = -2;
                    else
                        _direction = 2;

                    _beginconnection = true;
                    _endconnection = _roadOne.endconnection;

                    _beginConnectedTo = _roadTwo;
                    _endConnectedTo = _roadOne.endConnectedTo;

                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                    _controller.roads.Remove(_roadOne);
                    _controller.BuildDiagonalRoad(new Point(_point2.X + _direction, _point2.Y), _temp2, _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                }
                else
                {
                    if (_temp1.X < _temp2.X)
                        _direction = 2;
                    else
                        _direction = -2;

                    _beginconnection = _roadOne.beginconnection;
                    _endconnection = true;

                    _beginConnectedTo = _roadOne.beginConnectedTo;
                    _endConnectedTo = _roadTwo;

                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                    _controller.roads.Remove(_roadOne);
                    _controller.BuildDiagonalRoad(_temp1, new Point(_point2.X + _direction, _point2.Y), _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                }
            }

            else if (_roadOneEnds == 'h' && _roadTwoEnds == 'h')
            {
                if (Math.Abs(_temp1.Y - _point1.Y) < Math.Abs(_temp2.Y - _point1.Y))
                {
                    if (_temp1.Y < _temp2.Y)
                        _direction = -2;
                    else
                        _direction = 2;

                    _beginconnection = true;
                    _endconnection = _roadOne.endconnection;

                    _beginConnectedTo = _roadTwo;
                    _endConnectedTo = _roadOne.endConnectedTo;

                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                    _controller.roads.Remove(_roadOne);
                    _controller.BuildDiagonalRoad(new Point(_point2.X, _point2.Y + _direction), _temp2, _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                }
                else
                {
                    if (_temp1.Y < _temp2.Y)
                        _direction = 2;
                    else
                        _direction = -2;

                    _beginconnection = _roadOne.beginconnection;
                    _endconnection = true;

                    _beginConnectedTo = _roadOne.beginConnectedTo;
                    _endConnectedTo = _roadTwo;

                    //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                    _controller.roads.Remove(_roadOne);
                    _controller.BuildDiagonalRoad(_temp1, new Point(_point2.X, _point2.Y + _direction), _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                }
            }

            // If they don't have the same ending then we once again have to draw a curved road to connect them, we also ahve to adjust the original roads a little bit. 
            

            else if (_roadOneEnds == 'v' && _roadTwoEnds == 'h')
            {
                _controller.roads.Remove(_roadOne);
                _controller.roads.Remove(_roadTwo);
                Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);
                string _curveType = "Curved";

                // _roadOne

                if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                {
                    if (_temp1.X < _temp2.X)
                    {
                        //Console.WriteLine(1.1);
                        if (_point2.Y == Math.Min(_temp3.Y, _temp4.Y))
                        {
                            _curvedend = new Point(_temp1.X + _shift, (int)(_temp1.Y - _shift * Math.Abs(_roadOne.slp)));
                            _controller.BuildDiagonalRoad(new Point(_temp1.X + (_shift - 2), (int)(_temp1.Y - (_shift - 2) * Math.Abs(_roadOne.slp))), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            _curvedend = new Point(_temp1.X + _shift, (int)(_temp1.Y + _shift * Math.Abs(_roadOne.slp)));
                            _controller.BuildDiagonalRoad(new Point(_temp1.X + (_shift - 2), (int)(_temp1.Y + (_shift - 2) * Math.Abs(_roadOne.slp))), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                    else
                    {
                        //Console.WriteLine(1.2);
                        if (_point2.Y == Math.Min(_temp3.Y, _temp4.Y))
                        {
                            _curvedend = new Point(_temp1.X - _shift, (int)(_temp1.Y - _shift * Math.Abs(_roadOne.slp)));
                            _controller.BuildDiagonalRoad(new Point(_temp1.X - (_shift - 2), (int)(_temp1.Y - (_shift - 2) * Math.Abs(_roadOne.slp))), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            _curvedend = new Point(_temp1.X - _shift, (int)(_temp1.Y + _shift * Math.Abs(_roadOne.slp)));
                            _controller.BuildDiagonalRoad(new Point(_temp1.X - (_shift - 2), (int)(_temp1.Y + (_shift - 2) * Math.Abs(_roadOne.slp))), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                }
                else if (Math.Abs(_temp1.X - _point1.X) > Math.Abs(_temp2.X - _point1.X))
                {
                    if (_temp1.X < _temp2.X)
                    {
                        //Console.WriteLine(1.3);
                        if (_point2.Y == Math.Min(_temp3.Y, _temp4.Y))
                        {
                            _curvedend = new Point(_temp2.X - _shift, (int)(_temp2.Y - _shift * Math.Abs(_roadOne.slp)));
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X - (_shift - 2), (int)(_temp2.Y - (_shift - 2) * Math.Abs(_roadOne.slp))), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedend = new Point(_temp2.X - _shift, (int)(_temp2.Y + _shift * Math.Abs(_roadOne.slp)));
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X - (_shift - 2), (int)(_temp2.Y + (_shift - 2) * Math.Abs(_roadOne.slp))), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }
                    else
                    {
                        //Console.WriteLine(1.4);
                        if(_point2.Y == Math.Min(_temp3.Y, _temp4.Y))
                        {
                            _curvedend = new Point(_temp2.X + _shift, (int)(_temp2.Y - _shift * Math.Abs(_roadOne.slp)));
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X + (_shift - 2), (int)(_temp2.Y - (_shift - 2) * Math.Abs(_roadOne.slp))), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedend = new Point(_temp2.X + _shift, (int)(_temp2.Y + _shift * Math.Abs(_roadOne.slp)));
                            _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X + (_shift - 2), (int)(_temp2.Y + (_shift - 2) * Math.Abs(_roadOne.slp))), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }
                }

                // _roadTwo

                if (Math.Abs(_temp3.Y - _point2.Y) < Math.Abs(_temp4.Y - _point2.Y))
                {
                    if (_temp3.Y < _temp4.Y)
                    {
                        //Console.WriteLine(1.5);
                        _curveType = "Curved";
                        if (_point1.X == Math.Min(_temp1.X, _temp2.X))
                        {
                            _curvedstart = new Point((int)(_temp3.X - _shift / Math.Abs(_roadTwo.slp)), _temp3.Y + _shift);
                            _controller.BuildDiagonalRoad(new Point((int)(_temp3.X - (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp3.Y + (_shift - 1)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            _curvedstart = new Point((int)(_temp3.X + _shift / Math.Abs(_roadTwo.slp)), _temp3.Y + _shift);
                            _controller.BuildDiagonalRoad(new Point((int)(_temp3.X + (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp3.Y + (_shift - 1)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else
                    {
                        _curveType = "Curved2";
                        //Console.WriteLine(1.6);
                        if (_point1.X == Math.Min(_temp1.X, _temp2.X))
                        {
                            _curvedstart = new Point((int)(_temp3.X - _shift / Math.Abs(_roadTwo.slp)), _temp3.Y - _shift);
                            _controller.BuildDiagonalRoad(new Point((int)(_temp3.X - (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp3.Y - (_shift - 1)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            _curvedstart = new Point((int)(_temp3.X + _shift / Math.Abs(_roadTwo.slp)), _temp3.Y - _shift);
                            _controller.BuildDiagonalRoad(new Point((int)(_temp3.X + (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp3.Y - (_shift - 1)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                }
                else if (Math.Abs(_temp3.Y - _point2.Y) > Math.Abs(_temp4.Y - _point2.Y))
                {
                    _curveType = "Curved2";
                    if (_temp3.Y < _temp4.Y)
                    {
                        //Console.WriteLine(1.7);
                        if (_point1.X == Math.Min(_temp1.X, _temp2.X))
                        {
                            _curvedstart = new Point((int)(_temp4.X - _shift / Math.Abs(_roadTwo.slp)), _temp4.Y - _shift);
                            _controller.BuildDiagonalRoad(_temp3, new Point((int)(_temp4.X - (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp4.Y - (_shift - 1)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedstart = new Point((int)(_temp4.X + _shift / Math.Abs(_roadTwo.slp)), _temp4.Y - _shift);
                            _controller.BuildDiagonalRoad(_temp3, new Point((int)(_temp4.X + (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp4.Y - (_shift - 1)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                    else
                    {
                        _curveType = "Curved";
                        //Console.WriteLine(1.8);
                        if (_point1.X == Math.Min(_temp1.X, _temp2.X))
                        {
                            _curvedstart = new Point((int)(_temp4.X - _shift / Math.Abs(_roadTwo.slp)), _temp4.Y + _shift);
                            _controller.BuildDiagonalRoad(_temp3, new Point((int)(_temp4.X - (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp4.Y + (_shift - 1)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedstart = new Point((int)(_temp4.X + _shift / Math.Abs(_roadTwo.slp)), _temp4.Y + _shift);
                            _controller.BuildDiagonalRoad(_temp3, new Point((int)(_temp4.X + (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp4.Y + (_shift - 1)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                }
                _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curveType, true, true, _controller.roads[_controller.roads.Count - 1], _controller.roads[_controller.roads.Count - 2]);
            }

            else if (_roadOneEnds == 'h' && _roadTwoEnds == 'v')
            {
                _controller.roads.Remove(_roadOne);
                _controller.roads.Remove(_roadTwo);
                Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);
                string _curveType = "Curved";

                if (Math.Abs(_temp1.Y - _point1.Y) < Math.Abs(_temp2.Y - _point1.Y))
                {
                    // _roadOne

                    if (_temp1.Y < _temp2.Y)
                    {
                        _curveType = "Curved";
                        //Console.WriteLine(2.1);
                        if (_point2.X == Math.Min(_temp3.X, _temp4.X))
                        {
                            _curvedstart = new Point((int)(_temp1.X - _shift / Math.Abs(_roadOne.slp)), (_temp1.Y + _shift));
                            _controller.BuildDiagonalRoad(new Point((int)(_temp1.X - (_shift - 2) / Math.Abs(_roadOne.slp)), _temp1.Y + (_shift - 2)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            _curvedstart = new Point((int)(_temp1.X + _shift / Math.Abs(_roadOne.slp)), (_temp1.Y + _shift));
                            _controller.BuildDiagonalRoad(new Point((int)(_temp1.X + (_shift - 2) / Math.Abs(_roadOne.slp)), _temp1.Y + (_shift - 2)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                    else
                    {
                        //Console.WriteLine(2.2);

                        _curveType = "Curved2";
                        if (_point2.X == Math.Min(_temp3.X, _temp4.X))
                        {
                            _curvedstart = new Point((int)(_temp1.X - _shift / Math.Abs(_roadOne.slp)), (_temp1.Y - _shift));
                            _controller.BuildDiagonalRoad(new Point((int)(_temp1.X - (_shift - 2) / Math.Abs(_roadOne.slp)), _temp1.Y - (_shift - 2)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            _curvedstart = new Point((int)(_temp1.X + _shift / Math.Abs(_roadOne.slp)), (_temp1.Y - _shift));
                            _controller.BuildDiagonalRoad(new Point((int)(_temp1.X + (_shift - 2) / Math.Abs(_roadOne.slp)), _temp1.Y - (_shift - 2)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                }
                else if (Math.Abs(_temp1.Y - _point1.Y) > Math.Abs(_temp2.Y - _point1.Y))
                {
                    if (_temp1.Y < _temp2.Y)
                    {
                        //Console.WriteLine(2.3);
                        _curveType = "Curved2";
                        if (_point2.X == Math.Min(_temp3.X, _temp4.X))
                        {
                            _curvedstart = new Point((int)(_temp2.X - _shift / Math.Abs(_roadOne.slp)), _temp2.Y - _shift);
                            _controller.BuildDiagonalRoad(_temp1, new Point((int)(_temp2.X - (_shift - 2) / Math.Abs(_roadOne.slp)), _temp2.Y - (_shift - 2)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedstart = new Point((int)(_temp2.X + _shift / Math.Abs(_roadOne.slp)), _temp2.Y - _shift);
                            _controller.BuildDiagonalRoad(_temp1, new Point((int)(_temp2.X + (_shift - 2) / Math.Abs(_roadOne.slp)), _temp2.Y - (_shift - 2)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }
                    else
                    {
                        //Console.WriteLine(2.4);
                        _curveType = "Curved";
                        if (_point2.X == Math.Min(_temp3.X, _temp4.X))
                        {
                            _curvedstart = new Point((int)(_temp2.X - _shift / Math.Abs(_roadOne.slp)), _temp2.Y + _shift);
                            _controller.BuildDiagonalRoad(_temp1, new Point((int)(_temp2.X - (_shift - 2) / Math.Abs(_roadOne.slp)), _temp2.Y + (_shift - 2)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedstart = new Point((int)(_temp2.X + _shift / Math.Abs(_roadOne.slp)), _temp2.Y + _shift);
                            _controller.BuildDiagonalRoad(_temp1, new Point((int)(_temp2.X + (_shift - 2) / Math.Abs(_roadOne.slp)), _temp2.Y + (_shift - 2)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }
                }

                // _roadTwo 

                if (Math.Abs(_temp3.X - _point2.X) < Math.Abs(_temp4.X - _point2.X))
                {
                    if (_temp3.X < _temp4.X)
                    {
                        //Console.WriteLine(2.5);
                        if (_point1.Y == Math.Min(_temp1.Y, _temp2.Y))
                        {
                            _curvedend = new Point(_temp3.X + _shift, (int)(_temp3.Y - _shift * Math.Abs(_roadTwo.slp)));
                            _controller.BuildDiagonalRoad(new Point(_temp3.X + (_shift - 1), (int)(_temp3.Y - (_shift - 1) * Math.Abs(_roadTwo.slp))), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            _curvedend = new Point(_temp3.X + _shift, (int)(_temp3.Y + _shift * Math.Abs(_roadTwo.slp)));
                            _controller.BuildDiagonalRoad(new Point(_temp3.X + (_shift -1), (int)(_temp3.Y + (_shift -1) * Math.Abs(_roadTwo.slp))), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else
                    {
                        //Console.WriteLine(2.6);
                        if (_point1.Y == Math.Min(_temp1.Y, _temp2.Y))
                        {
                            _curvedend = new Point(_temp3.X - _shift, (int)(_temp3.Y - _shift * Math.Abs(_roadTwo.slp)));
                            _controller.BuildDiagonalRoad(new Point(_temp3.X - (_shift - 1), (int)(_temp3.Y - (_shift - 1) * Math.Abs(_roadTwo.slp))), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            _curvedend = new Point(_temp3.X - _shift, (int)(_temp3.Y + _shift * Math.Abs(_roadTwo.slp)));
                            _controller.BuildDiagonalRoad(new Point(_temp3.X - (_shift - 1), (int)(_temp3.Y + (_shift - 1) * Math.Abs(_roadTwo.slp))), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                }
                else if (Math.Abs(_temp3.X - _point2.X) > Math.Abs(_temp4.X - _point2.X))
                {
                    if (_temp3.X < _temp4.X)
                    {
                        //Console.WriteLine(2.7);
                        if (_point1.Y == Math.Min(_temp1.Y, _temp2.Y))
                        {
                            _curvedend = new Point(_temp4.X - _shift, (int)(_temp4.Y - _shift * Math.Abs(_roadTwo.slp)));
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X - (_shift - 1), (int)(_temp4.Y - (_shift - 1) * Math.Abs(_roadTwo.slp))), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedend = new Point(_temp4.X - _shift, (int)(_temp4.Y + _shift * Math.Abs(_roadTwo.slp)));
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X - (_shift - 1), (int)(_temp4.Y + (_shift - 1) * Math.Abs(_roadTwo.slp))), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                    else
                    {
                        //Console.WriteLine(2.8);
                        if (_point1.X == Math.Min(_temp1.Y, _temp2.Y))
                        {
                            _curvedend = new Point(_temp4.X + _shift, (int)(_temp4.Y - _shift * Math.Abs(_roadTwo.slp)));
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X + (_shift - 1), (int)(_temp4.Y - (_shift - 1) * Math.Abs(_roadTwo.slp))), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            _curvedend = new Point(_temp4.X + _shift, (int)(_temp4.Y + _shift * Math.Abs(_roadTwo.slp)));
                            _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X + (_shift - 1), (int)(_temp4.Y + (_shift - 1) * Math.Abs(_roadTwo.slp))), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                }
                _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curveType, true, true, _controller.roads[_controller.roads.Count - 1], _controller.roads[_controller.roads.Count - 2]);
            }
        }

        // This method connects two curved roads together.
        public void CurvedandCurved(Point _point1, Point _point2, int _lanes, Point _temp1, Point _temp2, Point _temp3, Point _temp4, AbstractRoad _roadOne, AbstractRoad _roadTwo, char _roadOneEnds, char _roadTwoEnds, string _dir, string _dir2)
        {

            // If the roads end the same (with a few exceptions), we can simply just enlarge the horizontal or vertical road. 

            RoadController _controller = General_Form.Main.BuildScreen.builder.roadBuilder;
            //Console.WriteLine("Curved and Curved");
            if (_roadOneEnds == _roadTwoEnds && _dir != _dir2)
            {
                double _distance = Math.Sqrt(Math.Pow(_point1.X - _point2.X, 2) + Math.Pow(_point1.Y - _point2.Y, 2)) + 1;
                int _direction;
                bool _beginconnection, _endconnection;
                AbstractRoad _beginConnectedTo, _endConnectedTo;
                

                if (_roadOneEnds == 'v')
                {
                    if ((_dir == "SE" && _dir2 == "NE") || (_dir == "NE" && _dir2 == "SE"))
                        return;
                    else if ((_dir == "SW" && _dir2 == "NW") || (_dir == "NW" && _dir2 == "SW"))
                        return;

                    else
                    {
                        if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                        {
                            if (_temp1.X < _temp2.X)
                                _direction = -1;

                            else
                                _direction = 1;

                            _beginconnection = true;
                            _endconnection = _roadOne.endconnection;
                            _beginConnectedTo = _roadTwo;
                            _endConnectedTo = _roadOne.endConnectedTo;

                            //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                            _controller.roads.Remove(_roadOne);
                            _controller.BuildCurvedRoad(new Point(_temp1.X + (int)(_distance * _direction), _temp1.Y), _temp2, _lanes, _roadOne.Type, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                        }
                        else if (Math.Abs(_temp1.X - _point1.X) > Math.Abs(_temp2.X - _point1.X))
                        {
                            if (_temp2.X < _temp1.X)
                                _direction = -1;

                            else
                                _direction = 1;

                            _beginconnection = _roadOne.beginconnection;
                            _endconnection = true;
                            _beginConnectedTo = _roadOne.beginConnectedTo;
                            _endConnectedTo = _roadTwo;

                            //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                            _controller.roads.Remove(_roadOne);
                            _controller.BuildCurvedRoad(_temp1, new Point(_temp2.X + (int)(_distance * _direction), _temp2.Y), _lanes, _roadOne.Type, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                        }
                    }
                }
                else //if (_roadOneEnds == 'h')
                {
                    if ((_dir == "SE" && _dir2 == "SW") || (_dir == "SW" && _dir2 == "SE"))
                        return;

                    else
                    {
                        if (Math.Abs(_temp1.Y - _point1.Y) < Math.Abs(_temp2.Y - _point1.Y))
                        {
                            if (_temp1.Y < _temp2.Y)
                                _direction = -1;

                            else
                                _direction = 1;

                            _beginconnection = true;
                            _endconnection = _roadOne.endconnection;
                            _beginConnectedTo = _roadTwo;
                            _endConnectedTo = _roadOne.endConnectedTo;

                            //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                            _controller.roads.Remove(_roadOne);
                            _controller.BuildCurvedRoad(new Point(_temp1.X, _temp1.Y + (int)(_distance * _direction)), _temp2, _lanes, _roadOne.Type, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                        }
                        else if (Math.Abs(_temp1.Y - _point1.Y) > Math.Abs(_temp2.Y - _point1.Y))
                        {
                            if (_temp2.Y < _temp1.Y)
                                _direction = -1;

                            else
                                _direction = 1;

                            _beginconnection = _roadOne.beginconnection;
                            _endconnection = true;
                            _beginConnectedTo = _roadOne.beginConnectedTo;
                            _endConnectedTo = _roadTwo;

                            //Console.WriteLine(_beginconnection + "---------" + _endconnection);
                            _controller.roads.Remove(_roadOne);
                            _controller.BuildCurvedRoad(_temp1, new Point(_temp2.X, _temp2.Y + (int)(_distance * _direction)), _lanes, _roadOne.Type, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                        }
                    }
                }
            }

            // If they don't have the same ending then we once again have to draw a curved road to connect them, we also have to adjust the original roads a little bit.

            else if (_roadOneEnds != _roadTwoEnds)
            {
                if (_roadOneEnds == 'v' && _roadTwoEnds == 'h')
                {
                    _controller.roads.Remove(_roadOne);
                    _controller.roads.Remove(_roadTwo);
                    Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);
                    string _curveType = "Curved";

                    // _roadOne

                    if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            //Console.WriteLine(1.1);
                            _curvedend = new Point(_temp1.X + _shift, _temp1.Y);
                            _controller.BuildCurvedRoad(new Point(_temp1.X + (_shift - 2), _temp1.Y), _temp2, _lanes, _roadOne.Type, true, _roadOne.endconnection, null ,_roadOne.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(1.2);
                            _curvedend = new Point(_temp1.X - _shift, _temp1.Y);
                            _controller.BuildCurvedRoad(new Point(_temp1.X - (_shift - 2), _temp1.Y), _temp2, _lanes, _roadOne.Type, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp1.X - _point1.X) > Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            //Console.WriteLine(1.3);
                            _curvedend = new Point(_temp2.X - _shift, _temp2.Y);
                            _controller.BuildCurvedRoad(_temp1, new Point(_temp2.X - (_shift - 2), _temp2.Y), _lanes, _roadOne.Type, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(1.4);
                            _curvedend = new Point(_temp2.X + _shift, _temp2.Y);
                            _controller.BuildCurvedRoad(_temp1, new Point(_temp2.X + (_shift - 2), _temp2.Y), _lanes, _roadOne.Type, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }

                    // _roadTwo


                    if (Math.Abs(_temp3.Y - _point2.Y) < Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.Y < _temp4.Y)
                        {
                            //Console.WriteLine(1.5);
                            _curveType = "Curved";
                            _curvedstart = new Point(_temp3.X, _temp3.Y + _shift);
                            _controller.BuildCurvedRoad(new Point(_temp3.X, _temp3.Y + _shift), _temp4, _lanes, _roadTwo.Type, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(1.6);
                            _curveType = "Curved2";
                            _curvedstart = new Point(_temp3.X, _temp3.Y - _shift);
                            _controller.BuildCurvedRoad(new Point(_temp3.X, _temp3.Y - _shift), _temp4, _lanes, _roadTwo.Type, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp3.Y - _point2.Y) > Math.Abs(_temp4.Y - _point2.Y))
                    {

                        if (_temp3.Y < _temp4.Y)
                        {
                            //Console.WriteLine(1.7);
                            _curveType = "Curved2";
                            _curvedstart = new Point(_temp4.X, _temp4.Y - _shift);
                            _controller.BuildCurvedRoad(_temp3, new Point(_temp4.X, _temp4.Y - _shift), _lanes, _roadTwo.Type, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(1.8);
                            _curveType = "Curved";
                            _curvedstart = new Point(_temp4.X, _temp4.Y + _shift);
                            _controller.BuildCurvedRoad(_temp3, new Point(_temp4.X, _temp4.Y + _shift), _lanes, _roadTwo.Type, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                    _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curveType, true, true, _controller.roads[_controller.roads.Count - 2], _controller.roads[_controller.roads.Count - 1]);
                }
                else if (_roadOneEnds == 'h' && _roadTwoEnds == 'v')
                {
                    _controller.roads.Remove(_roadOne);
                    _controller.roads.Remove(_roadTwo);
                    Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);
                    string _curveType = "Curved";

                    // _roadOne

                    if (Math.Abs(_temp1.Y - _point1.Y) < Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            //Console.WriteLine(2.1);
                            _curveType = "Curved";
                            _curvedstart = new Point(_temp1.X, _temp1.Y + _shift);
                            _controller.BuildCurvedRoad(new Point(_temp1.X, _temp1.Y + _shift), _temp2, _lanes, _roadOne.Type, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(2.2);
                            _curveType = "Curved2";
                            _curvedstart = new Point(_temp1.X, _temp1.Y - _shift);
                            _controller.BuildCurvedRoad(new Point(_temp1.X, _temp1.Y - _shift), _temp2, _lanes, _roadOne.Type, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp1.Y - _point1.Y) > Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            //Console.WriteLine(2.3);
                            _curveType = "Curved2";
                            _curvedstart = new Point(_temp2.X, _temp2.Y - _shift);
                            _controller.BuildCurvedRoad(_temp1, new Point(_temp2.X, _temp2.Y - _shift), _lanes, _roadOne.Type, _roadOne.beginconnection, true, _roadOne.endConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(2.4);
                            _curveType = "Curved";
                            _curvedstart = new Point(_temp2.X, _temp2.Y + _shift);
                            _controller.BuildCurvedRoad(_temp1, new Point(_temp2.X, _temp2.Y + _shift), _lanes, _roadOne.Type, _roadOne.beginconnection, true, _roadOne.endConnectedTo, null);
                        }
                    }

                    // _roadTwo

                    if (Math.Abs(_temp3.X - _point2.X) < Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            //Console.WriteLine(2.5);
                            _curvedend = new Point(_temp3.X + _shift, _temp3.Y);
                            _controller.BuildCurvedRoad(new Point(_temp3.X + (_shift - 2), _temp3.Y), _temp4, _lanes, _roadTwo.Type, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(2.6);
                            _curvedend = new Point(_temp3.X - _shift, _temp3.Y);
                            _controller.BuildCurvedRoad(new Point(_temp3.X - (_shift - 2), _temp3.Y), _temp4, _lanes, _roadTwo.Type, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp3.X - _point2.X) > Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            //Console.WriteLine(2.7);
                            _curvedend = new Point(_temp4.X - _shift, _temp4.Y);
                            _controller.BuildCurvedRoad(_temp3, new Point(_temp4.X - (_shift - 2), _temp4.Y), _lanes, _roadTwo.Type, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(2.8);
                            _curvedend = new Point(_temp4.X + _shift, _temp4.Y);
                            _controller.BuildCurvedRoad(_temp3, new Point(_temp4.X + (_shift - 2), _temp4.Y), _lanes, _roadTwo.Type, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                    _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curveType, true, true, _controller.roads[_controller.roads.Count - 1], _controller.roads[_controller.roads.Count - 2]);
                }
            }
        }
        
        // This method connects a curved road and a diagonal road together.
        public void DiagonalandCurved(Point _point1, Point _point2, int _lanes, Point _temp1, Point _temp2, Point _temp3, Point _temp4, AbstractRoad _roadOne, AbstractRoad _roadTwo, char _roadOneEnds, char _roadTwoEnds, string _dir, string _dir2)
        {
            // We first connect the newly drawn diagonal road to the already drawn curved road.

            RoadController _controller = General_Form.Main.BuildScreen.builder.roadBuilder;

            if (_roadOne.Type == "Diagonal" && (_roadTwo.Type == "Curved" || _roadTwo.Type == "Curved2"))
            {
                double _distance = Math.Sqrt(Math.Pow(_point1.X - _point2.X, 2) + Math.Pow(_point1.Y - _point2.Y, 2)) + 1;
                int _direction;
                bool _beginconnection, _endconnection;
                AbstractRoad _beginConnectedTo, _endConnectedTo;

                // If they have the same ending then we once again we can simply just enlarge one of them.

                if (_roadOneEnds == 'v' && _roadTwoEnds == 'v')
                {
                    if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                            _direction = -2;
                        else
                            _direction = 2;

                        _beginconnection = true;
                        _endconnection = _roadOne.endconnection;

                        _beginConnectedTo = _roadTwo;
                        _endConnectedTo = _roadOne.endConnectedTo;

                        //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                        _controller.roads.Remove(_roadOne);
                        _controller.BuildDiagonalRoad(new Point(_point2.X + _direction, _point2.Y), _temp2, _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                    }
                    else
                    {
                        if (_temp1.X < _temp2.X)
                            _direction = 2;
                        else
                            _direction = -2;

                        _beginconnection = _roadOne.beginconnection;
                        _endconnection = true;

                        _beginConnectedTo = _roadOne.beginConnectedTo;
                        _endConnectedTo = _roadTwo;

                        //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                        _controller.roads.Remove(_roadOne);
                        _controller.BuildDiagonalRoad(_temp1, new Point(_point2.X + _direction, _point2.Y), _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                    }
                }

                else if (_roadOneEnds == 'h' && _roadTwoEnds == 'h')
                {
                    if (Math.Abs(_temp1.Y - _point1.Y) < Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                            _direction = -2;
                        else
                            _direction = 2;

                        _beginconnection = true;
                        _endconnection = _roadOne.endconnection;

                        _beginConnectedTo = _roadTwo;
                        _endConnectedTo = _roadOne.endConnectedTo;

                        //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                        _controller.roads.Remove(_roadOne);
                        _controller.BuildDiagonalRoad(new Point(_point2.X, _point2.Y + _direction), _temp2, _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                    }
                    else
                    {
                        if (_temp1.Y < _temp2.Y)
                            _direction = 2;
                        else
                            _direction = -2;

                        _beginconnection = _roadOne.beginconnection;
                        _endconnection = true;

                        _beginConnectedTo = _roadOne.beginConnectedTo;
                        _endConnectedTo = _roadTwo;

                        //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                        _controller.roads.Remove(_roadOne);
                        _controller.BuildDiagonalRoad(_temp1, new Point(_point2.X, _point2.Y + _direction), _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                    }
                }

                // If they don't have the same ending then we once again have to draw a curved road to connect them, we also have to adjust the original roads a little bit.

                else if (_roadOneEnds == 'v' && _roadTwoEnds == 'h')
                {
                    _controller.roads.Remove(_roadOne);
                    _controller.roads.Remove(_roadTwo);
                    Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);
                    string _curveType = "Curved";

                    // _roadOne

                    if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            //Console.WriteLine(1.1);
                            if (_point2.Y == Math.Min(_temp3.Y, _temp4.Y))
                            {
                                _curvedend = new Point(_temp1.X + _shift, (int)(_temp1.Y - _shift * Math.Abs(_roadOne.slp)));
                                _controller.BuildDiagonalRoad(new Point(_temp1.X + (_shift - 2), (int)(_temp1.Y - (_shift - 2) * Math.Abs(_roadOne.slp))), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                            }
                            else
                            {
                                _curvedend = new Point(_temp1.X + _shift, (int)(_temp1.Y + _shift * Math.Abs(_roadOne.slp)));
                                _controller.BuildDiagonalRoad(new Point(_temp1.X + (_shift - 2), (int)(_temp1.Y + (_shift - 2) * Math.Abs(_roadOne.slp))), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                            }
                        }
                        else
                        {
                            //Console.WriteLine(1.2);
                            if (_point2.Y == Math.Min(_temp3.Y, _temp4.Y))
                            {
                                _curvedend = new Point(_temp1.X - _shift, (int)(_temp1.Y - _shift * Math.Abs(_roadOne.slp)));
                                _controller.BuildDiagonalRoad(new Point(_temp1.X - (_shift - 2), (int)(_temp1.Y - (_shift - 2) * Math.Abs(_roadOne.slp))), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                            }
                            else
                            {
                                _curvedend = new Point(_temp1.X - _shift, (int)(_temp1.Y + _shift * Math.Abs(_roadOne.slp)));
                                _controller.BuildDiagonalRoad(new Point(_temp1.X - (_shift - 2), (int)(_temp1.Y + (_shift - 2) * Math.Abs(_roadOne.slp))), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                            }
                        }
                    }
                    else if (Math.Abs(_temp1.X - _point1.X) > Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            //Console.WriteLine(1.3);
                            if (_point2.Y == Math.Min(_temp3.Y, _temp4.Y))
                            {
                                _curvedend = new Point(_temp2.X - _shift, (int)(_temp2.Y - _shift * Math.Abs(_roadOne.slp)));
                                _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X - (_shift - 2), (int)(_temp2.Y - (_shift - 2) * Math.Abs(_roadOne.slp))), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                            }
                            else
                            {
                                _curvedend = new Point(_temp2.X - _shift, (int)(_temp2.Y + _shift * Math.Abs(_roadOne.slp)));
                                _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X - (_shift - 2), (int)(_temp2.Y + (_shift - 2) * Math.Abs(_roadOne.slp))), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                            }
                        }
                        else
                        {
                            //Console.WriteLine(1.4);
                            if (_point2.Y == Math.Min(_temp3.Y, _temp4.Y))
                            {
                                _curvedend = new Point(_temp2.X + _shift, (int)(_temp2.Y - _shift * Math.Abs(_roadOne.slp)));
                                _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X + (_shift - 2), (int)(_temp2.Y - (_shift - 2) * Math.Abs(_roadOne.slp))), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                            }
                            else
                            {
                                _curvedend = new Point(_temp2.X + _shift, (int)(_temp2.Y + _shift * Math.Abs(_roadOne.slp)));
                                _controller.BuildDiagonalRoad(_temp1, new Point(_temp2.X + (_shift - 2), (int)(_temp2.Y + (_shift - 2) * Math.Abs(_roadOne.slp))), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                            }
                        }
                    }

                    // _roadTwo

                    if (Math.Abs(_temp3.Y - _point2.Y) < Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.Y < _temp4.Y)
                        {
                            //Console.WriteLine(1.5);
                            _curveType = "Curved";
                            _curvedstart = new Point(_temp3.X, _temp3.Y + _shift);
                            _controller.BuildCurvedRoad(new Point(_temp3.X, _temp3.Y + _shift), _temp4, _lanes, _roadTwo.Type, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(1.6);
                            _curveType = "Curved2";
                            _curvedstart = new Point(_temp3.X, _temp3.Y - _shift);
                            _controller.BuildCurvedRoad(new Point(_temp3.X, _temp3.Y - _shift), _temp4, _lanes, _roadTwo.Type, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp3.Y - _point2.Y) > Math.Abs(_temp4.Y - _point2.Y))
                    {

                        if (_temp3.Y < _temp4.Y)
                        {
                            //Console.WriteLine(1.7);
                            _curveType = "Curved2";
                            _curvedstart = new Point(_temp4.X, _temp4.Y - _shift);
                            _controller.BuildCurvedRoad(_temp3, new Point(_temp4.X, _temp4.Y - _shift), _lanes, _roadTwo.Type, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(1.8);
                            _curveType = "Curved";
                            _curvedstart = new Point(_temp4.X, _temp4.Y + _shift);
                            _controller.BuildCurvedRoad(_temp3, new Point(_temp4.X, _temp4.Y + _shift), _lanes, _roadTwo.Type, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                        }
                    }

                    _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curveType, true, true, _controller.roads[_controller.roads.Count - 2], _controller.roads[_controller.roads.Count - 1]);
                }

                else if (_roadOneEnds == 'h' && _roadTwoEnds == 'v')
                {
                    _controller.roads.Remove(_roadOne);
                    _controller.roads.Remove(_roadTwo);
                    Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);
                    string _curveType = "Curved";

                    if (Math.Abs(_temp1.Y - _point1.Y) < Math.Abs(_temp2.Y - _point1.Y))
                    {
                        // _roadOne

                        if (_temp1.Y < _temp2.Y)
                        {
                            _curveType = "Curved";
                            //Console.WriteLine(2.1);
                            if (_point2.X == Math.Min(_temp3.X, _temp4.X))
                            {
                                _curvedstart = new Point((int)(_temp1.X - _shift / Math.Abs(_roadOne.slp)), (_temp1.Y + _shift));
                                _controller.BuildDiagonalRoad(new Point((int)(_temp1.X - (_shift - 2) / Math.Abs(_roadOne.slp)), _temp1.Y + (_shift - 2)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                            }
                            else
                            {
                                _curvedstart = new Point((int)(_temp1.X + _shift / Math.Abs(_roadOne.slp)), (_temp1.Y + _shift));
                                _controller.BuildDiagonalRoad(new Point((int)(_temp1.X + (_shift - 2) / Math.Abs(_roadOne.slp)), _temp1.Y + (_shift - 2)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                            }
                        }
                        else
                        {
                            //Console.WriteLine(2.2);

                            _curveType = "Curved2";
                            if (_point2.X == Math.Min(_temp3.X, _temp4.X))
                            {
                                _curvedstart = new Point((int)(_temp1.X - _shift / Math.Abs(_roadOne.slp)), (_temp1.Y - _shift));
                                _controller.BuildDiagonalRoad(new Point((int)(_temp1.X - (_shift - 2) / Math.Abs(_roadOne.slp)), _temp1.Y - (_shift - 2)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                            }
                            else
                            {
                                _curvedstart = new Point((int)(_temp1.X + _shift / Math.Abs(_roadOne.slp)), (_temp1.Y - _shift));
                                _controller.BuildDiagonalRoad(new Point((int)(_temp1.X + (_shift - 2) / Math.Abs(_roadOne.slp)), _temp1.Y - (_shift - 2)), _temp2, _lanes, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                            }
                        }
                    }
                    else if (Math.Abs(_temp1.Y - _point1.Y) > Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            //Console.WriteLine(2.3);
                            _curveType = "Curved2";
                            if (_point2.X == Math.Min(_temp3.X, _temp4.X))
                            {
                                _curvedstart = new Point((int)(_temp2.X - _shift / Math.Abs(_roadOne.slp)), _temp2.Y - _shift);
                                _controller.BuildDiagonalRoad(_temp1, new Point((int)(_temp2.X - (_shift - 2) / Math.Abs(_roadOne.slp)), _temp2.Y - (_shift - 2)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                            }
                            else
                            {
                                _curvedstart = new Point((int)(_temp2.X + _shift / Math.Abs(_roadOne.slp)), _temp2.Y - _shift);
                                _controller.BuildDiagonalRoad(_temp1, new Point((int)(_temp2.X + (_shift - 2) / Math.Abs(_roadOne.slp)), _temp2.Y - (_shift - 2)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                            }
                        }
                        else
                        {
                            //Console.WriteLine(2.4);
                            _curveType = "Curved";
                            if (_point2.X == Math.Min(_temp3.X, _temp4.X))
                            {
                                _curvedstart = new Point((int)(_temp2.X - _shift / Math.Abs(_roadOne.slp)), _temp2.Y + _shift);
                                _controller.BuildDiagonalRoad(_temp1, new Point((int)(_temp2.X - (_shift - 2) / Math.Abs(_roadOne.slp)), _temp2.Y + (_shift - 2)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                            }
                            else
                            {
                                _curvedstart = new Point((int)(_temp2.X + _shift / Math.Abs(_roadOne.slp)), _temp2.Y + _shift);
                                _controller.BuildDiagonalRoad(_temp1, new Point((int)(_temp2.X + (_shift - 2) / Math.Abs(_roadOne.slp)), _temp2.Y + (_shift - 2)), _lanes, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                            }
                        }
                    }

                    // _roadTwo

                    if (Math.Abs(_temp3.X - _point2.X) < Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            //Console.WriteLine(2.5);
                            _curvedend = new Point(_temp3.X + _shift, _temp3.Y);
                            _controller.BuildCurvedRoad(new Point(_temp3.X + (_shift - 2), _temp1.Y), _temp4, _lanes, _roadTwo.Type, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(2.6);
                            _curvedend = new Point(_temp3.X - _shift, _temp3.Y);
                            _controller.BuildCurvedRoad(new Point(_temp3.X - (_shift - 2), _temp3.Y), _temp4, _lanes, _roadTwo.Type, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                    else if (Math.Abs(_temp3.X - _point2.X) > Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            //Console.WriteLine(2.7);
                            _curvedend = new Point(_temp4.X - _shift, _temp4.Y);
                            _controller.BuildCurvedRoad(_temp3, new Point(_temp4.X - (_shift - 2), _temp4.Y), _lanes, _roadTwo.Type, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(2.8);
                            _curvedend = new Point(_temp4.X + _shift, _temp4.Y);
                            _controller.BuildCurvedRoad(_temp3, new Point(_temp4.X + (_shift - 2), _temp4.Y), _lanes, _roadTwo.Type, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                        }
                    }
                    _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curveType, true, true, _controller.roads[_controller.roads.Count - 1], _controller.roads[_controller.roads.Count - 2]);

                }

            }

            // If it is the other way around we first connect the newly drawn curved road to the already drawn diagonal road.

            else //if((_roadOne.Type == "Curved" || _roadOne.Type == "Curved2") && _roadTwo.Type == "Diagonal" )
            {
                double _distance = Math.Sqrt(Math.Pow(_point1.X - _point2.X, 2) + Math.Pow(_point1.Y - _point2.Y, 2)) + 1;
                int _direction;
                bool _beginconnection, _endconnection;
                AbstractRoad _beginConnectedTo, _endConnectedTo;

                // If they have the same ending then we once again we can simply just enlarge one of them.

                if (_roadOneEnds == 'v' && _roadTwoEnds == 'v')
                {
                    if (Math.Abs(_temp3.X - _point2.X) < Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp3.X < _temp4.X)
                            _direction = -2;
                        else
                            _direction = 2;

                        _beginconnection = true;
                        _endconnection = _roadTwo.endconnection;

                        _beginConnectedTo = _roadOne;
                        _endConnectedTo = _roadTwo.endConnectedTo;

                        //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                        _controller.roads.Remove(_roadTwo);
                        _controller.BuildDiagonalRoad(new Point(_point1.X + _direction, _point1.Y), _temp4, _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                    }
                    else
                    {
                        if (_temp3.X < _temp4.X)
                            _direction = 2;
                        else
                            _direction = -2;

                        _beginconnection = _roadTwo.beginconnection;
                        _endconnection = true;

                        _beginConnectedTo = _roadTwo.beginConnectedTo;
                        _endConnectedTo = _roadOne;

                        //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                        _controller.roads.Remove(_roadTwo);
                        _controller.BuildDiagonalRoad(_temp3, new Point(_point1.X + _direction, _point1.Y), _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                    }
                }

                else if (_roadOneEnds == 'h' && _roadTwoEnds == 'h')
                {
                    if (Math.Abs(_temp3.Y - _point2.Y) < Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.Y < _temp4.Y)
                            _direction = -2;
                        else
                            _direction = 2;

                        _beginconnection = true;
                        _endconnection = _roadTwo.endconnection;

                        _beginConnectedTo = _roadOne;
                        _endConnectedTo = _roadTwo.endConnectedTo;

                        //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                        _controller.roads.Remove(_roadTwo);
                        _controller.BuildDiagonalRoad(new Point(_point1.X, _point1.Y + _direction), _temp4, _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                    }
                    else
                    {
                        if (_temp3.Y < _temp4.Y)
                            _direction = 2;
                        else
                            _direction = -2;

                        _beginconnection = _roadTwo.beginconnection;
                        _endconnection = true;

                        _beginConnectedTo = _roadTwo.beginConnectedTo;
                        _endConnectedTo = _roadOne;

                        //Console.WriteLine(_beginconnection + "---------" + _endconnection);

                        _controller.roads.Remove(_roadTwo);
                        _controller.BuildDiagonalRoad(_temp3, new Point(_point1.X, _point1.Y + _direction), _lanes, _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo);
                    }
                }

                // If they don't have the same ending then we once again have to draw a curved road to connect them, we also have to adjust the original roads a little bit.

                else if (_roadOneEnds == 'v' && _roadTwoEnds == 'h')
                {
                    _controller.roads.Remove(_roadOne);
                    _controller.roads.Remove(_roadTwo);
                    Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);
                    string _curveType = "Curved";

                    // _roadOne

                    if (Math.Abs(_temp1.X - _point1.X) < Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            //Console.WriteLine(1.1);
                            _curvedend = new Point(_temp1.X + _shift, _temp1.Y);
                            _controller.BuildCurvedRoad(new Point(_temp1.X + (_shift - 2), _temp1.Y), _temp2, _lanes, _roadOne.Type, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(1.2);
                            _curvedend = new Point(_temp1.X - _shift, _temp1.Y);
                            _controller.BuildCurvedRoad(new Point(_temp1.X - (_shift - 2), _temp1.Y), _temp2, _lanes, _roadOne.Type, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }
                    else if (Math.Abs(_temp1.X - _point1.X) > Math.Abs(_temp2.X - _point1.X))
                    {
                        if (_temp1.X < _temp2.X)
                        {
                            //Console.WriteLine(1.3);
                            _curvedend = new Point(_temp2.X - _shift, _temp2.Y);
                            _controller.BuildCurvedRoad(_temp1, new Point(_temp2.X - (_shift - 2), _temp2.Y), _lanes, _roadOne.Type, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                        else
                        {
                            //Console.WriteLine(1.4);
                            _curvedend = new Point(_temp2.X + _shift, _temp2.Y);
                            _controller.BuildCurvedRoad(_temp1, new Point(_temp2.X + (_shift - 2), _temp2.Y), _lanes, _roadOne.Type, _roadOne.beginconnection, true, _roadOne.beginConnectedTo, null);
                        }
                    }

                    // _roadTwo

                    if (Math.Abs(_temp3.Y - _point2.Y) < Math.Abs(_temp4.Y - _point2.Y))
                    {
                        if (_temp3.Y < _temp4.Y)
                        {
                            //Console.WriteLine(1.5);
                            _curveType = "Curved";
                            if (_point1.X == Math.Min(_temp1.X, _temp2.X))
                            {
                                _curvedstart = new Point((int)(_temp3.X - _shift / Math.Abs(_roadTwo.slp)), _temp3.Y + _shift);
                                _controller.BuildDiagonalRoad(new Point((int)(_temp3.X - (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp3.Y + (_shift - 1)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                            }
                            else
                            {
                                _curvedstart = new Point((int)(_temp3.X + _shift / Math.Abs(_roadTwo.slp)), _temp3.Y + _shift);
                                _controller.BuildDiagonalRoad(new Point((int)(_temp3.X + (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp3.Y + (_shift - 1)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                            }
                        }
                        else
                        {
                            _curveType = "Curved2";
                            //Console.WriteLine(1.6);
                            if (_point1.X == Math.Min(_temp1.X, _temp2.X))
                            {
                                _curvedstart = new Point((int)(_temp3.X - _shift / Math.Abs(_roadTwo.slp)), _temp3.Y - _shift);
                                _controller.BuildDiagonalRoad(new Point((int)(_temp3.X - (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp3.Y - (_shift - 1)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                            }
                            else
                            {
                                _curvedstart = new Point((int)(_temp3.X + _shift / Math.Abs(_roadTwo.slp)), _temp3.Y - _shift);
                                _controller.BuildDiagonalRoad(new Point((int)(_temp3.X + (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp3.Y - (_shift - 1)), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                            }
                        }
                    }
                    else if (Math.Abs(_temp3.Y - _point2.Y) > Math.Abs(_temp4.Y - _point2.Y))
                    {
                        _curveType = "Curved2";
                        if (_temp3.Y < _temp4.Y)
                        {
                            //Console.WriteLine(1.7);
                            if (_point1.X == Math.Min(_temp1.X, _temp2.X))
                            {
                                _curvedstart = new Point((int)(_temp4.X - _shift / Math.Abs(_roadTwo.slp)), _temp4.Y - _shift);
                                _controller.BuildDiagonalRoad(_temp3, new Point((int)(_temp4.X - (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp4.Y - (_shift - 1)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                            }
                            else
                            {
                                _curvedstart = new Point((int)(_temp4.X + _shift / Math.Abs(_roadTwo.slp)), _temp4.Y - _shift);
                                _controller.BuildDiagonalRoad(_temp3, new Point((int)(_temp4.X + (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp4.Y - (_shift - 1)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                            }
                        }
                        else
                        {
                            _curveType = "Curved";
                            //Console.WriteLine(1.8);
                            if (_point1.X == Math.Min(_temp1.X, _temp2.X))
                            {
                                _curvedstart = new Point((int)(_temp4.X - _shift / Math.Abs(_roadTwo.slp)), _temp4.Y + _shift);
                                _controller.BuildDiagonalRoad(_temp3, new Point((int)(_temp4.X - (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp4.Y + (_shift - 1)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                            }
                            else
                            {
                                _curvedstart = new Point((int)(_temp4.X + _shift / Math.Abs(_roadTwo.slp)), _temp4.Y + _shift);
                                _controller.BuildDiagonalRoad(_temp3, new Point((int)(_temp4.X + (_shift - 1) / Math.Abs(_roadTwo.slp)), _temp4.Y + (_shift - 1)), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                            }
                        }
                    }
                    
                    _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curveType, true, true, _controller.roads[_controller.roads.Count - 1], _controller.roads[_controller.roads.Count - 2]);
                }

                else if (_roadOneEnds == 'h' && _roadTwoEnds == 'v')
                {
                    _controller.roads.Remove(_roadOne);
                    _controller.roads.Remove(_roadTwo);
                    Point _curvedstart = new Point(0, 0), _curvedend = new Point(0, 0);
                    string _curveType = "Curved";

                    // _roadOne

                    if (Math.Abs(_temp1.Y - _point1.Y) < Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            //Console.WriteLine(2.1);
                            _curveType = "Curved";
                            _curvedstart = new Point(_temp1.X, _temp1.Y + _shift);
                            _controller.BuildCurvedRoad(new Point(_temp1.X, _temp1.Y + _shift), _temp2, _lanes, _roadOne.Type, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(2.2);
                            _curveType = "Curved2";
                            _curvedstart = new Point(_temp1.X, _temp1.Y - _shift);
                            _controller.BuildCurvedRoad(new Point(_temp1.X, _temp1.Y - _shift), _temp2, _lanes, _roadOne.Type, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }
                    else if (Math.Abs(_temp1.Y - _point1.Y) > Math.Abs(_temp2.Y - _point1.Y))
                    {
                        if (_temp1.Y < _temp2.Y)
                        {
                            //Console.WriteLine(2.3);
                            _curveType = "Curved2";
                            _curvedstart = new Point(_temp2.X, _temp2.Y - _shift);
                            _controller.BuildCurvedRoad(_temp1, new Point(_temp2.X, _temp2.Y - _shift), _lanes, _roadOne.Type, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                        else
                        {
                            //Console.WriteLine(2.4);
                            _curveType = "Curved";
                            _curvedstart = new Point(_temp2.X, _temp2.Y + _shift);
                            _controller.BuildCurvedRoad(_temp1, new Point(_temp2.X, _temp2.Y + _shift), _lanes, _roadOne.Type, true, _roadOne.endconnection, null, _roadOne.endConnectedTo);
                        }
                    }

                    // _roadTwo 

                    if (Math.Abs(_temp3.X - _point2.X) < Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            //Console.WriteLine(2.5);
                            if (_point1.Y == Math.Min(_temp1.Y, _temp2.Y))
                            {
                                _curvedend = new Point(_temp3.X + _shift, (int)(_temp3.Y - _shift * Math.Abs(_roadTwo.slp)));
                                _controller.BuildDiagonalRoad(new Point(_temp3.X + (_shift - 1), (int)(_temp3.Y - (_shift - 1) * Math.Abs(_roadTwo.slp))), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                            }
                            else
                            {
                                _curvedend = new Point(_temp3.X + _shift, (int)(_temp3.Y + _shift * Math.Abs(_roadTwo.slp)));
                                _controller.BuildDiagonalRoad(new Point(_temp3.X + (_shift - 1), (int)(_temp3.Y + (_shift - 1) * Math.Abs(_roadTwo.slp))), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                            }
                        }
                        else
                        {
                            //Console.WriteLine(2.6);
                            if (_point1.Y == Math.Min(_temp1.Y, _temp2.Y))
                            {
                                _curvedend = new Point(_temp3.X - _shift, (int)(_temp3.Y - _shift * Math.Abs(_roadTwo.slp)));
                                _controller.BuildDiagonalRoad(new Point(_temp3.X - (_shift - 1), (int)(_temp3.Y - (_shift - 1) * Math.Abs(_roadTwo.slp))), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                            }
                            else
                            {
                                _curvedend = new Point(_temp3.X - _shift, (int)(_temp3.Y + _shift * Math.Abs(_roadTwo.slp)));
                                _controller.BuildDiagonalRoad(new Point(_temp3.X - (_shift - 1), (int)(_temp3.Y + (_shift - 1) * Math.Abs(_roadTwo.slp))), _temp4, _lanes, true, _roadTwo.endconnection, null, _roadTwo.endConnectedTo);
                            }
                        }
                    }
                    else if (Math.Abs(_temp3.X - _point2.X) > Math.Abs(_temp4.X - _point2.X))
                    {
                        if (_temp3.X < _temp4.X)
                        {
                            //Console.WriteLine(2.7);
                            if (_point1.Y == Math.Min(_temp1.Y, _temp2.Y))
                            {
                                _curvedend = new Point(_temp4.X - _shift, (int)(_temp4.Y - _shift * Math.Abs(_roadTwo.slp)));
                                _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X - (_shift - 1), (int)(_temp4.Y - (_shift - 1) * Math.Abs(_roadTwo.slp))), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                            }
                            else
                            {
                                _curvedend = new Point(_temp4.X - _shift, (int)(_temp4.Y + _shift * Math.Abs(_roadTwo.slp)));
                                _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X - (_shift - 1), (int)(_temp4.Y + (_shift - 1) * Math.Abs(_roadTwo.slp))), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                            }
                        }
                        else
                        {
                            //Console.WriteLine(2.8);
                            if (_point1.X == Math.Min(_temp1.Y, _temp2.Y))
                            {
                                _curvedend = new Point(_temp4.X + _shift, (int)(_temp4.Y - _shift * Math.Abs(_roadTwo.slp)));
                                _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X + (_shift - 1), (int)(_temp4.Y - (_shift - 1) * Math.Abs(_roadTwo.slp))), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                            }
                            else
                            {
                                _curvedend = new Point(_temp4.X + _shift, (int)(_temp4.Y + _shift * Math.Abs(_roadTwo.slp)));
                                _controller.BuildDiagonalRoad(_temp3, new Point(_temp4.X + (_shift - 1), (int)(_temp4.Y + (_shift - 1) * Math.Abs(_roadTwo.slp))), _lanes, _roadTwo.beginconnection, true, _roadTwo.beginConnectedTo, null);
                            }
                        }
                    }

                    _controller.BuildCurvedRoad(_curvedstart, _curvedend, _lanes, _curveType, true, true, _controller.roads[_controller.roads.Count - 1], _controller.roads[_controller.roads.Count - 2]);
                }
            }
        }
    }
}


