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
    //primary class that handles all INI data
    public class INI
    {
        private List<Tree> data;

        private string fileLocation;

        private string lastError;

        private bool fileLoaded;
        public bool FileLoaded
        { get { return fileLoaded; } }

		private bool logErrorsToConsole;
		public bool LogErrorsToConsole
		{ get { return logErrorsToConsole; } set { logErrorsToConsole = value; } }


        //default constructor
		//prepares a default INI
        public INI()
        {
            Clear();
        }

		//load constructor
		//loads a given file into memory at constr
		public INI(string argFileLocation)
		{
			Clear();
			Load(argFileLocation);
		}

		//load constructors same as above
		public INI(string argFileLocation, string argFileName)
		{
			Clear();
			Load(argFileLocation + "\\" + argFileName);
		}


		//loads a file from given location into internal buffer
		//self handles file exceptions
		//returns true if load was sucessful, returns false if unsucessful, if unsucessful a error will be logged into lastError
		public bool Load(string argFileLocation)
		{
			if (argFileLocation == "")
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

			//load the file
			try
			{
				//FileNotFoundException sometimes not catching non existant files, this will be used as a backup
				if (!File.Exists(argFileLocation))
				{
					LogError("INI Editor - Load: File does not exist at '" + argFileLocation + "'");
					return false;
				}

				List<String> data = new List<string>();
				StreamReader sr = new StreamReader(argFileLocation);

				//load the file into local memory
				while (!sr.EndOfStream)
				{
					string s = sr.ReadLine();
					if (s != "")
						data.Add(s);
				}

				sr.Close();

				//prepare to sort the data into our object
				for (int i = 0; i < data.Count; i++)
				{
					//a tree was found
					if (data[i].First<char>().Equals('[') && data[i].Last<char>().Equals(']'))
					{
						string treeName = data[i];
						treeName = treeName.Remove(0, 1);
						treeName = treeName.Remove(treeName.Length - 1, 1);

						//add a new Tree object to the list
						AddTree(treeName);

						//prepare and add all the Data objects under this tree
						for (int n = (i + 1); n < data.Count; n++)
							if (!(data[n].First<char>().Equals('[') && data[n].Last<char>().Equals(']')))
							{
								string valueName = data[n].Remove(data[n].IndexOf('='), data[n].Length - data[n].IndexOf('='));
								string value = data[n].Remove(0, (data[n].IndexOf('=') + 1));
								AddValue(treeName, valueName, value);
							}
							else
								break;//either found a new Tree or reached the end of the file
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

			//a file has been loaded, prepare file location data and set a file loaded value for future use
			this.fileLocation = argFileLocation;
			fileLoaded = true;

			return true;
		}

		//passes off to the main load function, reques both the location of the file AND the name of the file
		//returns true if load was sucessful, returns false if unsucessful, if unsucessful a error will be logged into lastError
		public bool Load(string argFileLocation, string argFileName)
		{
			return Load(argFileLocation + "\\" + argFileName); ;
		}

		//saves to the same location as was originally opened
		//returns true if successfully saved returns false if unsucessful, if unsucessful a error will be logged into lastError
		public bool Save()
        {
            if (!fileLoaded)
            {
				LogError("INI Editor-Save: No file has been loaded. if trying to save a program created ini file please use Saveto()");
                return false;
            }

            SaveTo(fileLocation);

            return true;
        }

        //saves to a specified file location
        //self handles file exceptions
        //returns true if successfully saved returns false if unsucessful, if unsucessful a error will be logged into lastError
        public bool SaveTo(string argFileLocation)
        {
			//check if location is blank
			if (argFileLocation == "")
			{
				LogError("INI Editor-SaveTo: given file location was blank!");
				return false;
			}

			//attempt to save
			try
            {		
                StreamWriter sw = new StreamWriter(argFileLocation);

                List<string> data = ToStringArray();

                for (int i = 0; i < data.Count; i++)
                    sw.WriteLine(data[i]);

                sw.Flush();
                sw.Close();
            }
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

		//passes off to the main SaveTo function - Requires both the file location AND the file name
		//returns true if successfully saved returns false if unsucessful, if unsucessful a error will be logged into lastError
		public bool SaveTo(string argFileLocation, string argFileName)
		{
			return SaveTo(argFileLocation + "\\" + argFileName);
		}

		//saves and closes the current INI, uses Save() and Close() functions to cause this
		//returns true if Save() and Close() are successful, returns false if either failed. if either failed a error will be logged into lastError by their respective functions
		public bool SaveAndClose()
        {
            if (!Save())
                return false;

            Close();

            return true;
        }

		//checks if the given tree name exists
		//returns true if tree already exists, returns false if it does not
		public bool TreeExists(string tree)
		{
			if (tree == "")
			{
				LogError("INI Editor-TreeExists: No given tree name");
				return false;
			}

			for (int i = 0; i < data.Count; i++)
				if (data[i].treeName == tree)
					return true;//tree exists

			//tree does not exist
			return false;
		}

		//checks if a given value inside of a tree exists
		//returns true if it exists, returns false if not
		public bool ValueExists(string tree, string value)
		{
			if (tree == "")
			{
				LogError("INI Editor-ValueExists: No given tree name");
				return false;
			}

			if (value == "")
			{
				LogError("INI Editor-ValueExists: No given value name");
				return false;
			}

			Tree t = new Tree();
			t = GetTree(tree);

			if (t != null)
				for (int i = 0; i < t.tree.Count; i++)
					if (t.tree[i].dataName == value)
						return true;//value exists

			//value does not exist
			return false;
		}

        //returns the data from a given value in a given tree
        //returns a string, returns blank if failed
        public string GetValue(string tree, string value)
        {
			if (tree == "")
			{
				LogError("INI Editor-GetValue: No given tree name");
				return "";
			}

			if (value == "")
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

            //value does not exist
            return "";
        }

		//returns true or false depending on the given value in a given tree
		//returns bool, an error will be logged if the valeu was neither true or false in this situation will default to return false
		public bool GetValueAsBool(string tree, string value)
		{
			if (tree == "")
			{
				LogError("INI Editor-GetValueAsBool: No given tree name");
				return false;
			}

			if (value == "")
			{
				LogError("INI Editor-GetValueAsBool: No given value name");
				return false;
			}

			Tree t = new Tree();
			t = GetTree(tree);

			if (t != null)
				for (int i = 0; i < t.tree.Count; i++)
					if (t.tree[i].dataName == value)
						if (t.tree[i].data.ToLower() == "true")
							return true;
						else if (t.tree[i].data.ToLower() == "false")
							return false;
						else
						{
							LogError("INI Editor-GetValueAsBool: Warning! [" + tree + "]-'" + value + "' was neither true or false, defaulting return value to false");
							return false;
						}

			LogError("INI Editor-GetValueAsBool: Warning! [" + tree + "]-'" + value + "' was not found");
			return false;
		}

		//returns the given value as a int
		//returns int, logs an error if uanble to parse string as int
		public int GetValueAsInt(string tree, string value)
		{
			if (tree == "")
			{
				LogError("INI Editor-GetValueAsInt: No given tree name");
				return 0;
			}

			if (value == "")
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

		//returns the given value as a double
		//returns double, logs an error if unable to parse string as double
		public double GetValueAsDouble(string tree, string value)
		{
			if (tree == "")
			{
				LogError("INI Editor-GetValueAsDouble: No given tree name ");
				return 0.0;
			}

			if (value == "")
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
			return 0;
		}

		//returns the first instance of any tree with the name given
		//returns a list of type Tree, returns null if tree not found or file not loaded. if unsuccessful a error will be logged into lastError
		public Tree GetTree(string tree)
        {
			if (tree == "")
			{
				LogError("INI Editor-GetTree: No given tree name ");
				return null;
			}

			for (int i = 0; i < data.Count; i++)
                if (data[i].treeName == tree)
                    return data.ElementAt(i);

			LogError("INI Editor-GetTree: tree [" + tree + "] not found");
            return null;
        }

        //gets all trees and their values
        //returns List<Tree>
        public List<Tree> GetAll()
        {
            return data;
        }

		//adds a new value to the given tree, does not create the tree if it does not exist
		//returns true if value was added, returns false otherwise. if unsuccessful a error will be logged into lastError
		public bool AddValue(string treeName, string valueName, string value)
		{
			if (treeName == "")
			{
				LogError("INI Editor-AddValue: No tree name was given");
				return false;
			}

			if (valueName == "")
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

		//creates a new tree in the data set using given name
		//returns true if tree was created, returns false if tree was not created
		public bool AddTree(string treeName)
        {
			if (treeName != "")
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

		//adds the given tree to the data set
		//returns true if tree was added, returns false if tree was not created
		public bool AddTree(Tree argTree)
		{
			if (argTree != null)
			{
				if (argTree.treeName != "")
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

		//edits the given value in the given tree
		//returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError
		public bool EditValue(string treeName, string valueName, string newData)
		{
			if (treeName == "")
			{
				LogError("INI Editor-EditValue: No tree name was given");
				return false;
			}

			if (valueName == "")
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

		//edits the given value in the given tree and also renames the value
		//returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError
		public bool EditValue(string treeName, string valueName, string newValueName, string newValue)
		{
			if (treeName == "")
			{
				LogError("INI Editor-EditValue: No tree name was given");
				return false;
			}

			if (valueName == "")
			{
				LogError("INI Editor-EditValue: No value name was given");
				return false;
			}

			if (newValueName == "")
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

		//overloads for EditValue handling variables otehr then string
		public bool EditValue(string treeName, string valueName, int newData)
		{
			string str = "";
			str = newData.ToString();
			return EditValue(treeName, valueName, str);
		}

		public bool EditValue(string treeName, string valueName, float newData)
		{
			string str = "";
			str = newData.ToString();
			return EditValue(treeName, valueName, str);
		}

		public bool EditValue(string treeName, string valueName, double newData)
		{
			string str = "";
			str = newData.ToString();
			return EditValue(treeName, valueName, str);
		}

		public bool EditValue(string treeName, string valueName, bool newData)
		{
			string str = "";
			str = newData.ToString();
			return EditValue(treeName, valueName, str);
		}

		public bool EditValue(string treeName, string valueName, string newValueName, int newData)
		{
			string str = "";
			str = newData.ToString();
			return EditValue(treeName, valueName, newValueName, str);
		}

		public bool EditValue(string treeName, string valueName, string newValueName, float newData)
		{
			string str = "";
			str = newData.ToString();
			return EditValue(treeName, valueName, newValueName, str);
		}

		public bool EditValue(string treeName, string valueName, string newValueName, double newData)
		{
			string str = "";
			str = newData.ToString();
			return EditValue(treeName, valueName, newValueName, str);
		}

		public bool EditValue(string treeName, string valueName, string newValueName, bool newData)
		{
			string str = "";
			str = newData.ToString();
			return EditValue(treeName, valueName, newValueName, str);
		}

		//edits the given trees name
		//returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError
		public bool EditTree(string treeName, string newName)
        {
			if (treeName == "")
			{
				LogError("INI Editor-EditValue: No tree name was given");
				return false;
			}

			if (newName == "")
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

        //edits the given trees name and gives it a new set of internal data
        //returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError
        public bool EditTree(string treeName, string newName, List<Data> tree)
        {
			if (treeName == "")
			{
				LogError("INI Editor-EditTree: No tree name was given");
				return false;
			}

			if (newName == "")
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

		//edits the given tree giving it a new set of internal data
		//returns true if successful, returns false if not. if unsuccessful a error will be logged into lastError
		public bool EditTree(string treeName, List<Data> tree)
		{
			if (treeName == "")
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


		//deletes the given tree from the INI
		public bool DeleteTree(string treeName)
		{
			//checking that the input is not blank
			if (treeName == "")
			{
				LogError("INI Editor-EditTree: no tree name was given");
				return false;
			}

			//find and delete the tree
			for (int i = 0; i < data.Count; i++)
				if (data[i].treeName == treeName)
				{
					data.RemoveAt(i);
					return true;
				}

			//tree not found
			LogError("INI Editor-DeleteTree: Tree [" + treeName + "] not found");
			return false;
		}

		//deletes the given value from the given tree, does not delete the tree
		public bool DeleteValue(string treeName, string value)
		{
			//checking that the input is not blank
			if (value == "")
			{
				LogError("INI Editor-EditValue: no value name was given");
				return false;
			}

			if (treeName == "")
			{
				LogError("INI Editor-EditValue: no tree name was given");
				return false;
			}

			//find the tree then find the value in the tree
			for (int i = 0; i < data.Count; i++)
				for (int n = 0; n < data[i].tree.Count; n++)
					if (data[i].tree[n].dataName == value)
					{
						data[i].tree.RemoveAt(n);
						return true;
					}

			//tree not found
			LogError("INI Editor-DeleteValue: tree [" + treeName + "] not found");
			return false;
		}

		//private function, clears all internal variables for class
		//returns void
		private void Clear()
		{
			data = new List<Tree>();
			fileLocation = "";
			lastError = "";
			fileLoaded = false;
			logErrorsToConsole = true;
		}


		//wipes the internal buffer of all data, preparing the class for new use again
		public void Close()
		{
			Clear();
		}

		//determines if the program has a console application running that it can log errors to
		//returns > 1 (true) if it is able to detect a console window, catches error and returns false if unable to detect a console window
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

		//logs the given error into lastError
		private void LogError(string error)
		{
			lastError = error;
			if (LogErrorsToConsole)
				if (ConsoleDetected())
					Console.WriteLine(lastError);
		}

		//get the last known error
		//returns the last known error, returns a blank string if no error text currently in buffer. clears error after returning
		public string GetLastError()
		{
			string result = lastError;
			lastError = "";
			return result;
		}

		//overridden ToString function
		//returns all data in the class as a single string, uses a new line for each value in data
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

        //returns all data in the class as a List of type string
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
