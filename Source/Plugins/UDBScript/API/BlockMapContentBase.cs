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
using System.Linq;
using System.Reflection;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	abstract class BlockMapContentBase
	{
		#region ================== Variables

		protected LinedefWrapper[] wrappedlines;
		protected ThingWrapper[] wrappedthings;
		protected SectorWrapper[] wrappedsectors;
		protected VertexWrapper[] wrappedvertices;

		#endregion

		#region ================== Methods

		abstract public LinedefWrapper[] getLinedefs();
		abstract public ThingWrapper[] getThings();
		abstract public SectorWrapper[] getSectors();
		abstract public VertexWrapper[] getVertices();

		/// <summary>
		/// Fills the container array with wrapped instances of the map elements in the given list.
		/// </summary>
		/// <typeparam name="W">Wrapped map element type</typeparam>
		/// <typeparam name="T">Regular map element type</typeparam>
		/// <param name="list">List of regular map elements</param>
		/// <param name="container">Array of wrapped map elements</param>
		/// <returns></returns>
		protected internal W[] GetArray<W, T>(IEnumerable<T> list, ref W[] container)
		{
			if (container == null)
			{
				if (list == null)
					container = new W[0];
				else
					container = list.Select(s => (W)Activator.CreateInstance(typeof(W), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { s }, null)).ToArray();
			}

			return container;
		}

		#endregion
	}
}
