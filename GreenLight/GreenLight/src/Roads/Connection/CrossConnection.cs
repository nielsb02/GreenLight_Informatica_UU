using System.ComponentModel;
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
    // The CrossConnection class deals with creating connections between two CrossRoads or a CrossRoad and another road type
    // Just like in the Connection class, there are a lot of thing to take into consideration while creating Connections,
    // so this class also has a lot of if-statements
    class CrossConnection
    {
        RoadController controller = General_Form.Main.BuildScreen.builder.roadBuilder;
        Point point1;
        Point point2;
        string dir1;
        string dir2;
        AbstractRoad roadOne;
        AbstractRoad roadTwo;
        Point temp1, temp2, temp3, temp4;

        // The constructor method sees what kind of roads have to be connected and calls another method accordingly.
        public CrossConnection(Point _point1, Point _point2, string _dir1, string _dir2, AbstractRoad _roadOne, AbstractRoad _roadTwo)
        {
            this.point1 = _point1;
            this.point2 = _point2;
            this.dir1 = _dir1;
            this.dir2 = _dir2;
            this.roadOne = _roadOne;
            this.roadTwo = _roadTwo;

            temp1 = _roadOne.getPoint1();
            temp2 = _roadOne.getPoint2();
            temp3 = _roadTwo.getPoint1();
            temp4 = _roadTwo.getPoint2();

            if (_roadOne.Type == "Cross" && _roadTwo.Type == "Cross")
            {
                CrossandCross();
            }
            else if ((_roadOne.Type == "Cross" && (_roadTwo.Type == "Diagonal" || _roadTwo.Type == "Curved" || _roadTwo.Type == "Curved2")) || ((_roadOne.Type == "Diagonal" || _roadOne.Type == "Curved" || _roadOne.Type == "Curved2") && _roadTwo.Type == "Cross"))
            {
                CrossandOther();
            }
        }

        public void CrossandCross()
        {
            ConnectionPoint _cp = null;
            ConnectionPoint _cp2 = null;
            ConnectionPoint _cpLink = null;
            ConnectionPoint _cpLink2 = null;
            Point _diagonalbegin = new Point(0,0);
            Point _diagonalend = new Point(0, 0);
            bool _buildroad = true;
            List<ConnectionPoint> _connectedLanes = new List<ConnectionPoint>();
            int _isEven = 0;

            foreach (ConnectionPoint x in roadOne.translatedconnectPoints)
            {
                if(point1 == x.Location)
                {
                    _cp = x;
                    foreach (ConnectionPoint y in roadOne.connectPoints)
                    {
                        if (_cp.Side == y.Side && _cp.Place == y.Place)
                        {
                            _cpLink = y;
                        }
                    }
                }
            }
            foreach (ConnectionPoint x in roadTwo.translatedconnectPoints)
            {
                if (point2 == x.Location)
                {
                    _cp2 = x;
                    foreach (ConnectionPoint y in roadTwo.connectPoints)
                    {
                        if (_cp2.Side == y.Side && _cp2.Place == y.Place)
                        {
                            _cpLink2 = y;
                        }
                    }
                }
            }

            int _place, _place2, _dir, _side, _side2;
            if (_cp.Side == "Top")
            {
                _side = 0;
                _side2 = roadTwo.lanes;
            }
            else if (_cp.Side == "Bottom")
            {
                _side = roadOne.lanes;
                _side2 = 0;
            }
            else if (_cp.Side == "Left")
            {
                _side = roadOne.lanes * 2;
                _side2 = roadTwo.lanes * 3;
            }
            else
            {
                _side = roadOne.lanes * 3;
                _side2 = roadTwo.lanes * 2;
            }

            for (int t = 0; t < Math.Max(roadOne.lanes, roadTwo.lanes) && _buildroad; t++)
            {
                for (int x = 0; x <= 1; x++)
                {
                    if (t == 0 && x == 1)
                        break;

                    if (x == 0)
                        _dir = -1;
                    else
                        _dir = 1;
                    

                    _place = _cp.Place + t * _dir;
                    _place2 = _cp2.Place + t * _dir;
                    if (_place >= 1 && _place2 >= 1 && _place <= roadOne.lanes && _place2 <= roadTwo.lanes)
                    {
                        if (!(roadOne.connectPoints[_place - 1 + _side].Active && roadTwo.connectPoints[_place - 1 + _side2].Active))
                        {
                            _buildroad = false;
                        }
                        else
                        {
                            _connectedLanes.Add(roadOne.connectPoints[_place - 1 + _side]);
                            _connectedLanes.Add(roadTwo.connectPoints[_place - 1 + _side2]);
                        }
                    }
                    else if (_place >= 1 && _place <= roadOne.lanes)
                    {
                        if(roadOne.connectPoints[_place - 1 + _side].Active)
                        {
                            _buildroad = false;
                        }
                    }
                    else if (_place2 >= 1 && _place2 <= roadTwo.lanes)
                    {
                        if (roadTwo.connectPoints[_place - 1 + _side].Active)
                        {
                            _buildroad = false;
                        }
                    }
                }
            }

            if(_buildroad)
            {
                if((_connectedLanes.Count / 2) % 2 == 0)
                {
                    _isEven = -10;
                }
                else
                {
                    _isEven = 0;
                }

                if(_cp.Side == "Top")
                {
                    int _middleX = 0;
                    for(int t = 0; t < _connectedLanes.Count; t++)
                    {
                        foreach(ConnectionPoint c in roadOne.translatedconnectPoints)
                        {
                            if (c.Side == _connectedLanes[t].Side && c.Place == _connectedLanes[t].Place && c.Side == "Top")
                            {
                                _middleX += c.Location.X;
                            }
                        }
                    }
                    _middleX = _middleX / (_connectedLanes.Count / 2) + _isEven;

                    controller.BuildDiagonalRoad(new Point(_middleX, _cp.Location.Y), new Point(_middleX, _cp2.Location.Y), _connectedLanes.Count / 2, true, true, roadOne, roadTwo);
                }
                else if (_cp.Side == "Bottom")
                {
                    int _middleX = 0;
                    for (int t = 0; t < _connectedLanes.Count; t++)
                    {
                        foreach (ConnectionPoint c in roadOne.translatedconnectPoints)
                        {
                            if (c.Side == _connectedLanes[t].Side && c.Place == _connectedLanes[t].Place && c.Side == "Bottom")
                            {
                                _middleX += c.Location.X;
                            }
                        }
                    }
                    _middleX = _middleX / (_connectedLanes.Count / 2) + _isEven;

                    controller.BuildDiagonalRoad(new Point(_middleX, _cp.Location.Y), new Point(_middleX, _cp2.Location.Y), _connectedLanes.Count / 2, true, true, roadOne, roadTwo);
                }
                else if (_cp.Side == "Left")
                {
                    int _middleY = 0;
                    for(int t = 0; t < _connectedLanes.Count; t++)
                    {
                        foreach (ConnectionPoint c in roadOne.translatedconnectPoints)
                        {
                            if (c.Side == _connectedLanes[t].Side && c.Place == _connectedLanes[t].Place && c.Side == "Left")
                            {
                                _middleY += c.Location.Y;
                            }
                        }
                        _middleY += _connectedLanes[t].Location.Y;
                    }
                    _middleY = _middleY / (_connectedLanes.Count / 2) + _isEven;

                    controller.BuildDiagonalRoad(new Point(_cp.Location.X, _middleY), new Point(_cp2.Location.X, _middleY), _connectedLanes.Count / 2, true, true, roadOne, roadTwo);
                }
                else
                {
                    int _middleY = 0;
                    for (int t = 0; t < _connectedLanes.Count; t++)
                    {
                        foreach (ConnectionPoint c in roadOne.translatedconnectPoints)
                        {
                            if (c.Side == _connectedLanes[t].Side && c.Place == _connectedLanes[t].Place && c.Side == "Right")
                            {
                                _middleY += c.Location.Y;
                            }
                        }
                        _middleY += _connectedLanes[t].Location.Y;
                    }
                    _middleY = _middleY / (_connectedLanes.Count / 2) + _isEven;

                    controller.BuildDiagonalRoad(new Point(_cp.Location.X, _middleY), new Point(_cp2.Location.X, _middleY), _connectedLanes.Count / 2, true, true, roadOne, roadTwo);
                }

                int _OuterInner = 0;
                for (int t = controller.roads[controller.roads.Count - 1].lanes; t > 0 && _connectedLanes.Count > 0; t--)
                {
                    int _tracker = 0;
                    if ((_OuterInner % 2 == 0 && controller.roads[controller.roads.Count - 1].lanes % 2 == 0) || (_OuterInner % 2 == 1 && controller.roads[controller.roads.Count - 1].lanes % 2 == 1))
                    {
                        ConnectionPoint _highest = null;
                        int x = -1;
                        foreach(ConnectionPoint c in _connectedLanes)
                        {
                            x++;
                            if (_highest == null || c.Place > _highest.Place)
                            {
                                _highest = c;
                                _tracker = x;
                            }
                        }
                    }
                    else
                    {
                        ConnectionPoint _lowest = null;
                        int x = -1;
                        foreach (ConnectionPoint c in _connectedLanes)
                        {
                            x++;
                            if (_lowest == null || c.Place < _lowest.Place)
                            {
                                _lowest = c;
                                _tracker = x;
                            }
                        }
                    }

                    Lane _l = controller.roads[controller.roads.Count - 1].Drivinglanes[t - 1];
                    if (_tracker % 2 == 0)
                    {
                        foreach (CrossLane c in roadOne.Drivinglanes)
                        {
                            if (c.link.begin == _connectedLanes[_tracker])
                            {
                                foreach (ConnectionPoint _translated in roadOne.translatedconnectPoints)
                                {
                                    if (_translated.Side == c.link.begin.Side && _translated.Place == c.link.begin.Place)
                                    {
                                        if (Math.Sqrt(Math.Pow(_l.points.First().cord.X - _translated.Location.X, 2) + Math.Pow(_l.points.First().cord.Y - _translated.Location.Y, 2)) < Math.Sqrt(Math.Pow(_l.points.Last().cord.X - _translated.Location.X, 2) + Math.Pow(_l.points.Last().cord.Y - _translated.Location.Y, 2)))
                                        {
                                            _l.FlipPoints();
                                        }
                                    }
                                }
                                _l.endConnectedTo.Add(c);
                                c.beginConnectedTo.Add(_l);
                            }
                            else if (c.link.end == _connectedLanes[_tracker])
                            {
                                foreach (ConnectionPoint _translated in roadOne.translatedconnectPoints)
                                {
                                    if (_translated.Side == c.link.end.Side && _translated.Place == c.link.end.Place)
                                    {
                                        if (Math.Sqrt(Math.Pow(_l.points.Last().cord.X - _translated.Location.X, 2) + Math.Pow(_l.points.Last().cord.Y - _translated.Location.Y, 2)) < Math.Sqrt(Math.Pow(_l.points.First().cord.X - _translated.Location.X, 2) + Math.Pow(_l.points.First().cord.Y - _translated.Location.Y, 2)))
                                        {
                                            _l.FlipPoints();
                                        }
                                    }
                                }
                                _l.beginConnectedTo.Add(c);
                                c.endConnectedTo.Add(_l);
                            }
                        }
                        foreach (CrossLane c in roadTwo.Drivinglanes)
                        {
                            if (c.link.begin == _connectedLanes[_tracker + 1])
                            {
                                _l.endConnectedTo.Add(c);
                                c.beginConnectedTo.Add(_l);
                            }
                            else if (c.link.end == _connectedLanes[_tracker + 1])
                            {
                                _l.beginConnectedTo.Add(c);
                                c.endConnectedTo.Add(_l);
                            }
                        }
                        _connectedLanes.RemoveAt(_tracker + 1);
                        _connectedLanes.RemoveAt(_tracker);
                    }
                    else
                    {
                        foreach (CrossLane c in roadOne.Drivinglanes)
                        {
                            if (c.link.begin == _connectedLanes[_tracker - 1])
                            {
                                foreach (ConnectionPoint _translated in roadOne.translatedconnectPoints)
                                {
                                    if (_translated.Side == c.link.begin.Side && _translated.Place == c.link.begin.Place)
                                    {
                                        if (Math.Sqrt(Math.Pow(_l.points.First().cord.X - _translated.Location.X, 2) + Math.Pow(_l.points.First().cord.Y - _translated.Location.Y, 2)) < Math.Sqrt(Math.Pow(_l.points.Last().cord.X - _translated.Location.X, 2) + Math.Pow(_l.points.Last().cord.Y - _translated.Location.Y, 2)))
                                        {
                                            _l.FlipPoints();
                                        }
                                    }
                                }
                                _l.endConnectedTo.Add(c);
                                c.beginConnectedTo.Add(_l);
                            }
                            else if (c.link.end == _connectedLanes[_tracker - 1])
                            {
                                foreach (ConnectionPoint _translated in roadOne.translatedconnectPoints)
                                {
                                    if (_translated.Side == c.link.end.Side && _translated.Place == c.link.end.Place)
                                    {
                                        if (Math.Sqrt(Math.Pow(_l.points.Last().cord.X - _translated.Location.X, 2) + Math.Pow(_l.points.Last().cord.Y - _translated.Location.Y, 2)) < Math.Sqrt(Math.Pow(_l.points.First().cord.X - _translated.Location.X, 2) + Math.Pow(_l.points.First().cord.Y - _translated.Location.Y, 2)))
                                        {
                                            _l.FlipPoints();
                                        }
                                    }
                                }
                                _l.beginConnectedTo.Add(c);
                                c.endConnectedTo.Add(_l);
                            }
                        }
                        foreach (CrossLane c in roadTwo.Drivinglanes)
                        {
                            if (c.link.begin == _connectedLanes[_tracker])
                            {
                                _l.endConnectedTo.Add(c);
                                c.beginConnectedTo.Add(_l);
                            }
                            else if (c.link.end == _connectedLanes[_tracker])
                            {
                                _l.beginConnectedTo.Add(c);
                                c.endConnectedTo.Add(_l);
                            }
                        }
                        _connectedLanes.RemoveAt(_tracker);
                        _connectedLanes.RemoveAt(_tracker - 1);
                    }
                } 
            }
        }

        public void CrossandOther()
        {
            char _roadEnds;
            ConnectionPoint _cp = null;
            ConnectionPoint _cpLink = null;
            bool _buildroad = true;
            List<ConnectionPoint> _connectedLanes = new List<ConnectionPoint>();
            List<DrivingLane> _connectedDrivingLanes = new List<DrivingLane>();
            int _isEven = 0;
            CrossRoad _Crossroad;
            AbstractRoad _road;
            Point _Crosspoint, _roadPoint;

            if (roadOne.Type == "Diagonal" && temp1.Y == temp2.Y && roadOne.slp == 0)
            {
                _roadEnds = 'v';
            }
            else if (roadOne.Type == "Diagonal" && temp1.X == temp2.X && roadOne.slp == 0)
            {
                _roadEnds = 'h';
            }
            else if (roadTwo.Type == "Diagonal" && temp3.Y == temp4.Y && roadTwo.slp == 0)
            {
                _roadEnds = 'v';
            }
            else if (roadTwo.Type == "Diagonal" && temp3.X == temp4.X && roadTwo.slp == 0)
            {
                _roadEnds = 'h';
            }
            else if (roadOne.Type == "Diagonal" && roadOne.slp < 1 && roadOne.slp > -1)
            {
                _roadEnds = 'v';
            }
            else if (roadOne.Type == "Diagonal" && (roadOne.slp >= 1 || roadOne.slp <= -1))
            {
                _roadEnds = 'h';
            }
            else if (roadTwo.Type == "Diagonal" && roadTwo.slp < 1 && roadTwo.slp > -1)
            {
                _roadEnds = 'v';
            }
            else if (roadTwo.Type == "Diagonal" && (roadTwo.slp >= 1 || roadTwo.slp <= -1))
            {
                _roadEnds = 'h';
            }
            else if (roadOne.Type == "Curved" || roadOne.Type == "Curved2")
            {
                if (Math.Abs(temp1.X - point1.X) < Math.Abs(temp2.X - point1.X))
                {
                    _roadEnds = 'h';
                }
                else
                {
                    _roadEnds = 'v';
                }
            }
            else
            {
                if (Math.Abs(temp3.X - point2.X) < Math.Abs(temp4.X - point2.X))
                {
                    _roadEnds = 'h';
                }
                else
                {
                    _roadEnds = 'v';
                }
            }

            

            if (roadOne.Type == "Cross")
            {
                _Crossroad = (CrossRoad)roadOne;
                _Crosspoint = point1;
                _road = roadTwo;
                _roadPoint = point2;
            }
            else
            {
                _Crossroad = (CrossRoad)roadTwo;
                _Crosspoint = point2;
                _road = roadOne;
                _roadPoint = point1;
            }
            bool[] _flipLanes = new bool[_road.lanes];

            foreach (ConnectionPoint x in _Crossroad.translatedconnectPoints)
            {
                if (_Crosspoint == x.Location)
                {
                    _cp = x;
                    foreach (ConnectionPoint y in _Crossroad.connectPoints)
                    {
                        if (_cp.Side == y.Side && _cp.Place == y.Place)
                        {
                            _cpLink = y;
                        }
                    }
                }
            }
            
            if (((_cp.Side == "Top" || _cp.Side == "Bottom") && _roadEnds == 'v') || ((_cp.Side == "Left" || _cp.Side == "Right") && _roadEnds == 'h'))
                return;


            int _place, _dir, _side;

            if (_cp.Side == "Top")
            {
                _side = 0;
            }
            else if (_cp.Side == "Bottom")
            {
                _side = _Crossroad.lanes;
            }
            else if (_cp.Side == "Left")
            {
                _side = _Crossroad.lanes * 2;
            }
            else
            {
                _side = _Crossroad.lanes * 3;
            }

            bool _checkRight = true;
            bool _checkLeft = true;
            for (int t = 0; t < _Crossroad.lanes && _buildroad; t++)
            {
                if (!_checkLeft && !_checkRight)
                    break;
                for (int x = 0; x <= 1; x++)
                {
                    if ((t == 0 && x == 1) || (x == 0 && !_checkLeft) || (x == 1 && !_checkRight))
                        break;

                    if (x == 0)
                        _dir = -1;
                    else
                        _dir = 1;


                    _place = _cp.Place + t * _dir;

                    if (_place >= 1 && _place <= _Crossroad.lanes)
                    {
                        if (_Crossroad.connectPoints[_place - 1 + _side].Active)
                        {
                            foreach (ConnectionPoint c in _Crossroad.translatedconnectPoints)
                            {
                                bool _found = false;
                                if (c.Side == _Crossroad.connectPoints[_place - 1 + _side].Side && c.Place == _Crossroad.connectPoints[_place - 1 + _side].Place)
                                {
                                    int _counter = 0;
                                    foreach (DrivingLane d in _road.Drivinglanes)
                                    {
                                        if (_cp.Side == "Top" || _cp.Side == "Bottom")
                                        {
                                            if (Math.Abs(d.points.First().cord.Y - _roadPoint.Y) < Math.Abs(d.points.Last().cord.Y - _roadPoint.Y))
                                            {
                                                if (c.Location.X == d.points.First().cord.X)
                                                {
                                                    foreach (CrossLane crosslane in _Crossroad.Drivinglanes)
                                                    {
                                                        if (crosslane.link.begin == _Crossroad.connectPoints[_place - 1 + _side])
                                                        {
                                                            _flipLanes[_counter] = true;
                                                            d.beginConnectedTo.Add(crosslane);
                                                        }
                                                        else if (crosslane.link.end == _Crossroad.connectPoints[_place - 1 + _side])
                                                        {
                                                            _flipLanes[_counter] = false;
                                                            d.beginConnectedTo.Add(crosslane);
                                                        }
                                                    }
                                                    _connectedLanes.Add(_Crossroad.connectPoints[_place - 1 + _side]);
                                                    _connectedDrivingLanes.Add(d);
                                                    _found = true;
                                                }
                                            }
                                            else
                                            {
                                                if (c.Location.X == d.points.Last().cord.X)
                                                {
                                                    foreach (CrossLane crosslane in _Crossroad.Drivinglanes)
                                                    {
                                                        if (crosslane.link.begin == _Crossroad.connectPoints[_place - 1 + _side])
                                                        {
                                                            _flipLanes[_counter] = false;
                                                            d.endConnectedTo.Add(crosslane);
                                                        }
                                                        else if (crosslane.link.end == _Crossroad.connectPoints[_place - 1 + _side])
                                                        {
                                                            _flipLanes[_counter] = true;
                                                            d.endConnectedTo.Add(crosslane);
                                                        }
                                                    }
                                                    _connectedLanes.Add(_Crossroad.connectPoints[_place - 1 + _side]);
                                                    _connectedDrivingLanes.Add(d);
                                                    _found = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Math.Abs(d.points.First().cord.X - _roadPoint.X) < Math.Abs(d.points.Last().cord.X - _roadPoint.X))
                                            {
                                                if (c.Location.Y == d.points.First().cord.Y)
                                                {
                                                    foreach (CrossLane crosslane in _Crossroad.Drivinglanes)
                                                    {
                                                        if (crosslane.link.begin == _Crossroad.connectPoints[_place - 1 + _side])
                                                        {
                                                            _flipLanes[_counter] = true;
                                                            d.beginConnectedTo.Add(crosslane);
                                                        }
                                                        else if (crosslane.link.end == _Crossroad.connectPoints[_place - 1 + _side])
                                                        {
                                                            _flipLanes[_counter] = false;
                                                            d.beginConnectedTo.Add(crosslane);
                                                        }
                                                    }
                                                    _connectedLanes.Add(_Crossroad.connectPoints[_place - 1 + _side]);
                                                    _connectedDrivingLanes.Add(d);
                                                    _found = true;
                                                }
                                            }
                                            else
                                            {
                                                if (c.Location.Y == d.points.Last().cord.Y)
                                                {
                                                    foreach (CrossLane crosslane in _Crossroad.Drivinglanes)
                                                    {
                                                        if (crosslane.link.begin == _Crossroad.connectPoints[_place - 1 + _side])
                                                        {
                                                            _flipLanes[_counter] = false;
                                                            d.endConnectedTo.Add(crosslane);
                                                        }
                                                        else if (crosslane.link.end == _Crossroad.connectPoints[_place - 1 + _side])
                                                        {
                                                            _flipLanes[_counter] = true;
                                                            d.endConnectedTo.Add(crosslane);
                                                        }
                                                    }
                                                    _connectedLanes.Add(_Crossroad.connectPoints[_place - 1 + _side]);
                                                    _connectedDrivingLanes.Add(d);
                                                    _found = true;
                                                }
                                            }
                                        }
                                        _counter++;
                                    }

                                }
                                else
                                    _found = true;

                                if (!_found)
                                {
                                    _buildroad = false;
                                }
                            }
                        }
                        else if (x == 1)
                            _checkRight = false;
                        else if (x == 0)
                            _checkLeft = false;
                    }
                }
            }
            if (_road.lanes > _connectedLanes.Count)
                _buildroad = false;

            if (_buildroad)
            {
                if (_connectedLanes.Count % 2 == 0)
                {
                    _isEven = -10;
                }
                else
                {
                    _isEven = 0;
                }

                controller.roads.Remove(_road);
                int _BottomorRight = 0;
                if (_cp.Side == "Top" || _cp.Side == "Bottom")
                {
                    int _middleX = 0;
                    foreach (ConnectionPoint c in _connectedLanes)
                    {
                        foreach (ConnectionPoint transcp in _Crossroad.translatedconnectPoints)
                        {
                            if (transcp.Side == c.Side && transcp.Place == c.Place)
                                _middleX += transcp.Location.X;
                        }
                    }
                    _middleX = (_middleX / _connectedLanes.Count) + _isEven;

                    if (_cp.Side == "Bottom")
                        _BottomorRight = 1;

                    if (_roadPoint == _road.point1)
                    {
                        if (_road.Type == "Diagonal")
                        {
                            controller.BuildDiagonalRoad(new Point(_middleX, _Crosspoint.Y - _BottomorRight), _road.point2, _connectedLanes.Count, true, _road.endconnection, _Crossroad, _road.endConnectedTo);
                        }
                        else
                        {
                            if (_road.Dir == "NE" || _road.Dir == "SW")
                            {
                                _middleX += 20;
                            }
                            controller.BuildCurvedRoad(new Point(_middleX, _Crosspoint.Y - _BottomorRight), _road.point2, _connectedLanes.Count, _road.Type, true, _road.endconnection, _Crossroad, _road.endConnectedTo);
                        }
                    }
                    else
                    {
                        if (_road.Type == "Diagonal")
                            controller.BuildDiagonalRoad(_road.point1, new Point(_middleX, _Crosspoint.Y - _BottomorRight), _connectedLanes.Count, _road.beginconnection, true, _road.beginConnectedTo, _Crossroad);
                        else
                        {
                            if (_road.Dir == "NE" || _road.Dir == "SW")
                                {
                                _middleX += 20;
                                }
                            controller.BuildCurvedRoad(_road.point1, new Point(_middleX, _Crosspoint.Y - _BottomorRight), _connectedLanes.Count, _road.Type, _road.beginconnection, true, _road.beginConnectedTo, _Crossroad);
                        }
                    }
                }
                
                else
                {
                    int _middleY = 0;
                    foreach (ConnectionPoint c in _connectedLanes)
                    {
                        foreach (ConnectionPoint transcp in _Crossroad.translatedconnectPoints)
                        {
                            if (transcp.Side == c.Side && transcp.Place == c.Place)
                                _middleY += transcp.Location.Y;
                        }
                    }
                    _middleY = _middleY / _connectedLanes.Count + _isEven;

                    if (_cp.Side == "Right")
                        _BottomorRight = 1;

                    if (_roadPoint == _road.point1)
                    {
                        if (_road.Type == "Diagonal")
                            controller.BuildDiagonalRoad(new Point(_Crosspoint.X - _BottomorRight, _middleY), _road.point2, _connectedLanes.Count, true, _road.endconnection, _Crossroad, _road.endConnectedTo);
                        else
                            controller.BuildCurvedRoad(new Point(_Crosspoint.X - _BottomorRight, _middleY), _road.point2, _connectedLanes.Count, _road.Type, true, _road.endconnection, _Crossroad, _road.endConnectedTo);
                    }
                    else
                    {
                        if (_road.Type == "Diagonal")
                            controller.BuildDiagonalRoad(_road.point1, new Point(_Crosspoint.X - _BottomorRight, _middleY), _connectedLanes.Count, _road.beginconnection, true, _road.beginConnectedTo, _Crossroad);
                        else
                            controller.BuildCurvedRoad(_road.point1, new Point(_Crosspoint.X - _BottomorRight, _middleY), _connectedLanes.Count, _road.Type, _road.beginconnection, true, _road.beginConnectedTo, _Crossroad);
                    }
                }

                for (int t = 0; t < _flipLanes.Length; t++)
                {
                    if (_flipLanes[t])
                        controller.roads.Last().Drivinglanes[t].FlipPoints();
                }
                for (int t = 0; t < _connectedLanes.Count; t++)
                {
                    foreach (CrossLane c in _Crossroad.Drivinglanes)
                    {
                        if (_connectedLanes[t].Side == c.link.begin.Side && _connectedLanes[t].Place == c.link.begin.Place)
                        {
                            foreach(DrivingLane d in controller.roads.Last().Drivinglanes)
                            {
                                if(d.thisLane == _connectedDrivingLanes[t].thisLane)
                                {
                                    d.beginConnectedTo = _connectedDrivingLanes[t].beginConnectedTo;
                                    d.endConnectedTo = _connectedDrivingLanes[t].endConnectedTo;
                                    c.beginConnectedTo.Add(d);
                                }
                            }
                        }
                        else if(_connectedLanes[t].Side == c.link.end.Side && _connectedLanes[t].Place == c.link.end.Place)
                        {
                            foreach (DrivingLane d in controller.roads.Last().Drivinglanes)
                            {
                                if (d.thisLane == _connectedDrivingLanes[t].thisLane)
                                {
                                    d.beginConnectedTo = _connectedDrivingLanes[t].beginConnectedTo;
                                    d.endConnectedTo = _connectedDrivingLanes[t].endConnectedTo;
                                    c.endConnectedTo.Add(d);
                                }
                            }
                        }
                    }
                }
            }

            else
            {
                if (_roadPoint == _road.point1)
                {
                    foreach (DrivingLane d in _road.Drivinglanes)
                    {
                        d.beginConnectedTo.Clear();
                    }
                    foreach (CrossLane crosslane in _Crossroad.Drivinglanes)
                    {
                        foreach (ConnectionPoint c in _connectedLanes)
                        {
                            if (crosslane.link.begin.Side == c.Side && crosslane.link.begin.Place == c.Place)
                                crosslane.beginConnectedTo.Clear();
                            else if (crosslane.link.end.Side == c.Side && crosslane.link.end.Place == c.Place)
                                crosslane.endConnectedTo.Clear();
                        }
                    }
                }
                else
                {
                    foreach (DrivingLane d in _road.Drivinglanes)
                    {
                        d.endConnectedTo.Clear();
                    }
                    foreach (CrossLane crosslane in _Crossroad.Drivinglanes)
                    {
                        foreach (ConnectionPoint c in _connectedLanes)
                        {
                            if (crosslane.link.begin.Side == c.Side && crosslane.link.begin.Place == c.Place)
                                crosslane.beginConnectedTo.Clear();
                            else if (crosslane.link.end.Side == c.Side && crosslane.link.end.Place == c.Place)
                                crosslane.endConnectedTo.Clear();
                        }
                    }
                }
            }

        }
    }
}
