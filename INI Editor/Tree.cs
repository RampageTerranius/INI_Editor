//C# ini file loader, treats all files as WIN INI type
//DOES NOT handle multiple trees or values of same name!!!!!
//Created By Tyler Brown

/* 
	This program is free software: you can redistribute it and/or modify

	it under the terms of the GNU General Public License as published by

	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of

	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the

	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.If not, see<https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;

namespace INI_Editor
{
    /// <summary>
    /// Handles storing the tree data of an INI file.
    /// </summary>
    public class Tree
	{
		public string treeName;
		public List<Data> tree;

        /// <summary>
        /// Basic constructor, sets the tree name to blank and creates a new list of tree data.
        /// </summary>
		public Tree()
		{
			treeName = "";
			tree = new List<Data>();
		}

        /// <summary>
        /// Sets the tree name and creates a new list of tree data.
        /// </summary>
		public Tree(string argTreeName)
		{
			treeName = argTreeName;
			tree = new List<Data>();
		}


        /// <summary>
        /// Returns only the name of the tree, DOES NOT return data inside the tree, use TreeToString for that purpose.
        /// </summary>
        new public string ToString()
		{
			return "[" + treeName + "]";
		}
        
        /// <summary>
        /// Returns the entire tree in the same format as would be in the file.
        /// </summary>
        public string TreeToString()
		{
			string result = "";
			result += "[" + treeName + "]\n";

			for (int i = 0; i < tree.Count; i++)
				result += tree[i].ToString() + "\n";

			return result;
		}

        /// <summary>
        /// Returns tree as multidimensional array (format is [name, value])
        /// </summary>
        public string[,] TreeToStringArray()
		{
			string[,] result = new string[tree.Count, 2];

			for (int i = 0; i < result.Length; i++)
			{
				result[i, 0] = tree[i].dataName;
				result[i, 1] = tree[i].data;
			}
			
			return result;
		}
	}
}
