using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

//C# ini file loader, treats all files as WIN INI type
//DOES NOT handle multiple trees or values of same name!!!!!
//Created By Tyler Brown

namespace INI_Editor
{ 
	//handles internal data for trees
    public class Data
    {
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

        //tostring override, returns value in same format as would be in file
        new public string ToString()
        {
            string result = dataName + "=" + data;
            return result;
        }		

		//returns a Tuple with both the data name and the data in seperate strings
		public Tuple<string, string> ToTupleString()
		{
			return Tuple.Create(dataName, data);
		}

        public string dataName;
        public string data;
    }
    
    //handles storing the tree its self
    public class Tree : INI
    {
        public Tree()
        {
            treeName = "";
            tree = new List<Data>();
        }

        public Tree(string treeName)
        {
            this.treeName = treeName;
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

        public string treeName;
        public List<Data> tree;
    }

    //primary class that handles all INI data
    public class INI
    {
        private List<Tree> data;

        private string fileLocation;
        private string fileName;

        private string lastError;

        private bool fileLoaded;
        public bool FileLoaded
        { get { return fileLoaded; } }


        //default constructor
        public INI()
        {
            Clear();
        }

		//passes off to the main load function, reques both the location of the file AND the name of the file
		//returns true if load was sucessful, returns false if unsucessful, if unsucessful a error will be logged into lastError
		public bool Load(string argFileLocation, string argFileName)
        {
			bool result = load(argFileLocation + "\\" + argFileName);
			fileName = argFileName;
			return result;
        }

		//loads a file from given location into internal buffer
		//self handles file exceptions
		//returns true if load was sucessful, returns false if unsucessful, if unsucessful a error will be logged into lastError
		public bool load(string argFileLocation)
		{

			if (fileLoaded)
			{
				lastError = "INI Editor-Load: A file is already open, please use Close() or SaveAndClose() first";
				return false;
			}

			Clear();

			//load the file
			try
			{
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
			catch (FileNotFoundException e)
			{
				lastError = "INI Editor-Load: File was not found '" + fileLocation + "\\" + fileName + "'";
				return false;
			}
			catch (Exception e)
			{
				lastError = "INI Editor-Load: Unhandled Exception";
				return false;
			}

			//a file has been loaded, prepare file location data and set a file loaded value for future use
			this.fileLocation = argFileLocation;
			fileLoaded = true;

			return true;
		}

        //saves to the same location as was originally opened
        //returns true if successfully saved returns false if unsucessful, if unsucessful a error will be logged into lastError
        public bool Save()
        {
            if (!fileLoaded)
            {
                lastError = "INI Editor-Save: No file has been loaded. if trying to save a program created ini file please use Saveto()";
                return false;
            }

            SaveTo(fileLocation, fileName);

            return true;
        }

        //saves to a specified file location
        //self handles file exceptions
        //returns true if successfully saved returns false if unsucessful, if unsucessful a error will be logged into lastError
        public bool SaveTo(string fileLocation, string fileName)
        {
            if (!Directory.Exists(fileLocation))
            {
                lastError = "INI Editor-SaveTo: Directoy does not exist '" + fileLocation + "'";
                return false;
            }

            try
            {
                StreamWriter sw = new StreamWriter(fileLocation + "\\" + fileName);

                List<string> data = ToStringArray();

                for (int i = 0; i < data.Count; i++)
                    sw.WriteLine(data[i]);

                sw.Flush();
                sw.Close();
            }
            catch (FileLoadException e)
            {
                lastError = "INI Editor-SaveTo: File save exception '" + fileLocation + "\\" + fileName + "'";
                return false;
            }
            catch (Exception e)
            {
                lastError = "INI Editor-SaveTo: Unhandled exception";
                return false;
            }

            return true;
        }

        //private function, clears all internal variables for class
        //returns void
        private void Clear()
        {
            data = new List<Tree>();
            fileLocation = "";
            lastError = "";
            fileLoaded = false;
        }


        //wipes the internal buffer of all data, preparing the class for new use again
        public void Close()
        {
            Clear();
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
            Tree t = new Tree();
            t = GetTree(tree);

            if (t != null)
                for (int i = 0; i < t.tree.Count; i++)
                    if (t.tree[i].dataName == value)
                        return t.tree[i].data;//value exists

            //value does not exist
            return "";
        }

        //returns the first instance of any tree with the name given
        //returns an object of type <Tree>, returns null if tree not found or file not loaded. if unsuccessful a error will be logged into lastError
        public Tree GetTree(string tree)
        {
            for (int i = 0; i < data.Count; i++)
                if (data[i].treeName == tree)
                    return data.ElementAt(i);

            lastError = "INI Editor-GetTree: tree [" + tree + "] not found";
            return null;
        }

        //gets all trees and their values
        //returns List<Tree>
        public List<Tree> GetAll()
        {
            return data;
        }

        //adds a new tree to the data set
        //returns true if tree was added, returns false if tree was not created
        public bool AddTree(string treeName)
        {
            if (!TreeExists(treeName))
            {
                data.Add(new Tree(treeName));
                return true;
            }

            lastError = "INI Editor-AddTree: tree [" + treeName + "] already exists"; ;

            return false;
        }

        //adds a new value to the given tree, does not create the tree if it does not exist
        //returns true if value was added, returns false otherwise. if unsuccessful a error will be logged into lastError
        public bool AddValue(string treeName, string valueName, string value)
        {
            for (int i = 0; i < data.Count; i++)
                if (data[i].treeName == treeName)
                {
                    data[i].tree.Add(new Data(valueName, value));
                    return true;
                }

            lastError = "INI Editor-AddValue: Tree [" + treeName + "] not found";
            return false;
        }

        //edits the given trees name
        //returns true if sucessful, returns false if not. if unsuccessful a error will be logged into lastError
        public bool EditTree(string treeName, string newName)
        {
            for (int i = 0; i < data.Count; i++)
                if (data[i].treeName == treeName)
                {
                    data[i].treeName = newName;
                    return true;
                }

            lastError = "INI Editor-EditTree: Tree [" + treeName + "] not found";
            return false;
        }

        //edits the given trees name and gives it a new set of internal data
        //returns true if sucessful, returns false if not. if unsuccessful a error will be logged into lastError
        public bool EditTree(string treeName, string newName, List<Data> tree)
        {
            for (int i = 0; i < data.Count; i++)
                if (data[i].treeName == treeName)
                {
                    data[i].treeName = newName;
                    data[i].tree = tree;
                    return true;
                }

            lastError = "INI Editor-EditTree: Tree [" + treeName + "] not found";
            return false;
        }

        //edits the given value in the given tree
        //returns true if sucessful, returns false if not. if unsuccessful a error will be logged into lastError
        public bool EditValue(string treeName, string valueName, string newValue)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].treeName == treeName)
                {
                    for (int n = 0; n < data[i].tree.Count; n++)
                    {
                        if (data[i].tree[n].dataName == valueName)
                        {
                            data[i].tree[n].data = newValue;
                            return true;
                        }
                            else if (n == data[i].tree.Count - 1)
                        {
                            lastError = "INI Editor-EditValue: value '" + valueName + "' not found";
                            return false;
                        }


                    }
                }
                else if (i == data.Count - 1)                
                    lastError = "INI Editor-EditValue: tree '" + valueName + "' not found";
            }

            return false;
        }

        //edits the given value in the given tree and also renames the value
        //returns true if sucessful, returns false if not. if unsuccessful a error will be logged into lastError
        public bool EditValue(string treeName, string valueName, string newValueName, string newValue)
        {
            for (int i = 0; i < data.Count; i++)
                for (int n = 0; n < data[i].tree.Count; n++)                
                    if (data[i].tree[n].dataName == valueName)
                    {
                        data[i].tree[n].dataName = newValueName;
                        data[i].tree[n].data = newValue;
                        return true;
                    }

            lastError = "INI Editor-EditValue: value " + treeName + "= not found";
            return false;
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
        //returns all data in the class as a single string
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
