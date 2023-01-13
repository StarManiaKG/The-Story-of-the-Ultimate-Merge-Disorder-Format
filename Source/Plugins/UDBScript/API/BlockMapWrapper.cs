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
using System.Drawing;
using System.Dynamic;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	/// <summary>
	/// A blockmap is used to retrieve a collection of localized map elements (things, linedefs, sectors, vertices). It can help to significantly speed up costly computations that would otherwise be applied to a large portion of the map elements. The blockmap divides the map into rectangular blocks and computes which map elements are fully or partially in each block. Then you can query the blockmap about only some of those blocks, and perform any further actions only on the map elements that are in those blocks.
	/// 
	/// If you for example wanted to find out which sector is at the (0, 0) position you could write something like this without using a blockmap:
	/// ```
	/// UDB.Map.getSectors().findIndex((s, i) => {
	///		if(s.intersect([ 0, 0 ]))
	///		{
	///			UDB.log(`Found ${s} after ${i} tries.`)
	///			return true;
	///		}
	/// });
	/// ```
	/// This loops through all sectors of the map and uses the `intersect` method to test if the point is inside the sector. While `intersect` is quite fast on its own, doing it potentially thousands of times adds up quickly, especially if you have to loop through all sectors multiple times.
	/// A pretty extreme example for this is the map Bastion of Chaos. The map contains nearly 32500 sectors, and the sector at (0, 0) is number 25499. That means that the above script has to run `intersect` on 25499 sectors, even on those that are not remotely near the (0, 0) position.
	/// 
	/// Using a blockmap the code could look like this:
	/// ```
	/// const blockmap = new UDB.BlockMap();
	/// 
	/// blockmap.getBlockAt([ 0, 0 ]).getSectors().findIndex((s, i) => {
	///		if (s.intersect([0, 0]))
	///		{
	///			UDB.log(`Found ${s} after ${i} tries.`)
	///			return true;
	///		}
	/// });
	/// ```
	/// As you can see the code is quite similar, the difference being that a blockmap is created, and `UDB.Map` is replaced by `blockmap.getBlockAt([ 0, 0 ])`, the latter only getting a single block from the blockmap, that only contains the map elements that are in this block. Taking Bastion of Chaos as an example again, this code finds the sector after only 20 checks, instead of the 25499 checks in the first code example.
	/// 
	/// !!! note
	///     Creating a blockmap has a small overhead, since it has to compute which map elements are in which blocks. This overhead, however, is quickly compensated by the time saved by not looping through irrelevant map elements. You can decrease this overhead by using a `BlockMap` constructor that only adds certain map element types to the blockmap.
	/// 
	/// </summary>
	class BlockMapWrapper
	{
		#region ================== Variables

		private BlockMap<BlockEntry> blockmap;
		private Dictionary<BlockEntry, BlockEntryWrapper> blockentries;

		#endregion

		#region ================== Constructors

		/// <summary>
		/// Creates a blockmap that includes linedefs, things, sectors, and vertices.
		/// ```
		/// // Create a blockmap that includes all linedefs, things, sectors, and vertices
		/// const blockmap = new UDB.BlockMap();
		/// ```
		/// </summary>
		[UDBScriptSettings(MinVersion = 5)]
		public BlockMapWrapper()
		{
			CreateBlockmap(true, true, true, true);
		}

		/// <summary>
		/// Creates a blockmap that only includes certain map element types.
		/// ```
		/// // Create a blockmap that only includes sectors
		/// const blockmap = new UDB.BlockMap(false, false, true, false);
		/// ```
		/// </summary>
		/// <param name="lines">If linedefs should be added or not</param>
		/// <param name="things">If thigs should be added or not</param>
		/// <param name="sectors">If sectors should be added or not</param>
		/// <param name="vertices">If vertices should be added or not</param>
		/// [UDBScriptSettings(MinVersion = 5)]
		public BlockMapWrapper(bool lines, bool things, bool sectors, bool vertices)
		{
			CreateBlockmap(lines, things, sectors, vertices);
		}

		internal BlockMapWrapper(ExpandoObject options)
		{
			bool lines = IsOptionSet(options, "lines");
			bool things = IsOptionSet(options, "things");
			bool sectors = IsOptionSet(options, "sectors");
			bool vertices = IsOptionSet(options, "vertices");

			CreateBlockmap(lines, things, sectors, vertices);
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Generates the blockmap and adds the wanted map elements.
		/// </summary>
		/// <param name="lines">If linedefs should be added or not</param>
		/// <param name="things">If thigs should be added or not</param>
		/// <param name="sectors">If sectors should be added or not</param>
		/// <param name="vertices">If vertices should be added or not</param>
		[UDBScriptSettings(MinVersion = 5)]
		private void CreateBlockmap(bool lines, bool things, bool sectors, bool vertices)
		{
			RectangleF area = MapSet.CreateArea(General.Map.Map.Vertices);

			if (things)
				area = MapSet.IncreaseArea(area, General.Map.Map.Things);

			blockmap = new BlockMap<BlockEntry>(area);

			if (lines)
				blockmap.AddLinedefsSet(General.Map.Map.Linedefs);

			if (things)
				blockmap.AddThingsSet(General.Map.Map.Things);

			if (sectors)
				blockmap.AddSectorsSet(General.Map.Map.Sectors);

			if (vertices)
				blockmap.AddVerticesSet(General.Map.Map.Vertices);

			blockentries = new Dictionary<BlockEntry, BlockEntryWrapper>();
		}

		/// <summary>
		/// Checks if a dictionary contains a given key and if it's set to true or false.
		/// </summary>
		/// <param name="options">The dictionary to check</param>
		/// <param name="name">Name of the option to check</param>
		/// <returns>true if the option exists and is set to true, false if the option doesn't exist or is set to false</returns>
		private bool IsOptionSet(IDictionary<string, object> options, string name)
		{
			return options != null && options.ContainsKey(name) && options[name] is bool value && value == true;
		}

		/// <summary>
		/// Gets the `BlockEntry` at a point. The given point can be a `Vector2D` or an `Array` of two numbers.
		/// ```
		/// const blockmap = new UDB.BlockMap();
		/// const blockentry = blockmap.getBlockAt([ 64, 128 ]);
		/// ```
		/// </summary>
		/// <param name="pos">The point to get the `BlockEntry` of</param>
		/// <returns>The `BlockEntry` on the given point</returns>
		[UDBScriptSettings(MinVersion = 5)]
		public BlockEntryWrapper getBlockAt(object pos)
		{
			Vector2D p = BuilderPlug.Me.GetVector3DFromObject(pos);
			BlockEntry be = blockmap.GetBlockAt(p);

			if (!blockentries.ContainsKey(be))
				blockentries[be] = new BlockEntryWrapper(be);

			return blockentries[be];
		}

		/// <summary>
		/// Gets a `BlockMapQueryResult` for the blockmap along a line between two points. The given points can be `Vector2D`s or an `Array`s of two numbers.
		/// ```
		/// const blockmap = new UDB.BlockMap();
		/// const result = blockmap.getLineBlocks([ 0, 0 ], [ 512, 256 ]);
		/// ```
		/// </summary>
		/// <param name="v1">The first point</param>
		/// <param name="v2">The second point</param>
		/// <returns>The `BlockMapQueryResult` for the line between the two points</returns>
		[UDBScriptSettings(MinVersion = 5)]
		public BlockMapQueryResult getLineBlocks(object v1, object v2)
		{
			Vector2D p1 = BuilderPlug.Me.GetVector3DFromObject(v1);
			Vector2D p2 = BuilderPlug.Me.GetVector3DFromObject(v2);

			return new BlockMapQueryResult(blockmap.GetLineBlocks(p1, p2));
		}

		/// <summary>
		/// Gets a `BlockMapQueryResult` for the blockmap in a rectangle.
		/// ```
		/// const blockmap = new UDB.BlockMap();
		/// const result = blockmap.getRectangleBlocks(0, 0, 512, 256);
		/// ```
		/// </summary>
		/// <param name="x">X position of the top-left corner of the rectangle</param>
		/// <param name="y">Y position of the top-left corner of the rectangle</param>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <returns></returns>
		[UDBScriptSettings(MinVersion = 5)]
		public BlockMapQueryResult getRectangleBlocks(int x, int y, int width, int height)
		{
			return new BlockMapQueryResult(blockmap.GetSquareRange(new RectangleF(x, y, width, height)));
		}

		#endregion
	}
}
