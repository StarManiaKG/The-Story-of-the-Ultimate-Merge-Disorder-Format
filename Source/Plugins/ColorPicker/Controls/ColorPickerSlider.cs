using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.ColorPicker.Controls {
	public partial class ColorPickerSlider : UserControl {

		private bool blockEvents;
		public event EventHandler<ColorPickerSliderEventArgs> OnValueChanged;

		public int Value 
		{ 
			get { return (int)numericUpDown1.Value; }
			set 
			{
				blockEvents = true;
				numericUpDown1.Value = General.Clamp(value, (int)numericUpDown1.Minimum, (int)numericUpDown1.Maximum);
				blockEvents = false;
			}
		}

		private bool showLimits;
		public bool ShowLimits 
		{ 
			get { return showLimits; }
			set 
			{
				showLimits = value;
				labelMin.Visible = showLimits;
				labelMax.Visible = showLimits;
			}
		}

		public string Label { set { label1.Text = value; } }
		
		public ColorPickerSlider() 
		{
			InitializeComponent();

			trackBar1.Visible = false;
			ShowLimits = false;
		}

		public void SetLimits(int tbMin, int tbMax, int nudMin, int nudMax) 
		{
			bool blockEventsStatus = blockEvents;
			blockEvents = true;

			trackBar1.Value = General.Clamp(trackBar1.Value, tbMin, tbMax);
			trackBar1.Minimum = tbMin;
			trackBar1.Maximum = tbMax;

			labelMin.Text = tbMin.ToString();
			labelMax.Text = tbMax.ToString();

			numericUpDown1.Value = General.Clamp((int)numericUpDown1.Value, nudMin, nudMax);
			numericUpDown1.Minimum = nudMin;
			numericUpDown1.Maximum = nudMax;

			blockEvents = blockEventsStatus;
		}

		public void UseSlider(bool use)
		{
			ShowLimits = use;
			trackBar1.Visible = use;
			button1.Visible = !use;
			button2.Visible = !use;
			button3.Visible = !use;
			button4.Visible = !use;
		}

//events
		private void trackBar1_ValueChanged(object sender, EventArgs e) 
		{
			numericUpDown1.Value = ((TrackBar)sender).Value;
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e) 
		{
			bool blockEventsStatus = blockEvents;
			
			int val = (int)((NumericUpDown)sender).Value;

			if(!blockEventsStatus) 
			{
				EventHandler<ColorPickerSliderEventArgs> handler = OnValueChanged;
				if(handler != null) handler(this, new ColorPickerSliderEventArgs(val));
			}

			blockEvents = true;
			trackBar1.Value = General.Clamp(val, trackBar1.Minimum, trackBar1.Maximum); //clamp it!
			blockEvents = blockEventsStatus;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			numericUpDown1.Value = 64;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			numericUpDown1.Value = 128;
		}

		private void button3_Click(object sender, EventArgs e)
		{
			numericUpDown1.Value = 256;
		}

		private void button4_Click(object sender, EventArgs e)
		{
			numericUpDown1.Value = 512;
		}
	}
}
