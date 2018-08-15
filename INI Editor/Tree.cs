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
	//handles storing the tree its self
	public class Tree
	{
		public string treeName;
		public List<Data> tree;

		public Tree()
		{
			treeName = "";
			tree = new List<Data>();
		}

		public Tree(string argTreeName)
		{
			treeName = argTreeName;
			tree = new List<Data>();
		}

		//tostring override
		//returns only the name of the tree, DOES NOT return data inside the tree, use TreeToString for that purpose
		new public string ToString()
		{
			return "[" + treeName + "]";
		}

		//returns the entire tree in the same format as would be in the file
		public string TreeToString()
		{
			string result = "";
			result += "[" + treeName + "]\n";

			for (int i = 0; i < tree.Count; i++)
				result += tree[i].ToString() + "\n";

			return result;
		}
	}
}
