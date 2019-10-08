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

using System;

namespace INI_Editor
{
	// Handles internal data for trees.
	public class Data
	{
		public string dataName;
		public string data;

		public Data()
		{
			dataName = "";
			data = "";
		}

		public Data(string valueName, string value)
		{
			dataName = valueName;
			data = value;
		}

		// ToString override, returns value in same format as would be in file.
		new public string ToString()
		{
			string result = dataName + "=" + data;
			return result;
		}

		// Returns a Tuple with both the data name and the data in seperate strings.
		public Tuple<string, string> ToTupleString()
		{
			return Tuple.Create(dataName, data);
		}
	}
}
