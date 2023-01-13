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

using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.UZBScript
{
	public partial class UZBScriptErrorForm : DelayedForm
	{
		#region ================== Constructors

		public UZBScriptErrorForm(string message, string stacktrace, string internalstacktrace)
		{
			InitializeComponent();

			tbStackTrace.Text = message + "\r\n" + stacktrace;
			tbStackTrace.Select(0, 0);

			tbInternalStackTrace.Text = internalstacktrace;
			tbInternalStackTrace.Select(0, 0);

			if (string.IsNullOrWhiteSpace(stacktrace))
				tabControl1.SelectedIndex = 1;
		}

		#endregion
	}
}
