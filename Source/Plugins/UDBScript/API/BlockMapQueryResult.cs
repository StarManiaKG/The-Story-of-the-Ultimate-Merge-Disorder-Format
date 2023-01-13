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

using System.Collections.Generic;
using System.Linq;
using System.Collections;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	/// <summary>
	/// A `BlockMapQueryResult` is an object returned by the `getLineBlocks` and `getRectangleBlocks` methods of the `BlockMap` class. It has methods It has methods to retrieve the linedefs, things, sectors, and vertices that are in the queried blocks. The object is also iterable, returning each block, in cases where more fine-grained control is needed.
	/// ```
	/// const blockmap = new UDB.BlockMap();
	/// const result = blockmap.getLineBlocks([ 0, 0 ], [ 512, 256 ]);
	/// 
	/// // Print all linedefs in the blocks
	/// result.getLinedefs().forEach(ld => UDB.log(ld));
	/// ```
	/// Looping over each block:
	/// ```
	/// const blockmap = new UDB.BlockMap();
	/// const result = blockmap.getLineBlocks([ 0, 0 ], [ 512, 256 ]);
	/// 
	/// for(const block of result)
	/// {
	///		UDB.log('--- New block ---');
	///		block.getLinedefs().forEach(ld => UDB.log(ld));
	/// }
	/// ```
	/// !!! note
	///     The methods to retrieve map elements from `BlockMapQueryResult` return arrays that only contain each map element once, since linedefs and sectors can be in multiple blocks, looping over a `BlockMapQueryResult` using `for...of` can return the same map elements multiple times.
	/// </summary>
	class BlockMapQueryResult : BlockMapContentBase, IEnumerable<BlockEntryWrapper>
	{
		#region ================== Variables

		private BlockEntryWrapper[] wrappedentries;
		private IEnumerable<BlockEntry> entries;
		private HashSet<Linedef> lines;
		private HashSet<Thing> things;
		private HashSet<Sector> sectors;
		private HashSet<Vertex> vertices;

		#endregion

		#region ================== Constructors

		internal BlockMapQueryResult(IEnumerable<BlockEntry> entries)
		{
			this.entries = entries;
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Gets all `Linedef`s in the blockmap query result.
		/// </summary>
		/// <returns>`Array` of `Linedef`s</returns>
		[UDBScriptSettings(MinVersion = 5)]
		public override LinedefWrapper[] getLinedefs()
		{
			if (lines == null)
				lines = new HashSet<Linedef>(entries.SelectMany(be => be.Lines));

			return GetArray(lines, ref wrappedlines);
		}

		/// <summary>
		/// Gets all `Thing`s in the blockmap query result.
		/// </summary>
		/// <returns>`Array` of `Thing`s</returns>
		[UDBScriptSettings(MinVersion = 5)]
		public override ThingWrapper[] getThings()
		{
			if (things == null)
				things = new HashSet<Thing>(entries.SelectMany(be => be.Things));

			return GetArray(things, ref wrappedthings);
		}


		/// <summary>
		/// Gets all `Sector`s in the blockmap query result.
		/// </summary>
		/// <returns>`Array` of `Sector`s</returns>
		[UDBScriptSettings(MinVersion = 5)]
		public override SectorWrapper[] getSectors()
		{
			if (sectors == null)
				sectors = new HashSet<Sector>(entries.SelectMany(be => be.Sectors));

			return GetArray(sectors, ref wrappedsectors);
		}

		/// <summary>
		/// Gets all `Vertex` in the blockmap query result.
		/// </summary>
		/// <returns>`Array` of `Vertex`</returns>
		[UDBScriptSettings(MinVersion = 5)]
		public override VertexWrapper[] getVertices()
		{
			if (vertices == null)
				vertices = new HashSet<Vertex>(entries.SelectMany(be => be.Vertices));

			return GetArray(vertices, ref wrappedvertices);
		}

		#endregion

		#region ================== Enumeration

		public IEnumerator<BlockEntryWrapper> GetEnumerator() => ((IEnumerable<BlockEntryWrapper>)GetArray(entries, ref wrappedentries)).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetArray(entries, ref wrappedentries).GetEnumerator();

		#endregion
	}
}
