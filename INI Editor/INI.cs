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
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace INI_Editor
{
    /// <summary>
    /// Handles all INI file data.
    /// </summary>
    public class INI
    {
        private List<Tree> data;

        private string lastError;

        private string fileLocation;
        private bool fileLoaded;
        public bool FileLoaded
        { get { return fileLoaded; } }

		private bool logErrorsToConsole;
		public bool LogErrorsToConsole
		{ get { return logErrorsToConsole; } set { logErrorsToConsole = value; } }

        /// <summary>
        /// Basic constructor, creates a blank INI.
        /// </summary>
        public INI()
        {
            Clear();
        }

        /// <summary>
        /// Loads a file into the INI automatically.
        /// </summary>
		public INI(string argFileLocation)
		{
			Clear();
			Load(argFileLocation);
		}

        /// <summary>
        /// Loads a file into the INI automatically.
        /// </summary>
        public INI(string argFileLocation, string argFileName)
		{
			Clear();
			Load(argFileLocation + "\\" + argFileName);
		}

        /// <summary>
        /// Creates a file at the given location and prepares it for editing.
        /// </summary>
        public bool Create(string argFileLocation)
        {
            if (argFileLocation == "")
            {
                LogError("INI Editor-Create: No file location given");
                return false;
            }

            if (File.Exists(argFileLocation))
            {
                LogError("INI Editor-Create: File already exist's at '" + argFileLocation + "'");
                return false;
            }
            try
            {
                // Attempt to create the file and then load it if possible.
                FileStream str = File.Create(argFileLocation);

                str.Close();

                return Load(argFileLocation);
            }
            catch
            {
                LogError("INI Editor-Create: Unable to create file at '" + argFileLocation + "'");

                return false;
            }
        }

        /// <summary>
        /// Creates a file at the given locatino and prepares it for editing.
        /// </summary>
        public bool Create(string argFileLocation, string argFileName)
        {
            return Create(argFileLocation + "\\" + argFileName);
        }

        /// <summary>
        /// Loads a file from given location into internal buffer.
        /// Self handles file exceptions.
        /// </summary>
        /// <param name="argFileLocation"></param>
        /// <returns>Returns true if load was successful, returns false if unsuccessful, if unsuccessful a error will be logged into lastError.</returns>
        public bool Load(string argFileLocation)
		{
            // Make sure a file location has been supplied and that we dont already have a file open.
			if (argFileLocation == string.Empty)
			{
				LogError("INI Editor-Load: No file location given");
				return false;
			}

			if (fileLoaded)
			{
				LogError("INI Editor-Load: A file is already open, please use Close() or SaveAndClose() first");
				return false;
			}

			Clear();

			// Load the file.
			try
			{
				// FileNotFoundException sometimes fails to catch non existant files, this is used as a backup.
				if (!File.Exists(argFileLocation))
				{
					LogError("INI Editor - Load: File does not exist at '" + argFileLocation + "'");
					return false;
				}

				List<String> data = new List<string>();
				StreamReader sr = new StreamReader(argFileLocation);

				// Load the file into local memory.
				while (!sr.EndOfStream)
				{
					string s = sr.ReadLine();
					if (s != "")
						data.Add(s);
				}

				sr.Close();

				// Prepare to sort the data into our object.
				for (int i = 0; i < data.Count; i++)
				{
					// A tree was found.
					if (data[i].First<char>().Equals('[') && data[i].Last<char>().Equals(']'))
					{
						string treeName = data[i];
						treeName = treeName.Remove(0, 1);
						treeName = treeName.Remove(treeName.Length - 1, 1);

						// Add a new Tree object to the list.
						AddTree(treeName);

						// Prepare and add all the Data objects under this tree.
						for (int n = (i + 1); n < data.Count; n++)
							if (!(data[n].First<char>().Equals('[') && data[n].Last<char>().Equals(']')))
							{
								string valueName = data[n].Remove(data[n].IndexOf('='), data[n].Length - data[n].IndexOf('='));
								string value = data[n].Remove(0, (data[n].IndexOf('=') + 1));
								AddValue(treeName, valueName, value);
							}
							else
								break;// Either found a new Tree or reached the end of the file.
					}
				}
			}
			catch (FileNotFoundException)
			{
				LogError("INI Editor-Load: File was not found '" + fileLocation + "'");
				return false;
			}
			catch (Exception)
			{
				LogError("INI Editor-Load: Unhandled Exception");
				return false;
			}

			// A file has been loaded, prepare file location data and set a file loaded value for future use.
			this.fileLocation = argFileLocation;
			fileLoaded = true;

			return true;
		}

        /// <summary>
        /// Loads a file from given location with the given name into internal buffer.
        /// Self handles file exceptions.
        /// </summary>
        /// <param name="argFileLocation"></param>
        /// <returns>Returns true if load was successful, returns false if unsuccessful, if unsuccessful a error will be logged into lastError.</returns>
        public bool Load(string argFileLocation, string argFileName)
		{
			return Load(argFileLocation + "\\" + argFileName); ;
		}

        /// <summary>
        /// Saves to the same location as was originally opened.
        /// </summary>
        /// <returns>Returns true if successfully saved returns false if unsucessful, if unsucessful a error will be logged into lastError.</returns>
        public bool Save()
        {
            if (!fileLoaded)
            {
				LogError("INI Editor-Save: No file has been loaded. if trying to save a programatically created ini file please use Saveto()");
                return false;
            }            

            return SaveTo(fileLocation);
        }


        /// <summary>
        /// Saves to a specified file location.
        /// Self handles file exceptions.
        /// </summary>
        /// <returns>Returns true if successfully saved returns false if unsucessful, if unsucessful a error will be logged into lastError.</returns>
        public bool SaveTo(string argFileLocation)
        {
			// Check if location is blank.
			if (argFileLocation == string.Empty)
			{
				LogError("INI Editor-SaveTo: given file location was blank!");
				return false;
			}

			// Attempt to save.
			try
            {		
                StreamWriter sw = new StreamWriter(argFileLocation);

                List<string> data = ToStringArray();

                for (int i = 0; i < data.Count; i++)
                    sw.WriteLine(data[i]);

                sw.Flush();
                sw.Close();
            }
            // If failed to save log the error.
			catch (DirectoryNotFoundException)
			{
				LogError("INI Editor-SaveTo: Given directory does not exist! '" + argFileLocation + "'");
				return false;
			}
            catch (Exception)
            {
				LogError("INI Editor-SaveTo: Unhandled exception");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves to a specified file location with a specified file name.
        /// Self handles file exceptions.
        /// </summary>
        /// <returns>Returns true if successfully saved returns false if unsucessful, if unsucessful a error will be logged into lastError.</returns>
        public bool SaveTo(string argFileLocation, string argFileName)
		{
			return SaveTo(argFileLocation + "\\" + argFileName);
		}

        /// <summary>
        /// Saves and closes the current INI, uses Save() and Close() functions to cause this.
        /// </summary>
        /// <returns>Returns true if Save() and Close() are successful, returns false if either failed. if either failed a error will be logged into lastError by their respective functions.</returns>
        public bool SaveAndClose()
        {
            if (!Save())
                return false;

            Close();

            return true;
        }

        /// <summary>
        /// Checks if the given tree name exists.
        /// </summary>
        /// <returns>Returns true if tree already exists, returns false if it does not.</returns>
        public bool TreeExists(string tree)
		{
            // Make sure we have data to work with.
            if (tree == string.Empty)
			{
				LogError("INI Editor-TreeExists: No given tree name");
				return false;
			}

            // Check if the tree exists.
			for (int i = 0; i < data.Count; i++)
				if (data[i].treeName == tree)
					return true;

			// If the tree does not exist then return false.
			return false;
		}

        /// <summary>
        /// Checks if a given value inside of a tree exists.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="value"></param>
        /// <returns>Returns true if it exists, returns false if not.</returns>
        public bool ValueExists(string tree, string value)
		{
            // Use GetValue to check if data exists.
            if (GetValue(tree, value) != string.Empty)
                return true;

			// Value does not exist.
			return false;
		}

        /// <summary>
        /// Returns the data from a given value in a given tree.
        /// </summary>
        /// <returns>Returns any fould value as a string, returns empty string if failed.</returns>
        public string GetValue(string tree, string value)
        {
            // Make sure we have data to work with.
            if (tree == string.Empty)
			{
				LogError("INI Editor-GetValue: No given tree name");
				return "";
			}

			if (value == string.Empty)
			{
				LogError("INI Editor-GetValue: No given value name");
				return "";
			}

			Tree t = new Tree();
            t = GetTree(tree);

            if (t != null)
                for (int i = 0; i < t.tree.Count; i++)
                    if (t.tree[i].dataName == value)
                        return t.tree[i].data;//value exists

            // Value does not exist.
            return string.Empty;
        }

        /// <summary>
        /// Attempts to return the given values data in a given tree as a boolean.
        /// </summary>
        /// <returns>Returns bool. an error will be logged if the value was neither true or false, in this situation will default to return false.</returns>
        public bool GetValueAsBool(string tree, string value)
		{
            // Make sure we have data to work with.
            if (tree == string.Empty)
			{
				LogError("INI Editor-GetValueAsBool: No given tree name");
				return false;
			}

			if (value == string.Empty)
			{
				LogError("INI Editor-GetValueAsBool: No given value name");
				return false;
			}

			Tree t = new Tree();
			t = GetTree(tree);

			if (t != null)
				for (int i = 0; i < t.tree.Count; i++)
					if (t.tree[i].dataName == value)
						if (t.tree[i].data.ToString().ToLower() == "true")
							return true;
						else if (t.tree[i].data.ToString().ToLower() == "false")
							return false;
						else
						{
							LogError("INI Editor-GetValueAsBool: Warning! [" + tree + "]-'" + value + "' was neither true or false, defaulting return value to false");
							return false;
						}

			LogError("INI Editor-GetValueAsBool: Warning! [" + tree + "]-'" + value + "' was not found");
			return false;
		}

        /// <summary>
        /// returns the given value as a int.
        /// </summary>
        /// <returns>returns int, logs an error if unable to parse string as int.</returns>
        public int GetValueAsInt(string tree, string value)
		{
            // Make sure we have data to work with.
            if (tree == string.Empty)
			{
				LogError("INI Editor-GetValueAsInt: No given tree name");
				return 0;
			}

			if (value == string.Empty)
			{
				LogError("INI Editor-GetValueAsInt: No given value name");
				return 0;
			}

			Tree t = new Tree();
			t = GetTree(tree);

			if (t != null)
				for (int i = 0; i < t.tree.Count; i++)
					if (t.tree[i].dataName == value)
						try
						{
							int val = int.Parse(t.tree[i].data);
							return val;
						}
						catch (Exception)
						{
							LogError("INI Editor-GetValueAsInt: Warning! [" + tree + "]-'" + value + "=" + t.tree[i].data + " was unable to be parsed as an int, defaulting return value to 0");
						}


			LogError("INI Editor-GetValueAsInt: Warning! ["+ tree + "]-'" + value + "' was not found");
			return 0;
		}

        /// <summary>
        /// Returns the given value as a double.
        /// </summary>
        /// <returns>Returns double, logs an error if unable to parse string as double.</returns>
        public double GetValueAsDouble(string tree, string value)
		{
            // Make sure we have data to work with.
            if (tree == string.Empty)
			{
				LogError("INI Editor-GetValueAsDouble: No given tree name ");
				return 0.0;
			}

			if (value == string.Empty)
			{
				LogError("INI Editor-GetValueAsDouble: No given value name");
				return 0.0;
			}

			Tree t = new Tree();
			t = GetTree(tree);

			if (t != null)
				for (int i = 0; i < t.tree.Count; i++)
					if (t.tree[i].dataName == value)
						try
						{
							double val = float.Parse(t.tree[i].data);
							return val;
						}
						catch (Exception)
						{
							LogError("INI Editor-GetValueAsDouble: Warning! [" + tree + "]-'" + value + "=" + t.tree[i].data + " was unable to be parsed as an double, defaulting return value to 0.0");
							return 0.0;
						}


			LogError("INI Editor-GetValueAsDouble: Warning! [" + tree + "]-'" + value + "' was not found");
			return 0.0;
		}

        /// <summary>
        /// returns the first instance of any tree with the name given
        /// </summary>
        /// <returns>Returns a list of type Tree, returns null if tree not found or file not loaded. if unsuccessful a error will be logged into lastError.</returns>
        public Tree GetTree(string tree)
        {
            // Make sure we have data to work with.
            if (tree == string.Empty)
			{
				LogError("INI Editor-GetTree: No given tree name ");
				return null;
			}

			for (int i = 0; i < data.Count; i++)
                if (data[i].treeName == tree)
                    return data.ElementAt(i);

            // If tree was not found return nothing.
			LogError("INI Editor-GetTree: tree [" + tree + "] not found");
            return null;
        }

        /// <summary>
        /// Gets all trees and their values.
        /// </summary>
        /// <returns>returns List<Tree> with all of INI.data in it.</returns>
        public List<Tree> GetAll()
        {
            return data;
        }
       
        /// <summary>
        /// Adds a new value to the given tree, does not create the tree if it does not exist.
        /// </summary>
        /// <returns>Returns true if value was added, returns false otherwise. if unsuccessful a error will be logged into lastError.</returns>
        public bool AddValue(string treeName, string valueName, string value)
		{
            // Make sure we have data to work with.
            if (treeName == string.Empty)
			{
				LogError("INI Editor-AddValue: No tree name was given");
				return false;
			}

			if (valueName == string.Empty)
			{
				LogError("INI Editor-AddValue: No value name was given");
				return false;
			}


			for (int i = 0; i < data.Count; i++)
				if (data[i].treeName == treeName)
				{
					data[i].tree.Add(new Data(valueName, value));
					return true;
				}

			LogError("INI Editor-AddValue: Tree [" + treeName + "] not found");
			return false;
		}

        /// <summary>
        /// Adds a new value to the given tree, does not create the tree if it does not exist.
        /// </summary>
        /// <returns>Returns true if value was added, returns false otherwise. if unsuccessful a error will be logged into lastError.</returns>
		public bool AddValue(string treeName, string valueName, int value)
		{
			return AddValue(treeName, valueName, value.ToString());
		}

        /// <summary>
        /// Adds a new value to the given tree, does not create the tree if it does not exist.
        /// </summary>
        /// <returns>Returns true if value was added, returns false otherwise. if unsuccessful a error will be logged into lastError.</returns>
        public bool AddValue(string treeName, string valueName, double value)
		{
			return AddValue(treeName, valueName, value.ToString());
		}

        /// <summary>
        /// Adds a new value to the given tree, does not create the tree if it does not exist.
        /// </summary>
        /// <returns>Returns true if value was added, returns false otherwise. if unsuccessful a error will be logged into lastError.</returns>
        public bool AddValue(string treeName, string valueName, float value)
		{
			return AddValue(treeName, valueName, value.ToString());
		}

        /// <summary>
        /// Adds a new value to the given tree, does not create the tree if it does not exist.
        /// </summary>
        /// <returns>Returns true if value was added, returns false otherwise. if unsuccessful a error will be logged into lastError.</returns>
        public bool AddValue(string treeName, string valueName, bool value)
		{
			return AddValue(treeName, valueName, value.ToString().ToLower());
		}

        /// <summary>
        /// Creates a new tree in the data set using given name.
        /// </summary>
        /// <returns>Returns true if tree was created, returns false if tree was not created.</returns>
        public bool AddTree(string treeName)
        {
			if (treeName != string.Empty)
			{
				if (!TreeExists(treeName))
				{
					data.Add(new Tree(treeName));
					return true;
				}
				else
					LogError("INI Editor-AddTree: tree [" + treeName + "] already exists");
			}
			else
				LogError("INI Editor-AddTree: treename is blank, please give a tree name");

			return false;
        }

        //overloads for AddTree handling variables other then string

        //
        //
        /// <summary>
        /// Adds the given tree to the data set.
        /// </summary>
        /// <returns>Returns true if tree was added, returns false if tree was not created.</returns>
        public bool AddTree(Tree argTree)
		{
			if (argTree != null)
			{
				if (argTree.treeName != string.Empty)
				{

					string treeName = argTree.treeName;
					if (!TreeExists(treeName))
					{
						data.Add(argTree);
						return true;
					}
					else 
						LogError("INI Editor-AddTree: tree [" + treeName + "] already exists");
				}
				else
					LogError("INI Editor-AddTree: treename is blank, please give a tree name");
			}
			else
				LogError("INI Editor-AddTree: argTree is null");

			return false;
		}

        /// <summary>
        /// Edits the given value in the given tree.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
        public bool EditValue(string treeName, string valueName, string newData)
		{
            // Make sure we have data to work with.
            if (treeName == string.Empty)
			{
				LogError("INI Editor-EditValue: No tree name was given");
				return false;
			}

			if (valueName == string.Empty)
			{
				LogError("INI Editor-EditValue: No value name was given");
				return false;
			}

			for (int i = 0; i < data.Count; i++)
			{
				if (data[i].treeName == treeName)
				{
					for (int n = 0; n < data[i].tree.Count; n++)
					{
						if (data[i].tree[n].dataName == valueName)
						{
							data[i].tree[n].data = newData;
							return true;
						}
						else if (n == data[i].tree.Count - 1)
						{
							LogError("INI Editor-EditValue: value '" + valueName + "' not found in tree " + treeName);
							return false;
						}
					}
				}
				else if (i == data.Count - 1)
					LogError("INI Editor-EditValue: tree '" + valueName + "' not found");
			}

			return false;
		}

        /// <summary>
        /// Edits the given value in the given tree and also renames the value.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
        public bool EditValue(string treeName, string valueName, string newValueName, string newValue)
		{
            // Make sure we have data to work with.
            if (treeName == string.Empty)
			{
				LogError("INI Editor-EditValue: No tree name was given");
				return false;
			}

			if (valueName == string.Empty)
			{
				LogError("INI Editor-EditValue: No value name was given");
				return false;
			}

			if (newValueName == string.Empty)
			{
				LogError("INI Editor-EditValue: newValueName is blank, can not create a blank value name");
				return false;
			}

			for (int i = 0; i < data.Count; i++)
				for (int n = 0; n < data[i].tree.Count; n++)
					if (data[i].tree[n].dataName == valueName)
					{
						data[i].tree[n].dataName = newValueName;
						data[i].tree[n].data = newValue;
						return true;
					}

			LogError("INI Editor-EditValue: value " + treeName + "= not found");
			return false;
		}

        //overloads for EditValue handling variables other then string
        /// <summary>
        /// Edits the given value in the given tree and also renames the value.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
        public bool EditValue(string treeName, string valueName, int newData)
		{
			return EditValue(treeName, valueName, newData.ToString());
		}

        /// <summary>
        /// Edits the given value in the given tree and also renames the value.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
		public bool EditValue(string treeName, string valueName, float newData)
		{
			return EditValue(treeName, valueName, newData.ToString());
		}

        /// <summary>
        /// Edits the given value in the given tree and also renames the value.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
		public bool EditValue(string treeName, string valueName, double newData)
		{
			return EditValue(treeName, valueName, newData.ToString());
		}

        /// <summary>
        /// Edits the given value in the given tree and also renames the value.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
		public bool EditValue(string treeName, string valueName, bool newData)
		{
			return EditValue(treeName, valueName, newData.ToString().ToLower());
		}

        /// <summary>
        /// Edits the given value in the given tree and also renames the value.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
		public bool EditValue(string treeName, string valueName, string newValueName, int newData)
		{
			return EditValue(treeName, valueName, newValueName, newData.ToString());
		}

        /// <summary>
        /// Edits the given value in the given tree and also renames the value.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
		public bool EditValue(string treeName, string valueName, string newValueName, float newData)
		{
			return EditValue(treeName, valueName, newValueName, newData.ToString());
		}

        /// <summary>
        /// Edits the given value in the given tree and also renames the value.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
		public bool EditValue(string treeName, string valueName, string newValueName, double newData)
		{
			return EditValue(treeName, valueName, newValueName, newData.ToString());
		}

        /// <summary>
        /// Edits the given value in the given tree and also renames the value.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
		public bool EditValue(string treeName, string valueName, string newValueName, bool newData)
		{
			return EditValue(treeName, valueName, newValueName, newData.ToString().ToLower());
		}

        /// <summary>
        /// Edits the given trees name.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
        public bool EditTree(string treeName, string newName)
        {
            // Make sure we have data to work with.
            if (treeName == string.Empty)
			{
				LogError("INI Editor-EditValue: No tree name was given");
				return false;
			}

			if (newName == string.Empty)
			{
				LogError("INI Editor-EditValue: No new tree name was given");
				return false;
			}

			for (int i = 0; i < data.Count; i++)
                if (data[i].treeName == treeName)
                {
                    data[i].treeName = newName;
                    return true;
                }

			LogError("INI Editor-EditTree: Tree [" + treeName + "] not found");
            return false;
        }

        /// <summary>
        /// Edits the given trees name and gives it a new set of internal data.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
        public bool EditTree(string treeName, string newName, List<Data> tree)
        {
            // Make sure we have data to work with.
            if (treeName == string.Empty)
			{
				LogError("INI Editor-EditTree: No tree name was given");
				return false;
			}

			if (newName == string.Empty)
			{
				LogError("INI Editor-EditTree: No new tree name was given");
				return false;
			}

			if (tree == null)
			{
				LogError("INI Editor-EditTree: given tree is null, unable to edit tree");
				return false;
			}

			for (int i = 0; i < data.Count; i++)
                if (data[i].treeName == treeName)
                {
                    data[i].treeName = newName;
                    data[i].tree = tree;
                    return true;
                }

			LogError("INI Editor-EditTree: Tree [" + treeName + "] not found");
            return false;
        }

        /// <summary>
        /// Edits the given tree giving it a new set of internal data.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
        public bool EditTree(string treeName, List<Data> tree)
		{
            // Make sure we have data to work with.
            if (treeName == string.Empty)
			{
				LogError("INI Editor-EditTree: No tree name was given");
				return false;
			}

			if (tree == null)
			{
				LogError("INI Editor-EditTree: given tree is null, unable to edit tree");
				return false;
			}

			for (int i = 0; i < data.Count; i++)
				if (data[i].treeName == treeName)
				{					
					data[i].tree = tree;
					return true;
				}

			LogError("INI Editor-EditTree: Tree [" + treeName + "] not found");
			return false;
		}

        /// <summary>
        /// Deletes the given tree from the INI.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
        public bool DeleteTree(string treeName)
		{
            // Make sure we have data to work with.
            if (treeName == string.Empty)
			{
				LogError("INI Editor-EditTree: no tree name was given");
				return false;
			}

			// Find and delete the tree.
			for (int i = 0; i < data.Count; i++)
				if (data[i].treeName == treeName)
				{
					data.RemoveAt(i);
					return true;
				}

			// Tree not found.
			LogError("INI Editor-DeleteTree: Tree [" + treeName + "] not found");
			return false;
		}

        /// <summary>
        /// Clears all values from the given tree, does not delete the tree itself.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
        public bool ClearTree(string treeName)
		{
            // Make sure we have data to work with.
            if (treeName == string.Empty)
			{
				LogError("INI Editor-EditTree: no tree name was given");
				return false;
			}

			// Find and clear the tree.
			for (int i = 0; i < data.Count; i++)
				if (data[i].treeName == treeName)
				{
					data[i].tree = new List<Data>();
					data[i].treeName = treeName;
					return true;
				}

			// Tree not found.
			LogError("INI Editor-ClearTree: Tree [" + treeName + "] not found");
			return false;
		}

        /// <summary>
        /// Deletes the given value from the given tree, does not delete the tree.
        /// </summary>
        /// <returns>Returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError.</returns>
        public bool DeleteValue(string treeName, string value)
		{
            // Make sure we have data to work with.
            if (value == string.Empty)
			{
				LogError("INI Editor-EditValue: no value name was given");
				return false;
			}

			if (treeName == string.Empty)
			{
				LogError("INI Editor-EditValue: no tree name was given");
				return false;
			}

			// Find the tree then find the value in the tree.
			for (int i = 0; i < data.Count; i++)
				for (int n = 0; n < data[i].tree.Count; n++)
					if (data[i].tree[n].dataName == value)
					{
						data[i].tree.RemoveAt(n);
						return true;
					}

			// Tree was not found.
			LogError("INI Editor-DeleteValue: tree [" + treeName + "] not found");
			return false;
		}

        /// <summary>
        /// Private function, clears all internal variables for class.
        /// </summary>
        private void Clear()
		{
			data = new List<Tree>();
			fileLocation = "";
			lastError = "";
			fileLoaded = false;
			logErrorsToConsole = true;
		}

        /// <summary>
        /// Wipes the internal buffer of all data, preparing the class for new use again.
        /// </summary>
        public void Close()
		{
			Clear();
		}

        /// <summary>
        /// Determines if the program has a console application running that it can log errors to.
        /// </summary>
        /// <returns>Returns true if it is able to detect a console window, catches error and returns false if unable to detect a console window.</returns>
        private bool ConsoleDetected()
		{
			try
			{
				return Console.WindowHeight > 0;
			}
			catch
			{
				return false;
			}
		}

        /// <summary>
        /// Logs the given error into lastError.
        /// </summary>
        private void LogError(string error)
		{
			lastError = error;
			if (LogErrorsToConsole)
				if (ConsoleDetected())
					Console.WriteLine(lastError);
		}

        /// <summary>
        /// Get the last known error.
        /// </summary>
        /// <returns>Returns the last known error, returns a blank string if no error text currently in buffer. clears error after returning.</returns>
        public string GetLastError()
		{
			string result = lastError;
			lastError = "";
			return result;
		}

        /// <summary>
        /// Overridden ToString function.
        /// </summary>
        /// <returns>Returns all data in the class as a single string, uses a new line for each value in data.</returns>
        override public string ToString()
        {
            string result = "";

            for (int i = 0; i < data.Count; i++)
            {
                result+= "[" + data[i].treeName + "]" + "\n";
                for (int n = 0; n < data[i].tree.Count; n++)
                    result+= data[i].tree[n].dataName + "=" + data[i].tree[n].data + "\n";                
            }                

            return result;
        }

        /// <summary>
        /// Returns all data in the class as a List of type string.
        /// </summary>
        /// <returns>Returns all data in a string array.</returns>
        public List<string> ToStringArray()
        {
            List<string> result = new List<string>();

            for (int i = 0; i < data.Count; i++)
            {
                result.Add(data[i].ToString());
                for (int n = 0; n < data[i].tree.Count; n++)
                {
                    result.Add(data[i].tree[n].ToString());
                }
            }

            return result;
        }
    }
}
