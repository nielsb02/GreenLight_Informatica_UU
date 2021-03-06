using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GreenLight
{
    // This is the StraightRoad class, we created it in an early stage of our project to create horizontal and vertical roads
    // This class is not used anymore since we decided the DiagonalRoad class could take care of horizontal and vertical roads without to much added complexity

    public class StraightRoad : AbstractRoad
    {
        private string dir;

        private int roadwidth = 10;

        public StraightRoad(Point _point1, Point _point2, int _lanes, string _dir, string _type, bool _beginconnection, bool _endconnection, AbstractRoad _beginConnectedTo, AbstractRoad _endConnectedTo) : base(_point1, _point2, _lanes, "StraightRoad", _beginconnection, _endconnection, _beginConnectedTo, _endConnectedTo)
        {

            this.Type = _type;

            Point[] _points = hitBoxPoints(_point1, _point2);
            this.hitbox = new RectHitbox(_points[1], _points[0], _points[3], _points[2], Color.Yellow);

            this.dir = _dir;

            for(int x = 1; x <= this.lanes; x++)
            {
                this.Drivinglanes.Add(CalculateLanes(point1, point2, x));
            }
        }

        protected static DrivingLane CalculateDrivingLane(Point _point1, Point _point2, int _thisLane, StraightRoad _road)
        {
            List<LanePoints> _lanePoints = new List<LanePoints>();
            Point _normpoint1 = _point1; Point _normpoint2 = _point2;

            Tuple<int, int> _dir = _road.GetDirection(_normpoint1, _normpoint2);
            Point _prev = _normpoint1;
            int temp = 0;

            while (Math.Abs(_normpoint1.X - _normpoint2.X) > 0 || Math.Abs(_normpoint1.Y - _normpoint2.Y) > 0)
            {
                temp++;

                _normpoint1 = new Point(_normpoint1.X - _dir.Item1, _normpoint1.Y - _dir.Item2);

                _lanePoints.Add(new LanePoints(_normpoint1, RoadMath.CalculateAngle(_prev, _normpoint1)));

                _prev = _normpoint1;

                if (temp > 500)
                {
                    break;
                }
            }

            foreach (LanePoints x in _lanePoints)
            {
                Log.Write(x.ToString());
            }

            return new DrivingLane(_lanePoints, _road.dir, _road.lanes, _thisLane, null);

        }

        private Tuple<int,int> GetDirection(Point _point1, Point _point2)
        {
            int dirx = 0; int diry = 0;

            if (_point1.X < _point2.X)
            {
                dirx = -1;
            }
            else if (_point1.X > _point2.X)
            {
                dirx = 1;
            }
            if (_point1.Y < _point2.Y)
            {
                diry = -1;
            }
            else if (_point1.Y > _point2.Y)
            {
                diry = 1;
            }

            return Tuple.Create(dirx, diry);
        }

        private DrivingLane CalculateLanes(Point _firstPoint, Point _secondPoint, int t)
        {
            string _Direction = this.dir;
            int drivingLaneDistance = 20;

            if (_Direction == "E" || _Direction == "W")
            {
                if (lanes % 2 == 0)
                {
                    if (t % 2 == 0)
                    {
                        _firstPoint.Y += (t / 2 - 1) * drivingLaneDistance + drivingLaneDistance / 2;
                        _secondPoint.Y += (t / 2 - 1) * drivingLaneDistance + drivingLaneDistance / 2;
                    }
                    else
                    {
                        _firstPoint.Y -= (t - 1) / 2 * drivingLaneDistance + drivingLaneDistance / 2;
                        _secondPoint.Y -= (t - 1) / 2 * drivingLaneDistance + drivingLaneDistance / 2;
                    }
                }
                else // (lanes % 2 == 1)
                {
                    if (t % 2 == 0)
                    {
                        _firstPoint.Y -= t / 2 * drivingLaneDistance;
                        _secondPoint.Y -= t / 2 * drivingLaneDistance;
                    }
                    else if (t % 2 == 1 && t != 1)
                    {
                        _firstPoint.Y += (t - 1) / 2 * drivingLaneDistance;
                        _secondPoint.Y += (t - 1) / 2 * drivingLaneDistance;
                    }
                }
            }
            else if (_Direction == "N" || _Direction == "S")
            {
                if (lanes % 2 == 0)
                {
                    if (t % 2 == 0)
                    {
                        _firstPoint.X += (t / 2 - 1) * drivingLaneDistance + drivingLaneDistance / 2;
                        _secondPoint.X += (t / 2 - 1) * drivingLaneDistance + drivingLaneDistance / 2;
                    }
                    else
                    {
                        _firstPoint.X -= (t - 1) / 2 * drivingLaneDistance + drivingLaneDistance / 2;
                        _secondPoint.X -= (t - 1) / 2 * drivingLaneDistance + drivingLaneDistance / 2;
                    }
                }
                else // (lanes % 2 == 1)
                {
                    if (t % 2 == 0)
                    {
                        _firstPoint.X -= t / 2 * drivingLaneDistance;
                        _secondPoint.X -= t / 2 * drivingLaneDistance;
                    }
                    else if (t % 2 == 1 && t != 1)
                    {
                        _firstPoint.X += (t - 1) / 2 * drivingLaneDistance;
                        _secondPoint.X += (t - 1) / 2 * drivingLaneDistance;
                    }
                }
            }
            return CalculateDrivingLane(_firstPoint, _secondPoint, t, this);
        }

        private Point[] hitBoxPoints(Point one, Point two)
        {
            Point _one, _two;
            int _laneWidth = Roads.Config.laneWidth;
            int _roadWidth = (_laneWidth * this.lanes) / 2;

            if (one.Y <= two.Y)
            {
                _one = one;
                _two = two;
            }
            else
            {
                _two = one;
                _one = two;
            }

            Point[] _points = new Point[4];
            int _angle;

            float xDiff = _one.X - _two.X;
            float yDiff = _one.Y - _two.Y;
            _angle = (int)(Math.Atan2(yDiff, xDiff) * (180 / Math.PI));
            _angle = Math.Abs(_angle);


            if (_angle >= 45 && (_angle < 135 || _angle > 180))
            {
                _points[0] = new Point(_one.X + _roadWidth, _one.Y);
                _points[1] = new Point(_one.X - _roadWidth, _one.Y); 
                _points[2] = new Point(_two.X + _roadWidth, _two.Y); 
                _points[3] = new Point(_two.X - _roadWidth, _two.Y);
            }
            else
            {

                _points[1] = new Point(_one.X, _one.Y - _roadWidth);
                _points[2] = new Point(_two.X, _two.Y + _roadWidth); 

                if (_one.Y + _roadWidth <= _two.Y - _roadWidth)
                {
                    _points[0] = new Point(_one.X, _one.Y + _roadWidth);
                    _points[3] = new Point(_two.X, _two.Y - _roadWidth);
                }
                else
                {
                    _points[3] = new Point(_one.X, _one.Y + _roadWidth);
                    _points[0] = new Point(_two.X, _two.Y - _roadWidth);
                }
            }

            return _points;
        }

        

        public override Hitbox CreateHitbox(Point[] _array)
        {
            throw new NotImplementedException();
        }
    }
}
