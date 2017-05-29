using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowScrape.Types;
using System.Web.Script.Serialization;

namespace UI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("You are about to save all new positions.",
                                     "Confirm Save",
                                     MessageBoxButtons.OKCancel);
            if(confirmResult == DialogResult.Cancel)
            {
                return;
            }

            List<WindowModel> models = new List<WindowModel>();
            List<HwndObject> windows = WindowScrape.Types.HwndObject.GetWindows();
            Dictionary<string, string> uniqueTitles = new Dictionary<string, string>();
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.AppendLine("[");

            foreach(HwndObject window in windows)
            {

                Size size = window.Size;
                string title = window.Title;

                if(window.Visible && title != "" && !uniqueTitles.ContainsKey(title))
                {
                    WindowModel model = new WindowModel() { Dimensions = size, Location = window.Location, Identifier = "*" + title };

                    models.Add(model);
                    uniqueTitles[title] = title;
                }
            }
            
            for(int i = 0; i < models.Count; i++)
            {
                var json = new JavaScriptSerializer().Serialize(models[i]);

                jsonBuilder.AppendLine(json + ((i < (models.Count - 1)) ? "," : ""));
            }

            jsonBuilder.AppendLine("]");
            System.IO.File.WriteAllText("EasyDock.json", jsonBuilder.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string jsonString = System.IO.File.ReadAllText("EasyDock.json");
            List<WindowModel> models = new JavaScriptSerializer().Deserialize<List<WindowModel>>(jsonString);

            List<HwndObject> windows = WindowScrape.Types.HwndObject.GetWindows();
            foreach (HwndObject window in windows)
            {
                foreach (WindowModel model in models)
                {
                    string identifier = model.Identifier.Substring(1);
                    bool isEquals = (model.Identifier[0] == '=');
                    bool shouldSet = false;
                    if (isEquals)
                    {
                        if(identifier == window.Title)
                        {
                            shouldSet = true;
                        }
                    }
                    else
                    {
                        if (window.Title.Contains(identifier))
                        {
                            shouldSet = true;
                        }
                    }

                    if(shouldSet)
                    {
                        //window.RestoreWindow();
                        window.Location = model.Location;
                        window.Size = model.Dimensions;
                    }
                }
            }

            
        }
    }
}
