using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;


namespace GreenLight
{
    //This is the InterfaceController that handles everything interface related, it holds all the different menus that can be opened.

    public class InterfaceController : AbstractController
    {
        public Form MainForm;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]

        private static extern IntPtr CreateRoundRectRgn(
                int left,
                int top,
                int right,
                int bottom,
                int width,
                int height
                );

        StartSubMenu SSM;
        StartMainMenu SMM;
        public BuildSubMenu BSM;
        SimulationSubMenu SimSM;
        SimulationMainMenu SimMM;
        SimulationCarSpawnRateSubMenu SimSWM;
        public SimulationSubVehicleMenu SimSVM;
        public SimulationSubDriverMenu SimSDM;
        public SimulationDataMenu SimDataM;
        ElementsSubSettingsMenu ElemSBM;
        public ElementsSubRoadsMenu ElemSRM;
        ElementsSubSignsMenu ElemSSM;
        DataSubMenu DataSM;
        public StartSubRecentProjectsMenu SSRPM;

        int Sub_menu_width = 250;
        FontFamily Dosis_font_family;
        
        string[] Recent_projects = File.ReadAllLines(General_Form.Main.recent_project);
       

        public InterfaceController(Form _form)
        {
            this.MainForm = _form;
            Recent_projects.Count();
        }


        public override void Initialize()
        {

            MainForm.FormBorderStyle = FormBorderStyle.None;
            MainForm.StartPosition = FormStartPosition.CenterScreen;
            MainForm.Size = new Size(1200, 600);
            Refresh_region(MainForm);
            MainForm.MinimumSize = new Size(800, 400);
            MainForm.Icon = new Icon("../../src/User Interface Recources/Logo.ico");

            PrivateFontCollection Font_collection = new PrivateFontCollection();
            Font_collection.AddFontFile("../../Fonts/Dosis-bold.ttf");
            Dosis_font_family = Font_collection.Families[0];

            MainForm.SizeChanged += (object o, EventArgs EA) => { Size_adjust(); };

            SSM = new StartSubMenu(Sub_menu_width, MainForm, Dosis_font_family);
            SSRPM = new StartSubRecentProjectsMenu(Sub_menu_width, MainForm, Dosis_font_family);
            SMM = new StartMainMenu(MainForm.Width - Sub_menu_width, MainForm, Dosis_font_family);

            BSM = new BuildSubMenu(Sub_menu_width, MainForm, Dosis_font_family);

            SimDataM = new SimulationDataMenu(Sub_menu_width, MainForm, 30, Dosis_font_family);

            SimSM = new SimulationSubMenu(Sub_menu_width, MainForm, Dosis_font_family);
            SimMM = new SimulationMainMenu(MainForm.Width - Sub_menu_width, MainForm, Dosis_font_family);
            SimSWM = new SimulationCarSpawnRateSubMenu(Sub_menu_width, MainForm, Dosis_font_family);
            SimSVM = new SimulationSubVehicleMenu(Sub_menu_width, MainForm, Dosis_font_family);
            SimSDM = new SimulationSubDriverMenu(Sub_menu_width, MainForm, Dosis_font_family);

            ElemSBM = new ElementsSubSettingsMenu(Sub_menu_width, MainForm, Dosis_font_family);
            ElemSRM = new ElementsSubRoadsMenu(Sub_menu_width, MainForm, Dosis_font_family);
            ElemSSM = new ElementsSubSignsMenu(Sub_menu_width, MainForm, Dosis_font_family);

            DataSM = new DataSubMenu(Sub_menu_width, MainForm);

            MainForm.Controls.Add(SMM);
            MainForm.Controls.Add(SSRPM);
            MainForm.Controls.Add(SSM);
            MainForm.Controls.Add(BSM);
            MainForm.Controls.Add(SimDataM);
            MainForm.Controls.Add(SimMM);
            MainForm.Controls.Add(SimSM);
            MainForm.Controls.Add(SimSWM);
            MainForm.Controls.Add(SimSVM);
            MainForm.Controls.Add(SimSDM);
            MainForm.Controls.Add(ElemSBM);
            MainForm.Controls.Add(ElemSRM);
            MainForm.Controls.Add(ElemSSM);
            MainForm.Controls.Add(DataSM);
        }

        public void Refresh_region(Form Form)
        {
            if (MainForm.WindowState == FormWindowState.Normal)
                Form.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, MainForm.ClientSize.Width, MainForm.ClientSize.Height, 50, 50));
            else Form.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, MainForm.ClientSize.Width, MainForm.ClientSize.Height, 0, 0));
        }

        public void Size_adjust()
        {
            SMM.Size_adjust(MainForm, Sub_menu_width);
            SSRPM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
            SSM.Size_adjust(MainForm, Sub_menu_width, Dosis_font_family);
            BSM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
            SimDataM.AdjustSize(MainForm, Sub_menu_width, 30);
            SimMM.AdjustSize(MainForm, Sub_menu_width);
            SimSM.Size_adjust(MainForm, Sub_menu_width, Dosis_font_family);
            SimSWM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
            SimSVM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
            SimSDM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
            ElemSRM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
            ElemSBM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
            ElemSSM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
            DataSM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
        }

        public void Size_adjust_SSRPM()
        {
            SSRPM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
        }

        public void Open(string File_name)
        {
            StreamReader Open = new StreamReader(File_name);
        }

        public void Menu_to_start() 
        {
            Hide_all_menus();
            SSRPM.AdjustSize(MainForm, Sub_menu_width, Dosis_font_family);
            SSM.Show();
            SMM.Show();
            SSRPM.Show();
            SSRPM.BringToFront();
        }
        public void Menu_to_build() 
        {
            Hide_all_menus();
            ResetAllButtons(BSM.roadButton, BSM.roadButton.Image_path);
            BSM.Show();
        }
        public void Menu_to_simulation() 
        {
            Hide_all_menus();
            ResetAllButtons(SimSM.Weather, SimSM.Weather.Image_path);
            SimSM.Show();
            SimMM.Show();
            SimDataM.Show();
            SimDataM.BringToFront();
            
        }
        public void Menu_to_simulation_weather() 
        {
            SimSVM.Hide();
            SimSDM.Hide();
            SimSWM.Show();
            SimSWM.BringToFront();
        }
        public void Menu_to_simulation_vehicle() 
        {
            SimSWM.Hide();
            SimSDM.Hide();
            SimSVM.Show();
            SimSVM.BringToFront();
        }
        public void Menu_to_simulation_driver() 
        {
            SimSWM.Hide();
            SimSVM.Hide();
            SimSDM.Show();
            SimSDM.BringToFront();
        }
        public void Menu_to_roads()
        {
            ElemSBM.Hide();
            ElemSSM.Hide();
            ElemSRM.Show();
            ElemSRM.BringToFront();
        }
        public void Menu_to_signs()
        {
            ElemSBM.Hide();
            ElemSRM.Hide();
            ElemSSM.Show();
            ElemSSM.BringToFront();
        }

        public void Menu_to_buildings()
        {
            ElemSSM.Hide();
            ElemSRM.Hide();
            ElemSBM.Show();
            ElemSBM.BringToFront();
        }
        public void Menu_to_data()
        {
            Hide_all_menus();
            DataSM.Show();
            SimMM.Show();
            SimDataM.Show();
            SimDataM.BringToFront();
            DataSM.BringToFront();

        }
        private void Hide_all_menus()
        {
            SSM.Hide();
            SSRPM.Hide();
            SMM.Hide();
            BSM.Hide();
            SimDataM.Hide();
            SimSM.Hide();
            SimMM.Hide();
            SimSWM.Hide();
            SimSVM.Hide();
            SimSDM.Hide();
            ElemSSM.Hide();
            ElemSRM.Hide();
            ElemSBM.Hide();
            DataSM.Hide();
        }

        public void ResetAllButtons(CurvedButtons Selected, string Filepath)
        {
            foreach (CurvedButtons x in BSM.bsmButtons)
            {
                x.Selected = false;
                x.Image = Image.FromFile(x.Image_path.Remove(x.Image_path.Length - 10) + "Button.png");
            }
            foreach (CurvedButtons x in ElemSSM.essmButtons)
            {
                x.Selected = false;
                x.Image = Image.FromFile(x.Image_path.Remove(x.Image_path.Length - 10) + "Button.png");
            }
            foreach (CurvedButtons x in ElemSRM.esrmButtons)
            {
                x.Selected = false;
                x.Image = Image.FromFile(x.Image_path.Remove(x.Image_path.Length - 10) + "Button.png");
            }
            foreach (CurvedButtons x in SimSM.ssmButtons)
            {
                x.Selected = false;
                x.Image = Image.FromFile(x.Image_path.Remove(x.Image_path.Length - 10) + "Button.png");
            }
            try
            {
                Selected.Selected = true;
                Selected.Image = Image.FromFile(Filepath.Remove(Filepath.Length - 10) + "Select.png");
            }
            catch (Exception e) { }
        }
    }
}
