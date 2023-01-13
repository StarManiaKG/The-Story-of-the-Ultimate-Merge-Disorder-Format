#region ================== Copyright (c) 2022 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

#region ================== Namespaces

using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal partial class ToastControl : UserControl
	{
		#region ================== Variables

		private long startime;
		private long lifetime;
		private bool pausedecay;
		private bool remove;

		#endregion

		#region ================== Constructors

		public ToastControl(ToastType type, string title, string text, long lifetime = 3000)
		{
			InitializeComponent();

			this.lifetime = lifetime;
			startime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

			// Set icon
			if(type == ToastType.INFO)
				icon.BackgroundImage = SystemIcons.Information.ToBitmap();
			else if(type == ToastType.WARNING)
				icon.BackgroundImage = SystemIcons.Warning.ToBitmap();
			else if(type == ToastType.ERROR)
				icon.BackgroundImage = SystemIcons.Error.ToBitmap();

			lbTitle.Text = title;
			lbText.Text = text;

			// The text label is auto-size, but we need to programatically set a max width so that longer texts are
			// automatically broken into multiple lines
			lbText.MaximumSize = new Size(Width - lbText.Location.X - Margin.Right, lbText.MaximumSize.Height);

			// Resize the height of the control if the text doesn't fit vertically
			if (lbText.Location.Y + lbText.Height + Margin.Bottom > Height)
				Height = lbText.Location.Y + lbText.Height + lbTitle.Location.Y + Margin.Bottom;

			pausedecay = false;
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Checks if the toast is decaying, i.e. the cursor is currently not inside the control.
		/// </summary>
		public void CheckDecay()
		{
			if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
			{
				pausedecay = true;
			}
			else if(pausedecay)
			{
				pausedecay = false;

				// Reset the start time, so that the control will only die "lifetime" ms after the cursor left the control
				startime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			}
		}

		/// <summary>
		/// Checks if the control is still "alive" (has not reached its lifetime).
		/// </summary>
		/// <returns>true if it's alive, false if it isn't</returns>
		public bool IsAlive()
		{
			if (remove || (!pausedecay && DateTimeOffset.Now.ToUnixTimeMilliseconds() - startime > lifetime))
				return false;

			return true;
		}

		/// <summary>
		/// Sets the toast to be removed.
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The event arguments</param>
		private void btnClose_Click(object sender, EventArgs e)
		{
			remove = true;
		}

		#endregion
	}
}
