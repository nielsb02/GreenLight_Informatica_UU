﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

//This is the controller that deals with everything simulation related, and holds the vehicle and AI controller

namespace GreenLight
{
    public class SimulationController : AbstractController
    {
        public VehicleController vehicleController;
        public AIController aiController;
        public WorldController worldController;

        public SimulationController()
        {
            this.vehicleController = new VehicleController();
            this.aiController = new AIController();
            this.worldController = new WorldController();
            this.worldController.Initialize();
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
