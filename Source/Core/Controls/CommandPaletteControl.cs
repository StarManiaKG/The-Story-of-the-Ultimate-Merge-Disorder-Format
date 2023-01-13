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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	public partial class CommandPaletteControl : UserControl
	{
		#region ================== Constants

		private const int MAX_ITEMS = 20;
		private const int MAX_RECENT_ACTIONS = 5;
		private const int GROUP_RECENT = 0;
		private const int GROUP_USABLE = 1;
		private const int GROUP_UNUSABLE = 2;


		#endregion

		#region ================== Variables

		private readonly List<Actions.Action> recentactions;

		#endregion

		#region ================== Constructor

		public CommandPaletteControl()
		{
			InitializeComponent();

			recentactions = new List<Actions.Action>();

			Enabled = false;
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Hides the palette. Disabled it and sends it to the background.
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The event args</param>
		private void HidePalette(object sender, EventArgs e)
		{
			commandsearch.LostFocus -= HidePalette;
			Enabled = false;

			if (Parent is MainForm mf)
			{
				mf.Resize -= Reposition;

				mf.Controls.SetChildIndex(this, 0xffff);
				mf.ActiveControl = null;
				mf.Focus();
			}
		}

		/// <summary>
		/// Sets the color of the currently selected item.
		/// </summary>
		private void HighlightSelectedItem()
		{
			if (commandlist.SelectedItems.Count > 0)
			{
				commandlist.SelectedItems[0].BackColor = SystemColors.Highlight;
				commandlist.SelectedItems[0].ForeColor = SystemColors.HighlightText;
			}
		}

		/// <summary>
		/// Shows the palette
		/// </summary>
		public void MakeVisible()
		{
			if (Parent is MainForm mf)
			{
				// Reset everything to a blank slate
				commandsearch.Text = string.Empty;
				//				commandsearch_TextChanged(this, EventArgs.Empty);
				FillCommandList(withrecent: true);
				HighlightSelectedItem();

				// Set the width of each column to the max width of its fields
				commandlist.Columns[0].Width = -1;
				commandlist.Columns[1].Width = -1;
				commandlist.Columns[2].Width = -1;

				// Compute the new width. It's the width of the columns, the vertical scroll bar and some buffer
				Width = commandlist.Columns[0].Width + commandlist.Columns[1].Width + commandlist.Columns[2].Width + SystemInformation.VerticalScrollBarWidth + commandlist.Location.X * 4;

				// Center the control at the top middle
				Location = new Point(mf.Display.Width / 2 - Width / 2, mf.Display.Location.Y + 5);

				Enabled = true;

				commandsearch.Focus();

				// We want to hide the control when the focus is lost
				commandsearch.LostFocus += HidePalette;

				// Bring it to the foreground
				mf.Controls.SetChildIndex(this, 0);

				// Always keep the control in the center
				mf.Resize += Reposition;
			}
		}

		/// <summary>
		/// Keeps the control positioned in the top middle of the window when it is rezied.
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The event args</param>
		private void Reposition(object sender, EventArgs e)
		{
			// Center the control at the top middle
			if (Parent is MainForm mf)
				Location = new Point(mf.Display.Width / 2 - Width / 2, mf.Display.Location.Y + 5);
		}

		/// <summary>
		/// Selects the item before or after the current item in the command list.
		/// </summary>
		/// <param name="changeindexby">By how much the index should be changed. Positive numbers mean that it will scroll up, negative numbers will scroll down.</param>
		/// <param name="wraparound">If the selection should wrap around to the opposite side if the top or bottom of the list is reached</param>		
		private void SetSelectedItem(int changeindexby, bool wraparound)
		{
			if (commandlist.Items.Count > 1)
			{
				int newindex = commandlist.SelectedIndices[0] + changeindexby;

				if (newindex >= commandlist.Items.Count)
				{
					if (wraparound)
						newindex = 0;
					else
						newindex = commandlist.Items.Count - 1;
				}
				else if (newindex < 0)
				{
					if (wraparound)
						newindex = commandlist.Items.Count - 1;
					else
						newindex = 0;
				}

				// Reset the colors of the currently selected item to the defaults
				commandlist.SelectedItems[0].BackColor = SystemColors.Window;
				commandlist.SelectedItems[0].ForeColor = SystemColors.WindowText;

				// Set the new item, scroll the list to it, and set the highlight color
				commandlist.Items[newindex].Selected = true;
				commandlist.EnsureVisible(newindex);
				HighlightSelectedItem();
			}
		}

		/// <summary>
		/// Checks if a search string matches a text. It replicates the behavior of Visual Stuido Code.
		/// At first it tries to match the whole search string. If that didn't produce a result it'll try to match as much of the search
		/// string at the *beginning* of a word in the text. If that worked the matching characters are removed from the search text and
		/// all words in the text up to (including) the found word are removed. This is repeated until all characters in the search string
		/// are gone. This means:
		/// "le cl" matches "Toggle classic rendering"
		///                      ^^^^^
		/// "tore" matches  "Toggle classic rendering"
		///                  ^^             ^^
		/// "tcl" matches   "Toggle classic rendering"
		///                  ^      ^       ^
		/// "tof" matches   "Toggle Full Brightness"
		///                  ^^     ^
		///                 "Align Floor Textures to Front Side"
		///                                       ^^ ^
		///                 "Reset Texture Offsets"
		///                        ^       ^^
		///                 (and a couple other)
		/// </summary>
		/// <param name="text">The string to search in</param>
		/// <param name="search">The string to search for</param>
		/// <returns></returns>
		private bool MatchText(string text, string search)
		{
			text = text.ToLowerInvariant().Trim();
			text = Regex.Replace(text, @"\s+", " ");

			search = search.ToLowerInvariant().Trim();
			search = Regex.Replace(search, @"\s+", " ");

			// Check if the search string is empty or the whole search string is in the text to search
			if (string.IsNullOrWhiteSpace(search) || text.Contains(search))
				return true;

			// No match yet, so let's check if all search tokens are at the beginning of a text token. This is the same(ish?) behavior as Visual Studio Code.
			// This means that searching for "op ma" will match "Open Map", but not "Open Command Palette", because the "ma" in "Command" is not in the beginning.
			List<string> textitems = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			string[] searchitems = search.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			for(int i=0; i < searchitems.Length; i++)
			{
				string si = searchitems[i];

				// If the search item is empty it means we processed all its characters, so go to the next search item
				if (string.IsNullOrEmpty(si))
					continue;

				string result = null;

				// Search token not found, so try to match parts of the search token
				while (si.Length > 0)
				{
					// Try to find the first text token that starts with the search token
					result = textitems.FirstOrDefault(ti => ti.StartsWith(si));

					// We found something, so remove the matching part of the search token and prepare processing this search token again
					if (result != null)
					{
						searchitems[i] = searchitems[i].Remove(0, si.Length);
						i--;
						break;
					}

					// Nothing found, so remove the last character and keep going
					si = si.Remove(si.Length - 1);
				}

				// Nothing found, so abort
				if (result == null)
					return false;

				// We found a search token (or part of it), so remove all text tokens up to including the found text token
				int index = textitems.IndexOf(result);
				textitems.RemoveRange(0, index + 1);
			}

			// We didn't return yet, so we must have found everything
			return true;
		}

		/// <summary>
		/// Adds an action to the command list, either in the "usable" or "unsuable" group.
		/// </summary>
		/// <param name="action">The action to add</param>
		private void AddActionToList(Actions.Action action, bool isrecent = false)
		{
			string actiontitle = action.Title;
			string catname = string.Empty;
			bool isbound = action.BeginBound || action.EndBound;

			if (General.Actions.Categories.ContainsKey(action.Category))
				catname = General.Actions.Categories[action.Category];

			ListViewItem item = commandlist.Items.Add(action.Name, actiontitle, 0);

			// Store the action in the tag, so we can invoke the action later
			item.Tag = action;

			// Add the item to the appropriate group, either the "usable" (0) or "unusable" (1) one
			if (isrecent)
				item.Group = commandlist.Groups[GROUP_RECENT];
			else
				item.Group = commandlist.Groups[isbound ? GROUP_USABLE : GROUP_UNUSABLE];

			item.SubItems.Add(catname);
			item.SubItems.Add(Actions.Action.GetShortcutKeyDesc(action.ShortcutKey));
		}

		/// <summary>
		/// Runs an action and adds it to the list of recent actions
		/// </summary>
		/// <param name="action"></param>
		private void RunAction(Actions.Action action)
		{
			// Remove the action (if it's in the list) and then insert it at the beginning
			recentactions.Remove(action);
			recentactions.Insert(0, action);

			// Remove all actions that exceed the limit of the max number of recent actions
			if (recentactions.Count > MAX_RECENT_ACTIONS)
				recentactions.RemoveRange(4, recentactions.Count - MAX_RECENT_ACTIONS);

			General.Actions.InvokeAction(action.Name);
		}

		/// <summary>
		/// Fills the control, filtering it so that only the actions that match the search string are shown.
		/// </summary>
		/// <param name="searchtext">Text to search for in the action name</param>
		/// <param name="withrecent">If recently shown actions should be shown or not</param>
		private void FillCommandList(string searchtext = "", bool withrecent = false)
		{
			List<Actions.Action> usableactions = new List<Actions.Action>();
			List<Actions.Action> unusableactions = new List<Actions.Action>();

			commandlist.BeginUpdate();
			commandlist.Items.Clear();

			Actions.Action[] actions = General.Actions.GetAllActions();

			// Crawl through all actions and check if they are usable or not in the current context
			foreach (Actions.Action a in actions)
			{
				if (MatchText(a.Title, searchtext))
				{
					if (a.BeginBound || a.EndBound)
						usableactions.Add(a);
					else
						unusableactions.Add(a);
				}
			}

			// If there are matching actions we have to change the control's height and set the default selection
			if (usableactions.Count + unusableactions.Count > 0)
			{
				noresults.Visible = false;
				commandlist.Visible = true;

				if (withrecent)
					foreach (Actions.Action a in recentactions) if (a != null) AddActionToList(a, true);

				// We have to do the sorting on our own, because otherwise the groups will screw with the selection logic when pressing the up/down keys
				foreach (Actions.Action a in usableactions.OrderBy(o => o.Title)) AddActionToList(a);
				foreach (Actions.Action a in unusableactions.OrderBy(o => o.Title)) AddActionToList(a);
				
				// We want to show at most MAX_ITEMS items before having a scroll bar
				int numitems = commandlist.Items.Count > MAX_ITEMS ? MAX_ITEMS : commandlist.Items.Count;

				// Get the height of a row
				int itemheight = commandlist.Items[0].GetBounds(ItemBoundsPortion.Entire).Height;

				// Get the number of shown groups
				int numgroups = (usableactions.Count == 0 ? 0 : 1) + (unusableactions.Count == 0 ? 0 : 1);

				// Set the new height, which is the number of items times the row height, the groups, the search textbox and some buffer
				Height = itemheight * numitems + commandsearch.Height + numgroups * (int)(itemheight * 1.4) + commandlist.Location.X * 5;

				// Select the topmost item and highlight it
				commandlist.Items[0].Selected = true;
				HighlightSelectedItem();

				noresults.Visible = false;
			}
			else // No matching actions, hide line command list and tell the user that there are no matches
			{
				commandlist.Visible = false;
				noresults.Visible = true;

				Height = noresults.Location.Y + noresults.Height + noresults.Margin.Left * 2;
			}

			commandlist.EndUpdate();
		}

		#endregion

		#region ================== Events

		private void commandsearch_TextChanged(object sender, EventArgs e)
		{
			string searchtext = commandsearch.Text.Trim();

			if (string.IsNullOrWhiteSpace(searchtext))
				FillCommandList(withrecent: true);
			else
				FillCommandList(searchtext);
		}

		/// <summary>
		/// Handles certain special keys. Esc will close the palette, the Up and Down keys will change the selection, and Enter will start the command.
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The event args</param>
		private void commandsearch_KeyDown(object sender, KeyEventArgs e)
		{
			switch(e.KeyCode)
			{
				case Keys.Escape:
				case Keys.Down:
				case Keys.Up:
				case Keys.PageDown:
				case Keys.PageUp:
				//case Keys.End:
				//case Keys.Home:
				case Keys.Enter:
					e.Handled = true;
					e.SuppressKeyPress = true;
					break;
			}

			if (e.KeyCode == Keys.Escape)
				HidePalette(this, EventArgs.Empty);
			else if (e.KeyCode == Keys.Down)
				SetSelectedItem(1, true);
			else if (e.KeyCode == Keys.Up)
				SetSelectedItem(-1, true);
			else if (e.KeyCode == Keys.PageDown)
				SetSelectedItem(MAX_ITEMS - 1, false);
			else if (e.KeyCode == Keys.PageUp)
				SetSelectedItem(-MAX_ITEMS + 1, false);
			//else if (e.KeyCode == Keys.End)
			//	SetSelectedItem(commandlist.Items.Count, false);
			//else if (e.KeyCode == Keys.Home)
			//	SetSelectedItem(0, false);
			else if (e.KeyCode == Keys.Enter)
			{
				if (commandlist.Items.Count > 0)
				{
					HidePalette(this, EventArgs.Empty);
					RunAction((Actions.Action)commandlist.SelectedItems[0].Tag);
				}
			}
		}

		/// <summary>
		/// Run the command that was clicked on
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The event args</param>
		private void commandlist_ItemActivate(object sender, EventArgs e)
		{
			HidePalette(this, EventArgs.Empty);

			RunAction((Actions.Action)commandlist.SelectedItems[0].Tag);
		}

		#endregion
	}
}
