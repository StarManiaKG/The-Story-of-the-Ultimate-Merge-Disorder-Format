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

using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	/// <summary>
	/// A `BlockEntry` is a single block in a `BlockMap`. It has methods to retrieve the linedefs, things, sectors, and vertices that are in this block.
	/// </summary>
	class BlockEntryWrapper : BlockMapContentBase
	{
		#region ================== Variables

		private BlockEntry entry;

		#endregion

		#region ================== Constructors

		internal BlockEntryWrapper(BlockEntry entry)
		{
			this.entry = entry;
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Gets all `Linedef`s in the blockmap entry.
		/// </summary>
		/// <returns>`Array` of `Linedef`s</returns>
		[UDBScriptSettings(MinVersion = 5)]
		public override LinedefWrapper[] getLinedefs()
		{
			return GetArray(entry.Lines, ref wrappedlines);
		}

		/// <summary>
		/// Gets all `Thing`s in the blockmap entry.
		/// </summary>
		/// <returns>`Array` of `Thing`s</returns>
		[UDBScriptSettings(MinVersion = 5)]
		public override ThingWrapper[] getThings()
		{
			return GetArray(entry.Things, ref wrappedthings);
		}

		/// <summary>
		/// Gets all `Sector`s in the blockmap entry.
		/// </summary>
		/// <returns>`Array` of `Sector`s</returns>
		[UDBScriptSettings(MinVersion = 5)]
		public override SectorWrapper[] getSectors()
		{
			return GetArray(entry.Sectors, ref wrappedsectors);
		}

		/// <summary>
		/// Gets all `Vertex` in the blockmap entry.
		/// </summary>
		/// <returns>`Array` of `Vertex`</returns>
		[UDBScriptSettings(MinVersion = 5)]
		public override VertexWrapper[] getVertices()
		{
			return GetArray(entry.Vertices, ref wrappedvertices);
		}

		#endregion
	}
}
