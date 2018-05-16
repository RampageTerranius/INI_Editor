﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        new public string ToString()
        {
            string result = dataName + "=" + data;
            return result;
        }

        public string dataName;
        public string data;
    }
    
    //handles storing the tree its self
    public class Tree
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

        new public string ToString()
        {            
            return "[" + treeName + "]";
        }

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

        //loads a file from given location into internal buffer
        //returns true if load was sucessful, returns false if unsucessful, if unsucessful a error will be logged into lastError
        public bool Load(string fileLocation, string fileName)
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
                StreamReader sr = new StreamReader(fileLocation + "\\" + fileName);

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
                    //found a tree
                    if (data[i].First<char>().Equals('[') && data[i].Last<char>().Equals(']'))
                    {
                        string treeName = data[i];
                        treeName = treeName.Remove(0, 1);
                        treeName = treeName.Remove(treeName.Length - 1, 1);

                        AddTree(treeName);
                            
                        for (int n = (i + 1); n < data.Count; n++)
                            if (!(data[n].First<char>().Equals('[') && data[n].Last<char>().Equals(']')))
                            {
                                string valueName = data[n].Remove(data[n].IndexOf('='), data[n].Length - data[n].IndexOf('='));
                                string value = data[n].Remove(0, (data[n].IndexOf('=') + 1));
                                AddValue(treeName, valueName, value);
                            }
                            else
                                break;
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



            this.fileLocation = fileLocation;
            this.fileName = fileName;
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
        //returns true if succesfully closed, returns false if unsuccessful, if unsucessful a error will be logged into lastError
        public bool Close()
        {
            Clear();
            return true;
        }

        //saves and closes the current INI, uses Save() and Close() functions to cause this
        //returns true if Save() and Close() are successful, returns false if either failed. if either failed a error will be logged into lastError by their respective functions
        public bool SaveAndClose()
        {
            if (!Save())            
                return false;            
            else if (!Close())            
                return false;            
            
            return true;
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
        //returns void
        public void AddTree(string treeName)
        {
            data.Add(new Tree(treeName));
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
                for (int n = 0; n < data[i].tree.Count; n++)                
                    if (data[i].tree[n].dataName == valueName)
                    {
                        data[i].tree[n].data = newValue;
                        return true;
                    }

            lastError = "INI Editor-EditValue: value " + treeName + "= not found";
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

        //returns all data in the class as a List
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